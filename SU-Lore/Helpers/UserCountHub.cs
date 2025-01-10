using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using Serilog;

namespace SU_Lore.Helpers;

public class UserCountHub : Hub
{
    private static readonly ConcurrentDictionary<string, string> ConnectedUsers = new();

    public override Task OnConnectedAsync()
    {
        Log.Information("User connected: {ConnectionId}", Context.ConnectionId);
        var ipAddress = Context.GetHttpContext()?.Connection.RemoteIpAddress?.ToString();

        if (string.IsNullOrEmpty(ipAddress))
            return base.OnConnectedAsync();

        ConnectedUsers.TryAdd(Context.ConnectionId, ipAddress);
        Clients.All.SendAsync("UpdateUserCount", GetUniqueUserCount());

        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        Log.Information("User disconnected: {ConnectionId}", Context.ConnectionId);
        ConnectedUsers.TryRemove(Context.ConnectionId, out _);
        Clients.All.SendAsync("UpdateUserCount", GetUniqueUserCount());

        return base.OnDisconnectedAsync(exception);
    }

    public void GetUserCount()
    {
        Clients.Caller.SendAsync("UpdateUserCount", GetUniqueUserCount());
    }

    private int GetUniqueUserCount()
    {
        return ConnectedUsers
            .GroupBy(kvp => kvp.Value) // Group by IP address
            .Count();                 // Count unique IPs
    }
}