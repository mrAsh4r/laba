using System;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Utils;
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
        string senderName = SomeUtils.GetSenderName("Client");

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
                    
                    await SomeUtils.RecvMsg(client);


                    // Отправляем ответное сообщение серверу
                    Console.Write("Введите ответное сообщение для сервера: ");
                    string response = Console.ReadLine()!;
                    await SomeUtils.ClientSendMsg(client, new (senderName, response ));

                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ошибка: " + ex.Message);
        }
    }

}
