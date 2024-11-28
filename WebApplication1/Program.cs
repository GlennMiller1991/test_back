using Constants;
using Dto;
using MessageValidators;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using WebApplication1.Services.AdminNotifier;
using WebApplication1.Services.TgClient;

var portfolioOrigin = new Origin("portfolio", "http://localhost:3000");
var builder = WebApplication.CreateBuilder(args);
int id = 1;

builder.Services.AddCors(opts =>
{
    opts.AddPolicy(name: portfolioOrigin.Key, (policy) =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddSingleton<TgClient, TgClient>();
builder.Services.AddSingleton<IAdminNotifier, TgAdminNotifier>();

var app = builder.Build();

app.UseExceptionHandler("/error");
app.UseCors(portfolioOrigin.Key);


var messages = new ConcurrentDictionary<string, FrontMessage>();

app.MapGet("/error", () => "Sorry! It seems that error is occured");
app.MapGet("/api/v1", Results.NoContent);
app.MapPost("/api/v1/messages", async (MessageDto dto) =>
    {
        id++;
        var message = new FrontMessage(id.ToString(), dto.Author, dto.Email, dto.Subject, dto.Body);

        app.Services.GetService<IAdminNotifier>()
            ?.SendMessage($"{message.Author} said: {message.Body}\n\nEmail: {message.Email}");

        return messages.TryAdd(message.Id, message)
            ? TypedResults.Created($"/api/v1/messages/{dto.Id}", dto)
            : Results.BadRequest();
    })
    .AddEndpointFilter(MessageValidator.ValidateEmptyId)
    .AddEndpointFilter(MessageValidator.ValidateEmail);
app.MapGet("/api/v1/messages", () => messages);

app.MapGet("/api/v1/messages/{id}",
    (string id) =>
    {
        return messages.TryGetValue(id, out var message)
            ? TypedResults.Ok(message)
            : Results.NotFound(new Error("An entry is not found"));
    });

app.MapFallback(() => Results.NotFound());


app.UseStaticFiles();
app.Run();

namespace Dto
{
    public record MessageDto(string? Id, string Author, string Email, string Subject, string Body);
}

namespace Constants
{
    public record Origin(string Key, string HostName);

    public record FrontMessage(string Id, string Author, string Email, string Subject, string Body);

    public record Error(string Message);

    public record Empty();
}

namespace MessageValidators
{
    public static class MessageValidator
    {
        internal static async ValueTask<object?> ValidateEmptyId(
            EndpointFilterInvocationContext context,
            EndpointFilterDelegate next
        )
        {
            var messageDto = context.GetArgument<MessageDto>(0);
            if (!string.IsNullOrEmpty(messageDto.Id))
            {
                return Results.BadRequest(new Error("incorrect type"));
            }

            return await next(context);
        }

        internal static async ValueTask<object?> ValidateEmail(
            EndpointFilterInvocationContext context,
            EndpointFilterDelegate next)
        {
            var messageDto = context.GetArgument<MessageDto>(0);

            if (!Regex.IsMatch(messageDto.Email, @"[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,4}", RegexOptions.IgnoreCase))
            {
                return Results.BadRequest(new Error("incorrect format"));
            }

            return await next(context);
        }
    }
}