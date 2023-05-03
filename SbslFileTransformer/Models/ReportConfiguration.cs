namespace SbslFileTransformer.Models
{
    public class ReportConfiguration
    {
        public int Id { get; set; }

        public string ReportDescription { get; set; }

        public string NameKeywords { get; set; }

        public string ColumnKeywords { get; set; }

        public int DaysOverdue { get; set; }

        public string RecipientEmails { get; set; }

        public bool IsEnabled { get; set; }
    }
}
