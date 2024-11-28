using Telegram.Bot;

namespace WebApplication1.Services.TgClient;

public class TgClient
{
    public TelegramBotClient Bot;

    public TgClient()
    {
        var token = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN");
        Bot = new TelegramBotClient(string.IsNullOrEmpty(token) ? "" : token);
    }
    
}