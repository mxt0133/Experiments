using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerTCPSocket
{
    class Program
    {
        public static ManualResetEvent connectEnd = new ManualResetEvent(false);
        public static TcpListener listener;


        static void Main(string[] args)
        {
            
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);           
                
            // do some work

            IPEndPoint ep = new IPEndPoint(IPAddress.Loopback, 1234);
            listener = new TcpListener(ep);
            listener.Start();

            try
            {
                Console.WriteLine("Accepting clients on port: 1234");
                Console.WriteLine("BeginAcceptSocket called, waiting until current connection ends");

                while (true)
                {
                    listener.BeginAcceptSocket(onClientConnect, listener);
                    connectEnd.WaitOne();
                    connectEnd.Reset();

                    Console.WriteLine("Current client connection terminated...");
                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.Read();
                listener.Stop();
                listener = null;
            }      
        }

   
         

        static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            if(listener != null)
                listener.Stop();
        }

        private static void onClientConnect(IAsyncResult ar)
        {
            const byte ACK = 0x6;
            const byte NACK = 0x15;

            Console.WriteLine("Client Connected...");

            TcpListener listener = (TcpListener)ar.AsyncState;
            TcpClient client = listener.EndAcceptTcpClient(ar);

            StringBuilder sb = new StringBuilder();
            var data = new byte[client.ReceiveBufferSize];

            using (NetworkStream ns = client.GetStream())
            {

                try
                {
                    bool endOfMessageReceived;
                    byte[] clientMessage = new byte[100240];

                    // Read data

                    Console.WriteLine("-----------------------------");
                    Console.WriteLine("Reading client data....");
                    int i = 0;

                    while (true)
                    {
                        endOfMessageReceived = false;


                        while (!endOfMessageReceived)
                        {
                            ns.Read(clientMessage, 0, clientMessage.Length);

                            Console.WriteLine("Checking if message contains end of message char..");
                            endOfMessageReceived = containsEndOfMessageChar(clientMessage);

                            Console.WriteLine("End of message received, " + endOfMessageReceived);

                            Console.WriteLine("Client message segment: " + i);
                            i++;
                        }
                        
                        var reply = " Message ACK: " + DateTime.Now.ToString();
                        Byte[] replyData = System.Text.Encoding.Unicode.GetBytes(reply);

                        if (new Random().Next(2) % 2 == 1)
                        {
                            Console.WriteLine("Sending message ACK");
                            replyData[0] = ACK;

                        }
                        else
                        {
                            Console.WriteLine("Sending message NACK");
                            replyData[0] = NACK;
                        }
                        ns.Write(replyData, 0, replyData.Length);
                        ns.Flush();
                        Console.WriteLine("-----------------------------");
                        endOfMessageReceived = false;
                    }
                    
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception: " + ex.Message);
                    client.Close();
                    connectEnd.Set();
                }
            }
        }

        private static bool containsEndOfMessageChar(byte[] clientMessage)
        {
            byte endOfMessage = 28;

            return System.Array.IndexOf(clientMessage, endOfMessage) >= 0;
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
