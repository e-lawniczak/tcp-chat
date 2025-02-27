// See https://aka.ms/new-console-template for more information

using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Xml.Linq;
using ChatSharedUtilities;

internal class ConnectionHandler
{
    private bool _connected;
    private int _bufferSize;
    private readonly string _serverName;
    private readonly Dictionary<TcpClient, string> _names;

    public ConnectionHandler(int bufferSize, string serverName, Dictionary<TcpClient, string> names)
    {
        _connected = false;
        _bufferSize = bufferSize;
        _serverName = serverName;
        _names = names;
    }

    internal ChatClient HandleNewConnection(TcpClient tcpClient)
    {
        TcpClient newClient = tcpClient;
        newClient.SendBufferSize = _bufferSize;
        newClient.ReceiveBufferSize = _bufferSize;
        NetworkStream netStream = newClient.GetStream();
        EndPoint? endPoint = newClient.Client.RemoteEndPoint;
        Console.WriteLine($"New connection from {endPoint}");

        byte[] msgbuffer = new byte[_bufferSize];
        int bytesRead = netStream.Read(msgbuffer, 0, msgbuffer.Length);

        if (bytesRead > 0)
        {


            string msg = Encoding.UTF8.GetString(msgbuffer, 0, bytesRead);

            if (msg.Equals("viewer"))
            {
                return HandleViewer(msg, msgbuffer, endPoint, newClient, netStream);
            }
            else if (msg.StartsWith("name:"))
            {
                return HandleMessenger(msg, endPoint, newClient);
            }
            else
            {
                Console.WriteLine("Wasn't able to identify {0} as a Viewer or Messenger.", endPoint);
                TCPSharedFunctions.CleanupClient(newClient);
                return new ChatClient
                {
                    Type = ClientType.None,
                    Client = null,
                    Name = string.Empty
                };
            }
        }
        if (!_connected)
            newClient.Close();

        Console.WriteLine($"No bytes read from {endPoint}.");
        TCPSharedFunctions.CleanupClient(newClient);
        return new ChatClient
        {
            Type = ClientType.None,
            Client = null,
            Name = string.Empty
        };
    }

    private ChatClient HandleViewer(string msg, byte[] msgbuffer, EndPoint? endPoint, TcpClient newClient, NetworkStream netStream)
    {
        _connected = true;
        Console.WriteLine("{0} is a Viewer.", endPoint);

        msg = String.Format($"Welcome to the {_serverName} Chat Server!");
        msgbuffer = Encoding.UTF8.GetBytes(msg);
        netStream.Write(msgbuffer, 0, msgbuffer.Length);
        return new ChatClient
        {
            Type = ClientType.Viewer,
            Client = newClient
        };
    }

    private ChatClient HandleMessenger(string msg, EndPoint? endPoint, TcpClient newClient)
    {
        string name = msg.Substring(msg.IndexOf(':') + 1);

        if ((name != string.Empty) && (!_names.ContainsValue(name)))
        {
            _connected = true;

            Console.WriteLine($"{endPoint} is a Messenger with the name {name}.");
        }
        return new ChatClient
        {
            Type = ClientType.Messenger,
            Client = newClient,
            Name = name
        };
    }
}