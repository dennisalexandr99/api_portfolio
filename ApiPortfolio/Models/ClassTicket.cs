namespace ApiPortfolio.Models
{
    public class ClassTicket
    {
        public int id_ticket { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string picture_url { get; set; }
        public string created_by { get; set; }
        public DateTime create_time { get; set; }
        public DateTime update_time { get; set; }
        public ClassUser user { get; set; }
    }
}
