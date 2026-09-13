using Microsoft.EntityFrameworkCore;
using SkyBook.Api.Models;

namespace SkyBook.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<WalletTransaction> WalletTransactions => Set<WalletTransaction>();
    public DbSet<PaymentMethod> PaymentMethods => Set<PaymentMethod>();
    public DbSet<Payment> Payments => Set<Payment>();

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
            entity.Property(u => u.Provider).HasColumnName("provider").IsRequired().HasMaxLength(20).HasDefaultValue("password");
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
            entity.Property(b => b.PaymentMethod).HasColumnName("payment_method").HasMaxLength(20);
            entity.Property(b => b.PaymentMethodLabel).HasColumnName("payment_method_label").HasMaxLength(120);
            entity.Property(b => b.PaymentReference).HasColumnName("payment_reference").HasMaxLength(40);
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
            entity.Property(w => w.Method).HasColumnName("method").HasMaxLength(20);
            entity.Property(w => w.Reference).HasColumnName("reference").HasMaxLength(40);
            entity.Property(w => w.CreatedAt).HasColumnName("created_at");
            entity.HasIndex(w => w.UserId);
            entity.HasOne<User>().WithMany().HasForeignKey(w => w.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PaymentMethod>(entity =>
        {
            entity.ToTable("payment_methods");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Id).HasColumnName("id");
            entity.Property(p => p.UserId).HasColumnName("user_id").IsRequired();
            entity.Property(p => p.Type).HasColumnName("type").IsRequired().HasMaxLength(20);
            entity.Property(p => p.Label).HasColumnName("label").IsRequired().HasMaxLength(120);
            entity.Property(p => p.Brand).HasColumnName("brand").HasMaxLength(20);
            entity.Property(p => p.Last4).HasColumnName("last4").HasMaxLength(4);
            entity.Property(p => p.ExpiryMonth).HasColumnName("expiry_month").HasMaxLength(2);
            entity.Property(p => p.ExpiryYear).HasColumnName("expiry_year").HasMaxLength(4);
            entity.Property(p => p.BankName).HasColumnName("bank_name").HasMaxLength(120);
            entity.Property(p => p.AccountLast4).HasColumnName("account_last4").HasMaxLength(4);
            entity.Property(p => p.OtherProvider).HasColumnName("other_provider").HasMaxLength(60);
            entity.Property(p => p.IsDefault).HasColumnName("is_default");
            entity.Property(p => p.CreatedAt).HasColumnName("created_at");
            entity.HasIndex(p => p.UserId);
            entity.HasOne<User>().WithMany().HasForeignKey(p => p.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.ToTable("payments");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Id).HasColumnName("id");
            entity.Property(p => p.UserId).HasColumnName("user_id").IsRequired();
            entity.Property(p => p.Reference).HasColumnName("reference").IsRequired().HasMaxLength(40);
            entity.Property(p => p.Method).HasColumnName("method").IsRequired().HasMaxLength(20);
            entity.Property(p => p.MethodLabel).HasColumnName("method_label").IsRequired().HasMaxLength(120);
            entity.Property(p => p.Amount).HasColumnName("amount").HasColumnType("numeric(10,2)");
            entity.Property(p => p.Status).HasColumnName("status").IsRequired().HasMaxLength(20);
            entity.Property(p => p.Purpose).HasColumnName("purpose").IsRequired().HasMaxLength(30);
            entity.Property(p => p.CreatedAt).HasColumnName("created_at");
            entity.HasIndex(p => p.UserId);
            entity.HasOne<User>().WithMany().HasForeignKey(p => p.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        base.OnModelCreating(modelBuilder);
    }
}

