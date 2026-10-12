
using BloodApp.DAL.Entity.BloodDonationAPI.DAL.Models;
using BloodDonationAPI.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace BloodDonationAPI.DAL.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // ==============================
        // TABLES
        // ==============================

        public DbSet<User> Users { get; set; }

        public DbSet<RefreshToken> RefreshTokens { get; set; }

        public DbSet<BloodRequest> BloodRequests { get; set; }

        public DbSet<EmergencyRequest> EmergencyRequests { get; set; }

        public DbSet<EmergencyResponse> EmergencyResponses { get; set; }

        public DbSet<Notification> Notifications { get; set; }

        public DbSet<CommunityNotification> CommunityNotifications { get; set; }

        public DbSet<BloodInventory> BloodInventory { get; set; }

        public DbSet<BloodBatch> BloodBatches { get; set; }

        public DbSet<BloodBankSetting> BloodBankSettings { get; set; }

        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

        public DbSet<BloodRequestResponse> BloodRequestResponses { get; set; }

        // NEW: Email Verification
        public DbSet<EmailVerificationToken> EmailVerificationTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ==========================================
            // USER
            // ==========================================

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.HasIndex(x => x.Email)
                    .IsUnique();

                entity.Property(x => x.FullName)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(x => x.Email)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(x => x.PasswordHash)
                    .IsRequired();

                entity.Property(x => x.Role)
                    .HasDefaultValue("user");

                entity.Property(x => x.IsActive)
                    .HasDefaultValue(true);

                entity.Property(x => x.IsEmailVerified)
                    .HasDefaultValue(false);

                entity.Property(x => x.EmailVerifiedAt)
                    .IsRequired(false);

                entity.Property(x => x.AvailableToDonate)
                    .HasDefaultValue(true);

                entity.Property(x => x.DonationsCount)
                    .HasDefaultValue(0);

                entity.Property(x => x.LivesSaved)
                    .HasDefaultValue(0);

                entity.Property(x => x.DonatedMl)
                    .HasDefaultValue(0);
            });

            // ==========================================
            // REFRESH TOKEN
            // ==========================================

            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Token)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.HasIndex(x => x.Token)
                    .IsUnique();

                entity.HasIndex(x => x.UserId);

                entity.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ==========================================
            // BLOOD REQUEST
            // ==========================================

            modelBuilder.Entity<BloodRequest>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Status)
                    .HasDefaultValue("Pending");

                entity.Property(x => x.RequestType)
                    .HasDefaultValue("BloodRequest");

                entity.HasOne(x => x.Reporter)
                    .WithMany(x => x.BloodRequests)
                    .HasForeignKey(x => x.ReporterUid)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Emergency)
                    .WithMany(x => x.BloodRequests)
                    .HasForeignKey(x => x.EmergencyId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // ==========================================
            // BLOOD REQUEST RESPONSE
            // ==========================================

            modelBuilder.Entity<BloodRequestResponse>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Status)
                    .HasDefaultValue("Pending");

                entity.Property(x => x.DonorName)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(x => x.DonorPhone)
                    .HasMaxLength(30);

                entity.Property(x => x.BloodType)
                    .HasMaxLength(10);

                entity.HasOne(x => x.BloodRequest)
                    .WithMany(x => x.DonorResponses)
                    .HasForeignKey(x => x.BloodRequestId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.Donor)
                    .WithMany()
                    .HasForeignKey(x => x.DonorId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => new
                {
                    x.BloodRequestId,
                    x.DonorId
                })
                .IsUnique();

                entity.HasIndex(x => x.BloodRequestId);

                entity.HasIndex(x => x.DonorId);
            });

            // ==========================================
            // EMERGENCY REQUEST
            // ==========================================

            modelBuilder.Entity<EmergencyRequest>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Status)
                    .HasDefaultValue("Active");

                entity.Property(x => x.Priority)
                    .HasDefaultValue("High");

                entity.Property(x => x.Unread)
                    .HasDefaultValue(true);

                entity.Property(x => x.ReservedUnits)
                    .HasDefaultValue(0);

                entity.Property(x => x.RespondingDonorsCount)
                    .HasDefaultValue(0);

                entity.HasOne(x => x.Reporter)
                    .WithMany(x => x.EmergencyRequests)
                    .HasForeignKey(x => x.ReporterUid)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ==========================================
            // EMERGENCY RESPONSE
            // ==========================================

            modelBuilder.Entity<EmergencyResponse>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Status)
                    .HasDefaultValue("Accepted");

                entity.HasOne(x => x.EmergencyRequest)
                    .WithMany(x => x.DonorResponses)
                    .HasForeignKey(x => x.EmergencyRequestId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.Donor)
                    .WithMany(x => x.EmergencyResponses)
                    .HasForeignKey(x => x.DonorId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => new
                {
                    x.EmergencyRequestId,
                    x.DonorId
                })
                .IsUnique();
            });

            // ==========================================
            // PRIVATE NOTIFICATIONS
            // ==========================================

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Unread)
                    .HasDefaultValue(true);

                entity.HasOne(x => x.User)
                    .WithMany(x => x.Notifications)
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(x => x.UserId);

                entity.HasIndex(x => new
                {
                    x.UserId,
                    x.Unread
                });
            });

            // ==========================================
            // COMMUNITY NOTIFICATIONS
            // ==========================================

            modelBuilder.Entity<CommunityNotification>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Unread)
                    .HasDefaultValue(true);

                entity.HasIndex(x => x.CreatedAt);

                entity.HasIndex(x => x.Type);

                entity.HasIndex(x => x.EmergencyRequestId);

                entity.HasIndex(x => x.BloodRequestId);
            });

            // ==========================================
            // BLOOD INVENTORY
            // ==========================================

            modelBuilder.Entity<BloodInventory>(entity =>
            {
                entity.HasKey(x => x.BloodType);

                entity.Property(x => x.BloodType)
                    .HasMaxLength(10);

                entity.Property(x => x.Units)
                    .HasDefaultValue(0);
            });

            // ==========================================
            // BLOOD BATCH
            // ==========================================

            modelBuilder.Entity<BloodBatch>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.BloodType)
                    .IsRequired()
                    .HasMaxLength(10);

                entity.Property(x => x.Units)
                    .IsRequired();

                entity.HasIndex(x => x.BloodType);

                entity.HasIndex(x => x.ExpiresAt);

                entity.HasIndex(x => new
                {
                    x.BloodType,
                    x.ExpiresAt
                });
            });

            // ==========================================
            // BLOOD BANK SETTING
            // ==========================================

            modelBuilder.Entity<BloodBankSetting>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Name)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(x => x.PhoneNumber)
                    .IsRequired()
                    .HasMaxLength(30);
            });

            // ==========================================
            // PASSWORD RESET TOKEN
            // ==========================================

            modelBuilder.Entity<PasswordResetToken>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Token)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.HasIndex(x => x.Token);

                entity.HasIndex(x => x.UserId);

                entity.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ==========================================
            // EMAIL VERIFICATION TOKEN - NEW
            // ==========================================

            modelBuilder.Entity<EmailVerificationToken>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.TokenHash)
                    .IsRequired()
                    .HasMaxLength(64);

                entity.HasIndex(x => x.TokenHash)
                    .IsUnique();

                entity.HasIndex(x => x.UserId);

                entity.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
