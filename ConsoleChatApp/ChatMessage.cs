public class ChatMessage
{
    public string Id { get; set; } = null!; // MongoDB document Id
    public string Sender { get; set; } = null!;
    public string Receiver { get; set; } = null!;
    public string Message { get; set; } = null!;
    public DateTime Timestamp { get; set; }
}
