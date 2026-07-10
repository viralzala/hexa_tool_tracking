namespace HexaERP.MVC.Models
{
    public class UploadMapEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public string FloorMap
        {
            get { return Name.Replace(" ", string.Empty) + ".jpg"; }
        }
    }
}