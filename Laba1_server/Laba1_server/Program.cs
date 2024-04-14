using System;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

struct Message
{
    public string Sender { get; set; }
    public string Text { get; set; }
}

class NamedPipeServer
{
    static async Task Main(string[] args)
    {
        string senderName = GetSenderName();

        try
        {
            // Создаем именованный канал
            using (var server = new NamedPipeServerStream("MyNamedPipe"))
            {
                Console.WriteLine("Ожидание подключения клиента...");
                await server.WaitForConnectionAsync();
                Console.WriteLine("Клиент подключен.");

                while (true)
                {
                    // Отправляем сообщение клиенту
                    Console.Write("Введите сообщение для клиента: ");
                    string text = Console.ReadLine();
                    SendMsg(server, senderName, text);


                    if (await RecvMsg(server))
                    {
                        continue;
                    };
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ошибка: " + ex.Message);
        }
    }

    static async Task<bool> RecvMsg(NamedPipeServerStream con)
    {
        // Читаем сообщение от сервера
        byte[] buffer = new byte[4096];
        int bytesRead = await con.ReadAsync(buffer, 0, buffer.Length);

        if (bytesRead == 0)
            return true;

        Message receivedMessage = GetMessageFromBytes(buffer, bytesRead);
        //string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
        Console.WriteLine("(Клиент) [" + receivedMessage.Sender + "] >> " + receivedMessage.Text);

        return false;
    }

    static async void SendMsg(NamedPipeServerStream con, string senderName, string response)
    {
        Message messageToSend = new Message { Sender = senderName, Text = response };

        byte[] responseBytes = GetMessageBytes(messageToSend);
        // byte[] responseBytes = Encoding.UTF8.GetBytes(response);
        await con.WriteAsync(responseBytes, 0, responseBytes.Length);
    }

    static string GetSenderName()
    {
        Console.Write("Введите имя отправителя: ");
        return Console.ReadLine();
    }

    static byte[] GetMessageBytes(Message message)
    {
        string messageJson = JsonSerializer.Serialize(message);
        return Encoding.UTF8.GetBytes(messageJson);
    }

    static Message GetMessageFromBytes(byte[] buffer, int length)
    {
        string messageJson = Encoding.UTF8.GetString(buffer, 0, length);
        return JsonSerializer.Deserialize<Message>(messageJson);
    }
}
