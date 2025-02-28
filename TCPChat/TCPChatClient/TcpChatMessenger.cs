
using System.Net;
using System.Net.Sockets;
using System.Text;

internal class TcpChatMessenger
{
    private string Host;
    private int Port;
    private string? Name;
    private TcpClient Client;
    private int BufferSize = 2 * 1024;
    public bool Running { get; private set; }

    private NetworkStream? MsgStream = null;

    public TcpChatMessenger(string host, int port, string? name)
    {
        Host = host;
        Port = port;
        Name = name;
        Client = new TcpClient();
        Client.SendBufferSize = BufferSize;
        Client.ReceiveBufferSize = BufferSize;
        Running = false;


    }


    internal void Connect()
    {
        Client.Connect(Host, Port);
        EndPoint? endPoint = Client.Client.RemoteEndPoint;

        if (Client.Connected)
        {
            Console.WriteLine($"Connected to the server at {endPoint}.");

            MsgStream = Client.GetStream();
            byte[] msgBuffer = Encoding.UTF8.GetBytes($"name:{Name}");
            MsgStream.Write(msgBuffer, 0, msgBuffer.Length);

            if (!ChatSharedUtilities.TCPSharedFunctions.IsClientDisconnected(Client))
            {
                Running = true;
            }
            else
            {
                CleanupNetworkResources();
                Console.WriteLine($"The server rejected us; Name {Name} is probably in use.");
            }
        }
        else
        {
            CleanupNetworkResources();
            Console.WriteLine($"Wasn't able to connect to server at {endPoint}.");

        }
    }

    internal void SendMessages()
    {
        bool wasRunning = false;

        while (Running && MsgStream != null)
        {
            Console.Write($"{Name}>");
            string msg = Console.ReadLine();

            if (msg.ToLower().Equals("quit") || msg.ToLower().Equals("exit"))
            {
                Console.WriteLine("Disconnecting...");
                Running = false;
            }
            else if (msg != string.Empty)
            {
                // Send the message
                byte[] msgBuffer = Encoding.UTF8.GetBytes(msg);
                MsgStream.Write(msgBuffer, 0, msgBuffer.Length);   // Blocks
            }
            Thread.Sleep(10);

            if (ChatSharedUtilities.TCPSharedFunctions.IsClientDisconnected(Client))
            {
                Running = false;
                Console.WriteLine("Server has disconnected from us.\n:[");
            }
        }
        CleanupNetworkResources();
        if (wasRunning)
        {
            Console.WriteLine("Disconnected.");
        }
    }
    private void CleanupNetworkResources()
    {
        MsgStream?.Close();
        MsgStream = null;
        Client.Close();
    }
}