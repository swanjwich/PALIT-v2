using System.ComponentModel.DataAnnotations;

namespace it15_palit.Models.Auth
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Username is required.")]
        [MaxLength(50, ErrorMessage = "Maximum of 50 characters are allowed.")]
        public required string Username { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        [StringLength(20, MinimumLength = 8, ErrorMessage = "Minimum of 8 characters are allowed.")]
        public required string Password { get; set; }
    }
}
