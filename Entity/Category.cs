using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace it15_palit.Entity
{
    public class Category
    {
        [Key]
        public int Id {  get; set; }

        [MaxLength(50)]
        public required string Name { get; set; }
        public string? Description { get; set; }
        public string? Image_url { get; set; }
        public DateTime Created_at { get; set; }
        public DateTime Updated_at { get; set; }

        // child
        public ICollection<Product> Products { get; set; }

    }
}
