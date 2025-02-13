using Microsoft.EntityFrameworkCore;
using it15_palit.Entity;

namespace it15_palit.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
           : base(options)
        {
        }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Status> Statuses { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Order>()
                .Property(p => p.Grand_Total)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Category>()
                .HasMany(c => c.Products)
                .WithOne(p => p.Category)
                .HasForeignKey(p => p.Category_id);

            modelBuilder.Entity<Cart>()
                .HasKey(c => new { c.Customer_Id, c.Product_Id });

            modelBuilder.Entity<Cart>()
                .HasOne(c => c.Customer)
                .WithMany(cust => cust.Carts)
                .HasForeignKey(c => c.Customer_Id);

            modelBuilder.Entity<Cart>()
                .HasOne(c => c.Product)
                .WithMany(prod => prod.Carts)
                .HasForeignKey(c => c.Product_Id);

            // Order - Customer relationship (1 Customer to many Orders)
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.Customer_Id);

            // Order - Status relationship (1 Status to many Orders)
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Status)
                .WithMany(s => s.Orders)
                .HasForeignKey(o => o.Status_Id);

            // Order - OrderDetails relationship (1 Order to many OrderDetails)
            modelBuilder.Entity<Order>()
                .HasMany(o => o.OrderDetails)
                .WithOne(od => od.Order)
                .HasForeignKey(od => od.Order_Id);

            // OrderDetail - Product relationship (1 Product to many OrderDetails)
            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Product)
                .WithMany(p => p.OrderDetails)
                .HasForeignKey(od => od.Product_Id);

            // Order - Payment relationship (1 Order to many Payments)
            modelBuilder.Entity<Order>()
                .HasMany(o => o.Payments)
                .WithOne(p => p.Order)
                .HasForeignKey(p => p.Order_Id);

            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "hadmin",
                    Password = "password",
                    Name = "Hannah Tano",
                    Email = "hadmin@email.com",
                    Status = "Active",
                    Created_At = DateTime.UtcNow,
                    Updated_At = DateTime.UtcNow
                },
                new User
                {
                    Id = 2,
                    Username = "odmin",
                    Password = "password",
                    Name = "Oscar Queman",
                    Email = "odmin@email.com",
                    Status = "Active",
                    Created_At = DateTime.UtcNow,
                    Updated_At = DateTime.UtcNow
                }
            );

            modelBuilder.Entity<Status>().HasData(
                new Status
                {
                    Id = 1,
                    Name = "Pending",
                    Created_at = DateTime.UtcNow,
                    Updated_at = DateTime.UtcNow
                },
                new Status
                {
                    Id = 2,
                    Name = "To Ship",
                    Created_at = DateTime.UtcNow,
                    Updated_at = DateTime.UtcNow
                },
                new Status
                {
                    Id = 3,
                    Name = "To Receive",
                    Created_at = DateTime.UtcNow,
                    Updated_at = DateTime.UtcNow
                },
                new Status
                {
                    Id = 4,
                    Name = "Completed",
                    Created_at = DateTime.UtcNow,
                    Updated_at = DateTime.UtcNow
                },
                new Status
                {
                    Id = 5,
                    Name = "Cancelled",
                    Created_at = DateTime.UtcNow,
                    Updated_at = DateTime.UtcNow
                }
            );
        }
    }
}
