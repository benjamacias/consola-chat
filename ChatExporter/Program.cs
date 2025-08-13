using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

var client = new MongoClient(Environment.GetEnvironmentVariable("MONGODB_URI") ?? "mongodb://localhost:27017");
var db = client.GetDatabase("chatdb");
var collection = db.GetCollection<ChatMessage>("messages");

var messages = await collection.Find(FilterDefinition<ChatMessage>.Empty)
    .SortBy(m => m.Timestamp)
    .ToListAsync();

var path = "conversation.txt";
await File.WriteAllLinesAsync(path, messages.Select(m => $"[{m.Timestamp:O}] {m.Sender} -> {m.Receiver}: {m.Message}"));

// Simula 'cat' mostrando el contenido del archivo
Console.WriteLine(await File.ReadAllTextAsync(path));

class ChatMessage
{
    [BsonId]
    public ObjectId Id { get; set; }
    public string Sender { get; set; } = null!;
    public string Receiver { get; set; } = null!;
    public string Message { get; set; } = null!;
    public DateTime Timestamp { get; set; }
}
