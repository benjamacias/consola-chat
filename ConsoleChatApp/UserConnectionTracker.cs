using System.Collections.Concurrent;

public static class UserConnectionTracker
{
    private static readonly ConcurrentDictionary<string, HashSet<string>> UserConnections = new();

    public static void AddConnection(string username, string connectionId)
    {
        var connections = UserConnections.GetOrAdd(username, _ => new HashSet<string>());
        lock (connections)
        {
            connections.Add(connectionId);
        }
    }
    public static IEnumerable<string> GetOnlineUsers()
    {
        return UserConnections.Keys;
    }
    public static void RemoveConnection(string username, string connectionId)
    {
        if (UserConnections.TryGetValue(username, out var connections))
        {
            lock (connections)
            {
                connections.Remove(connectionId);
                if (connections.Count == 0)
                {
                    UserConnections.TryRemove(username, out _);
                }
            }
        }
    }

    public static bool IsUserConnected(string username)
    {
        return UserConnections.ContainsKey(username);
    }
}
