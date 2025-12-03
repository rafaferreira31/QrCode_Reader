using SQLite;

namespace QrCode_Reader.Models
{
    public class Project
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
