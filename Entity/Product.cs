using it15_palit.Entity;
using System;
using System.ComponentModel.DataAnnotations;

namespace it15_palit.Entity
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public required string Name { get; set; }

        [MaxLength()]
        public string? Description { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Stock_quantity { get; set; }

        [Required]
        public int Category_id { get; set; }

        [Url]
        public string? Image_url { get; set; }

        public DateTime Created_at { get; set; }

        public DateTime Updated_at { get; set; }

        //parent
        public Category Category { get; set; }

        // child
        public ICollection<Cart> Carts { get; set; }
        public ICollection<OrderDetail> OrderDetails { get; set; }

    }
}
