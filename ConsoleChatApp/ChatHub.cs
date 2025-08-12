using Microsoft.AspNetCore.SignalR;

public class ChatHub : Hub
{
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
    }
}
