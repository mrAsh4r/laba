using System;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;


struct Message
{
    public string Sender { get; set; }
    public string Text { get; set; }
}
class NamedPipeClient
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Запуск клиента...");
        string senderName = GetSenderName();

        try
        {
            // Подключаемся к именованному каналу сервера
            using (var client = new NamedPipeClientStream(".", "MyNamedPipe", PipeDirection.InOut))
            {
                Console.WriteLine("Подключение к серверу...");
                await client.ConnectAsync();
                Console.WriteLine("Подключение установлено.");


                while (true)
                {
                    if (await RecvMsg(client))
                    {
                        continue;
                    };

                    // Отправляем ответное сообщение серверу
                    Console.Write("Введите ответное сообщение для сервера: ");
                    string response = Console.ReadLine();
                    await SendMsg(client, senderName, response);

                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ошибка: " + ex.Message);
        }
    }
    static async Task<bool> RecvMsg(NamedPipeClientStream connection)
    {
        // Читаем сообщение от сервера
        byte[] buffer = new byte[4096];
        int bytesRead = await connection.ReadAsync(buffer, 0, buffer.Length);

        if (bytesRead == 0)
            return true;

        Message receivedMessage = GetMessageFromBytes(buffer, bytesRead);
        //string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
        Console.WriteLine("(Сервер) [" + receivedMessage.Sender + "] >> " + receivedMessage.Text);

        return false;
    }

    static async Task SendMsg(NamedPipeClientStream connection, string senderName, string response)
    {
        Message messageToSend = new Message { Sender = senderName, Text = response };

        byte[] responseBytes = GetMessageBytes(messageToSend);
        // byte[] responseBytes = Encoding.UTF8.GetBytes(response);
        await connection.WriteAsync(responseBytes, 0, responseBytes.Length);
    }

    static string GetSenderName()
    {
        Console.Write("Введите имя отправителя: ");
        string senderName = Console.ReadLine();
        if (string.IsNullOrEmpty(senderName)) senderName = "Client";
        return senderName;
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
