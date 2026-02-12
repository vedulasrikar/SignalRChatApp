namespace ChatContractsLibrary
{
    public class ConnectedUser
    {
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public string? ConnectionId { get; set; }
        public List<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }
}
