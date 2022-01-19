using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
namespace TimeServer
{
    class Program
    {
        private static byte[] _buffer = new byte[1024];
        private static List<Socket> _clientSockets = new List<Socket>(); 
        private static Socket _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);   
        static void Main(string[] args)
        {
            Console.Title = "Server";
            setupServer();
            Console.ReadLine();
        }

        private static void setupServer()
        {
            Console.WriteLine("Setting up server.....");
            _serverSocket.Bind(new IPEndPoint(IPAddress.Any,100));
            _serverSocket.Listen(1);
            _serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
        }
        private static void AcceptCallback(IAsyncResult AR)
        {
            Socket socket = _serverSocket.EndAccept(AR);
            _clientSockets.Add(socket);
            Console.WriteLine("Client Connected");
            socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(receiveCallback), socket);
            _serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);    
        }
        private static void receiveCallback(IAsyncResult AR)
        {
            Socket socket = (Socket)AR.AsyncState;
            int received = socket.EndReceive(AR);
            byte[] databuf = new byte[received];
            Array.Copy(_buffer, databuf, received);
            string text = Encoding.ASCII.GetString(databuf);
            Console.WriteLine("Text Receieved: "+text);

            string responce = string.Empty;
            
            if(text.ToLower()!="get time")
            {
                responce = text;
            }
            else
            {
                responce = DateTime.Now.ToLongTimeString();
               
            }
            byte[] data = Encoding.ASCII.GetBytes(responce);
            socket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(SendCallback), socket);
            socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(receiveCallback), socket);
        }
        private static void SendCallback(IAsyncResult AR)
        {
            Socket socket = (Socket)AR.AsyncState;
            socket.EndSend(AR);
        }
    }
}
