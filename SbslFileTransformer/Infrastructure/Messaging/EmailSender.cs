using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using MimeKit;
using SbslFileTransformer.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SbslFileTransformer.Infrastructure.Messaging
{
    public class EmailSender
    {
        private EmailConfiguration _emailConfig;
        private ILogger<EmailSender> _logger;

        public EmailSender(ILogger<EmailSender> logger)
        {
            _logger = logger;

            //get values from dbcontext to populate the email configuration
        }

        public async void SendMessage(IEnumerable<string> recipients, string subject, string content, bool isHtml = false)
        {
            var message = new Message(recipients, subject, content);

            var mimeMessage = CreateEmailMessage(message, isHtml);

            await Send(mimeMessage);
        }

        private MimeMessage CreateEmailMessage(Message message, bool isHtml = false)
        {
            var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress(_emailConfig.Name, _emailConfig.EmailAddress));

            emailMessage.To.AddRange(message.To);

            emailMessage.Subject = message.Subject;

            if (isHtml)
            {
                emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = message.Content }; //html content string
            }
            else
            {
                emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Text) { Text = message.Content };
            }
            return emailMessage;
        }

        private async Task Send(MimeMessage mailMessage)
        {
            using (var client = new SmtpClient())
            {
                try
                {
                    await client.ConnectAsync(_emailConfig.SmtpServer, _emailConfig.Port, true);

                    client.AuthenticationMechanisms.Remove("XOAUTH2");

                    await client.AuthenticateAsync(_emailConfig.UserName, _emailConfig.Password);

                    await client.SendAsync(mailMessage);
                }
                catch(Exception ex)
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

    public class EmailConfiguration
    {
        public string Name { get; set; }
        public string EmailAddress { get; set; }
        public string SmtpServer { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }

    public class Message
    {
        public List<MailboxAddress> To { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }

        public Message(IEnumerable<string> to, string subject, string content)
        {
            To = new List<MailboxAddress>();

            To.AddRange(to.Select(x => new MailboxAddress(x.Split("@").FirstOrDefault(), x)));
            Subject = subject;
            Content = content;
        }
    }
}
