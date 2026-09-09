using Microsoft.EntityFrameworkCore;
using SkyBook.Api.Models;

namespace SkyBook.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<WalletTransaction> WalletTransactions => Set<WalletTransaction>();

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

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.ToTable("bookings");
            entity.HasKey(b => b.Id);
            entity.Property(b => b.Id).HasColumnName("id");
            entity.Property(b => b.UserId).HasColumnName("user_id").IsRequired();
            entity.Property(b => b.BookingRef).HasColumnName("booking_ref").IsRequired().HasMaxLength(20);
            entity.Property(b => b.Airline).HasColumnName("airline").IsRequired().HasMaxLength(120);
            entity.Property(b => b.FlightCode).HasColumnName("flight_code").IsRequired().HasMaxLength(20);
            entity.Property(b => b.DepartCode).HasColumnName("depart_code").IsRequired().HasMaxLength(10);
            entity.Property(b => b.ArriveCode).HasColumnName("arrive_code").IsRequired().HasMaxLength(10);
            entity.Property(b => b.DepartTime).HasColumnName("depart_time").IsRequired().HasMaxLength(10);
            entity.Property(b => b.ArriveTime).HasColumnName("arrive_time").IsRequired().HasMaxLength(10);
            entity.Property(b => b.DepartDate).HasColumnName("depart_date").IsRequired();
            entity.Property(b => b.Duration).HasColumnName("duration").HasMaxLength(20);
            entity.Property(b => b.Stops).HasColumnName("stops").HasMaxLength(20);
            entity.Property(b => b.SeatNumber).HasColumnName("seat_number").HasMaxLength(10);
            entity.Property(b => b.CabinClass).HasColumnName("cabin_class").HasMaxLength(30);
            entity.Property(b => b.Passengers).HasColumnName("passengers");
            entity.Property(b => b.TotalPrice).HasColumnName("total_price").HasColumnType("numeric(10,2)");
            entity.Property(b => b.Status).HasColumnName("status").HasMaxLength(20);
            entity.Property(b => b.CreatedAt).HasColumnName("created_at");
            entity.HasIndex(b => b.UserId);
            entity.HasOne<User>().WithMany().HasForeignKey(b => b.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<WalletTransaction>(entity =>
        {
            entity.ToTable("wallet_transactions");
            entity.HasKey(w => w.Id);
            entity.Property(w => w.Id).HasColumnName("id");
            entity.Property(w => w.UserId).HasColumnName("user_id").IsRequired();
            entity.Property(w => w.Label).HasColumnName("label").IsRequired().HasMaxLength(120);
            entity.Property(w => w.Amount).HasColumnName("amount").HasColumnType("numeric(10,2)");
            entity.Property(w => w.CreatedAt).HasColumnName("created_at");
            entity.HasIndex(w => w.UserId);
            entity.HasOne<User>().WithMany().HasForeignKey(w => w.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        base.OnModelCreating(modelBuilder);
    }
}

