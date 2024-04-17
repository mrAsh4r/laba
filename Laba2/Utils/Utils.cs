using System.IO.Pipes;
using System.Text.Json;
using System.Text;



namespace Utils;

public struct Message
{
    public Message(string sender, string text)
    {
        Sender = sender;
        Text = text;
    }
    public string Sender { get; set; }
    public string Text { get; set; }
}
    public class SomeUtils
{
    public static async Task ClientSendMsg(NamedPipeClientStream connection, Message message)
    {
        //Message messageToSend = new Message { Sender = senderName, Text = response };

        byte[] responseBytes = GetMessageBytes(message);
        await connection.WriteAsync(responseBytes, 0, responseBytes.Length);
    }
    public static async Task ServerSendMsg(NamedPipeServerStream connection, Message message)
    {
        //Message messageToSend = new Message { Sender = senderName, Text = response };

        byte[] responseBytes = GetMessageBytes(message);
        await connection.WriteAsync(responseBytes, 0, responseBytes.Length);
    }

    public static async Task ClientRecvMsg(NamedPipeClientStream connection)
    {
        // Читаем сообщение от сервера
        while (true)
        {
            byte[] buffer = new byte[4096];
            int bytesRead = await connection.ReadAsync(buffer, 0, buffer.Length);


            Message receivedMessage = GetMessageFromBytes(buffer, bytesRead);

            if (receivedMessage.Text == "endofc") break;

            Console.WriteLine("(Сервер) [" + receivedMessage.Sender + "] >> " + receivedMessage.Text);
        }
    }

    public static async Task ServerRecvMsg(NamedPipeServerStream connection)
    {
        Queue<Message> receivedMessages = new();
        // Читаем сообщение от сервера
        while (true)
        {
            byte[] buffer = new byte[4096];
            int bytesRead = await connection.ReadAsync(buffer, 0, buffer.Length);

            if (bytesRead == 0) continue;
            Message receivedMessage = GetMessageFromBytes(buffer, bytesRead);

            if (receivedMessage.Text == "endofc") break;
            receivedMessages.Enqueue(receivedMessage);
            
        }
        while (receivedMessages.Count > 0)
        {
            var message = receivedMessages.Dequeue();
            if (String.IsNullOrEmpty(message.Text)) continue;
            Console.WriteLine("(Клиент) [" + message.Sender + "] >> " + message.Text);
        }
        
    }

    public static string GetSenderName(string AltName)
    {
        Console.Write("Введите имя отправителя: ");
        string senderName = Console.ReadLine()!;
        return string.IsNullOrEmpty(senderName) ? AltName : senderName;
    }

    public static byte[] GetMessageBytes(Message message)
    {
        string messageJson = JsonSerializer.Serialize(message);
        return Encoding.UTF8.GetBytes(messageJson);
    }

    public static Message GetMessageFromBytes(byte[] buffer, int length)
    {
        string messageJson = Encoding.UTF8.GetString(buffer, 0, length);
        return JsonSerializer.Deserialize<Message>(messageJson);
    }
}
