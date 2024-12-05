using WebApplication1.models;
using WebApplication1.models.message;
using WebApplication1.Services;
using WebApplication1.Services.AdminNotifier;
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

var app = builder.Build();

app.UseExceptionHandler("/error");
app.UseCors(portfolioOrigin.Key);

app.MapGet("/error", () => "Sorry! It seems that error is occured");
app.MapGet("/api/v1", Results.NoContent);
app.MapPost("/api/v1/messages", async (MessageDto dto) =>
    {
        app.Services.GetService<IAdminNotifier>()
            ?.SendMessage($"{dto.Author} said: {dto.Body}\n\nEmail: {dto.Email}");

        return TypedResults.Created("", dto);
    })
    .AddEndpointFilter(MessageValidator.ValidateEmptyId)
    .AddEndpointFilter(MessageValidator.ValidateEmail);

app.MapFallback(() => Results.NotFound());


app.UseStaticFiles();
app.Run();