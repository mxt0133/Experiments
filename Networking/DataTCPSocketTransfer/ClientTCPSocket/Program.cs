using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
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

                try
                {
                    using (NetworkStream stream = server.GetStream())
                    {

                        Console.WriteLine("Press enter to begin sending messages");

                        int input = Console.Read();
                        char y = Convert.ToChar(input);
                        Console.WriteLine("Input: " + input + ", '" + y + "'");

                        //ContinousSendMessage(stream);

                        SendLagreMessage(stream);

                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception thrown: " + ex.Message);
                }

                server.Close();

            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception thrown: " + ex.Message);                
            }

            Console.Read();
        }

        private static void ContinousSendMessage(NetworkStream stream)
        {
            byte[] messageBytes = null;
            int input;
            char y;
            var continueCondition = true;

            while (continueCondition)
            {
                messageBytes = System.Text.Encoding.Unicode.GetBytes(DateTime.Now.ToString());
                messageBytes[messageBytes.Length - 1] = 28;
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


                input = Convert.ToChar(Console.Read());
                char x = Convert.ToChar(input);
                //Console.WriteLine("Input: " + input + ", '" + x + "'");


                if (x == 'x' || x == 'X')
                    continueCondition = false;

            }

        }

        private static void SendLagreMessage(NetworkStream stream)
        {
            byte[] messageBytes = null;
            int input;
            char y;
            var continueCondition = true;

            const byte ACK = 0x6;
            const byte NACK = 0x15;
 


            while (continueCondition)
            {
                Console.WriteLine("-----------------------------");
                Console.WriteLine("Ready to send...");
                Console.Read();
                Console.WriteLine("Sending large 5 segment message to server..");

                messageBytes = new byte[20480];
                Random random = new Random();

                for (int i = 0; i < 5; i++ )
                {
                    
                    createNextBytes(messageBytes); 

                    stream.Write(messageBytes, 0, messageBytes.Length); // Write the bytes  
                    stream.Flush();
                    Console.WriteLine(string.Format("Segment {0} message sent to server..", i));

                    Thread.Sleep(random.Next(500, 2000));
                }

                byte[] endOfMessage = new byte[1];
                endOfMessage[0] = 28;
                
                Console.WriteLine("Sending end of message delimiter...");
                stream.Write(endOfMessage, 0, endOfMessage.Length); // Write the bytes  
                stream.Flush();

                Console.WriteLine("Waiting for acknoledgment..");
                byte[] serverMessage = new byte[1024];
                stream.Read(serverMessage, 0, serverMessage.Length);


                
                byte [] messageBody = serverMessage.Skip(2).ToArray();
                // Receive the stream of bytes  
                Console.WriteLine("Server message received..");

                if (serverMessage[0] == ACK)
                {
                    Console.WriteLine("ACK message received..");
                }
                else if (serverMessage[0] == NACK)
                {
                    Console.WriteLine("NACK message received..");
                }
                else
                {
                    Console.WriteLine("Server message first bye is not ACK or NAC.");
                }

                
                Console.WriteLine(cleanMessage(messageBody));
                Console.WriteLine("-----------------------------");

                input = Convert.ToChar(Console.Read());
                char x = Convert.ToChar(input);
                Console.WriteLine("Input: " + input + ", '" + x + "'");

                if (x == 'x' || x == 'X')
                    continueCondition = false;
            }
        }

        private static void createNextBytes(byte[] messageBytes)
        {
            Random rand = new Random();

            for (int i = 0; i < messageBytes.Length; i++)
                messageBytes[i] = Convert.ToByte(rand.Next(30, 255));
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
