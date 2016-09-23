using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using RiotSharp;
using RiotSharp.LeagueEndpoint;

namespace InGameProbabilitiesPlugin.Network
{
    public class NetworkResponse
    {
        public double team1;
        public double team2;
    }

    public class CurrentGameResponse
    {
        public long[] summonerIds;
        public long[] championIds;
    }

    public class NetworkInterface
    {
        private HttpClient client;
        private string url;
        private RiotApi apiClient;
        private string apiKey;

        public NetworkInterface(string addr, int port, string key)
        {
            client = new HttpClient();
            url = addr + ":" + port;
            apiClient = RiotApi.GetInstance(apiKey);
            apiKey = key;
        }

        public NetworkResponse Post(string path, IDictionary<string, string> content)
        {
            var body = JsonConvert.SerializeObject(content);

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = client.PostAsync(url + path, new StringContent(body, Encoding.UTF8, "application/json")).Result;

            return JsonConvert.DeserializeObject<NetworkResponse>(response.Content.ReadAsStringAsync().Result);
        }

        public long[] GetSummonerIds(string[] summonerNames)
        {
            try
            {
                return apiClient.GetSummoners(Region.na, new List<string>(summonerNames)).Select((RiotSharp.SummonerEndpoint.Summoner summoner) =>
                {
                    return summoner.Id;
                }).ToArray();
            }
            catch (RiotSharpException ex)
            {
                Console.Write(ex);
            }
            return null;
        }
           
        public CurrentGameResponse GetCurrentGame(long summonerId)
        {
            var result = new CurrentGameResponse { summonerIds = new long[10], championIds = new long[10] };
            try
            {
                var participants = apiClient.GetCurrentGame(Platform.NA1, summonerId).Participants;
                for (var i = 0; i < participants.Count; i++)
                {
                    var currentParticipant = participants[i];
                    result.summonerIds[i] = currentParticipant.SummonerId;
                    result.championIds[i] = currentParticipant.ChampionId;
                }
            }
            catch (RiotSharpException ex)
            {
                Console.Write(ex);
            }
            return result;
        }

        public long[] GetChampionIds(string[] championNames)
        {
            var staticApi = RiotSharp.StaticRiotApi.GetInstance(apiKey);
            var championKeys = staticApi.GetChampions(Region.na).Keys;
            var result = new List<long>();
            foreach (var key in championKeys.Keys)
            {
                if (championNames.Contains(championKeys[key]))
                {
                    result.Add((long)key);
                }
            }
            return result.ToArray();
        }

        public RiotSharp.LeagueEndpoint.Tier[] GetRank(long[] summonerIds)
        {
            var summonerList = new List<int>();

            foreach (var i in summonerIds)
            {
                summonerList.Add((int)i);
            }

            try
            {
                var participants = apiClient.GetLeagues(Region.na, summonerList);
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
