using System.Collections.Generic;
using System.Linq;
using MimeKit;

namespace SbslFileTransformer.Infrastructure.Messaging
{
    public class Message
    {
        public Message(IEnumerable<string> to, string subject, string content, IEnumerable<string> filePaths)
        {
            To = new List<MailboxAddress>();

            var range = to.Select(x => new MailboxAddress(x.Split("@").FirstOrDefault().Trim(), x.Trim()));

            To.AddRange(range);

            Subject = subject;
            Content = content;
            FilePaths = filePaths;
        }

        public List<MailboxAddress> To { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }

        public IEnumerable<string> FilePaths { get; set; }
    }
}