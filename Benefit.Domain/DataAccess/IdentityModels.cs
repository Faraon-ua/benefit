using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.Validation;
using System.Linq;
using Benefit.Domain.Models;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Benefit.Domain.DataAccess
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection")
        {
            var adapter = (IObjectContextAdapter)this;
            var objectContext = adapter.ObjectContext;
            objectContext.CommandTimeout = 1200; // value in seconds 20 min
        }

        public DbSet<Seller> Sellers { get; set; }
        public DbSet<Localization> Localizations { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<SellerCategory> SellerCategories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Currency> Currencies { get; set; }
        public DbSet<Region> Regions { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<ShippingMethod> ShippingMethods { get; set; }
        public DbSet<ProductOption> ProductOptions { get; set; }
        public DbSet<ProductParameter> ProductParameters { get; set; }
        public DbSet<ProductParameterValue> ProductParameterValues { get; set; }
        public DbSet<ProductParameterProduct> ProductParameterProducts { get; set; }
        public DbSet<InfoPage> InfoPages { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderProduct> OrderProducts { get; set; }
        public DbSet<OrderProductOption> OrderProductOptions { get; set; }
        public DbSet<Personnel> Personnels { get; set; }
        public DbSet<BenefitCard> BenefitCards { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Banner> Banners { get; set; }
        public DbSet<OrderStatusStamp> OrderStatusStamps { get; set; }
        public DbSet<SellerBusinessLevelIndex> SellerBusinessLevelIndexes { get; set; }
        public DbSet<CompanyRevenue> CompanyRevenues { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Promotion> Promotions { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ApplicationUser>().Property(e => e.Email).HasColumnAnnotation(
                IndexAnnotation.AnnotationName,
                new IndexAnnotation(new IndexAttribute() { IsUnique = true }));
            modelBuilder.Entity<ApplicationUser>().Property(e => e.CardNumber).HasColumnAnnotation(
              IndexAnnotation.AnnotationName,
              new IndexAnnotation(new IndexAttribute() { IsUnique = false }));
            modelBuilder.Entity<ApplicationUser>().Property(e => e.PhoneNumber).HasColumnAnnotation(
                IndexAnnotation.AnnotationName,
                new IndexAnnotation(new IndexAttribute() { IsUnique = true }));
            modelBuilder.Entity<ApplicationUser>().Property(e => e.CardNumber).HasMaxLength(10);
            modelBuilder.Entity<ApplicationUser>().Property(e => e.PhoneNumber).HasMaxLength(16);
            modelBuilder.Entity<ApplicationUser>().Property(e => e.Email).HasMaxLength(64);
            modelBuilder.Entity<IdentityUserLogin>().HasKey<string>(l => l.UserId);
            modelBuilder.Entity<IdentityRole>().HasKey<string>(r => r.Id);
            modelBuilder.Entity<IdentityUserRole>().HasKey(r => new { r.RoleId, r.UserId });

            modelBuilder.Entity<Seller>()
                  .HasRequired<ApplicationUser>(s => s.Owner)
                  .WithMany(s => s.OwnedSellers).WillCascadeOnDelete(false);

            modelBuilder.Entity<Seller>()
                  .HasOptional<ApplicationUser>(s => s.WebSiteReferal)
                  .WithMany(s => s.ReferedWebSiteSellers).WillCascadeOnDelete(false);

            modelBuilder.Entity<Seller>()
                  .HasOptional<ApplicationUser>(s => s.BenefitCardReferal)
                  .WithMany(s => s.ReferedBenefitCardSellers).WillCascadeOnDelete(false);

            modelBuilder.Entity<ApplicationUser>()
                  .HasOptional<ApplicationUser>(s => s.Referal)
                  .WithMany(s => s.Partners).WillCascadeOnDelete(false);
            
            modelBuilder.Entity<Transaction>()
                  .HasOptional<ApplicationUser>(s => s.Payee)
                  .WithMany(s => s.Transactions).WillCascadeOnDelete(false);
        }

        public override int SaveChanges()
        {
            try
            {
                return base.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                // Retrieve the error messages as a list of strings.
                var errorMessages = ex.EntityValidationErrors
                        .SelectMany(x => x.ValidationErrors)
                        .Select(x => x.ErrorMessage);

                // Join the list to a single string.
                var fullErrorMessage = string.Join("; ", errorMessages);

                // Combine the original exception message with the new one.
                var exceptionMessage = string.Concat(ex.Message, " The validation errors are: ", fullErrorMessage);

                // Throw a new DbEntityValidationException with the improved exception message.
                throw new DbEntityValidationException(exceptionMessage, ex.EntityValidationErrors);
            }
        }
    }
}