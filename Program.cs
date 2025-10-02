using System.Net.WebSockets;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseWebSockets(); // Enable WebSocket middleware
app.UseRouting();

// WebSocket endpoint
app.Map("/ws", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        await HandleWebSocketConnection(webSocket);
    }
    else
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsync("Expected a WebSocket request");
    }
});

// Regular HTTP endpoint
app.MapGet("/", () => "WebSocket Server is running. Connect to /ws");

app.Run();

async Task HandleWebSocketConnection(WebSocket webSocket)
{
    var buffer = new byte[1024 * 4];

    try
    {
        // Send welcome message
        var welcomeMessage = "Welcome to WebSocket Server! Type 'exit' to close.";
        var welcomeBytes = Encoding.UTF8.GetBytes(welcomeMessage);
        await webSocket.SendAsync(
            new ArraySegment<byte>(welcomeBytes),
            WebSocketMessageType.Text,
            true,
            CancellationToken.None
        );

        // Listen for messages
        while (webSocket.State == WebSocketState.Open)
        {
            var result = await webSocket.ReceiveAsync(
                new ArraySegment<byte>(buffer),
                CancellationToken.None
            );

            if (result.MessageType == WebSocketMessageType.Text)
            {
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                Console.WriteLine($"Received: {message}");

                if (message.ToLower() == "exit")
                {
                    break;
                }

                // Echo the message back
                var response = $"Echo: {message}";
                var responseBytes = Encoding.UTF8.GetBytes(response);
                await webSocket.SendAsync(
                    new ArraySegment<byte>(responseBytes),
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None
                );
            }
            else if (result.MessageType == WebSocketMessageType.Close)
            {
                await webSocket.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    "Closed by client",
                    CancellationToken.None
                );
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"WebSocket error: {ex.Message}");
    }
    finally
    {
        webSocket?.Dispose();
    }
}