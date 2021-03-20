
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MimeKit;
using SbslFileTransformer.Data;
using SbslFileTransformer.Infrastructure.Encryption;
using SbslFileTransformer.Models;
using SbslFileTransformer.Models.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Threading.Tasks;
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
                        Password = encryptionManager.Decrypt(configurations.FirstOrDefault(c => c.Key == "Password")?.Value ?? "#"),
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
                recipients = _emailConfig.Recipients.Split(new[] { ',', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            }

            var message = new Message(recipients, subject, content, filePaths);

            var mimeMessage = CreateEmailMessage(message, isHtml);

            await Send(mimeMessage);
        }

        private (MimeMessage, Message) CreateEmailMessage(Message message, bool isHtml = false)
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

                    var contentType = new ContentType(mediaType.Split('/')[0], mediaType.Split('/')[1]);

                    builder.Attachments.Add(Path.GetFileName(file), File.OpenRead(file), contentType);
                }
            }

            emailMessage.Body = builder.ToMessageBody();

            return (emailMessage, message);
        }

        private async Task Send((MimeMessage, Message) mailMessage)
        {
            if (_emailConfig.UseDefaultCredentials)
            {
                using (var client = new SmtpClient(_emailConfig.SmtpServer, _emailConfig.Port))
                {
                    client.UseDefaultCredentials = _emailConfig.UseDefaultCredentials;
                    client.EnableSsl = _emailConfig.UseSsl;

                    var message = new MailMessage
                    {
                        From = new MailAddress(_emailConfig.EmailAddress, _emailConfig.Name),
                        Body = mailMessage.Item2.Content,
                        Subject = mailMessage.Item2.Subject
                    };

                    foreach (var email in mailMessage.Item2.To)
                    {
                        if(!string.IsNullOrEmpty(email.Address))
                            message.To.Add(email.Address);
                    }

                    if (mailMessage.Item2.FilePaths != null)
                    {
                        foreach (var file in mailMessage.Item2.FilePaths)
                        {
                            Attachment data = new Attachment(file, MediaTypeNames.Application.Octet);

                            var disposition = data.ContentDisposition;
                            disposition.CreationDate = File.GetCreationTime(file);
                            disposition.ModificationDate = File.GetLastWriteTime(file);
                            disposition.ReadDate = File.GetLastAccessTime(file);

                            message.Attachments.Add(data);
                        }
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

                    await client.SendAsync(mailMessage.Item1);

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
