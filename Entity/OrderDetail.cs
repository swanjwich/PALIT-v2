using System.ComponentModel.DataAnnotations;

namespace it15_palit.Entity
{
    public class OrderDetail
    {
        [Key]
        public int Id { get; set; }
        public int Order_Id { get; set; }
        public int Product_Id { get; set; }
        public int Quantity { get; set; }

        public DateTime Created_at { get; set; }
        public DateTime Updated_at { get; set; }

        // parents
        public Order Order { get; set; }
        public Product Product { get; set; }
    }
}
