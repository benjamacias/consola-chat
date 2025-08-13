using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class ChatMessage
{
    [BsonId]
    public ObjectId Id { get; set; } = ObjectId.GenerateNewId();
    public string Sender { get; set; } = null!;
    public string Receiver { get; set; } = null!;
    public string Message { get; set; } = null!;
    public DateTime Timestamp { get; set; }
}
