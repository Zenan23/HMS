using API.Models;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore;
using API.Enums;

namespace API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Hotel> Hotels { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<BookingService> BookingServices { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<BookingStatusHistory> BookingStatusHistories { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<PaymentAuditLog> PaymentAuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Hotel configuration
            modelBuilder.Entity<Hotel>(entity =>
            {
                entity.HasKey(h => h.Id);
                entity.Property(h => h.Name).IsRequired().HasMaxLength(100);
                entity.Property(h => h.Address).IsRequired().HasMaxLength(200);
                entity.Property(h => h.City).IsRequired().HasMaxLength(50);
                entity.Property(h => h.Country).IsRequired().HasMaxLength(50);
                entity.Property(h => h.Email).IsRequired().HasMaxLength(100);
                entity.Property(h => h.PhoneNumber).HasMaxLength(20);
                entity.Property(h => h.StarRating).HasDefaultValue(0);
                entity.Property(h => h.ImageUrl).HasMaxLength(500);

                entity.HasMany(h => h.Rooms)
                    .WithOne(r => r.Hotel)
                    .HasForeignKey(r => r.HotelId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Room configuration
            modelBuilder.Entity<Room>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.Property(r => r.RoomNumber).IsRequired().HasMaxLength(10);
                entity.Property(r => r.PricePerNight).HasColumnType("decimal(18,2)");
                entity.Property(r => r.MaxOccupancy).HasDefaultValue(1);
                entity.Property(r => r.IsAvailable).HasDefaultValue(true);

                entity.HasMany(r => r.Bookings)
                    .WithOne(b => b.Room)
                    .HasForeignKey(b => b.RoomId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Booking configuration
            modelBuilder.Entity<Booking>(entity =>
            {
                entity.HasKey(b => b.Id);
                entity.Property(b => b.TotalPrice).HasColumnType("decimal(18,2)");
                entity.Property(b => b.NumberOfGuests).HasDefaultValue(1);
                entity.Property(b => b.SpecialRequests).HasMaxLength(500);
            });

            // BookingService configuration
            modelBuilder.Entity<BookingService>(entity =>
            {
                entity.HasKey(bs => bs.Id);
                entity.Property(bs => bs.UnitPrice).HasColumnType("decimal(18,2)");
                entity.Property(bs => bs.Quantity).HasDefaultValue(1);

                entity.HasIndex(bs => new { bs.BookingId, bs.ServiceId }).IsUnique();

                entity.HasOne(bs => bs.Booking)
                    .WithMany(b => b.BookingServices)
                    .HasForeignKey(bs => bs.BookingId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(bs => bs.Service)
                    .WithMany()
                    .HasForeignKey(bs => bs.ServiceId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Username).IsRequired().HasMaxLength(50);
                entity.Property(u => u.Email).IsRequired().HasMaxLength(100);
                entity.Property(u => u.PasswordHash).IsRequired();
                entity.Property(u => u.FirstName).IsRequired().HasMaxLength(50);
                entity.Property(u => u.LastName).IsRequired().HasMaxLength(50);
                entity.Property(u => u.PhoneNumber).HasMaxLength(20);
                entity.Property(u => u.Role).IsRequired().HasDefaultValue(UserRole.Guest);
                entity.Property(u => u.IsActive).HasDefaultValue(true);

                // Create unique indexes
                entity.HasIndex(u => u.Email).IsUnique();
                entity.HasIndex(u => u.Username).IsUnique();

                entity.HasMany(u => u.Bookings)
                    .WithOne(b => b.User)
                    .HasForeignKey(b => b.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Service configuration
            modelBuilder.Entity<Service>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.Property(s => s.Name).IsRequired().HasMaxLength(100);
                entity.Property(s => s.Category).IsRequired().HasMaxLength(50);
                entity.Property(s => s.Price).HasColumnType("decimal(18,2)");

                entity.HasOne(s => s.Hotel)
                    .WithMany()
                    .HasForeignKey(s => s.HotelId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Review configuration
            modelBuilder.Entity<Review>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.Property(r => r.Title).IsRequired().HasMaxLength(100);
                entity.Property(r => r.Comment).IsRequired().HasMaxLength(1000);
                entity.Property(r => r.Rating).IsRequired();

                entity.HasOne(r => r.Hotel)
                    .WithMany()
                    .HasForeignKey(r => r.HotelId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(r => r.User)
                    .WithMany()
                    .HasForeignKey(r => r.UserId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(r => r.Booking)
                    .WithMany()
                    .HasForeignKey(r => r.BookingId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Notification configuration
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(n => n.Id);
                entity.Property(n => n.Title).IsRequired().HasMaxLength(200);
                entity.Property(n => n.Message).IsRequired();
                entity.Property(n => n.Type).IsRequired().HasMaxLength(50);
                entity.Property(n => n.Priority).HasMaxLength(20);

                entity.HasOne(n => n.User)
                    .WithMany()
                    .HasForeignKey(n => n.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(n => n.Booking)
                    .WithMany()
                    .HasForeignKey(n => n.BookingId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // BookingStatusHistory configuration
            modelBuilder.Entity<BookingStatusHistory>(entity =>
            {
                entity.HasKey(bsh => bsh.Id);
                entity.Property(bsh => bsh.Reason).HasMaxLength(500);
                entity.Property(bsh => bsh.Notes).HasMaxLength(1000);

                entity.HasOne(bsh => bsh.Booking)
                    .WithMany()
                    .HasForeignKey(bsh => bsh.BookingId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(bsh => bsh.ChangedByUser)
                    .WithMany()
                    .HasForeignKey(bsh => bsh.ChangedByUserId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Payment configuration
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Amount).HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(p => p.Currency).IsRequired().HasMaxLength(3).HasDefaultValue("USD");
                entity.Property(p => p.TransactionId).HasMaxLength(100);
                entity.Property(p => p.FailureReason).HasMaxLength(500);
                entity.Property(p => p.Description).HasMaxLength(500);
                entity.Property(p => p.RefundAmount).HasColumnType("decimal(18,2)");

                entity.HasOne(p => p.User)
                    .WithMany()
                    .HasForeignKey(p => p.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.Booking)
                    .WithMany()
                    .HasForeignKey(p => p.BookingId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(p => p.AuditLogs)
                    .WithOne(pal => pal.Payment)
                    .HasForeignKey(pal => pal.PaymentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // PaymentAuditLog configuration
            modelBuilder.Entity<PaymentAuditLog>(entity =>
            {
                entity.HasKey(pal => pal.Id);
                entity.Property(pal => pal.Action).IsRequired().HasMaxLength(100);
                entity.Property(pal => pal.Details).HasMaxLength(1000);
                entity.Property(pal => pal.ErrorMessage).HasMaxLength(500);
                entity.Property(pal => pal.UserAgent).HasMaxLength(500);
                entity.Property(pal => pal.IpAddress).HasMaxLength(45);

                entity.HasOne(pal => pal.InitiatedByUser)
                    .WithMany()
                    .HasForeignKey(pal => pal.InitiatedByUserId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Add global query filter for soft delete
            modelBuilder.Entity<Hotel>().HasQueryFilter(h => !h.IsDeleted);
            modelBuilder.Entity<Room>().HasQueryFilter(r => !r.IsDeleted);
            modelBuilder.Entity<Booking>().HasQueryFilter(b => !b.IsDeleted);
            modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);
            modelBuilder.Entity<Service>().HasQueryFilter(s => !s.IsDeleted);
            modelBuilder.Entity<Review>().HasQueryFilter(r => !r.IsDeleted);
            modelBuilder.Entity<Notification>().HasQueryFilter(n => !n.IsDeleted);
            modelBuilder.Entity<BookingStatusHistory>().HasQueryFilter(bsh => !bsh.IsDeleted);
            modelBuilder.Entity<Payment>().HasQueryFilter(p => !p.IsDeleted);
            modelBuilder.Entity<PaymentAuditLog>().HasQueryFilter(pal => !pal.IsDeleted);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }

}
