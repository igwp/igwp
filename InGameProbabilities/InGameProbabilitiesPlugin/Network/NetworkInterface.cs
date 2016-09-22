using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace InGameProbabilitiesPlugin.Network
{
    public class NetworkResponse
    {
        public double probability;
    }

    public class NetworkInterface
    {
        private HttpClient client;
        private string url;

        public NetworkInterface(string addr, int port)
        {
            client = new HttpClient();
            url = addr + ":" + port;
        }

        public NetworkResponse Post(string path, IDictionary<string, string> content)
        {
            var body = JsonConvert.SerializeObject(content);

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = client.PostAsync(url + path, new StringContent(body, Encoding.UTF8, "application/json")).Result;

            return JsonConvert.DeserializeObject<NetworkResponse>(response.Content.ReadAsStringAsync().Result);
        }
           
    }
}
