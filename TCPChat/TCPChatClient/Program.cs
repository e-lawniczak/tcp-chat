Console.Write("Enter a name to use: ");
string name = Console.ReadLine();

string host = "localhost";
int port = 6000;
TcpChatMessenger messenger = new TcpChatMessenger(host, port, name);


messenger.Connect();
messenger.SendMessages();
