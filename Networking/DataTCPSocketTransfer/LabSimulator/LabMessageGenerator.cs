using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabSimulator
{
    public class LabMessageGenerator
    {
        private byte mDelimiter;
        private byte rtDelimiter;

        List<byte[]> byteRecordTypeArray;

        public LabMessageGenerator(byte messageDelimiter, byte recordTypeDelimiter, List<string> recordTypes)
        {
            if (recordTypes == null || recordTypes.Count == 0)
                throw new ArgumentException("RecortTypes is NULL or empty list");

            mDelimiter = messageDelimiter;
            rtDelimiter = recordTypeDelimiter;

            byteRecordTypeArray = new List<byte[]>();

            recordTypes.ForEach(x => byteRecordTypeArray.Add(System.Text.Encoding.ASCII.GetBytes(x)));
        }


        public byte[] GenerateCompleteMessage(string recordData, string header, int numberOfRecordTypes, int numberOfRecords)
        {
            try
            {
                string messageBody = CreateMessageBody(recordData, numberOfRecords);
                byte[] messageBodyBytes = System.Text.Encoding.ASCII.GetBytes(messageBody);
                byte[] headerBytes = System.Text.Encoding.ASCII.GetBytes(header);


                //Message size will be:
                //  1 byte for the starting delimiter +
                //  lenght of header +
                //  (1 byte for record delimiter + 3 bytes for recordTypeLength + messageBody lenght) * number of record types +
                //  1 bye for the ending delimiter
                int messageLength = 1 + headerBytes.Length + (1 + 3 + messageBodyBytes.Length) * numberOfRecordTypes + 1;

                byte[] message = new byte[messageLength]; 
                               


                int messageIndex = 0;

                message[messageIndex] = mDelimiter;
                messageIndex++;



                // add message header
                for (int j = 0; j < headerBytes.Length; j++)
                {
                    message[messageIndex] = headerBytes[j];
                    messageIndex++;
                }


                for (int k = 0; k < numberOfRecordTypes; k++)
                {
                    // add recordtype delimter
                    message[messageIndex] = rtDelimiter;
                    messageIndex++;

                    int byteRecordTypeIndex = k % byteRecordTypeArray.Count();

                    // Add recordTypeValue, that varies 
                    for (int l = 0; l < byteRecordTypeArray[byteRecordTypeIndex].Length; l++)
                    {
                        message[messageIndex] = byteRecordTypeArray[byteRecordTypeIndex][l];
                        messageIndex++;
                    }

                    // add message body
                    for (int m = 0; m < messageBodyBytes.Length; m++)
                    {
                        if (messageIndex >= message.Length)
                        {
                            Console.WriteLine("MessageIndex >= message.Lenght");
                        }

                        if (m >= messageBodyBytes.Length)
                        {
                            Console.WriteLine("MessageIndex >= message.Lenght");
                        }

                        message[messageIndex] = messageBodyBytes[m];
                        messageIndex++;
                        //Debug.Print("messageIndex " + messageIndex.ToString());
                        //Debug.Print("messageBodyBytes.Lenght == "+ messageBodyBytes.Length + 
                        ///    ", m ==" + m.ToString() + ", k == " + k.ToString());
                    }
                }

                // Add ending Control28
                message[messageIndex] = mDelimiter;

                return message;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error on CreateMessage: " + ex.Message);
                Console.ReadLine();
                throw ex;
            }
        }

        public byte[] GenerateRecordTypeDataSegment(string recordData, int numberOfRecordTypes, int numberOfRecords)
        {
            try
            {
                string messageBody = CreateMessageBody(recordData, numberOfRecords);
                byte[] messageBodyBytes = System.Text.Encoding.ASCII.GetBytes(messageBody);
           
                //Message size will be:
                //  1 byte for the starting delimiter +
                //  lenght of header +
                //  (1 byte for record delimiter + 3 bytes for recordTypeLength + messageBody lenght) * number of record types +
                //  1 bye for the ending delimiter
                int messageLength = (1 + 3 + messageBodyBytes.Length) * numberOfRecordTypes;

                byte[] message = new byte[messageLength];

                int messageIndex = 0;

                for (int k = 0; k < numberOfRecordTypes; k++)
                {
                    // add recordtype delimter
                    message[messageIndex] = rtDelimiter;
                    messageIndex++;

                    int byteRecordTypeIndex = k % byteRecordTypeArray.Count();

                    // Add recordTypeValue, that varies 
                    for (int l = 0; l < byteRecordTypeArray[byteRecordTypeIndex].Length; l++)
                    {
                        message[messageIndex] = byteRecordTypeArray[byteRecordTypeIndex][l];
                        messageIndex++;
                    }

                    // add message body
                    for (int m = 0; m < messageBodyBytes.Length; m++)
                    {
                        if (messageIndex >= message.Length)
                        {
                            Console.WriteLine("MessageIndex >= message.Lenght");
                        }

                        if (m >= messageBodyBytes.Length)
                        {
                            Console.WriteLine("MessageIndex >= message.Lenght");
                        }

                        message[messageIndex] = messageBodyBytes[m];
                        messageIndex++;
                        //Debug.Print("messageIndex " + messageIndex.ToString());
                        //Debug.Print("messageBodyBytes.Lenght == "+ messageBodyBytes.Length + 
                        ///    ", m ==" + m.ToString() + ", k == " + k.ToString());
                    }
                }

                return message;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error on CreateMessage: " + ex.Message);
                Console.ReadLine();
                throw ex;
            }
        }




        private string CreateMessageBody(string recordData, int numberOfRecords)
        {

            // Instantiate a SB with the lenght of the message in the file times the number of records
            // plust the new line for each record minus 1
            StringBuilder message = new StringBuilder(numberOfRecords * recordData.Length + numberOfRecords - 1);

            for (int i = 0; i < numberOfRecords; i++)
            {
                message.Append(recordData + System.Environment.NewLine);
            }

            return message.ToString();
        }
    }
}
