using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace it15_palit.Entity
{
    [Index(nameof(Email), IsUnique = true)]
    [Index(nameof(Username), IsUnique = true)]

    public class Customer
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Username is required.")]
        [MaxLength(50, ErrorMessage = "Maximum of 50 characters are allowed.")]
        public required string Username { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public required string Password { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [MaxLength(100, ErrorMessage = "Maximum of 100 characters are allowed.")]
        public required string Name { get; set; }

        [Required(ErrorMessage = "Address is required.")]
        [MaxLength(100, ErrorMessage = "Maximum of 100 characters are allowed.")]
        public required string Address { get; set; }

        [Required(ErrorMessage = "Email address is required.")]
        [DataType(DataType.EmailAddress)]
        [MaxLength(50, ErrorMessage = "Maximum of 50 characters are allowed.")]
        public required string Email { get; set; }
        public bool IsVerified { get; set; } = false;
        public DateTime? Email_verified_at { get; set; }

        [Required(ErrorMessage = "Contact number is required.")]
        public required string Contact_Number { get; set; }
        public string? Display_Picture { get; set; }

        public DateTime Created_at { get; set; }
        public DateTime Updated_at { get; set; }

        public ICollection<Cart> Carts { get; set; }
        public ICollection<Order> Orders { get; set; }


    }
}
