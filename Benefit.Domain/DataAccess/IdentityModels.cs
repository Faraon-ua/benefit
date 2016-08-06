using System.Data.Entity;
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
        }

//        public DbSet<Seller> Sellers { get; set; }
        //        public DbSet<Category> Categories { get; set; }
        //        public DbSet<Localization> Localizations { get; set; }
        //        public DbSet<InfoPage> InfoPages { get; set; }
        //        public DbSet<Filter> Filters { get; set; }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IdentityUserLogin>().HasKey<string>(l => l.UserId);
            modelBuilder.Entity<IdentityRole>().HasKey<string>(r => r.Id);
            modelBuilder.Entity<IdentityUserRole>().HasKey(r => new { r.RoleId, r.UserId });
            /*modelBuilder.Entity<ApplicationUser>()
               .HasOptional(u => u.Seller)
               .WithRequired(s => s.User);*/
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