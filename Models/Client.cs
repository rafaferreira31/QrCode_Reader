using SQLite;

namespace QrCode_Reader.Models
{
    public class Client
    {
        [PrimaryKey]
        public int Id { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public bool Delivered { get; set; }
        public int ProjectId { get; set; }
        public DateTime? DeliverDate { get; set; }
        public string DeliveryNote { get; set; }
    }
}



/* TODO: IMPLEMENTAR CLIENT CERTO
ESTRUTURA DA TABELA CLIENTE FINAL

Id 
First Name
Last Name
Rotation do Produto
Delivered 
Data de Entrega

*/