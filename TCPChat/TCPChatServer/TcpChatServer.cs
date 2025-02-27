// See https://aka.ms/new-console-template for more information

using ChatSharedFunctions;
using ChatSharedUtilities;
using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;

internal class TcpChatServer
{
    public bool Running
    {
        get;
        private set;
    }
    private string _serverName;
    private int _port;
    private TcpListener _listener;
    private int _bufferSize = 2 * 1024;
    private List<TcpClient> _viewers;
    private List<TcpClient> _messengers;
    private Dictionary<TcpClient, string> _names;
    private Queue<string> _messageQueue;
    private ConnectionHandler _connectionHandler;

    public TcpChatServer(string chatName, int port)
    {
        Running = false;
        _serverName = chatName;
        _port = port;
        _viewers = new List<TcpClient>();
        _messengers = new List<TcpClient>();
        _names = new Dictionary<TcpClient, string>();
        _messageQueue = new Queue<string>();


        _connectionHandler = new ConnectionHandler(_bufferSize, _serverName, _names);
        _listener = new TcpListener(IPAddress.Any, _port);
    }


    public void Shutdown()
    {
        Running = false;
        Console.WriteLine("Shutting down server");
    }

    public void InterruptHandler(object sender, ConsoleCancelEventArgs args)
    {
        Shutdown();
        args.Cancel = true;
    }

    internal void Run()
    {
        Console.WriteLine($"Starting server {_serverName} on port {_port}");
        Console.WriteLine("Press Ctrl+C to shutdown server");

        _listener.Start();
        Running = true;

        while (Running)
        {
            if (_listener.Pending())
            {
                CheckNewConnection();
            }

            CheckForDisconnects();
            CheckForNewMessages();
            SendMessages();

            Thread.Sleep(20);
        }

        foreach (TcpClient v in _viewers)
            TCPSharedFunctions.CleanupClient(v);
        foreach (TcpClient m in _messengers)
            TCPSharedFunctions.CleanupClient(m);
        _listener.Stop();

        Console.WriteLine("Server is shut down.");
    }

    private void CheckNewConnection()
    {
        ChatClient newClient = _connectionHandler.HandleNewConnection(_listener.AcceptTcpClient());

        if (newClient.Type.Equals(ClientType.Viewer) && newClient.Client != null)
        {
            _viewers.Add(newClient.Client);
        }
        if (newClient.Type.Equals(ClientType.Messenger) && newClient.Client != null)
        {
            _messengers.Add(newClient.Client);
            _names.Add(newClient.Client, newClient.Name);
            _messageQueue.Enqueue(String.Format($"{newClient.Name} has joined the chat."));

        }
    }

    private void SendMessages()
    {
        foreach (string msg in _messageQueue)
        {
            byte[] msgBuffer = Encoding.UTF8.GetBytes(msg);

            foreach (TcpClient viewer in _viewers)
                viewer.GetStream().Write(msgBuffer, 0, msgBuffer.Length);
        }

        // clear out the queue
        _messageQueue.Clear();
    }

    private void CheckForNewMessages()
    {
        foreach (TcpClient messenger in _messengers)
        {
            int messageLength = messenger.Available;
            if (messageLength > 0)
            {
                byte[] msgBuffer = new byte[messageLength];
                messenger.GetStream().Read(msgBuffer, 0, msgBuffer.Length);

                string msg = $"{_names[messenger]}: {Encoding.UTF8.GetString(msgBuffer)}";
                _messageQueue.Enqueue(msg);
            }
        }
    }

    private void CheckForDisconnects()
    {
        foreach (TcpClient viewer in _viewers.ToArray())
        {
            if (TCPSharedFunctions.IsClientDisconnected(viewer))
            {
                Console.WriteLine($"Viewer {viewer.Client.RemoteEndPoint}");

                _viewers.Remove(viewer);
                TCPSharedFunctions.CleanupClient(viewer);
            }
        }

        foreach (TcpClient messenger in _messengers.ToArray())
        {
            if (TCPSharedFunctions.IsClientDisconnected(messenger))
            {
                Console.WriteLine($"Messenger {messenger.Client.RemoteEndPoint}");

                _messengers.Remove(messenger);
                TCPSharedFunctions.CleanupClient(messenger);
            }
        }
    }




}