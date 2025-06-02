using System.ComponentModel.DataAnnotations;

namespace cce106_palit.Entity
{
    public class Notification
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public int? CustomerId { get; set; }
        public string? Message { get; set; }
        public string? Link { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public User? User { get; set; }
        public Customer? Customer { get; set; }

    }
}
