using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

class Program
{
    static void Main()
    {
        int PORT = 8000;

        Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        serverSocket.Bind(new IPEndPoint(IPAddress.Any, PORT));

        var clients = new Dictionary<IPEndPoint, string>();

        Console.WriteLine("Server started...");

        while (true)
        {
            EndPoint clientAddress = new IPEndPoint(IPAddress.Any, 0);
            byte[] buffer = new byte[1024];
            int bytesRead = serverSocket.ReceiveFrom(buffer, ref clientAddress);

            if (!clients.ContainsKey((IPEndPoint)clientAddress))
            {
                string name = Encoding.ASCII.GetString(buffer, 0, bytesRead).Split()[1];
                clients[(IPEndPoint)clientAddress] = name;
                Console.WriteLine("Client {0} joined the chat.", name);
            }
            else if (Encoding.ASCII.GetString(buffer, 0, bytesRead).StartsWith("PRIVMSG "))
            {
                string input = Encoding.ASCII.GetString(buffer, 0, bytesRead).Substring(8);
                string[] inputParts = input.Split(new[] { ' ' }, 2);
                if (inputParts.Length != 2)
                {
                    Console.WriteLine("Invalid private message format. Use PRIVMSG <recipient> <message>");
                    continue;
                }
                string recipientName = inputParts[0];
                string messageContent = inputParts[1];
                IPEndPoint recipientAddress = null;
                foreach (var client in clients)
                {
                    if (client.Value == recipientName)
                    {
                        recipientAddress = client.Key;
                        break;
                    }
                }
                if (recipientAddress == null)
                {
                    Console.WriteLine("SERVER: No client with the name {0}.", recipientName);
                }
                else
                {
                    string senderName = clients[(IPEndPoint)clientAddress];
                    byte[] privateMessage = Encoding.ASCII.GetBytes(senderName + " sent in PRIVATE: " + messageContent);
                    serverSocket.SendTo(privateMessage, recipientAddress);
                }
            }
            else if (Encoding.ASCII.GetString(buffer, 0, bytesRead).StartsWith("BROADCAST "))
            {
                string senderName = clients[(IPEndPoint)clientAddress];
                string broadcastMessage = senderName + " BROADCASTED: " + Encoding.ASCII.GetString(buffer, 0, bytesRead).Substring(10);
                byte[] broadcast = Encoding.ASCII.GetBytes(broadcastMessage);
                Console.WriteLine(broadcastMessage);
                foreach (var client in clients)
                {
                    if (!client.Key.Equals(clientAddress))
                    {
                        serverSocket.SendTo(broadcast, client.Key);
                    }
                }
            }
            else
            {
                Console.WriteLine("Invalid message format: {0}", Encoding.ASCII.GetString(buffer, 0, bytesRead));
            }
        }
    }
}
