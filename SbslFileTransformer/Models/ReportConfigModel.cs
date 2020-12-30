namespace SbslFileTransformer.Models
{
    public class ReportConfigModel
    {
        public string BaseUrl { get; set; } = @".api.blackline.com/api";
        public string EnvironmentUrl { get; set; }
        public string UserToken { get; set;}
        public string UserName { get; set; }
        public string Password { get; set; }
        /// <summary>
        /// CSV, PDF, XLSX
        /// </summary>
        public string ExportType { get; set; } //CSV, PDF, XLSX
        public string EmailHeader { get; set; } = "Reconciliation Ageing Report";
        public string EmailBody { get; set; }
        public string 
    }
}
