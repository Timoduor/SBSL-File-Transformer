using SbslFileTransformer.Infrastructure.Messaging;
using SbslFileTransformer.Models;
using SbslFileTransformer.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SbslFileTransformer.Infrastructure.Helpers
{
    public class EmailHelpers
    {
        public static async Task SendEmails(IEnumerable<Configuration> configurations, string header, string body,
            IEnumerable<string> files, EmailSender emailSender)
        {
            Configuration config = configurations.FirstOrDefault(c =>
                c.ConfigType == ConfigurationType.Email && c.Key == "Recipients");

            string[] recipients = config.Value.Split(new[] { ',', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            await emailSender.SendMessage(recipients, header, body, false, files);
        }
    }
}