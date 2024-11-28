namespace WebApplication1.Services.AdminNotifier;

public interface IAdminNotifier
{
    Task SendMessage(string message);
}