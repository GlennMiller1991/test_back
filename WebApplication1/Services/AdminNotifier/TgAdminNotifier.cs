using Telegram.Bot;

namespace WebApplication1.Services.AdminNotifier;

public class TgAdminNotifier : IAdminNotifier
{
    private TgClient.TgClient _tgClient;

    public TgAdminNotifier(TgClient.TgClient tgClient)
    {
        _tgClient = tgClient;
    }


    public Task SendMessage(string message)
    {
        var chatId = Environment.GetEnvironmentVariable("ADMIN_TG_CHAT_ID");
        return _tgClient.Bot.GetChat(string.IsNullOrEmpty(chatId) ? "" : chatId)
            .ContinueWith((chatTask) => { _tgClient.Bot.SendMessage(chatTask.Result, message); });
    }
}