using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class Client
{
    static void Main(string[] args)
    {
        IPAddress serverIp = IPAddress.Parse("127.0.0.1");
        int serverPort = 9000;

        using (Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
        {
            client.Connect(new IPEndPoint(serverIp, serverPort));
            Console.WriteLine("Connected to the server");

            Thread receiveThread = new Thread(() => ReceiveMessages(client));
            receiveThread.Start();

            while (true)
            {
                Console.WriteLine("Enter some text to send to server (type 'exit' to quit): ");
                string text = Console.ReadLine();

                if (text.ToLower() == "exit")
                    break;

                byte[] bytesData = Encoding.UTF8.GetBytes(text);
                client.Send(bytesData);
            }
        }
    }

    static void ReceiveMessages(Socket client)
    {
        while (true)
        {
            try
            {
                byte[] receivedBuffer = new byte[1024];
                int bytesReceived = client.Receive(receivedBuffer);
                if (bytesReceived == 0)
                {
                    Console.WriteLine("Server disconnected");
                    break;
                }

                string senderIP = ((IPEndPoint)client.RemoteEndPoint).Address.ToString();

                string receivedText = Encoding.UTF8.GetString(receivedBuffer, 0, bytesReceived);
                Console.WriteLine($"{senderIP}: {receivedText}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                break;
            }
        }
    }
}