using Microsoft.EntityFrameworkCore;
using ProductManagementAPI.Models;

namespace ProductManagementAPI.Data
{
    public class ProductDbContext : DbContext
    {
        public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>().HasData(
                new Product { Id = 1, Name = "Laptop", Price = 1000.00M, Description = "A powerful laptop" },
                new Product { Id = 2, Name = "Smartphone", Price = 500.00M, Description = "A modern smartphone" }
            );
        }

        public DbSet<Product> Products { get; set; }
    }
}
