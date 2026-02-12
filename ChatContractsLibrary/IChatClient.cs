using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatContractsLibrary
{
   public  interface IChatClient
    {
        Task ReceiveSystemMessage(string message);
        Task UpdateUserList(List<ConnectedUser> users);

        Task ReceiveMessage(string fromUserId, string fromConnectionId, string message);
    }
}
