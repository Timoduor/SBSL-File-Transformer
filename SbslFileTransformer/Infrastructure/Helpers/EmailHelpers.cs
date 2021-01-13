using SbslFileTransformer.Data;
using SbslFileTransformer.Infrastructure.Messaging;
using SbslFileTransformer.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SbslFileTransformer.Infrastructure.Helpers
{
    public class EmailHelpers
    {
        public static void SendEmails(ApplicationDbContext dbContext, string header, string body, IEnumerable<string> files, EmailSender emailSender)
        {
            var config = dbContext.Configurations.FirstOrDefault(c => c.ConfigType == ConfigurationType.Email && c.Key == "Recipients");

            var recipients = config.Value.Split(new char[] { ',', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            emailSender.SendMessage(recipients, header, body, false, files);//yeah maybe
        }
    }
}
