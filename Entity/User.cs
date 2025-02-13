using System.ComponentModel.DataAnnotations;

namespace it15_palit.Entity
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        public required string Username { get; set; }
        public required string Password { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
        public bool IsVerified { get; set; } = false;
        public DateTime? Email_Verified_At { get; set; }

        public string Status = "Active";
        public DateTime Created_At { get; set; }
        public DateTime Updated_At { get; set; }
    }
}
