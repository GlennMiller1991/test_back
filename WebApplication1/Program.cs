using System.Collections.Concurrent;
using WebApplication1.models;
using WebApplication1.models.message;
using WebApplication1.Services;
using WebApplication1.Services.AdminNotifier;
using WebApplication1.Services.TgClient;

var builder = WebApplication.CreateBuilder(args);

var origin = builder.Configuration.GetValue<string>("ORIGIN");
if (string.IsNullOrEmpty(origin)) throw new Exception("Origin configuration is missing");

var portfolioOrigin = new Origin("portfolio", origin);

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