using System;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace SignalRChat.Hubs
{
  public class ChatHub : Hub
    {
        private IDistributedCache DistributedCache { get; }
        public ChatHub(IDistributedCache distributedCache)
        {
            DistributedCache = distributedCache;
        }

        public async Task SendMessage(Message message)
        {
            await Task.WhenAll(
                LuizPredict(message),
                RefreshCache(message),
                Clients.All.SendAsync("ReceiveMessage", message)
            );
        }

        private async Task RefreshCache(Message message)
        {
            var cache = await DistributedCache.GetStringAsync("Messages");
            var messages = cache is null ? new List<Message>() : JsonConvert.DeserializeObject<IList<Message>>(cache);
            messages.Add(message);
            await DistributedCache.SetStringAsync("Messages", JsonConvert.SerializeObject(messages));
        }

        private static async Task LuizPredict(Message message)
        {
            const string predictionKey ="";
            const string predictionEndpoint ="";
            const string appId ="";
            
            var client = new HttpClient();
            var queryString = HttpUtility.ParseQueryString(string.Empty);
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", predictionKey);
            queryString["query"] = message.Text;
            var predictionEndpointUri = 
                $"{predictionEndpoint}luis/prediction/v3.0/apps/{appId}/slots/production/predict?{queryString}";
            var response = await client.GetAsync(predictionEndpointUri);
            var strResponseContent = await response.Content.ReadAsStringAsync();
            
            Console.WriteLine(strResponseContent);
        }
    }
}