using System;

namespace SbslFileTransformer.Models
{
    public class LogEntries
    {
        public int Id { get; set; }
        public string TimeStamp { get; set; }
        public string Level { get; set; }
        public string Exception { get; set; }
        public string RenderedMessage { get; set; }
        public string Properties { get; set; }
        public DateTime Date { get; set; }
    }
}
