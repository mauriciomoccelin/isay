using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

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
                RefreshCache(message),
                Clients.All.SendAsync("ReceiveMessage", message)
            );
        }

        private async Task RefreshCache(Message message)
        {
            IList<Message> messages;
            var cache = await DistributedCache.GetStringAsync("Messages");
            
            if (cache is null)
                messages = new List<Message>();
            else
                messages = JsonConvert.DeserializeObject<IList<Message>>(cache);

            messages.Add(message);

            await DistributedCache.SetStringAsync("Messages", JsonConvert.SerializeObject(messages));
        }
    }
}