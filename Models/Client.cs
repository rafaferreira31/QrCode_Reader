using SQLite;

namespace QrCode_Reader.Models
{
    public class Client
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string UNID { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public bool Delivered { get; set; }
        public int ProjectId { get; set; }
        public DateTime? DeliverDate { get; set; }
        public string DeliveryNote { get; set; }


        public string FullName => $"{Name} {LastName}";
    }
}