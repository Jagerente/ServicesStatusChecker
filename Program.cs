using Serilog;
using ServicesStatusChecker.Extensions;
using ServicesStatusChecker.Modules;

namespace ServicesStatusChecker
{
    internal class Program
    {

        static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day, outputTemplate: "{Timestamp:HH:mm:ss} | [{Level:u3}] | {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            Log.Debug("Program has started.");
            
            var slackBot = new SlackBot(Constants.TokenPath);

            await slackBot.ScheduledCheck();

            Log.CloseAndFlush();

            Environment.Exit(0);
        }
    }
}