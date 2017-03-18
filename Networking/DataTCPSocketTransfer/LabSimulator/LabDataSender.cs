using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LabSimulator
{
    public class LabDataSender : IDisposable
    {
        public TcpClient deaLabListener;  
        const string messageFileNameAndPath = @"c:\Temp\Data\CPResultLabMessage.dat";
        public string labMessage;
        int numberOfRecordTypes, numberOfMessages;

        const byte ACK = 0x6;
        const byte NACK = 0x15;
        const byte Control28 = 0x1C;
        const byte Control11 = 0x0B;
        
        public LabDataSender()
        {
            // This will throw an exception if the connection cannot be made
            deaLabListener = new TcpClient("127.0.0.1", 1234); // Create a new connection

            LoadMessage();
        }

        private void LoadMessage()
        {
            try
            {   // Open the text file using a stream reader.
                using (StreamReader sr = new StreamReader(messageFileNameAndPath))
                {
                    // Read the stream to a string, and write the string to the console.
                    labMessage = sr.ReadToEnd();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
                throw e;
            }
        }


        public void Dispose()
        {
            throw new NotImplementedException();
        }

        internal void Start()
        {
            while (true)
            {

                Console.Write("Enter how many records types to include in message: ");

                string input = Console.ReadLine();

                int number;

                if (!Int32.TryParse(input, out number))
                {
                    Console.WriteLine("Invalid number entered.");
                    continue;
                }

                numberOfRecordTypes = number;

                Console.Write("Ready to send data, enter how many records to send: ");

                input = Console.ReadLine();

                if(!Int32.TryParse(input, out number))
                {
                    Console.WriteLine("Invalid number entered.");
                    continue;
                }

                numberOfMessages = number;
    
                SendMessages();

                Console.WriteLine("--------------------------------");

            }

        }

        private void SendMessages()
        {
            LabMessageGenerator messageGenerator = new LabMessageGenerator(Control28, Control11,
                new List<string>() { "001", "002", "003" });

            string header = "001,001," + String.Format("{0:yyyyMMddHHmmssfff}", DateTime.Now);


            byte[] message = messageGenerator.GenerateCompleteMessage(labMessage, header, numberOfRecordTypes, numberOfMessages);

            NetworkStream stream = deaLabListener.GetStream();
                            
            Console.WriteLine("-----------------------------");
            Console.WriteLine("Sending message to server..");
            stream.Write(message, 0, message.Length); // Write the bytes  
            stream.Flush();
            Console.WriteLine("Message sent to server..");
        
        
            byte[] serverMessage = new byte[1024];
            stream.Read(serverMessage, 0, serverMessage.Length);
                

            byte[] messageBody = serverMessage.Skip(2).ToArray();
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
            

        }            
    }
}

    
