using System;
using System.Linq;
using System.Timers;
using System.Collections.Generic;
using Fleck;
namespace JuhaKurisu.PopoTools.Multiplay.Server;

internal class Program
{
    static void Main(string[] args) => new Program();

    WebSocketServer server = new("ws://0.0.0.0:3000");
    Dictionary<Guid, IWebSocketConnection> sockets = new();
    List<Dictionary<Guid, byte[]>> inputsLogs = new();
    Dictionary<Guid, byte[]> inputs = new();
    Timer timer = new(1000 / 30d);

    private Program()
    {
        timer.Elapsed += (s, e) => SendInputs();

        server.Start(socket =>
        {
            socket.OnError += e => Console.WriteLine("error");
            socket.OnOpen += () =>
            {
                sockets[socket.ConnectionInfo.Id] = socket;
                inputs[socket.ConnectionInfo.Id] = new byte[0];
                Console.WriteLine($"open: {socket.ConnectionInfo.Id}");
                SendInputsLog(socket);
            };
            socket.OnClose += () =>
            {
                sockets.Remove(socket.ConnectionInfo.Id, out var value1);
                inputs.Remove(socket.ConnectionInfo.Id, out var value2);
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
        Console.WriteLine(string.Join(",", bytes));
        Console.WriteLine(string.Join("\n", sockets.Keys.Select(k => $"    {k.ToString()}")));
        foreach (var socket in sockets.Values) socket.Send(bytes);
    }

    private void OnBinary(byte[] bytes, IWebSocketConnection socket)
    {
        inputs[socket.ConnectionInfo.Id] = bytes;
    }

    private void SendInputsLog(IWebSocketConnection socket)
    {
        // 途中でデータを送られることを防ぐため、timerを一旦ストップ
        timer.Stop();

        // データを作る
        List<byte> bytes = new();

        // logの数を送る
        bytes.AddRange(BitConverter.GetBytes(inputsLogs.Count));

        foreach (var inputs in inputsLogs)
        {
            byte[] inputsBytes = CreateInputsBytes(inputs);

            // inputsbytesをbytesに追加する
            bytes.AddRange(BitConverter.GetBytes(inputsBytes.Length));
            bytes.AddRange(inputsBytes);
        }

        Message message = new(MessageType.InputLog, bytes.ToArray());

        // データを送る
        Console.WriteLine(string.Join(",", message.ToBytes()));
        socket.Send(message.ToBytes());

        // timer 再開
        timer.Start();
    }

    private void SendInputs()
    {
        // データを送る
        BroadCast(new Message(MessageType.Input, CreateInputsBytes(inputs)).ToBytes());

        // 現在のinputsをコピーしてlogの一番上にためておく
        inputsLogs.Add(new(inputs));

        // 現在のinputsのbytesを全て初期化する
        foreach (var id in inputs.Keys) inputs[id] = new byte[0];
    }

    private byte[] CreateInputsBytes(Dictionary<Guid, byte[]> inputs)
    {
        List<byte> bytes = new List<byte>();
        bytes.AddRange(BitConverter.GetBytes(inputs.Count));
        foreach (var id in inputs.Keys)
        {
            bytes.AddRange(id.ToByteArray());
            bytes.AddRange(BitConverter.GetBytes(inputs[id].Length));
            bytes.AddRange(inputs[id]);
        }

        return bytes.ToArray();
    }
}