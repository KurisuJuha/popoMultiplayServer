using System;
using System.Collections.Concurrent;
using Fleck;

namespace JuhaKurisu.PopoTools.Multiplay.Server;

public class SingleThreadWebSocket : IDisposable
{
    public readonly ConcurrentQueue<IWebSocketConnection> onOpenQueue = new();
    public readonly ConcurrentQueue<(byte[] bytes, IWebSocketConnection socket)> onBinaryQueue = new();
    public readonly ConcurrentQueue<(string message, IWebSocketConnection socket)> onMessageQueue = new();
    public readonly ConcurrentQueue<IWebSocketConnection> onCloseQueue = new();
    public readonly string location;
    private readonly WebSocketServer server;

    public SingleThreadWebSocket(string location)
    {
        this.location = location;
        server = new(this.location);
    }

    public void Dispose()
    {
        server.Dispose();
    }

    public void Start()
    {
        server.Start(socket =>
        {
            socket.OnOpen += () => onOpenQueue.Enqueue(socket);
            socket.OnBinary += bytes => onBinaryQueue.Enqueue((bytes, socket));
            socket.OnMessage += message => onMessageQueue.Enqueue((message, socket));
            socket.OnClose += () => onCloseQueue.Enqueue(socket);
        });
    }
}

