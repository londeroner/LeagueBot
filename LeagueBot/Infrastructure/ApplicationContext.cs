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

        public ApplicationContext(IConfiguration config, DbContextOptions<ApplicationContext> options) : base(options)
        {
            _config = config;
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        }

        public DbSet<DailyPoll> DailyPolls { get; set; }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<User> Users { get; set; }

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
