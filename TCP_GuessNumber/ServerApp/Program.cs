using System.Net;
using System.Net.Sockets;
using System.Text.Json;

const int serverPort = 7070;
IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
TcpListener tcpListener = new TcpListener(ipAddress, serverPort);

tcpListener.Start();
Console.WriteLine($"Server started on port {serverPort}...");

while (true)
{
    var tcpClient = await tcpListener.AcceptTcpClientAsync();
    Console.WriteLine("Client connected!");

    _ = Task.Run(async () =>
    {
        var tcpClientStream = tcpClient.GetStream();
        var streamReader = new StreamReader(tcpClientStream);
        var streamWriter = new StreamWriter(tcpClientStream) { AutoFlush = true };

        Random random = new Random();
        int secretNumber = random.Next(1, 101);
        int attemptsLeft = 5;
        bool isGuessed = false;

        while (attemptsLeft > 0 && !isGuessed)
        {
            try
            {
                string? clientMessage = await streamReader.ReadLineAsync();
                if (clientMessage == null) break;

                int guessedNumber;
                if (int.TryParse(clientMessage, out guessedNumber))
                {
                    if (guessedNumber == secretNumber)
                    {
                        await streamWriter.WriteLineAsync("You guessed the number! Win!");
                        isGuessed = true;
                    }
                    else if (guessedNumber > secretNumber)
                    {
                        await streamWriter.WriteLineAsync($"\nThe right number is lower. Attempts left: {--attemptsLeft}");
                    }
                    else
                    {
                        await streamWriter.WriteLineAsync($"\nThe right number is higher. Attempts left: {--attemptsLeft}");
                    }
                }
                else
                {
                    await streamWriter.WriteLineAsync("Wrong insert. Please try again");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                break;
            }
        }

        if (!isGuessed && attemptsLeft == 0)
        {
            await streamWriter.WriteLineAsync($"You lost! Correct number was: {secretNumber}");
        }

        tcpClient.Close();
    });
}
