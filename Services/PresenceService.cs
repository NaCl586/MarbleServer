using System.Collections.Generic;
using System.Linq;

public static class PlayerStatus
{
    public const string None = "";
    public const string LevelSelect = "Level Select";
    public const string Playing = "Playing";
}

public class PlayerPresence
{
    public string ConnectionId { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;
}

public class PresenceService
{
    private readonly object _lock = new();

    private readonly Dictionary<string, PlayerPresence> _players = new();

    public string? ReplaceConnection(
        string username,
        string connectionId)
    {
        lock (_lock)
        {
            _players.TryGetValue(
                username,
                out PlayerPresence? oldPresence);

            string? oldConnectionId =
                oldPresence?.ConnectionId;

            _players[username] = new PlayerPresence
            {
                ConnectionId = connectionId,
                Status = string.Empty
            };

            return oldConnectionId;
        }
    }

    public bool RemoveConnection(
        string username,
        string connectionId)
    {
        lock (_lock)
        {
            if (!_players.TryGetValue(
                    username,
                    out PlayerPresence? presence))
            {
                return false;
            }

            // Don't remove a newer connection.
            if (presence.ConnectionId != connectionId)
                return false;

            _players.Remove(username);

            return true;
        }
    }

    public bool SetStatus(
        string username,
        string connectionId,
        string status)
    {
        lock (_lock)
        {
            if (!_players.TryGetValue(
                    username,
                    out PlayerPresence? presence))
            {
                return false;
            }

            // Make sure this is still the current session.
            if (presence.ConnectionId != connectionId)
                return false;

            presence.Status = status ?? string.Empty;

            return true;
        }
    }

    public IReadOnlyList<OnlinePlayer> GetPlayers()
    {
        lock (_lock)
        {
            return _players
                .Select(x => new OnlinePlayer
                {
                    Username = x.Key,
                    Status = x.Value.Status
                })
                .ToList();
        }
    }

    public string GetStatus(
        string username,
        string connectionId)
    {
        lock (_lock)
        {
            if (!_players.TryGetValue(
                    username,
                    out PlayerPresence? presence))
            {
                return string.Empty;
            }

            // Don't return the status of an old session.
            if (presence.ConnectionId != connectionId)
                return string.Empty;

            return presence.Status ?? string.Empty;
        }
    }

    public bool IsCurrentConnection(
        string username,
        string connectionId)
    {
        lock (_lock)
        {
            return _players.TryGetValue(
                       username,
                       out PlayerPresence? presence)
                   && presence.ConnectionId ==
                      connectionId;
        }
    }
}