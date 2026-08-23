using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore;
using Persistence.Models;
using Contracts.Enums;

namespace Persistence.Data
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
        public DbSet<ProcessedWebhookEvent> ProcessedWebhookEvents { get; set; }
        public DbSet<RevokedToken> RevokedTokens { get; set; }
        public DbSet<HotelViewHistory> HotelViewHistories { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
        public DbSet<RoomMaintenanceLog> RoomMaintenanceLogs { get; set; }
        public DbSet<PriceAdjustment> PriceAdjustments { get; set; }
        public DbSet<InventoryItem> InventoryItems { get; set; }
        public DbSet<InventoryTransaction> InventoryTransactions { get; set; }
        public DbSet<LoyaltyPointsEarned> LoyaltyPointsEarned { get; set; }
        public DbSet<SupportTicket> SupportTickets { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<ServiceCategory> ServiceCategories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Country configuration (referentna/šifarnik tabela)
            modelBuilder.Entity<Country>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
                entity.HasIndex(c => c.Name).IsUnique();
            });

            // City configuration (referentna/šifarnik tabela)
            modelBuilder.Entity<City>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Name).IsRequired().HasMaxLength(100);

                entity.HasOne(c => c.Country)
                    .WithMany(co => co.Cities)
                    .HasForeignKey(c => c.CountryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ServiceCategory configuration (referentna/šifarnik tabela)
            modelBuilder.Entity<ServiceCategory>(entity =>
            {
                entity.HasKey(sc => sc.Id);
                entity.Property(sc => sc.Name).IsRequired().HasMaxLength(50);
                entity.HasIndex(sc => sc.Name).IsUnique();
            });

            // Hotel configuration
            modelBuilder.Entity<Hotel>(entity =>
            {
                entity.HasKey(h => h.Id);
                entity.Property(h => h.Name).IsRequired().HasMaxLength(100);
                entity.Property(h => h.Address).IsRequired().HasMaxLength(200);
                entity.Property(h => h.Email).IsRequired().HasMaxLength(100);
                entity.Property(h => h.PhoneNumber).HasMaxLength(20);
                entity.Property(h => h.StarRating).HasDefaultValue(0);
                entity.Property(h => h.ImageUrl).HasMaxLength(500);

                entity.HasOne(h => h.City)
                    .WithMany(c => c.Hotels)
                    .HasForeignKey(h => h.CityId)
                    .OnDelete(DeleteBehavior.Restrict);

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
                entity.Property(s => s.Price).HasColumnType("decimal(18,2)");

                entity.HasOne(s => s.Hotel)
                    .WithMany()
                    .HasForeignKey(s => s.HotelId)
                    .OnDelete(DeleteBehavior.Cascade);

                // FK umjesto slobodnog string polja za kategoriju — Restrict jer brisanje
                // kategorije koja se koristi ne smije tiho obrisati/osiročiti postojeće servise
                // (isti obrazac kao Hotel -> City).
                entity.HasOne(s => s.ServiceCategory)
                    .WithMany(sc => sc.Services)
                    .HasForeignKey(s => s.ServiceCategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
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
                entity.Property(p => p.TransactionId).HasMaxLength(200);
                entity.Property(p => p.CheckoutId).HasMaxLength(200);
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

            modelBuilder.Entity<ProcessedWebhookEvent>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Provider).IsRequired().HasMaxLength(32);
                entity.Property(e => e.EventId).IsRequired().HasMaxLength(256);
                entity.HasIndex(e => new { e.Provider, e.EventId }).IsUnique();
                entity.Property(e => e.ReceivedAt).IsRequired();

                // Opciono povezan sa Payment-om (popunjava se naknadno, best-effort — vidi model).
                entity.HasOne(e => e.Payment)
                    .WithMany()
                    .HasForeignKey(e => e.PaymentId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<RevokedToken>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Jti).IsRequired().HasMaxLength(64);
                entity.HasIndex(e => e.Jti).IsUnique();
                entity.Property(e => e.ExpiresAt).IsRequired();
                entity.Property(e => e.RevokedAt).IsRequired();

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<HotelViewHistory>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ViewedAt).IsRequired();

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Hotel)
                    .WithMany()
                    .HasForeignKey(e => e.HotelId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Brzo dohvatanje "koliko puta je hotel pregledan" za popularity signal.
                entity.HasIndex(e => e.HotelId);
                entity.HasIndex(e => new { e.UserId, e.HotelId });
            });

            modelBuilder.Entity<PasswordResetToken>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CodeHash).IsRequired();
                entity.Property(e => e.ExpiresAt).IsRequired();
                entity.HasIndex(e => e.UserId);

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
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

            // RoomMaintenanceLog configuration
            modelBuilder.Entity<RoomMaintenanceLog>(entity =>
            {
                entity.HasKey(rml => rml.Id);
                entity.Property(rml => rml.Description).HasMaxLength(1000);
                entity.Property(rml => rml.Cost).HasColumnType("decimal(18,2)");
                entity.Property(rml => rml.TechnicianName).HasMaxLength(100);

                entity.HasOne(rml => rml.Room)
                    .WithMany()
                    .HasForeignKey(rml => rml.RoomId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // PriceAdjustment configuration
            modelBuilder.Entity<PriceAdjustment>(entity =>
            {
                entity.HasKey(pa => pa.Id);
                entity.Property(pa => pa.Name).IsRequired().HasMaxLength(100);
                entity.Property(pa => pa.PercentageModifier).HasColumnType("decimal(5,2)");

                entity.HasOne(pa => pa.CreatedByUser)
                    .WithMany()
                    .HasForeignKey(pa => pa.CreatedByUserId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(pa => pa.Hotel)
                    .WithMany()
                    .HasForeignKey(pa => pa.HotelId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // InventoryItem configuration (referentna tabela artikala skladišta)
            modelBuilder.Entity<InventoryItem>(entity =>
            {
                entity.HasKey(ii => ii.Id);
                entity.Property(ii => ii.Name).IsRequired().HasMaxLength(150);
                entity.Property(ii => ii.Unit).IsRequired().HasMaxLength(20);
                entity.Property(ii => ii.Category).HasMaxLength(100);
            });

            // InventoryTransaction configuration
            modelBuilder.Entity<InventoryTransaction>(entity =>
            {
                entity.HasKey(it => it.Id);
                entity.Property(it => it.Reason).IsRequired().HasMaxLength(500);

                entity.HasOne(it => it.StaffUser)
                    .WithMany()
                    .HasForeignKey(it => it.StaffUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(it => it.InventoryItem)
                    .WithMany(ii => ii.Transactions)
                    .HasForeignKey(it => it.InventoryItemId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // LoyaltyPointsEarned configuration
            modelBuilder.Entity<LoyaltyPointsEarned>(entity =>
            {
                entity.HasKey(lpe => lpe.Id);
                entity.Property(lpe => lpe.PointsEarned).IsRequired();
                entity.Property(lpe => lpe.EarnedAt).IsRequired();
                entity.Property(lpe => lpe.Reason).IsRequired().HasMaxLength(200);

                entity.HasOne(lpe => lpe.User)
                    .WithMany()
                    .HasForeignKey(lpe => lpe.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(lpe => lpe.Booking)
                    .WithMany()
                    .HasForeignKey(lpe => lpe.BookingId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(lpe => lpe.Payment)
                    .WithMany()
                    .HasForeignKey(lpe => lpe.PaymentId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // SupportTicket configuration
            modelBuilder.Entity<SupportTicket>(entity =>
            {
                entity.HasKey(st => st.Id);
                entity.Property(st => st.Subject).IsRequired().HasMaxLength(200);
                entity.Property(st => st.MessageBody).IsRequired().HasMaxLength(5000);
                entity.Property(st => st.Status).IsRequired();
                entity.Property(st => st.Priority).IsRequired();
                entity.Property(st => st.AdminResponse).HasMaxLength(5000);

                entity.HasOne(st => st.User)
                    .WithMany()
                    .HasForeignKey(st => st.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(st => st.RespondedByUser)
                    .WithMany()
                    .HasForeignKey(st => st.RespondedByUserId)
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
            modelBuilder.Entity<RoomMaintenanceLog>().HasQueryFilter(rml => !rml.IsDeleted);
            modelBuilder.Entity<PriceAdjustment>().HasQueryFilter(pa => !pa.IsDeleted);
            modelBuilder.Entity<InventoryItem>().HasQueryFilter(ii => !ii.IsDeleted);
            modelBuilder.Entity<InventoryTransaction>().HasQueryFilter(it => !it.IsDeleted);
            modelBuilder.Entity<LoyaltyPointsEarned>().HasQueryFilter(lpe => !lpe.IsDeleted);
            modelBuilder.Entity<SupportTicket>().HasQueryFilter(st => !st.IsDeleted);
            modelBuilder.Entity<Country>().HasQueryFilter(c => !c.IsDeleted);
            modelBuilder.Entity<City>().HasQueryFilter(c => !c.IsDeleted);
            modelBuilder.Entity<ServiceCategory>().HasQueryFilter(sc => !sc.IsDeleted);
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
