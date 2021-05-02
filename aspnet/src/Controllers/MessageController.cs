using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace SignalRChat.Controllers
{
    [Route("messages")]
    public class MessageController : Controller
    {
        private IDistributedCache DistributedCache { get; }
        public MessageController(IDistributedCache distributedCache)
        {
            DistributedCache = distributedCache;
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAll()
        {
            var cache = await DistributedCache.GetStringAsync("Messages");
            if (cache is null) return Ok(Enumerable.Empty<Message>());

            var messages = JsonConvert.DeserializeObject<IEnumerable<Message>>(cache);
            return Ok(messages);
        }
    }
}