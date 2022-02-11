using Serilog;

namespace ServicesStatusChecker.Models
{
    public class Site
    {
        public Site(string url)
        {
            Url = url;

            try
            {
                Alive = CheckLocalStatus().Result;
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                Alive = null;
            }
        }

        public string Url { get; set; }

        public bool? Alive { get; set; }

        public string Status
        {
            get
            {
                if (Alive == null)
                {
                    return "Couldn't check status.";
                }

                return Alive.Value ? "Alive" : "Not Alive";
            }
        }

        public async Task<bool?> CheckLocalStatus()
        {
            var client = new HttpClient();

            try
            {
                var response = await client.GetAsync(Url);
                Log.Debug($"{Url} status: {response.StatusCode}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception e)
            {
                if (e.Message.Contains("302"))
                {
                    var response = await client.GetAsync(Url.Replace("https", "http"));
                    Log.Debug($"{Url} status: {response.StatusCode}");
                    return response.IsSuccessStatusCode;
                }

                Log.Error(e.Message);
                return null;
            }
        }
    }
}
