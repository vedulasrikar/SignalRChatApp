using HubServer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();

builder.Services.AddCors(options => 
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("https://localhost:7285").AllowAnyHeader().AllowAnyMethod().AllowCredentials();
    });
});

var app = builder.Build();

app.UseRouting();
app.UseCors();

app.MapHub<ChatHub>("/chathub");

//// Provide a simple root page so browsing to the app root does not return 404
app.MapGet("/", () => Results.Content("<html><body><h1>SignalR Hub running</h1><p>Open the Blazor client: <a href=\"https://localhost:7285\">https://localhost:7285</a></p></body></html>", "text/html"));

app.Run();
