using Microsoft.EntityFrameworkCore;
using cce106_palit.Entity;

namespace cce106_palit.Data
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

        public DbSet<StockIn> StockIns { get; set; }
        public DbSet<Notification> Notifications { get; set; }

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
                .HasForeignKey(o => o.Customer_Id)
                .OnDelete(DeleteBehavior.SetNull);

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

            modelBuilder.Entity<StockIn>()
                .HasOne(si => si.Product)
                .WithMany(p => p.StockIns)
                .HasForeignKey(si => si.ProductId);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade); 
            
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Customer)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "admin",
                    Password = "AQAAAAIAAYagAAAAELBZLJoNbTCB1DA7jr0aF56e9PBmGCg/5ORmvh6gnhTRnMGTGJm/dCoYt9iCWTFzbg==", // Admin123!
                    Name = "Hannah Tano",
                    Email = "hannahtano05@gmail.com",
                    IsVerified = true,
                    Status = "Active",
                    Role = "Super Admin",
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
