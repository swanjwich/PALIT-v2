using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace cce106_palit.Entity
{
    [Index(nameof(Email), IsUnique = true)]
    [Index(nameof(Username), IsUnique = true)]

    public class Customer
    {
        [Key]
        public int Id { get; set; }
        public string? Display_Picture { get; set; }

        [StringLength(100)]
        public string? Username { get; set; }

        [DataType(DataType.Password)]
        public required string Password { get; set; }

        [StringLength(100)]
        public required string Name { get; set; }

        [StringLength(255)]
        public string? Address { get; set; }
        public string? Contact_Number { get; set; }

        [DataType(DataType.EmailAddress)]
        [StringLength(255)]
        public required string Email { get; set; }
        public bool IsVerified { get; set; } = false;
        public string? VerificationToken { get; set; }
        public string? PasswordResetToken { get; set; }
        public DateTime? ResetTokenExpiry { get; set; }
        public bool Is_Deactivated { get; set; } = false;
        public bool Is_Deleted { get; set; } = false;

        public DateTime Created_At { get; set; }
        public DateTime Updated_At { get; set; }

        public ICollection<Cart>? Carts { get; set; }
        public ICollection<Order>? Orders { get; set; }
        public ICollection<Notification>? Notifications { get; set; }

    }
}
