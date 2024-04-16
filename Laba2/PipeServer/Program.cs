using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Utils;



class NamedPipeServer
{
    static PriorityQueue<Message, int> messageQueue = new ();
    static List<Message> receivedMessages = new ();


    static async Task Main(string[] args)
    {
        Console.WriteLine("Запуск сервера...");
        string senderName = SomeUtils.GetSenderName("Server");

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
                while (true)
                {
                    // Ожидание нажатия Ctrl+C
                    var task1 = Task.Run(() =>
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
                        if (!DoMagicQueue(senderName)) continue;
                    }

                    // Отправка сообщений из очереди по приоритету
                    while (messageQueue.Count > 0)
                    {
                        var message = messageQueue.Dequeue();
                        await SomeUtils.ServerSendMsg(server, message);
                    }
                    await SomeUtils.ServerSendMsg(server, new Message { Sender = "server", Text = "endofc" });
                    // Сохранение полученных сообщений в файл
                    //SaveReceivedMessagesToFile();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ошибка: " + ex.Message);
        }
    }

    static bool DoMagicQueue(string senderName) {
        Console.Write("Введите сообщение для отправки или нажмите Ctrl+C для завершения: ");
        string text = Console.ReadLine()!;
        if (text == null) return false;

        Console.Write("Введите приоритет (число от 1 до 10): ");
        int priority;

        while (!int.TryParse(Console.ReadLine(), out priority) || priority < 1 || priority > 10)
        {
            Console.WriteLine("Некорректный ввод. Пожалуйста, введите число от 1 до 10.");
            Console.Write("Введите приоритет (число от 1 до 10): ");
        }

        messageQueue.Enqueue(new Message { Sender = senderName, Text = text }, priority);

        return true;
    }
    static void SaveReceivedMessagesToFile()
    {
        string filePath = "received_messages.txt";
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            foreach (var message in receivedMessages)
            {
                writer.WriteLine($"({message.Sender}) {message.Text}");
            }
        }
        Console.WriteLine($"Сохранено в файл: {filePath}");
    }
}
