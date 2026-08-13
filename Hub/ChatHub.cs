using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using MarbleServer.Services;

[Authorize]
public class ChatHub : Hub
{
    private readonly PresenceService _presence;
    private readonly ChatMessageHistoryService _chatHistory;

    public ChatHub(
        PresenceService presence,
        ChatMessageHistoryService chatHistory)
    {
        _presence = presence;
        _chatHistory = chatHistory;
    }

    private string? GetUsername()
    {
        return Context.User?.FindFirstValue(
            ClaimTypes.Name);
    }

    public override async Task OnConnectedAsync()
    {
        string? username = GetUsername();

        if (string.IsNullOrEmpty(username))
        {
            Context.Abort();
            return;
        }

        string? oldConnectionId =
            _presence.ReplaceConnection(
                username,
                Context.ConnectionId);

        if (string.IsNullOrEmpty(oldConnectionId))
        {
            // Completely new login.

            await Clients.All.SendAsync(
                "PlayerJoined",
                username);

            string loginMessage =
                $"{username} has logged in.";

            _chatHistory.AddSystemMessage(
                loginMessage);

            await Clients.All.SendAsync(
                "SystemMessage",
                loginMessage);
        }
        else
        {
            // Another session of this account was opened.
            // Kick the old session.

            await Clients.Client(oldConnectionId)
                .SendAsync("ForceLogout");
        }

        await Clients.Caller.SendAsync(
            "OnlinePlayers",
            _presence.GetPlayers());

        await Clients.Caller.SendAsync(
            "RecentMessages",
            _chatHistory.GetRecentMessages());

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(
        Exception? exception)
    {
        string? username = GetUsername();

        if (!string.IsNullOrEmpty(username))
        {
            bool wentOffline =
                _presence.RemoveConnection(
                    username,
                    Context.ConnectionId);

            if (wentOffline)
            {
                await Clients.All.SendAsync(
                    "PlayerLeft",
                    username);

                string logoutMessage =
                    $"{username} has logged out.";

                _chatHistory.AddSystemMessage(
                    logoutMessage);

                await Clients.All.SendAsync(
                    "SystemMessage",
                    logoutMessage);
            }
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(string message)
    {
        string? username = GetUsername();

        if (string.IsNullOrEmpty(username))
            return;

        if (string.IsNullOrWhiteSpace(message))
            return;

        // Get the status of the CURRENT connection.
        string status =
            _presence.GetStatus(
                username,
                Context.ConnectionId);

        _chatHistory.AddMessage(
            username,
            message,
            status);

        await Clients.All.SendAsync(
            "ReceiveMessage",
            username,
            message,
            status);
    }

    public async Task SetStatus(string status)
    {
        string? username = GetUsername();

        if (string.IsNullOrEmpty(username))
            return;

        if (status != PlayerStatus.None &&
            status != PlayerStatus.LevelSelect &&
            status != PlayerStatus.Playing)
        {
            return;
        }

        bool success =
            _presence.SetStatus(
                username,
                Context.ConnectionId,
                status);

        if (!success)
            return;

        await Clients.All.SendAsync(
            "PlayerStatusChanged",
            username,
            status);
    }
}