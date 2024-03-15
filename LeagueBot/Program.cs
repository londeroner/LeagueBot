using LeagueBot.Services;
using LeagueBot.Services.Interfaces;
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
        private static readonly Timer _timer;

        static Program()
        {
            _timer = new Timer();
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
                            //services.AddTransient<IDataService, DataService>();
                            services.AddSingleton<ILeagueBot, LeagueBotClient>();
                        })
                        .UseSerilog() 
                        .Build();

            return host;
        }

        public static void Main()
        {
            var host = AppStartup();
            //var service = ActivatorUtilities.GetServiceOrCreateInstance<DataService>(host.Services);
            var bot = ActivatorUtilities.GetServiceOrCreateInstance<LeagueBotClient>(host.Services);
            
            while (true)
            {
            }
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            
        }
    }
}