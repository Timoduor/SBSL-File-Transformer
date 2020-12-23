namespace SbslFileTransformer.Models
{
    public class ReportConfigModel
    {
        public string BaseUrl { get; set; } = @".api.blackline.com/api";
        public string EnvironmentUrl { get; set; }
        public string UserToken { get; set;}
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
