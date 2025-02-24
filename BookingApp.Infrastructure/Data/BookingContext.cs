using BookingApp.Core.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BookingApp.Infrastructure.Data
{
    public class BookingContext : DbContext
    {
        private readonly IConfiguration _configuration;

        public BookingContext(DbContextOptions<BookingContext> options, IConfiguration configuration)
            : base(options)
        {
            this._configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public DbSet<Member> Members { get; set; }
        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<Booking> Bookings { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(this._configuration.GetConnectionString("DefaultConnection"), b => b.MigrationsAssembly("BookingApp.Infrastructure"));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Define relationship between Booking and Member
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Member)
                .WithMany()
                .HasForeignKey(b => b.MemberId);

            // Define relationship between Booking and Inventory
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Inventory)
                .WithMany()
                .HasForeignKey(b => b.InventoryId);

            base.OnModelCreating(modelBuilder);
        }
    }
}
