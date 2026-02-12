using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatContractsLibrary
{
    public class ChatMessage
    {
        public string? FromUserId { get; set; }
         public string? ToUserId { get; set; }
        public string? Message { get; set; }

        public bool? Unread { get; set; } = true;
       

    }
}
