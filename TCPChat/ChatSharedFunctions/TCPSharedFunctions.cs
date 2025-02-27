using System;
using System.Net.Sockets;

namespace ChatSharedUtilities
{
    public class TCPSharedFunctions
    {
        private TcpClient _client;
        public TCPSharedFunctions(TcpClient aClient)
        {
            _client = aClient;
        }
        public bool IsClientDisconnected()
        {
            try
            {
                Socket s = _client.Client;
                return s.Poll(10 * 1000, SelectMode.SelectRead) && (s.Available == 0);
            }
            catch (SocketException)
            {
                return true;
            }
        }
        public static bool IsClientDisconnected(TcpClient aClinet)
        {
            try
            {
                Socket s = aClinet.Client;
                return s.Poll(10 * 1000, SelectMode.SelectRead) && (s.Available == 0);
            }
            catch (SocketException)
            {
                return true;
            }
        }
        public static void CleanupClient(TcpClient client)
        {
            client.GetStream().Close();
            client.Close();
        }

    }
}
  

