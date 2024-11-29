using Telegram.Bot;

namespace WebApplication1.Services.TgClient;

public class TgClient
{
    public TelegramBotClient Bot;

    public TgClient(IConfiguration configuration)
    {
        var token = configuration.GetValue<string>("TELEGRAM_BOT_TOKEN");
        if (string.IsNullOrEmpty(token))
        {
            throw new Exception($"telegram token is invalid");
        }

        Bot = new TelegramBotClient(token);
        Console.WriteLine("telegram bot is starting....");
    }
}