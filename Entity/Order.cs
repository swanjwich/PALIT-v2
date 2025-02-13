using System.ComponentModel.DataAnnotations;

namespace it15_palit.Entity
{
    public class Order
    {
        [Key]
        public int Id { get; set; }
        public int Customer_Id { get; set; }
        public decimal Grand_Total { get; set; }
        public DateTime OrderDate { get; set; }
        public int Status_Id { get; set; }
        public DateTime Created_At { get; set; }
        public DateTime Updated_At { get; set; }

        //parents
        public Customer Customer { get; set; }
        public Status Status { get; set; }

        // child
        public ICollection<OrderDetail> OrderDetails { get; set; }
        public ICollection<Payment> Payments { get; set; }


    }
}
