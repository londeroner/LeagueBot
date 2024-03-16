using LeagueBot.Infrastructure;
using LeagueBot.Infrastructure.Repositories;
using LeagueBot.Infrastructure.Repositories.Interfaces;
using LeagueBot.Services;
using LeagueBot.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.IO;
using System.Timers;

namespace LeagueBot
{
    public class Program
    {
        public static void Main()
        {
            var host = AppStartup();
            var dbContext = ActivatorUtilities.GetServiceOrCreateInstance<DbContext>(host.Services);
            var lifeTimeCycle = ActivatorUtilities.GetServiceOrCreateInstance<LifeTimeCycle>(host.Services);

            while (true)
            {
            }
        }

        static void BuildConfig(IConfigurationBuilder builder)
        {
            builder.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();
        }

        static IHost AppStartup()
        {
            var builder = new ConfigurationBuilder();
            BuildConfig(builder);

            Log.Logger = new LoggerConfiguration()
                            .ReadFrom.Configuration(builder.Build())
                            .Enrich.FromLogContext()
                            .WriteTo.Console()
                            .WriteTo.File("log.txt", rollingInterval: RollingInterval.Day)
                            .CreateLogger();

            Log.Logger.Information("Application Starting");

            

            var host = Host.CreateDefaultBuilder()
                        .ConfigureServices((context, services) => {
                            var serviceProvider = services.BuildServiceProvider();
                            var config = serviceProvider.GetRequiredService<IConfiguration>();

                            services.AddDbContext<ApplicationContext>(x =>
                            {
                                x.UseNpgsql(config.GetConnectionString("DefaultConnection"));
                            });

                            services.AddScoped<DbContext, ApplicationContext>();
                            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
                            services.AddScoped<IPollRepository, PollRepository>();
                            services.AddScoped<IChatRepository, ChatRepository>();
                            services.AddScoped<IUserRepository, UserRepository>();
                            services.AddScoped<ILifeTimeCycle, LifeTimeCycle>();
                            services.AddScoped<ILeagueBotClient, LeagueBotClient>();
                        })
                        .UseSerilog() 
                        .Build();

            return host;
        }
    }
}