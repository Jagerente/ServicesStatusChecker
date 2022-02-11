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

        private static Slack.Webhooks.SlackClient WebHookClient;

        private static SlackAPI.SlackTaskClient Client;

        private static List<Site> Sites;

        public SlackBot(string tokenPath)
        {
            InitConfig(tokenPath);
            Log.Debug("Slack WebHook is ready.");
        }

        private static void InitConfig(string path)
        {
            _config = JsonConvert.DeserializeObject<SlackConfig>(File.ReadAllText(path));

            Client = new SlackAPI.SlackTaskClient(_config.Token);
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

        public async Task ClientStatusCheck()
        {
            ConfigureSites(Constants.SitesPath);

            foreach (var site in Sites)
            {
                Log.Debug("Configuring message.");

                var status = site.Status;

                if ((site.Alive == null || site.Alive.Value) && !Constants.Critical) continue;

                var attachments = new[]
                {
                    new SlackAPI.Attachment()
                    {
                        fallback = $"{site.Url} is {status}",
                        text = status + (site.Alive != null && !site.Alive.Value
                            ? $" <!subteam^{_config.TeamId}>"
                            : string.Empty),
                        color = site.Alive == null ? Color.Silver.ToHEX() :
                            site.Alive.Value ? Color.Green.ToHEX() : Color.Red.ToHEX(),
                        actions = new[]
                        {
                            new SlackAPI.AttachmentAction("button", "Open Web")
                            {
                                url = site.Url,
                                type = "button"
                            }
                        }
                    }
                };


                await Client.PostMessageAsync("status",
                    string.Empty,
                    site.Url,
                    icon_emoji: site.Alive == null ? Emoji.Warning : site.Alive.Value ? Emoji.WhiteCheckMark : Emoji.X,
                    attachments: attachments);
            }

            Log.Debug("Scheduled messages sent.");
        }

        [Obsolete]
        public async Task WebClientStatusCheck()
        {
            ConfigureSites(Constants.SitesPath);

            foreach (var site in Sites)
            {
                var attachments = new List<SlackAttachment>();
                
                Log.Debug("Configuring message.");
                
                var status = site.Status;

                if ((site.Alive == null || site.Alive.Value) && !Constants.Critical) continue;

                attachments.Add(new SlackAttachment()
                {
                    Fallback = $"{site.Url} is {status}",
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
                var slackMessage = new SlackMessage
                {
                    Username = site.Url,
                    IconEmoji = site.Alive == null ? Emoji.Warning : site.Alive.Value ? Emoji.WhiteCheckMark : Emoji.X,
                    Attachments = attachments,
                    
                };

                await WebHookClient.PostAsync(slackMessage);
            }

            Log.Debug("Scheduled messages sent.");
        }
    }
}
