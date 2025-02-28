// Setup the Viewer
string host = "localhost";//args[0].Trim();
int port = 6000;//int.Parse(args[1].Trim());
TcpChatListener viewer = new TcpChatListener(host, port);

// Add a handler for a Ctrl-C press
Console.CancelKeyPress += viewer.InterruptHandler;

// Try to connect & view messages
viewer.Connect();
viewer.ListenForMessages();