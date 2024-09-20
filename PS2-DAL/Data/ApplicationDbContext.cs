using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PS2_DAL.Models;

namespace PS2_DAL.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<ShoppingCart> ShoppingCarts { get; set; }
        public DbSet<OrderHeader> OrderHeaders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Action", DisplayOrder = 1 },
                new Category { Id = 2, Name = "Scifi", DisplayOrder = 2 },
                new Category { Id = 3, Name = "History", DisplayOrder = 3 }
                );

            modelBuilder.Entity<Product>().HasData(
                  new Product { Id = 1, Title = "Product 1", Author = "Author 1", Description = "Description for Product 1", ISBN = "ISBN001", ListPrice = 99, Price = 90, Price50 = 85, Price100 = 80, CategoryId = 1, ImageUrl = "" },
                  new Product { Id = 2, Title = "Product 2", Author = "Author 2", Description = "Description for Product 2", ISBN = "ISBN002", ListPrice = 89, Price = 80, Price50 = 75, Price100 = 70, CategoryId = 2, ImageUrl = "" },
                  new Product { Id = 3, Title = "Product 3", Author = "Author 3", Description = "Description for Product 3", ISBN = "ISBN003", ListPrice = 89, Price = 70, Price50 = 65, Price100 = 60, CategoryId = 3, ImageUrl = "" }
               );

            modelBuilder.Entity<Company>().HasData(
                 new Company { Id = 1, Name = "Company 1", StreetAddress = "Street Address 1", City = "City 1", State = "State", PostalCode = "PostalCode 1", PhoneNumber = "Phone Number 1" },
                 new Company { Id = 2, Name = "Company 2", StreetAddress = "Street Address 2", City = "City 2", State = "State", PostalCode = "PostalCode 2", PhoneNumber = "Phone Number 2" },
                 new Company { Id = 3, Name = "Company 3", StreetAddress = "Street Address 3", City = "City 3", State = "State", PostalCode = "PostalCode 2", PhoneNumber = "Phone Number 3" }
              );
        }
    }
}
