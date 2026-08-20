using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Portfolio.Application.Authentication;
using Portfolio.Application.Contact;

namespace Portfolio.Api.Hubs;

[Authorize(Policy = AdminAuthorization.Policy)]
public sealed class NotificationsHub(IContactService contactService) : Hub
{
    public const string HubUrl = "/hubs/notifications";
    public const string AdminGroupName = "AdminNotifications";

    public override async Task OnConnectedAsync()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, AdminGroupName);
        var counts = await contactService.GetUnreadCountAsync(Context.ConnectionAborted);
        await Clients.Caller.SendAsync("ReceiveUnreadCount", counts.UnreadCount);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, AdminGroupName);
        await base.OnDisconnectedAsync(exception);
    }
}
