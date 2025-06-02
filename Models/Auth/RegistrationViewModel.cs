using System.ComponentModel.DataAnnotations;

namespace cce106_palit.Models.Auth
{
    public class RegistrationViewModel
    {
        [Required(ErrorMessage = "Name is required.")]
        [MaxLength(100, ErrorMessage = "Maximum of 100 characters are allowed.")]
        public required string Name { get; set; }

        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [DataType(DataType.EmailAddress)]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        [StringLength(20, MinimumLength = 8, ErrorMessage = "Minimum of 8 characters are allowed.")]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$",
        ErrorMessage = "Password must contain at least one uppercase letter, one number, and one special character.")]
        public required string Password { get; set; }

    }
}
