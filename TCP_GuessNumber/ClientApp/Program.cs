using System.Net;
using System.Net.Sockets;

const int serverPort = 7070;
IPAddress ipAddress = IPAddress.Parse("127.0.0.1");

using TcpClient tcpClient = new TcpClient();
await tcpClient.ConnectAsync(ipAddress, serverPort);

var tcpClientStream = tcpClient.GetStream();
var streamReader = new StreamReader(tcpClientStream);
var streamWriter = new StreamWriter(tcpClientStream) { AutoFlush = true }; // dla togo chtobi ne pisat vezde FlushAsync()

Console.WriteLine("Connection established. Try to guess the number from 1 - 100. You have 5 attempts!");

_ = Task.Run(async () =>
{
    while (true)
    {
        string? serverMessage = await streamReader.ReadLineAsync();
        if (serverMessage != null)
        {
            Console.WriteLine(serverMessage);
            if (serverMessage.Contains("Win") || serverMessage.Contains("lost"))
            {
                tcpClient.Close();
                Environment.Exit(0); // close program with exit code 0
            }
        }
    }
});

while (true)
{
    Thread.Sleep(100);
    Console.Write("Enter the number: ");
    string guess = Console.ReadLine();
    await streamWriter.WriteLineAsync(guess);
}
