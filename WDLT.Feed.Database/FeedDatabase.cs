using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WDLT.Feed.Database.Entities;

namespace WDLT.Feed.Database
{
    public class FeedDatabase : DbContext
    {
        public DbSet<DBCard> Cards { get; set; }
        public DbSet<DBSubscription> Subscriptions { get; set; }
        public DbSet<DBBlacklist> Blacklist { get; set; }

        public FeedDatabase()
        {
            Database.Migrate();
        }

        public async Task SubscribeAndSaveAsync(DBSubscription subscription)
        {
            await Subscriptions.AddAsync(subscription);
            await SaveChangesAsync();
        }

        public Task<bool> HasSubscription(DBSubscription subscription)
        {
            return Subscriptions.AnyAsync(a => a.SourceId == subscription.SourceId && a.Source == subscription.Source);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Filename=database.db", options =>
            {
                options.MigrationsAssembly(Assembly.GetExecutingAssembly().FullName);
            }).UseLazyLoadingProxies();

            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DBBlacklist>(e =>
            {
                e.HasKey(k => k.Id);
                e.Property(p => p.Word).IsRequired().HasMaxLength(35);
                e.HasIndex(i => new { i.SubscriptionId, i.Word }).IsUnique();
            });

            modelBuilder.Entity<DBSubscription>(e =>
            {
                e.HasKey(k => k.Id);

                e.Property(p => p.LastTimestamp).HasDefaultValue(DateTimeOffset.MinValue);

                e.HasIndex(i => i.Source);
                e.HasIndex(i => new { i.SourceId, i.Source }).IsUnique();
            });

            modelBuilder.Entity<DBCard>(e =>
            {
                e.HasKey(k => k.Id);
                e.HasIndex(i => i.Timestamp);
                e.HasIndex(i => i.IsViewed);
                e.HasIndex(i => i.IsBookmark);
                e.HasIndex(i => new {i.CardId, i.SubscriptionId}).IsUnique();
            });
        }
    }
}
