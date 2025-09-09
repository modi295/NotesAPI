using Microsoft.AspNetCore.SignalR;
using NotesAPI.Repositories;

public class ChatHub : Hub
{
    private readonly IBotService _botService;

    public ChatHub(IBotService botService)
    {
        _botService = botService;
    }

    public async Task SendMessage(string user, string message)
    {
        // Broadcast user message
        await Clients.All.SendAsync("ReceiveMessage", user, message);

        // Bot response
        var reply = await _botService.GetResponseAsync(message);
        await Clients.All.SendAsync("ReceiveMessage", "ChatBot", reply);
    }
}
