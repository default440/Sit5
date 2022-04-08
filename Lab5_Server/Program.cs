using Models;
using Newtonsoft.Json;
using System.Net;
using System.Net.Sockets;
using System.Text;

var client1 = new Client
{
    Id = Guid.NewGuid(),
    Port = 9110
};
var client2 = new Client
{
    Id = Guid.NewGuid(),
    Port = 9111
};

client1.EndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), client1.Port);
client2.EndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), client2.Port);

Thread client1Thread = new(ClientThreadMethod);
Thread client2Thread = new(ClientThreadMethod);

client1Thread.Start(client1);
client2Thread.Start(client2);

void ClientThreadMethod(object? obj)
{
    var client = (Client)obj!;
    client = AwaitClient(client);

    client.Handler.Send(Encoding.Unicode.GetBytes(client.Id.ToString()));
    Console.WriteLine($"{client.Id} connected");

    var builder = new StringBuilder();

    while (true)
    {
        var data = new byte[1024];

        do
        {
            var bytes = client.Handler.Receive(data);
            builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
        } while (client.Handler.Available > 0);

        Console.WriteLine(builder.ToString());

        var message = JsonConvert.DeserializeObject<Message>(builder.ToString());

        if (message == null) continue;
        if (message.Text == "exit")
        {
            client.Handler.Send(Encoding.Unicode.GetBytes(JsonConvert.SerializeObject(new Message
            {
                Text = "confirm exit",
                Time = DateTime.Now,
                ClientId = client.Id
            })));
            ResendMessage(new Message
            {
                Text = $"[SYSTEM] Client {client.Id} leave",
                Time = DateTime.Now,
                ClientId = client.Id
            });
            break;
        }
        ResendMessage(message);

        builder.Clear();
        Thread.Sleep(100);
    }

    client.Handler.Shutdown(SocketShutdown.Both);
    client.Handler.Close();
}

Client AwaitClient(Client client)
{
    client.Listen.Bind(client.EndPoint);
    client.Listen.Listen(10);

    client.Handler = client.Listen.Accept();

    return client;
}

void ResendMessage(Message message)
{
    var data = Encoding.Unicode.GetBytes(JsonConvert.SerializeObject(message));

    try
    {
        if (message.ClientId == client1.Id)
        {
            client2.Handler.Send(data);
        }
        else
        {
            client1.Handler.Send(data);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
    }

}
