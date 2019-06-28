namespace PayEx.Net.Api.Models
{
    public class Error
    {
        public string SessionId { get; set; }
        public string Type { get; set; }
        public string Title { get; set; }
        public int Status { get; set; }
        public string Instance { get; set; }
        public string Detail { get; set; }
        public Problem[] Problems { get; set; }
    }
}
