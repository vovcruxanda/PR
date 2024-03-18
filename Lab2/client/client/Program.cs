using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class Program
{
    static void Main(string[] args)
    {
        Console.Write("Enter server IP address: ");
        string serverIP = Console.ReadLine();
        int PORT = 8000;

        Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        Console.Write("Enter your name: ");
        string name = Console.ReadLine();

        clientSocket.SendTo(Encoding.ASCII.GetBytes("JOIN " + name), new IPEndPoint(IPAddress.Parse(serverIP), PORT));

        Thread receiveThread = new Thread(ReceiveMessages);
        receiveThread.Start(clientSocket);

        while (true)
        {
            string user_input = Console.ReadLine();

            if (user_input.StartsWith("/msg "))
            {
                string[] input_parts = user_input.Substring(5).Split(new char[] { ' ' }, 2);
                if (input_parts.Length != 2)
                {
                    Console.WriteLine("Invalid private message format. Use /msg <recipient> <message>");
                    continue;
                }
                string recipient_name = input_parts[0];
                string message_content = input_parts[1];

                clientSocket.SendTo(Encoding.ASCII.GetBytes("PRIVMSG " + recipient_name + " " + message_content), new IPEndPoint(IPAddress.Parse(serverIP), PORT));
            }
            else
            {
                clientSocket.SendTo(Encoding.ASCII.GetBytes("BROADCAST " + user_input), new IPEndPoint(IPAddress.Parse(serverIP), PORT));
            }
        }
    }

    static void ReceiveMessages(object clientSocket)
    {
        Socket udpClient = (Socket)clientSocket;

        while (true)
        {
            byte[] buffer = new byte[1024];
            EndPoint serverEP = new IPEndPoint(IPAddress.Any, 0);
            int bytesRead = udpClient.ReceiveFrom(buffer, ref serverEP);
            Console.WriteLine(Encoding.ASCII.GetString(buffer, 0, bytesRead));
        }
    }
}