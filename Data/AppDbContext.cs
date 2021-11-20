using dm.KAE.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace dm.KAE.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Holder> Holders { get; set; }
        public DbSet<LastMessage> LastMessages { get; set; }
        public DbSet<Price> Prices { get; set; }
        public DbSet<Request> Requests { get; set; }
        public DbSet<Stat> Stats { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Holder>()
                .HasIndex(x => x.Value);
            modelBuilder.Entity<LastMessage>()
                .HasIndex(x => x.ChatId);
            modelBuilder.Entity<Price>()
                .HasIndex(x => x.Group);
            modelBuilder.Entity<Price>()
                .HasIndex(x => x.Date);
            modelBuilder.Entity<Request>()
                .HasIndex(x => x.Date);
            modelBuilder.Entity<Request>()
                .HasIndex(x => new { x.Response, x.Type });
            modelBuilder.Entity<Stat>()
                .HasIndex(x => x.Date);
        }
    }

    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<AppDbContext>();
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("Config.Data.json", optional: true, reloadOnChange: true)
                .AddJsonFile("Config.Data.Local.json", optional: true, reloadOnChange: true)
                .Build();

            builder.UseSqlServer(configuration.GetConnectionString("Database"));
            return new AppDbContext(builder.Options);
        }
    }
}