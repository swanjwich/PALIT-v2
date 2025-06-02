using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace cce106_palit.Entity
{
    public class StockIn
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Product")]
        public int ProductId { get; set; }

        public int Amount { get; set; }

        public DateOnly ExpirationDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime DateReceived { get; set; }

        [MaxLength(100)]
        public string? ReceivedBy { get; set; }

        [MaxLength(255)]
        public string? Remarks { get; set; }

        [MaxLength(50)]
        public string? BatchNumber { get; set; }

        public Product? Product { get; set; }

    }
}
