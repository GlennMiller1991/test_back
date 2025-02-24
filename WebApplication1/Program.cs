using Microsoft.EntityFrameworkCore;
using WebApplication1.models;
using WebApplication1.models.message;
using WebApplication1.Services;
using WebApplication1.Services.AdminNotifier;
using WebApplication1.Services.GuestEntryService;
using WebApplication1.Services.TgClient;

var builder = WebApplication.CreateBuilder(args);

var origin = builder.Configuration.GetValue<string>("ORIGIN");
if (string.IsNullOrEmpty(origin)) throw new Exception("Origin configuration is missing");

var portfolioOrigin = new Origin("portfolio", origin);

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
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite("Data Source=temporarily.db"));
builder.Services.AddScoped<GuestEntryService>();

var app = builder.Build();

app.UseExceptionHandler("/api/v1/error");
app.UseCors(portfolioOrigin.Key);

app.MapGet("/api/v1/error", () => "Sorry! It seems that error is occured");
app.MapGet("/api/v1", async (
    HttpContext context,
    GuestEntryService entryService,
    IAdminNotifier adminNotifier
) =>
{
    var ip = context.Request.Headers["X-Forwarded-For"];
    if (string.IsNullOrEmpty(ip))
    {
        ip = context.Request.Headers["X-Real-IP"];
    }

    if (!string.IsNullOrEmpty(ip))
    {
        var guestEntry = await entryService.GetGuestEntry(ip);

        if (guestEntry is null)
        {
            await entryService.CreateGuestEntry(ip);
            await entryService.GetGuestEntry(ip);
            adminNotifier.SendMessage($"new entry, {ip}");
        }
        else
        {
            await entryService.UpdateGuestEntry(ip, DateTime.Now);
            adminNotifier.SendMessage($"update entry, {ip}");
        }
    }
    else
    {
        adminNotifier.SendMessage("Someone come");
    }

    return Results.NoContent();
});

app.MapPost("/api/v1/messages", (MessageDto dto) =>
    {
        string backRoute = !string.IsNullOrEmpty(dto.Telegram) ? dto.Telegram : 
            !string.IsNullOrEmpty(dto.Email) ? dto.Email : "something went wrong";
        app.Services.GetService<IAdminNotifier>()
            ?.SendMessage($"{dto.Author} said: {dto.Body}\n\backRoute: {backRoute}");

        return TypedResults.Created("", dto);
    })
    .AddEndpointFilter(MessageValidator.ValidateEmptyId)
    .AddEndpointFilter(MessageValidator.ValidateEmail)
    .AddEndpointFilter(MessageValidator.ValidateTelegram);

app.MapFallback(() => Results.NotFound());


app.UseStaticFiles();
app.Run();