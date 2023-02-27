using System;
using System.Linq;
using System.Collections.Generic;
using System.Timers;
using Fleck;

namespace popoMultiplayServer;

internal class Program
{
    static void Main(string[] args) => new Program();

    WebSocketServer server = new WebSocketServer("ws://0.0.0.0:3000");
    Dictionary<Guid, IWebSocketConnection> sockets = new();
    Dictionary<Guid, byte[]> inputs = new();
    Timer timer;

    private Program()
    {
        timer = new Timer(1000 / 30d);
        timer.Elapsed += (s, e) => SendInputs();

        server.Start(socket =>
        {
            socket.OnError += e => Console.WriteLine("error");
            socket.OnOpen += () =>
            {
                sockets[socket.ConnectionInfo.Id] = socket;
                inputs[socket.ConnectionInfo.Id] = new byte[1];
                Console.WriteLine($"open: {socket.ConnectionInfo.Id}");
            };
            socket.OnClose += () =>
            {
                sockets.Remove(socket.ConnectionInfo.Id);
                inputs.Remove(socket.ConnectionInfo.Id);
                Console.WriteLine($"close: {socket.ConnectionInfo.Id}");
            };
            socket.OnMessage += message =>
            {
                sockets[socket.ConnectionInfo.Id] = socket;
                Console.WriteLine(string.Join("\n", sockets.Keys.Select(k => $"    {k.ToString()}")));
            };
            socket.OnBinary += bytes =>
            {
                Console.WriteLine($"{socket.ConnectionInfo.Id}: {string.Join(",", bytes)}");
                sockets[socket.ConnectionInfo.Id] = socket;
                OnBinary(bytes, socket);
            };
        });

        timer.Start();

        while (true) ;
    }

    private void BroadCast(byte[] bytes)
    {
        Console.Clear();
        Console.WriteLine(string.Join(",", bytes));
        Console.WriteLine(string.Join("\n", sockets.Keys.Select(k => $"    {k.ToString()}")));
        foreach (var socket in sockets.Values) socket.Send(bytes);
    }

    private void OnBinary(byte[] bytes, IWebSocketConnection socket)
    {
        inputs[socket.ConnectionInfo.Id] = bytes;
    }

    private void SendInputs()
    {
        List<byte> bytes = new List<byte>();

        bytes.AddRange(BitConverter.GetBytes(inputs.Count));

        foreach (var id in inputs.Keys)
        {
            bytes.AddRange(id.ToByteArray());
            bytes.AddRange(BitConverter.GetBytes(inputs[id].Length));
            bytes.AddRange(inputs[id]);
        }

        BroadCast(bytes.ToArray());
    }
}