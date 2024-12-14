namespace ApiPortfolio.Models
{
    public class ClassUser
    {
        public int id_user { get; set; }
        public string user_unique_id { get; set; }
        public string user_full_name { get; set; }
        public string user_email { get; set; }
        public ClassRole role { get; set; }

    }
}
