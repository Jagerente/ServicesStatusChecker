using Newtonsoft.Json;

namespace ServicesStatusChecker.Models
{
    public class SlackConfig
    {
        [JsonProperty("token")]
        public string Token { get; set; }
        [JsonProperty("webHookUrl")]
        public string WebHookUrl { get; set; }
    }
}
