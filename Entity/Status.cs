using System.ComponentModel.DataAnnotations;

namespace cce106_palit.Entity
{
    public class Status
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime Created_at { get; set; }
        public DateTime Updated_at { get; set; }

        public ICollection<Order> Orders { get; set; }

    }
}
