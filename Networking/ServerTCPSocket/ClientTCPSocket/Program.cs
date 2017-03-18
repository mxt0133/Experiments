using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ClientTCPSocket
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Connecting to the server..");
                TcpClient server = new TcpClient("127.0.0.1", 1234); // Create a new connection  
                Console.WriteLine("Connected to server..");
                NetworkStream stream = server.GetStream();

                byte[] messageBytes = null;

                Console.WriteLine("Press enter to begin sending messages");
                Console.Read();

                var continueCondition = true;

                while (continueCondition)
                {
                    messageBytes = System.Text.Encoding.Unicode.GetBytes(DateTime.Now.ToString());
                    Console.WriteLine("-----------------------------");
                    Console.WriteLine("Sending message to server..");
                    stream.Write(messageBytes, 0, messageBytes.Length); // Write the bytes  
                    stream.Flush();
                    Console.WriteLine("Message sent to server..");


                    Console.WriteLine("Waiting for acknoledgment..");
                    byte[] serverMessage = new byte[1024];
                    stream.Read(serverMessage, 0, serverMessage.Length);


                    // Receive the stream of bytes  
                    Console.WriteLine("Acknoledgment received..");
                    Console.WriteLine(cleanMessage(serverMessage));
                    Console.WriteLine("-----------------------------");

                    //stream.Read(messageBytes, 0, messageBytes.Length);


                    char x = Convert.ToChar(Console.Read());

                    if (x == 'x' || x == 'X')
                        continueCondition = false;

                    // always clear buffer if you are going to reuse it
                    for (int i = 0; i < messageBytes.Length; i++)
                        messageBytes[i] = 0;

                }

                // Clean up  
                stream.Dispose();                
                server.Close();


            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception thrown: " + ex.Message);
                Console.Read();
            }

           
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


        private static void onClientConnect(IAsyncResult ar)
        {
            Console.WriteLine("Client Connected");

            Console.Read();
        }
    }
}
