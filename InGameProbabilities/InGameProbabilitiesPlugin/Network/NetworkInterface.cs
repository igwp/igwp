
namespace InGameProbabilitiesPlugin.Network
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using GameData;
    using Newtonsoft.Json;
    using RiotSharp;
    using RiotSharp.LeagueEndpoint;

    public class NetworkResponse
    {
        public double team1 { get; set; }
        public double team2 { get; set; }
    }

    public class CurrentGameResponse
    {
        public long[] summonerIds { get; set; }
        public long[] championIds { get; set; }
    }

    internal class NetworkInterface
    {
        private readonly HttpClient _client;
        private readonly string _url;
        private readonly RiotApi _apiClient;
        private readonly string _apiKey;

        public NetworkInterface(string addr, int port, string key)
        {
            this._client = new HttpClient();
            this._url = $"http://{addr}:{port}";
            this._apiKey = key;
            this._apiClient = RiotApi.GetInstance(this._apiKey);
        }

        public async Task<NetworkResponse> GetPrediction(GameState content)
        {
            var body = JsonConvert.SerializeObject(content);

            this._client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = await this._client.PostAsync($"{this._url}/getmodel", new StringContent(body, Encoding.UTF8, "application/json"));
            var serializedPrediction = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<NetworkResponse>(serializedPrediction);
        }

        public int[] GetChampionIds(string[] championNames)
        {
            var staticApi = StaticRiotApi.GetInstance(this._apiKey);
            var champions = staticApi.GetChampions(Region.na).Champions;

            return championNames.Select(name => champions[name].Id).ToArray();
        }

        public Tier[] GetRank(long[] summonerIds)
        {
            var summonerList = new List<int>();

            foreach (var i in summonerIds)
            {
                summonerList.Add((int)i);
            }

            try
            {
                var participants = this._apiClient.GetLeagues(Region.na, summonerList);
                return summonerIds.Select((long summId) =>
                {
                    var leagues = participants[summId];
                    var tier = leagues.FirstOrDefault(l => l.Queue == Queue.RankedSolo5x5);
                    return tier?.Tier ?? Tier.Unranked;
                }).ToArray();
            }
            catch (RiotSharpException ex)
            {
                Console.Write(ex);
            }
            return null;
        }
    }
}
