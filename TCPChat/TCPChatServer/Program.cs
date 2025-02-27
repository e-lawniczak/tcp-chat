// See https://aka.ms/new-console-template for more information

using System;

string SERVER_NAME = "TCP SERVER";
int PORT = 6000;

TcpChatServer tcpChatServer = new TcpChatServer(SERVER_NAME, PORT);

Console.CancelKeyPress += tcpChatServer.InterruptHandler;

tcpChatServer.Run();
