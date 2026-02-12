using ChatContractsLibrary;
using Microsoft.AspNetCore.SignalR;

namespace HubServer
{
    public class ChatHub : Hub<IChatClient>
    {
        private static readonly object _lockUsers = new object();
        private static List<ConnectedUser> _connectedUsers = new List<ConnectedUser>();
        private static List<ChatMessage> _messageHistory = new List<ChatMessage>();

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

            // Send tailored user lists (with message history) to all connected clients
            await SendUpdatedUserListsToAll();
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
                await SendUpdatedUserListsToAll();
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task ForwardMessage(string fromUserId, string toConnectionId, string message)
        {
            if (!string.IsNullOrWhiteSpace(toConnectionId))
            {
                // find recipient user id
                var toUserId = _connectedUsers.FirstOrDefault(u => u.ConnectionId == toConnectionId)?.UserId;

                // Save message to history
                var chatMessage = new ChatMessage
                {
                    FromUserId = fromUserId,
                    ToUserId = toUserId,
                    Message = message,
                    Unread = true
                };

                lock (_lockUsers)
                {
                    _messageHistory.Add(chatMessage);
                }

                // Forward message to recipient
                await Clients.Client(toConnectionId).ReceiveMessage(fromUserId, Context.ConnectionId, message);

                // Update message lists for all clients (so reconnecting clients get history)
                await SendUpdatedUserListsToAll();
            }
        }

        private async Task SendUpdatedUserListsToAll()
        {
            // For each connected user, build a tailored list where each ConnectedUser.Messages contains
            // messages between that connected user and the current caller (so the client can show conversation history)
            List<ConnectedUser> snapshot;
            lock (_lockUsers)
            {
                snapshot = _connectedUsers.Select(u => new ConnectedUser
                {
                    UserId = u.UserId,
                    UserName = u.UserName,
                    ConnectionId = u.ConnectionId
                }).ToList();
            }

            // Send tailored lists to each client individually
            foreach (var target in snapshot)
            {
                // Build list for this target client
                var tailored = snapshot.Select(u => new ConnectedUser
                {
                    UserId = u.UserId,
                    UserName = u.UserName,
                    ConnectionId = u.ConnectionId,
                    Messages = _messageHistory
                        .Where(m =>
                            // include messages where either side is the target client and the other side is the listed user
                            ((m.FromUserId == u.UserId && m.ToUserId == target.UserId) ||
                             (m.FromUserId == target.UserId && m.ToUserId == u.UserId))
                        )
                        .ToList()
                }).ToList();

                // Send to the specific client connection id
                if (!string.IsNullOrEmpty(target.ConnectionId))
                {
                    await Clients.Client(target.ConnectionId).UpdateUserList(tailored);
                }
            }
        }
    }
}

