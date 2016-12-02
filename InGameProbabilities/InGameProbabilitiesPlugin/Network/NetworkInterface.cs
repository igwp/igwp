
namespace InGameProbabilitiesPlugin.Network
{
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using GameData;
    using Newtonsoft.Json;

    public class NetworkResponse
    {
        public double team1 { get; set; }
        public double team2 { get; set; }
    }
    
    internal class NetworkInterface
    {
        private readonly HttpClient _client;
        private readonly string _url;

        public NetworkInterface(string addr, int port)
        {
            this._client = new HttpClient();
            this._url = $"http://{addr}:{port}";
        }

        public async Task<NetworkResponse> GetPrediction(GameState content)
        {
            var body = JsonConvert.SerializeObject(content);

            this._client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = await this._client.PostAsync($"{this._url}/getmodel", new StringContent(body, Encoding.UTF8, "application/json"));
            var serializedPrediction = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<NetworkResponse>(serializedPrediction);
        }
    }
}
