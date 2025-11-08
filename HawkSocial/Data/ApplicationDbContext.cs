using HawkSocial.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HawkSocial.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // tablas extendidas
        public DbSet<HawkSocial.Models.Post> Posts { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // POST //
            builder.Entity<Post>(e =>
            {
                e.HasIndex(p=> p.CreatedAtUtc); // indice para consultas por fecha de creacion
                e.Property(p => p.Content)
                    .HasMaxLength(140)
                    .IsRequired();

                // FKs //
                e.HasOne(p => p.User)
                    .WithMany()
                    .HasForeignKey(p => p.UserId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade);
            });

        }
    }
}
