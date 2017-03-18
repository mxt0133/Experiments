using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerTCPSocket
{
    class Program
    {
        static void Main(string[] args)
        {

            IPEndPoint ep = new IPEndPoint(IPAddress.Loopback, 1234);
            TcpListener listener = new TcpListener(ep);
            listener.Start();

            try
            {
                

                Console.WriteLine("Accepting clients on port: 1234");
                listener.BeginAcceptSocket(onClientConnect, listener);
                Console.WriteLine("BeginAcceptSocket called, continue processing");
                    
                Console.Read();
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.Read();
            }

            listener.Stop();
        }

        private static void onClientConnect(IAsyncResult ar)
        {
            Console.WriteLine("Client Connected");


            TcpListener listener = (TcpListener)ar.AsyncState;
            TcpClient client = listener.EndAcceptTcpClient(ar);

            StringBuilder sb = new StringBuilder();
            var data = new byte[client.ReceiveBufferSize];

            using (NetworkStream ns = client.GetStream())
            {

                try
                {
                    byte[] clientMessage = new byte[1024];
                    while (true)
                    {
                        // Read data     
                        ns.Read(clientMessage, 0, clientMessage.Length);

                        Console.WriteLine("Client Message: " + cleanMessage(clientMessage));


                        // Test reply
                        var reply = "Message ACK: " + DateTime.Now.ToString();
                        Byte[] replyData = System.Text.Encoding.Unicode.GetBytes(reply);
                        ns.Write(replyData, 0, replyData.Length);
                        ns.Flush();

                        // always clear buffer before re-using
                        for (int i = 0; i < clientMessage.Length; i++)
                            clientMessage[i] = 0;
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine("Exception: " + ex.Message);
                    client.Close();
                }
            }

            
            Console.Read();
        }

        private static string cleanMessage(byte[] bytes)
        {
            string message = System.Text.Encoding.Unicode.GetString(bytes);

            string messageToPrint = null;
            foreach (var nullChar in message)
            {
                if (nullChar != '\0')
                {
                    messageToPrint += nullChar;
                }
            }
            return messageToPrint;
        }  
    }
}
