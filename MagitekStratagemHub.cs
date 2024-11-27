using Microsoft.AspNetCore.SignalR;

namespace MagitekStratagemServer.Hubs
{
    public class MagitekStratagemHub : Hub
    {
        public async Task Hello()
        {
            await Clients.All.SendAsync("hello", "Hello World!");
        }
    }
}
