using System.Collections.Generic;

namespace SbslFileTransformer.Models
{
    public class ReportConfigModel
    {
        public string BaseUrl { get; set; } = @".api.blackline.com/api";
        public string EnvironmentUrl { get; set; }
        public string UserToken { get; set; }
        public Dictionary<string, string> UserNamesAndPasswords { get; set; }

        /// <summary>
        ///     CSV, PDF, Excel
        /// </summary>
        public string ExportType { get; set; }

        public string EmailHeader { get; set; } = "Reconciliation Ageing Report";
        public string EmailBody { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Scope { get; set; }
        public string TokenUrl { get; set; }
    }
}