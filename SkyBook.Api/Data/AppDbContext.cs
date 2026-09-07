using Microsoft.EntityFrameworkCore;
using SkyBook.Api.Models;

namespace SkyBook.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Id).HasColumnName("id");
            entity.Property(u => u.FullName).HasColumnName("full_name").IsRequired().HasMaxLength(200);
            entity.Property(u => u.Email).HasColumnName("email").IsRequired().HasMaxLength(320);
            entity.Property(u => u.Phone).HasColumnName("phone").HasMaxLength(30);
            entity.Property(u => u.PasswordHash).HasColumnName("password_hash").IsRequired();
            entity.Property(u => u.CreatedAt).HasColumnName("created_at");
            entity.HasIndex(u => u.Email).IsUnique();
        });

        base.OnModelCreating(modelBuilder);
    }
}
