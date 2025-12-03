using SQLite;

namespace QrCode_Reader.Models
{
    public class Client
    {
        [PrimaryKey]
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Delivered { get; set; }
        public int ProjectId { get; set; }
    }
}
