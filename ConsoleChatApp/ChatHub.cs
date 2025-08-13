using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;
using Microsoft.AspNetCore.Authorization;
using System.Linq;

[Authorize]
public class ChatHub : Hub
{
    private readonly IMongoCollection<ChatMessage> _messages;

    public ChatHub(IMongoCollection<ChatMessage> messages)
    {
        _messages = messages;
    }

    public override async Task OnConnectedAsync()
    {
        var user = Context.User?.Identity?.Name;
        if (!string.IsNullOrEmpty(user))
        {
            UserConnectionTracker.AddConnection(user, Context.ConnectionId);
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var user = Context.User?.Identity?.Name;
        if (!string.IsNullOrEmpty(user))
        {
            UserConnectionTracker.RemoveConnection(user, Context.ConnectionId);
        }
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(string receiver, string message)
    {
        var sender = Context.User?.Identity?.Name ?? "Desconocido";
        var trimmed = message.Trim();

        // ✅ Comando especial: listar usuarios online
        if (trimmed.Equals("/online", StringComparison.OrdinalIgnoreCase))
        {
            var onlineUsers = UserConnectionTracker.GetOnlineUsers();
            var list = onlineUsers.Any()
                ? "Usuarios online: " + string.Join(", ", onlineUsers)
                : "No hay usuarios conectados.";

            await Clients.Caller.SendAsync("ReceiveMessage", "System", list);
            return;
        }

        // ✅ Comando especial: historial de conversación (cat)
        if (trimmed.StartsWith("/cat ", StringComparison.OrdinalIgnoreCase) ||
            trimmed.StartsWith("cat ", StringComparison.OrdinalIgnoreCase))
        {
            var parts = trimmed.Split(" ", 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2)
            {
                await Clients.Caller.SendAsync("ReceiveMessage", "System", "⚠️ Uso: cat <usuario>");
                return;
            }
            var other = parts[1];
            var filter = Builders<ChatMessage>.Filter.Or(
                Builders<ChatMessage>.Filter.And(
                    Builders<ChatMessage>.Filter.Eq(m => m.Sender, sender),
                    Builders<ChatMessage>.Filter.Eq(m => m.Receiver, other)
                ),
                Builders<ChatMessage>.Filter.And(
                    Builders<ChatMessage>.Filter.Eq(m => m.Sender, other),
                    Builders<ChatMessage>.Filter.Eq(m => m.Receiver, sender)
                )
            );
            var history = await _messages.Find(filter)
                .SortBy(m => m.Timestamp)
                .ToListAsync();

            if (!history.Any())
            {
                await Clients.Caller.SendAsync("ReceiveMessage", "System", $"No hay mensajes con {other}.");
                return;
            }

            var lines = history.Select(m => $"[{m.Timestamp:HH:mm}] {m.Sender} -> {m.Receiver}: {m.Message}");
            var text = string.Join("\n", lines);
            await Clients.Caller.SendAsync("ReceiveMessage", "System", text);
            return;
        }

        // ✅ Verificar si el usuario destino está conectado
        if (!UserConnectionTracker.IsUserConnected(receiver))
        {
            await Clients.Caller.SendAsync("ReceiveMessage", "System", $"⚠️ Usuario '{receiver}' no está conectado.");
            return;
        }

        // ✅ Enviar mensaje privado
        await Clients.User(receiver).SendAsync("ReceiveMessage", sender, message);

        var chatMessage = new ChatMessage
        {
            Sender = sender,
            Receiver = receiver,
            Message = message,
            Timestamp = DateTime.UtcNow
        };
        await _messages.InsertOneAsync(chatMessage);
    }
}
