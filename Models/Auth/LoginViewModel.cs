using System.ComponentModel.DataAnnotations;

namespace cce106_palit.Models.Auth
{
    public class LoginViewModel
    {
        [Required]
        [MaxLength(50, ErrorMessage = "Maximum of 50 characters are allowed.")]
        public required string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public required string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}
