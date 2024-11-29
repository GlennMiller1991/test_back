using Telegram.Bot;

namespace WebApplication1.Services.AdminNotifier;

public class TgAdminNotifier : IAdminNotifier
{
    private TgClient.TgClient _tgClient;
    private IConfiguration _configuration;

    public TgAdminNotifier(TgClient.TgClient tgClient, IConfiguration configuration)
    {
        _tgClient = tgClient;
        _configuration = configuration;
    }


    public Task SendMessage(string message)
    {
        var chatId = _configuration.GetValue<string>("ADMIN_TELEGRAM_CHAT_ID");
        if (string.IsNullOrWhiteSpace(chatId)) return Task.CompletedTask;

        return _tgClient.Bot.GetChat(chatId)
            .ContinueWith((chatTask) => { _tgClient.Bot.SendMessage(chatTask.Result, message); });
    }
}