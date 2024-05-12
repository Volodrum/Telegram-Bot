using AspBot;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHostedService<BotService>();
var app = builder.Build();

app.UseHttpsRedirection();

app.MapGet("/bot", () => "Bot is working");


app.Run();

