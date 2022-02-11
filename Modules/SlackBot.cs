using System.Drawing;
using Newtonsoft.Json;
using Serilog;
using ServicesStatusChecker.Extensions;
using ServicesStatusChecker.Models;
using Slack.Webhooks;

namespace ServicesStatusChecker.Modules
{
    public class SlackBot
    {
        private static SlackConfig _config = new();

        private static SlackClient WebHookClient;

        private static List<Site> Sites;

        public SlackBot(string tokenPath)
        {
            InitConfig(tokenPath);
            ConfigureSites(Constants.SitesPath);
            Log.Debug("Slack WebHook is ready.");
        }

        private static void InitConfig(string path)
        {
            _config = JsonConvert.DeserializeObject<SlackConfig>(File.ReadAllText(path));

            WebHookClient = new SlackClient($"{_config.WebHookUrl}");
            Log.Debug("WebHook client configured.");
        }

        private static void ConfigureSites(string path)
        {
            Sites = new List<Site>();

            var file = File.ReadAllLines(path);

            Parallel.ForEach(file, url =>
            {
                Sites.Add(new Site(url));
            });
        }

        public async Task ScheduledCheck()
        {
            var attachments = new List<SlackAttachment>();

            Log.Debug("Configuring message.");

            foreach (var site in Sites)
            {
                var status = site.Status;

                if ((site.Alive == null || site.Alive.Value) && !Constants.Critical) continue;

                attachments.Add(new SlackAttachment()
                {
                    Fallback = $"{site.Url} is {status}",
                    Title = $"<{site.Url}>",
                    Text = status + (site.Alive != null && !site.Alive.Value ? $" <!subteam^{_config.TeamId}>" : string.Empty),
                    Color = site.Alive == null ? Color.Silver.ToHEX() : site.Alive.Value ? Color.Green.ToHEX() : Color.Red.ToHEX(),
                    Actions = new List<SlackAction>()
                    {
                        new SlackAction()
                        {
                            Url = site.Url,
                            Text = "Open Web",
                            Type = SlackActionType.Button
                        }
                    }
                });
            }

            if (attachments.Count == 0)
            {
                Log.Debug("All services are alive.");
                return;
            }
            
            var slackMessage = new SlackMessage
            {
                Attachments = attachments
            };

            await WebHookClient.PostAsync(slackMessage);

            Log.Debug("Scheduled message sent.");
        }
    }
}
