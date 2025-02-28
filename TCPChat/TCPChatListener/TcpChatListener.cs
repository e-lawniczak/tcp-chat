// Setup the Viewer

using System.Net;
using System.Net.Sockets;
using System.Text;

internal class TcpChatListener
{
    private string Host;
    private int Port;
    private TcpClient Client;
    public bool Running { get; private set; }
    private bool DisconnectRequest = false;
    public readonly int BufferSize = 2 * 1024;
    private NetworkStream? MsgStream = null;

    public TcpChatListener(string host, int port)
    {
        this.Host = host;
        this.Port = port;

        Client = new TcpClient();
        Client.SendBufferSize = BufferSize;
        Client.ReceiveBufferSize = BufferSize;
        Running = false;
    }

    internal void InterruptHandler(object? sender, ConsoleCancelEventArgs e)
    {
        Disconnect();
        e.Cancel = true;
    }

    private void Disconnect()
    {
        Running = false;
        DisconnectRequest = true;
        Console.WriteLine("Disconnecting from the chat...");
    }

    internal void Connect()
    {
        Client.Connect(Host, Port);
        EndPoint? endPoint = Client.Client.RemoteEndPoint;

        if (Client.Connected)
        {
            Console.WriteLine($"Connected to the server at {endPoint}");

            MsgStream = Client.GetStream();
            MsgStream.ReadTimeout = 30;
            byte[] msgBuffer = Encoding.UTF8.GetBytes("viewer");
            MsgStream.Write(msgBuffer, 0, msgBuffer.Length);

            if (!ChatSharedUtilities.TCPSharedFunctions.IsClientDisconnected(Client))
            {
                Running = true;
                Console.WriteLine("Press Ctrl+C to exit the Viewer any time.");
            }
            else
            {
                CleanUpNetworkResources();
                Console.WriteLine($"Wasn't able to connect to server at {endPoint}.");
            }
        }
    }

    private void CleanUpNetworkResources()
    {
        MsgStream?.Close();
        MsgStream = null;
        Client.Close();
    }

    internal void ListenForMessages()
    {
        bool wasRunning = Running;

        while (Running)
        {
            int messageLength = Client.Available;
            if (messageLength > 0)
            {
                byte[] msgBuffer = new byte[messageLength];

                MsgStream.Read(msgBuffer, 0, messageLength);

                string msg = Encoding.UTF8.GetString(msgBuffer);
                Console.WriteLine(msg);
            }

            Thread.Sleep(10);

            if (ChatSharedUtilities.TCPSharedFunctions.IsClientDisconnected(Client))
            {
                Running = false;
                Console.WriteLine("Server has disconnected from us.\n:[");
            }

            Running &= !DisconnectRequest;
        }

        CleanUpNetworkResources();
        if (wasRunning)
            Console.WriteLine("Disconnected.");
    }
}
