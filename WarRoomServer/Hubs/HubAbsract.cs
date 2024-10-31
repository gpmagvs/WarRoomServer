using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace WarRoomServer.Hubs
{
    public abstract class HubAbsract : Hub
    {
        public abstract ConcurrentDictionary<string, ClientRequestState> clientStates { get; set; }


        public override Task OnDisconnectedAsync(Exception? exception)
        {
            if (clientStates.TryRemove(Context.ConnectionId, out ClientRequestState state))
            {
                state.cancelToskenSource.Cancel();
            }
            return base.OnDisconnectedAsync(exception);
        }

    }
}
