
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MimeKit;
using SbslFileTransformer.Data;
using SbslFileTransformer.Infrastructure.Encryption;
using SbslFileTransformer.Models;
using SbslFileTransformer.Models.Enums;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using MimeKit.IO;
using MimeKit.IO.Filters;
using ContentDisposition = MimeKit.ContentDisposition;
using ContentType = MimeKit.ContentType;

namespace SbslFileTransformer.Infrastructure.Messaging
{
    public class EmailSender
    {
        private SmtpConfigModel _emailConfig;
        private ILogger<EmailSender> _logger;

        public EmailSender(IServiceScopeFactory serviceScopeFactory, ILogger<EmailSender> logger, EncryptionManager encryptionManager)
        {
            _logger = logger;

            try
            {
                //get values from dbcontext to populate the email configuration
                using (var scope = serviceScopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                    var configurations = dbContext.Configurations.Where(c => c.ConfigType == ConfigurationType.Email).ToList();

                    _emailConfig = new SmtpConfigModel
                    {
                        Port = Convert.ToInt32(configurations.FirstOrDefault(c => c.Key == "Port")?.Value),
                        UserName = configurations.FirstOrDefault(c => c.Key == "UserName")?.Value,
                        Password = encryptionManager.Decrypt(configurations.FirstOrDefault(c => c.Key == "Password")?.Value),
                        EmailAddress = configurations.FirstOrDefault(c => c.Key == "EmailAddress")?.Value,
                        SmtpServer = configurations.FirstOrDefault(c => c.Key == "SmtpServer")?.Value,
                        Name = configurations.FirstOrDefault(c => c.Key == "Name")?.Value,
                        Recipients = configurations.FirstOrDefault(c => c.Key == "Recipients")?.Value,
                        UseSsl = Convert.ToBoolean(configurations.FirstOrDefault(c => c.Key == "UseSsl" && c.ConfigType == ConfigurationType.Email)?.Value),
                        UseDefaultCredentials = Convert.ToBoolean(configurations.FirstOrDefault(c => c.Key == "UseDefaultCredentials" && c.ConfigType == ConfigurationType.Email)?.Value),
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }

        public async Task SendMessage(IEnumerable<string> recipients, string subject, string content, bool isHtml = false, IEnumerable<string> filePaths = null)
        {
            if (recipients == null || !recipients.Any())
            {
                recipients = _emailConfig.Recipients.Split(',', '\n', '\r');
            }

            var message = new Message(recipients, subject, content, filePaths);

            var mimeMessage = CreateEmailMessage(message, isHtml);

            await Send(mimeMessage);
        }

        private MimeMessage CreateEmailMessage(Message message, bool isHtml = false)
        {
            var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress(_emailConfig.Name, _emailConfig.EmailAddress));

            emailMessage.To.AddRange(message.To);

            emailMessage.Subject = message.Subject;

            var builder = new BodyBuilder();

            if (isHtml)
            {
                builder.HtmlBody = message.Content; //html content string
            }
            else
            {
                builder.TextBody = message.Content;
            }

            if (message?.FilePaths != null)
            {
                foreach (var file in message.FilePaths)
                {
                    string mediaType = GetMediaType(file);

                    var disposition = new ContentDisposition(DispositionTypeNames.Attachment)
                    {
                        FileName = Path.GetFileName(file),
                        CreationDate = DateTime.Now,
                        IsAttachment = true,
                        ModificationDate = DateTime.Now,
                        Size = new FileInfo(file).Length,
                        ReadDate = DateTime.Now
                    };

                    var contentType = new ContentType(mediaType.Split('/')[0], mediaType.Split('/')[1])
                    {
                        Name = Path.GetFileName(file),
                        CharsetEncoding = Encoding.Default,
                    };

                    MimePart attachment = !contentType.IsMimeType("text", "*") ? new MimePart(contentType) : (MimePart)new TextPart(contentType.MediaSubtype);
                    LoadContent(attachment, File.OpenRead(file));
                    var mimeEntity = (MimeEntity)attachment;

                    mimeEntity.ContentDisposition = disposition;
                    mimeEntity.ContentDisposition.FileName = Path.GetFileName(file);
                    mimeEntity.ContentType.Name = Path.GetFileName(file);

                    builder.Attachments.Add(mimeEntity);
                }
            }

            emailMessage.Body = builder.ToMessageBody();

            return emailMessage;
        }

        private void LoadContent(MimePart attachment, FileStream stream)
        {
            MemoryBlockStream memoryBlockStream = new MemoryBlockStream();
            if (attachment.ContentType.IsMimeType("text", "*"))
            {
                byte[] numArray = ArrayPool<byte>.Shared.Rent(4096);
                BestEncodingFilter bestEncodingFilter = new BestEncodingFilter();
                try
                {
                    int num;
                    int outputIndex;
                    int outputLength;
                    while ((num = stream.Read(numArray, 0, 4096)) > 0)
                    {
                        bestEncodingFilter.Filter(numArray, 0, num, out outputIndex, out outputLength);
                        memoryBlockStream.Write(numArray, 0, num);
                    }
                    bestEncodingFilter.Flush(numArray, 0, 0, out outputIndex, out outputLength);
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(numArray);
                }
                attachment.ContentTransferEncoding = bestEncodingFilter.GetBestEncoding(EncodingConstraint.SevenBit);
            }
            else
            {
                attachment.ContentTransferEncoding = ContentEncoding.Base64;
                stream.CopyTo((Stream)memoryBlockStream, 4096);
            }
            memoryBlockStream.Position = 0L;
            attachment.Content = (IMimeContent)new MimeContent((Stream)memoryBlockStream);
        }

        private async Task Send(MimeMessage mailMessage)
        {
            if (_emailConfig.UseDefaultCredentials)
            {
                using (var client = new SmtpClient(_emailConfig.SmtpServer, _emailConfig.Port))
                {
                    client.UseDefaultCredentials = _emailConfig.UseDefaultCredentials;
                    client.EnableSsl = _emailConfig.UseSsl;


                    var address = mailMessage.From.Mailboxes.First().Address;
                    var name = mailMessage.From.Mailboxes.First().Name;

                    var message = new MailMessage()
                    {
                        From = new MailAddress(address, name),
                        Body = mailMessage.TextBody,
                    };

                    foreach (var email in mailMessage.To.Mailboxes)
                    {
                        message.To.Add(email.Address);
                    }

                    foreach (var attachment in mailMessage.Attachments)
                    {
                        var memoryStream = new MemoryStream();
                        await attachment.WriteToAsync(memoryStream);

                        var mediaType = GetMediaType(attachment.ContentDisposition.FileName);

                        message.Attachments.Add(new Attachment(memoryStream, attachment.ContentDisposition.FileName, mediaType)
                        {
                            ContentType = new System.Net.Mime.ContentType(mediaType),
                            NameEncoding = Encoding.Default,
                            Name = attachment.ContentDisposition.FileName,
                            TransferEncoding = TransferEncoding.Base64,
                            ContentDisposition =
                            {
                                FileName = attachment.ContentDisposition.FileName,
                                Inline = false,
                                CreationDate = DateTime.Now,
                                DispositionType = DispositionTypeNames.Attachment,
                                ModificationDate = DateTime.Now,
                                ReadDate = DateTime.Now,
                                Size =  attachment.ContentDisposition.Size ?? -1,
                            }
                        });
                    }

                    client.Send(message);

                }
            }
            else
            {
                using (var client = new MailKit.Net.Smtp.SmtpClient())
                {

                    await client.ConnectAsync(_emailConfig.SmtpServer, _emailConfig.Port, _emailConfig.UseSsl);

                    client.AuthenticationMechanisms.Remove("XOAUTH2");

                    await client.AuthenticateAsync(_emailConfig.UserName, _emailConfig.Password);

                    await client.SendAsync(mailMessage);

                    client.Disconnect(true);
                }
            }
        }

        private static string GetMediaType(string file)
        {
            var extension = Path.GetExtension(file)?.ToLower();

            string mediaType;

            switch (extension)
            {
                case ".txt":
                    mediaType = "text/plain";
                    break;
                case ".csv":
                    mediaType = "text/csv";
                    break;
                case ".xlsx":
                    mediaType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    break;
                default:
                    mediaType = "text/plain";
                    break;
            }

            return mediaType;
        }
    }



    public class Message
    {
        public List<MailboxAddress> To { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }

        public IEnumerable<string> FilePaths { get; set; }

        public Message(IEnumerable<string> to, string subject, string content, IEnumerable<string> filePaths)
        {
            To = new List<MailboxAddress>();

            var range = to.Select(x => new MailboxAddress(x.Split("@").FirstOrDefault().Trim(), x.Trim()));

            To.AddRange(range);

            Subject = subject;
            Content = content;
            FilePaths = filePaths;
        }
    }
}
