
internal class TcpChatMessenger
{
    private string host;
    private int port;
    private string? name;

    public TcpChatMessenger(string host, int port, string? name)
    {
        this.host = host;
        this.port = port;
        this.name = name;
    }

    internal void Connect()
    {
        throw new NotImplementedException();
    }

    internal void SendMessages()
    {
        throw new NotImplementedException();
    }
}