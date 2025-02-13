using System.ComponentModel.DataAnnotations;

namespace it15_palit.Entity
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
