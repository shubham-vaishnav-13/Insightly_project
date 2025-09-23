using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Insightly_project.Hubs
{
    [Authorize]
    public class RoleHub : Hub
    {
        // Allow server to notify a specific user their role changed
        public async Task Ping()
        {
            await Clients.Caller.SendAsync("Pong");
        }
    }
}
