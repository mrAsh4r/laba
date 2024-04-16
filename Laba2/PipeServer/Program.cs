using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

struct Message
{
    public string Sender { get; set; }
    public string Text { get; set; }
    public int Priority { get; set; }
}

class NamedPipeServer
{
    static Queue<Message> messageQueue = new Queue<Message>();
    static List<Message> receivedMessages = new List<Message>();

    static async Task Main(string[] args)
    {
        Console.WriteLine("Запуск сервера...");
        string senderName = GetSenderName();

        try
        {
            using (var server = new NamedPipeServerStream("MyNamedPipe"))
            {
                Console.WriteLine("Ожидание подключения клиента...");
                await server.WaitForConnectionAsync();
                Console.WriteLine("Клиент подключен.");

                // Создаем токен отмены для отслеживания Ctrl+C
                var cancellationTokenSource = new CancellationTokenSource();
                var cancellationToken = cancellationTokenSource.Token;

                // Ожидание нажатия Ctrl+C
                Task.Run(() =>
                {
                    Console.CancelKeyPress += (sender, e) =>
                    {
                        e.Cancel = true;
                        cancellationTokenSource.Cancel();
                    };
                });

                // Ожидание и добавление сообщений в очередь
                while (!cancellationToken.IsCancellationRequested)
                {
                    Console.Write("Введите сообщение для отправки или нажмите Ctrl+C для завершения: ");
                    string text = Console.ReadLine();
                    if (text == null) continue;
                    Console.Write("Введите приоритет (число от 1 до 10): ");
                    int priority;
                    while (!int.TryParse(Console.ReadLine(), out priority) || priority < 1 || priority > 10)
                    {
                        Console.WriteLine("Некорректный ввод. Пожалуйста, введите число от 1 до 10.");
                        Console.Write("Введите приоритет (число от 1 до 10): ");
                    }
                    messageQueue.Enqueue(new Message { Sender = senderName, Text = text, Priority = priority });
                }

                // Отправка сообщений из очереди по приоритету
                while (messageQueue.Count > 0)
                {
                    var message = messageQueue.Dequeue();
                    await SendMsg(server, message.Sender, message.Text, message.Priority);
                }

                // Сохранение полученных сообщений в файл
                SaveReceivedMessagesToFile();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ошибка: " + ex.Message);
        }
    }

    static async Task SendMsg(NamedPipeServerStream connection, string senderName, string response, int priority)
    {
        Message messageToSend = new Message { Sender = senderName, Text = response, Priority = priority };

        byte[] responseBytes = GetMessageBytes(messageToSend);
        await connection.WriteAsync(responseBytes, 0, responseBytes.Length);
    }

    static string GetSenderName()
    {
        Console.Write("Введите имя отправителя: ");
        string senderName = Console.ReadLine();
        return string.IsNullOrEmpty(senderName) ? "Server" : senderName;
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

    static void SaveReceivedMessagesToFile()
    {
        string filePath = "received_messages.txt";
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            foreach (var message in receivedMessages)
            {
                writer.WriteLine($"({message.Sender}) [{message.Priority}] {message.Text}");
            }
        }
        Console.WriteLine($"Сохранено в файл: {filePath}");
    }
}
