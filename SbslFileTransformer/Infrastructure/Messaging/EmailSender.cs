using MailKit.Net.Smtp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MimeKit;
using SbslFileTransformer.Data;
using SbslFileTransformer.Infrastructure.Encryption;
using SbslFileTransformer.Models;
using SbslFileTransformer.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
                    };
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }

        public async Task SendMessage(IEnumerable<string> recipients, string subject, string content, bool isHtml = false, IEnumerable<string> filePaths = null)
        {
            if(recipients == null || recipients.Count() == 0)
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
                    builder.Attachments.Add(file);
                }
            }

            emailMessage.Body = builder.ToMessageBody();

            return emailMessage;
        }

        private async Task Send(MimeMessage mailMessage)
        {
            using (var client = new SmtpClient())
            {
                try
                {
                    await client.ConnectAsync(_emailConfig.SmtpServer, _emailConfig.Port, _emailConfig.UseSsl);

                    client.AuthenticationMechanisms.Remove("XOAUTH2");

                    await client.AuthenticateAsync(_emailConfig.UserName, _emailConfig.Password);

                    await client.SendAsync(mailMessage);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message, ex);
                }
                finally
                {
                    client.Disconnect(true);
                    client.Dispose();
                }
            }
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
