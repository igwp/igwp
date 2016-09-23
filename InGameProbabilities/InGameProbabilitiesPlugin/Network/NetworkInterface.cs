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

namespace InGameProbabilitiesPlugin.Network
{
    public class NetworkResponse
    {
        public double probability;
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

        public NetworkInterface(string addr, int port, string apiKey)
        {
            client = new HttpClient();
            url = addr + ":" + port;
            apiClient = RiotApi.GetInstance(apiKey);
        }

        public NetworkResponse Post(string path, IDictionary<string, string> content)
        {
            var body = JsonConvert.SerializeObject(content);

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = client.PostAsync(url + path, new StringContent(body, Encoding.UTF8, "application/json")).Result;

            return JsonConvert.DeserializeObject<NetworkResponse>(response.Content.ReadAsStringAsync().Result);
        }

        public long GetSummonerId(string summonerName)
        {
            long summonerId = 0;
            try
            {
                summonerId = apiClient.GetSummoner(Region.na, summonerName).Id;
            
            }
            catch (RiotSharpException ex)
            {
                Console.Write(ex);
            }
            return summonerId;
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

        public RiotSharp.LeagueEndpoint.Tier[] GetRank(long[] summonerIds)
        {
            var summonerList = new List<int>();
            var result = new RiotSharp.LeagueEndpoint.Tier[10];

            foreach (var i in summonerIds)
            {
                summonerList.Add((int)i);
            }

            try
            {
                var participants = apiClient.GetLeagues(Region.na, summonerList);
                foreach (var i in summonerIds)
                {
                    var leagues = participants[i];
                    foreach (var league in leagues)
                    {
                        if (league.Queue == RiotSharp.Queue.RankedSolo5x5)
                        {
                            result[i] = league.Tier;
                        }
                    }
                }
            }
            catch (RiotSharpException ex)
            {
                Console.Write(ex);
            }
            return result;
        }
    }
}
