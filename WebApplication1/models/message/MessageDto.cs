namespace WebApplication1.models.message;

public record MessageDto(string? Id, string Author, string? Email, string? Telegram, string Subject, string Body);