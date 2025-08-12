using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;

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

        // ✅ Comando especial: listar usuarios online
        if (message.Trim().Equals("/online", StringComparison.OrdinalIgnoreCase))
        {
            var onlineUsers = UserConnectionTracker.GetOnlineUsers();
            var list = onlineUsers.Any() 
                ? "Usuarios online: " + string.Join(", ", onlineUsers)
                : "No hay usuarios conectados.";

            await Clients.Caller.SendAsync("ReceiveMessage", "System", list);
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
