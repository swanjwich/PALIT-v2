using System.ComponentModel.DataAnnotations;

namespace cce106_palit.Entity
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(100)]
        public string? Username { get; set; }

        [MaxLength(255)]
        public string? Password { get; set; }

        [MaxLength(100)]
        public string? Name { get; set; }

        [MaxLength(255)]
        public required string Email { get; set; }
        public bool IsVerified { get; set; } = false;
        public string? VerificationToken { get; set; }

        public string Status = "Active";

        [MaxLength(100)]
        public string? Role { get; set; } // Super Admin, Admin, Inventory Manager, Auditor, Staff
        public DateTime Created_At { get; set; }
        public DateTime Updated_At { get; set; }

        public ICollection<Notification>? Notifications { get; set; }

    }
}
