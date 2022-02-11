using Serilog;
using ServicesStatusChecker.Extensions;
using ServicesStatusChecker.Modules;

namespace ServicesStatusChecker
{
    internal class Program
    {

        static async Task Main(string[] args)
        {
            ParseArguments(args);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day, outputTemplate: "{Timestamp:HH:mm:ss} | [{Level:u3}] | {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            Log.Debug("Program has started.");
            
            var slackBot = new SlackBot(Constants.ConfigPath);

            await slackBot.ClientStatusCheck();

            //await slackBot.WebClientStatusCheck();

            Log.CloseAndFlush();

            Environment.Exit(0);
        }

        private static void ParseArguments(string[] args)
        {
            if (args.Length == 0) return;

            if (args.Any(arg => arg.Contains("critical")))
            {
                Constants.Critical = true;
                Console.WriteLine("Set critical level.");
            }
        }
    }
}