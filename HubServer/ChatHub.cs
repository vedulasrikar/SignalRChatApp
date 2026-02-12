using ChatContractsLibrary;
using Microsoft.AspNetCore.SignalR;

namespace HubServer
{
    public class ChatHub : Hub<IChatClient>
    {
        private static readonly object _lockUsers = new object();
        private static List<ConnectedUser> _connectedUsers = new List<ConnectedUser>();
        public override async Task OnConnectedAsync()
        {

            var userId = string.Empty;
            var userName = string.Empty;

            userId = Context.GetHttpContext()?.Request.Query["userId"];
            userName = Context.GetHttpContext()?.Request.Query["userName"];

            lock (_lockUsers)
            {
                _connectedUsers.Add(new ConnectedUser
                {
                    UserId = userId,
                    UserName = userName,
                    ConnectionId = Context.ConnectionId
                });
            }

            await Clients.Caller.ReceiveSystemMessage($"Hi {userName}, you have just connected.");
            await Clients.All.UpdateUserList(_connectedUsers);

        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            ConnectedUser? user = null;
            lock (_lockUsers)
            {
                user = _connectedUsers.FirstOrDefault(u => u.ConnectionId == Context.ConnectionId);
                if (user != null)
                {
                    _connectedUsers.Remove(user);
                }
            }
            if (user != null)
            {
                //Update the clients about the user list change
                await Clients.All.UpdateUserList(_connectedUsers);
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task ForwardMessage(string fromUserId, string toConnectionId, string message)
        {
            if (!string.IsNullOrWhiteSpace(toConnectionId))
            {
                await Clients.Client(toConnectionId).ReceiveMessage(fromUserId, Context.ConnectionId, message);
            }
        }
    }
}

