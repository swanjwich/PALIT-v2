using System.ComponentModel.DataAnnotations;

namespace it15_palit.Entity
{
    public class Payment
    {
        [Key]
        public int Id { get; set; }
        public int Order_Id { get; set; }
        public decimal Amount { get; set; }
        public required string Method { get; set; }
        public required string Transaction_Id { get; set; }
        public required string Status { get; set; }
        public DateTime Created_at { get; set; }
        public DateTime Updated_at { get; set; }

        //parent
        public Order Order { get; set; }
    }
}
