using cce106_palit.Entity;
using System.ComponentModel.DataAnnotations;

namespace cce106_palit.Models
{
    public class CartViewModel
    {
        [Required]
        public int Customer_Id { get; set; }

        [Required]
        public int Product_Id { get; set; }

        [Required]
        public int Quantity { get; set; }
        public bool Is_Checked { get; set; }

        public DateTime Created_At { get; set; }
        public DateTime Updated_At { get; set; }

        [Required]
        public Customer Customer { get; set; }

        [Required]
        public Product Product { get; set; }
    }
}
