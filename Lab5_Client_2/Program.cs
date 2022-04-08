using Models;
using Newtonsoft.Json;
using System.Net;
using System.Net.Sockets;
using System.Text;

const int port = 9111;
const string address = "127.0.0.1";

var ipPoint = new IPEndPoint(IPAddress.Parse(address), port);
var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
socket.Connect(ipPoint);

var data = new byte[1024]; // буфер для ответа
var bytes = socket.Receive(data, data.Length, 0); // количество полученных байт

var id = new Guid(Encoding.Unicode.GetString(data, 0, bytes));
Console.WriteLine($"i am {id}");

Thread sendThread = new(SendThreadMethod);
Thread receiveThread = new(ReceiveThreadMethod);

sendThread.Start();
receiveThread.Start();

while (sendThread.IsAlive || receiveThread.IsAlive)
{
    Thread.Sleep(100);
}

socket.Shutdown(SocketShutdown.Both);
socket.Close();

void SendThreadMethod()
{
    while (true)
    {
        var text = Console.ReadLine();

        if (text == null) continue;
        Message message = new()
        {
            Text = text,
            Time = DateTime.Now,
            ClientId = id
        };

        var data = Encoding.Unicode.GetBytes(JsonConvert.SerializeObject(message));
        socket.Send(data);

        if (text == "exit") break;
        Thread.Sleep(100);
    }
}

void ReceiveThreadMethod()
{
    while (true)
    {
        var data = new byte[1024]; // буфер для ответа
        var builder = new StringBuilder();

        do
        {
            var bytes = socket.Receive(data, data.Length, 0); // количество полученных байт
            builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
        }
        while (socket.Available > 0);

        var message = JsonConvert.DeserializeObject<Message>(builder.ToString());

        if (message == null) continue;
        if (message.Text == "confirm exit") break;

        Console.WriteLine($"[{message.Time}] {message.ClientId}: {message.Text}");

        builder.Clear();
        Thread.Sleep(100);
    }
}