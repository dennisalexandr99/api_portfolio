namespace ApiPortfolio.Models
{
    public class ClassTicketHeader
    {
        public int limit {  get; set; }
        public int last_index { get; set; }
        public string order_by { get; set; }
        public string order_type { get; set; }
        public ClassTicket[] tickets { get; set; }
    }
}
