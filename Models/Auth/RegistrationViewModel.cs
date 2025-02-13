using System.ComponentModel.DataAnnotations;

namespace it15_palit.Models.Auth
{
    public class RegistrationViewModel
    {

        [Required(ErrorMessage = "Username is required.")]
        [MaxLength(50, ErrorMessage = "Maximum of 50 characters are allowed.")]
        public required string Username { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        [StringLength(20, MinimumLength = 8, ErrorMessage = "Minimum of 8 characters are allowed.")]
        public required string Password { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [MaxLength(100, ErrorMessage = "Maximum of 100 characters are allowed.")]
        public required string Name { get; set; }

        [Required(ErrorMessage = "Address is required.")]
        [MaxLength(100, ErrorMessage = "Maximum of 100 characters are allowed.")]
        public required string Address { get; set; }

        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [DataType(DataType.EmailAddress)]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Contact number is required.")]
        public required string Contact_Number { get; set; }

    }
}
