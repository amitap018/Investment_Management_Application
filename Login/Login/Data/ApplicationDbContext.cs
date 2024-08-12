using Microsoft.EntityFrameworkCore;
using Login.Models;

namespace Login.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<UserDetails> UserDetails { get; set; }
        public DbSet<Authentication> Authentications { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<Grievance> Grievances { get; set; }
        public DbSet<Otp> Otps { get; set; }
        public DbSet<MutualFund> MutualFunds { get; set; }
        public DbSet<SIP> SIPs { get; set; }
        public DbSet<UserFund> UserFunds { get; set; }
        public DbSet<Order> Orders { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Define Feedback relationships
            modelBuilder.Entity<Feedback>()
                .HasOne(f => f.UserDetails)
                .WithMany(u => u.Feedbacks)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Define Grievance relationships
            modelBuilder.Entity<Grievance>()
                .HasOne(g => g.UserDetails)
                .WithMany(u => u.Grievances)
                .HasForeignKey(g => g.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure OTP model
            modelBuilder.Entity<Otp>()
                .HasIndex(o => o.Email)
                .IsUnique(false);

            // Define MutualFund relationships
            modelBuilder.Entity<MutualFund>()
                .HasKey(m => m.FundId);

            // Define SIP relationships
            modelBuilder.Entity<SIP>()
                .HasKey(s => s.SipId);

            modelBuilder.Entity<SIP>()
                .HasOne(s => s.MutualFund)
                .WithMany(m => m.SIPs)
                .HasForeignKey(s => s.FundId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SIP>()
                .HasOne(s => s.UserDetails)
                .WithMany(u => u.SIPs)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SIP>()
                .HasOne(s => s.UserFund)
                .WithMany()
                .HasForeignKey(s => s.FolioNumber)
                .OnDelete(DeleteBehavior.Restrict);

            // Define UserFund relationships
            modelBuilder.Entity<UserFund>()
                .HasKey(uf => uf.FolioNumber);

            modelBuilder.Entity<UserFund>()
                .HasOne(uf => uf.MutualFund)
                .WithMany(m => m.UserFunds)
                .HasForeignKey(uf => uf.FundId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserFund>()
                .HasOne(uf => uf.UserDetails)
                .WithMany(u => u.UserFunds)
                .HasForeignKey(uf => uf.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Define Order relationships
            modelBuilder.Entity<Order>()
                .HasKey(o => o.OrderId);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.MutualFund)
                .WithMany(m => m.Orders)
                .HasForeignKey(o => o.FundId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.UserDetails)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.UserFund)
                .WithMany(uf => uf.Orders)
                .HasForeignKey(o => o.FolioNumber)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
