using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WerefoxBot
{
    public struct ConfigJson
    {
        [JsonProperty("token")]
        public string Token { get; private set; }

        [JsonProperty("prefix")]
        public string CommandPrefix { get; private set; }
        
        public static async Task<ConfigJson> Load()
        {
            var json = await File.ReadAllTextAsync("config.json");
            return JsonConvert.DeserializeObject<ConfigJson>(json);
        }
    }
}