using System;
using System.Linq;
using System.Collections.Generic;
using JuhaKurisu.PopoTools.ByteSerializer;
using JuhaKurisu.PopoTools.Multiplay.Extentions;
using Fleck;

namespace JuhaKurisu.PopoTools.Multiplay.Server;

internal class Program
{
    static void Main(string[] args) => new Program();

    private Dictionary<Guid, IWebSocketConnection> sockets = new();
    private List<Dictionary<Guid, byte[]>> inputsLogs = new();
    private Dictionary<Guid, byte[]> inputs = new();
    private SingleThreadWebSocket webSocket = new("ws://0.0.0.0:3000");
    private Guid hostPlayerID;

    private Program()
    {
        webSocket.Start();

        new MainLoop(30, MainUpdate).Start();
    }

    private void MainUpdate()
    {
        WebSocketUpdate();
        SendInputs();
        Console.WriteLine(hostPlayerID);
    }

    private void WebSocketUpdate()
    {
        CheckOnClose();
        CheckOnOpen();
        CheckOnBinary();
        CheckOnMessage();
        CheckOnError();
    }

    private void CheckOnOpen()
    {
        while (webSocket.onOpenQueue.TryDequeue(out var socket))
        {
            // hostが空ならセット
            if (!sockets.ContainsKey(hostPlayerID)) hostPlayerID = socket.ConnectionInfo.Id;

            sockets[socket.ConnectionInfo.Id] = socket;
            inputs[socket.ConnectionInfo.Id] = new byte[0];
            Console.WriteLine($"open: {socket.ConnectionInfo.Id}");
            SendInputsLog(socket);
        }
    }

    private void CheckOnClose()
    {
        while (webSocket.onCloseQueue.TryDequeue(out var socket))
        {
            sockets.Remove(socket.ConnectionInfo.Id, out var value1);
            inputs.Remove(socket.ConnectionInfo.Id, out var value2);

            // hostが自分なら新しいhostを選ぶ
            if (hostPlayerID == socket.ConnectionInfo.Id)
            {
                if (sockets.Count == 0) hostPlayerID = Guid.Empty;
                else hostPlayerID = sockets.Keys.First();
            }

            Console.WriteLine($"close: {socket.ConnectionInfo.Id}");
        }
    }

    private void CheckOnMessage()
    {
        while (webSocket.onMessageQueue.TryDequeue(out var data))
        {
            sockets[data.socket.ConnectionInfo.Id] = data.socket;
            Console.WriteLine(string.Join("\n", sockets.Keys.Select(k => $"    {k.ToString()}")));
        }
    }

    private void CheckOnBinary()
    {
        while (webSocket.onBinaryQueue.TryDequeue(out var data))
        {
            Console.WriteLine($"{data.socket.ConnectionInfo.Id}: {string.Join(",", data.bytes)}");
            sockets[data.socket.ConnectionInfo.Id] = data.socket;
            OnBinary(data.bytes, data.socket);
        }
    }

    private void CheckOnError()
    {
        while (webSocket.onErrorQueue.TryDequeue(out var data))
        {
            data.socket.Close();
            Console.WriteLine(data.exception.Message);
        }
    }

    private void BroadCast(byte[] bytes)
    {
        List<Guid> removeSocketIDs = new();
        foreach (var key in sockets.Keys)
        {
            if (sockets[key].IsAvailable) sockets[key].Send(bytes).Wait();
            else removeSocketIDs.Add(key);
        }

        foreach (var removeKey in removeSocketIDs)
            sockets.Remove(removeKey);
    }

    private void OnBinary(byte[] bytes, IWebSocketConnection socket)
    {
        Message message = new DataReader(bytes).ReadMessage();

        switch (message.type)
        {
            case MessageType.Input:
                inputs[socket.ConnectionInfo.Id] = message.data;
                break;
            case MessageType.Close:
                Console.WriteLine("accept close request");
                Console.WriteLine(string.Join(",", new Message(MessageType.Close, new byte[0]).ToBytes()));
                sockets.Remove(socket.ConnectionInfo.Id);
                socket.Close();
                break;
        }
    }

    private void SendInputsLog(IWebSocketConnection socket)
    {
        // データを作る
        DataWriter writer = new DataWriter();

        // logの数を送る
        writer.Append(inputsLogs.Count);

        foreach (var inputs in inputsLogs)
        {
            // inputsbytesをbytesに追加する
            writer.AppendWithLength(CreateInputsBytes(inputs));
        }

        Message message = new(MessageType.InputLog, writer.bytes.ToArray());

        // データを送る
        socket.Send(message.ToBytes());
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
        DataWriter writer = new DataWriter();
        writer.Append(inputs.Count);
        foreach (var id in inputs.Keys)
        {
            writer.Append(id);
            writer.AppendWithLength(inputs[id]);
        }

        return writer.bytes.ToArray();
    }
}