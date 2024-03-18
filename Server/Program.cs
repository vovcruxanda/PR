using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class Server
{
    static List<Socket> clients = new List<Socket>();

    static void Main(string[] args)
    {
        IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
        int port = 9000;

        Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        serverSocket.Bind(new IPEndPoint(ipAddress, port));
        serverSocket.Listen(10);

        Console.WriteLine("Server started...");

        while (true)
        {
            Socket clientSocket = serverSocket.Accept();
            clients.Add(clientSocket);
            Console.WriteLine($"Client connected: {clientSocket.RemoteEndPoint}");

            Thread clientThread = new Thread(() => HandleClient(clientSocket));
            clientThread.Start();
        }
    }

    static void HandleClient(Socket clientSocket)
    {
        while (true)
        {
            try
            {
                byte[] receivedBuffer = new byte[1024];
                int bytesReceived = clientSocket.Receive(receivedBuffer);
                if (bytesReceived == 0)
                {
                    Console.WriteLine($"Client disconnected: {clientSocket.RemoteEndPoint}");
                    clients.Remove(clientSocket);
                    break;
                }

                string receivedText = Encoding.UTF8.GetString(receivedBuffer, 0, bytesReceived);
                Console.WriteLine($"Received from {clientSocket.RemoteEndPoint}: {receivedText}");

                BroadcastMessage(receivedText, clientSocket);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                clients.Remove(clientSocket);
                break;
            }
        }
    }

    static void BroadcastMessage(string message, Socket sender)
    {
        foreach (Socket client in clients)
        {
            if (client != sender)
            {
                byte[] bytesData = Encoding.UTF8.GetBytes(message);
                client.Send(bytesData);
            }
        }
    }
}

