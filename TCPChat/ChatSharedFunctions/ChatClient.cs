// See https://aka.ms/new-console-template for more information

using ChatSharedUtilities;
using System.Net.Sockets;

public class ChatClient
{
    public ClientType Type { get; set; }
    public TcpClient? Client { get; set; }
    public string Name { get; set; }

    public ChatClient()
    {
        Type = ClientType.None;
        Client = null;
        Name = string.Empty;
    }


}

