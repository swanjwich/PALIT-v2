using System.ComponentModel.DataAnnotations;

namespace cce106_palit.Entity
{
    public class Cart
    {
        public int Customer_Id { get; set; }
        public int Product_Id { get; set; }

        [Required]
        public int Quantity { get; set; }
        public bool Is_Checked { get; set; }
        public DateTime Created_At  { get; set; }
        public DateTime Updated_At { get;set; }

        //parents
        [Required]
        public Customer Customer { get; set; }

        [Required]
        public Product Product { get; set; }



    }
}
