namespace HexaERP.MVC.Models
{
    public class UserCredentials
    {
        public string organization { get; set; }
        public int? WorkingLocation { get; set; }
        public string email { get; set; }
        public string[] role { get; set; }
        public string userId { get; set; }
        public string Name { get; set; }
        public string Department { get; set; }
    }
}