using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Application.Hubs
{
    public class NotificationsHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var userId = GetUserId(Context.User);
            if (userId != null)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user:{userId}");
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = GetUserId(Context.User);
            if (userId != null)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user:{userId}");
            }
            await base.OnDisconnectedAsync(exception);
        }

        private static int? GetUserId(ClaimsPrincipal? user)
        {
            if (user == null) return null;
            var idClaim = user.FindFirst(ClaimTypes.NameIdentifier) ?? user.FindFirst("sub") ?? user.FindFirst("userId");
            return int.TryParse(idClaim?.Value, out var id) ? id : null;
        }
    }
}


