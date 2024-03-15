using LeagueBot.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueBot.Infrastructure
{
    public class ApplicationContext : DbContext
    {
        private readonly IConfiguration _config;

        public ApplicationContext(IConfiguration config)
        {
            _config = config;
        }

        public DbSet<DailyPoll> DailyPolls { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(_config.GetConnectionString("DefaultConnection"));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DailyPoll>(builder =>
            {
                builder.Property(x => x.TimeToStartGame)
                    .HasConversion<TimeOnlyConverter, TimeOnlyComparer>();
                builder.Property(x => x.TimeToStartVote)
                    .HasConversion<TimeOnlyConverter, TimeOnlyComparer>();
            });
        }
    }
}
