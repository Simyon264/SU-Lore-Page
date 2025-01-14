using System.Collections.Concurrent;
using System.Net;
using Microsoft.AspNetCore.SignalR;
using Serilog;

namespace SU_Lore.Helpers;

public class UserCountHub : Hub
{
    private static readonly SynchronizedCollection<HubUser> ConnectedUsers = new();

    public override Task OnConnectedAsync()
    {
        CheckAllStillConnected();

        Log.Information("User connected: {ConnectionId}", Context.ConnectionId);
        var ipAddress = GetIpAddress()?.ToString();

        if (string.IsNullOrEmpty(ipAddress))
        {
            Log.Warning("Could not get IP address for user {ConnectionId}", Context.ConnectionId);
            return base.OnConnectedAsync();
        }


        // Check if the IP address is already in the dictionary
        var user = ConnectedUsers.FirstOrDefault(u => u.IpAddress == ipAddress);
        if (user == null)
        {
            user = new HubUser {IpAddress = ipAddress};
            ConnectedUsers.Add(user);
        } else
        {
            user.ConnectionIds.Add(Context.ConnectionId);
        }

        Clients.All.SendAsync("UpdateUserCount", GetUniqueUserCount());

        return base.OnConnectedAsync();
    }

    private void CheckAllStillConnected()
    {
        foreach (var user in ConnectedUsers)
        {
            user.ConnectionIds.RemoveAll(c => Clients.Client(c).Equals(null));
            if (user.ConnectionIds.Count == 0)
            {
                ConnectedUsers.Remove(user);
            }
        }
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        Log.Information("User disconnected: {ConnectionId}", Context.ConnectionId);
        var ipAddress = GetIpAddress()?.ToString();

        if (string.IsNullOrEmpty(ipAddress))
        {
            Log.Warning("Could not get IP address for user {ConnectionId}", Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }

        var user = ConnectedUsers.FirstOrDefault(u => u.IpAddress == ipAddress);
        if (user != null)
        {
            user.ConnectionIds.Remove(Context.ConnectionId);
            if (user.ConnectionIds.Count == 0)
            {
                ConnectedUsers.Remove(user);
            }
        } else
        {
            Log.Warning("Could not find user with IP address {IpAddress} to remove", ipAddress);
        }

        Clients.All.SendAsync("UpdateUserCount", GetUniqueUserCount());

        return base.OnDisconnectedAsync(exception);
    }

    public void GetUserCount()
    {
        Clients.Caller.SendAsync("UpdateUserCount", GetUniqueUserCount());
    }

    private int GetUniqueUserCount()
    {
        return ConnectedUsers.Count;
    }

    /// <summary>
    /// Gets the IP address of the context. Respects X-Forwarded-For headers.
    /// </summary>
    /// <returns></returns>
    private IPAddress? GetIpAddress()
    {
        var httpContext = Context.GetHttpContext();
        var ipAddress = httpContext?.Connection.RemoteIpAddress;
        if (httpContext != null && httpContext.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
        {
            if (IPAddress.TryParse(forwardedFor, out var forwardedIp))
            {
                ipAddress = forwardedIp;
            }
        }

        return ipAddress;
    }

    public class HubUser
    {
        public List<string> ConnectionIds { get; set; } = new();
        public string IpAddress { get; set; } = string.Empty;
    }
}