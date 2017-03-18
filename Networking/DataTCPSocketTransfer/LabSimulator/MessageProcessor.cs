using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabSimulator
{  

    public class MessageSegmentReadyEventArgs : EventArgs
    {
        /*
         
        public List<string> MessageRecordType { get; set; }
        public string Version { get; set; }
        public string ClientId  { get; set; }
        public string BatchID { get; set; }
        public string TransactionID { get; set; }        
        public string BitMask { get; set; }
        
         */
        public DataExtractMessageSegment MessageSegment { get; set; }
        public string MessageHeader { get; set; }
         
    }

    public class MessageProcessorConfiguration
    {
        public byte StartDelimiter { get; set; }
        public byte EndDelimiter { get; set; }
        public byte RecordTypeDelimiter { get; set; }
        public int MessageSegmentSize { get; set; }
    }

    /// <summary>
    /// This class is responsible for taking data and breaking it up
    /// into segments that can be processed before the entire data message
    /// is received.
    /// 
    /// The data message can come in pieces or in complete to be processes
    /// 
    /// New data can be feed to the processor until the End of Message delimiter
    /// is encountered which will then reset the processor to process a new data
    /// message
    /// 
    /// 
    /// </summary>
    public class MessageProcessor
    {
        private DataExtractMessageSegment messageSegment = null;
        private bool dataProcessingInProgress = false;
        private byte[] messageHeader = null;
        private string messageHeaderString = string.Empty;
        private MessageProcessorConfiguration messageProcessorConfig;
        private int lastRectypeIndex = 0;


        public event EventHandler<MessageSegmentReadyEventArgs> MessageSegmentReady;

        public MessageProcessor(MessageProcessorConfiguration config )
        {
            messageProcessorConfig = config;            
        }
               
                
        public void NewMessage(byte[] dataMessage)
        {
            bool completeDataMessage = false;
            int recordtypeIndexOfDataMessage = -1;
            int indexOfEoFDelimiter = -1;

            // this is the first data segment
            if(!dataProcessingInProgress)
            {
                // new message to process, initilzie buffer
                if(messageSegment == null)
                    messageSegment = new DataExtractMessageSegment(
                                        messageProcessorConfig.MessageSegmentSize);

                ValidateFirstDataSegment(dataMessage);
                ExtractHeader(dataMessage);

                recordtypeIndexOfDataMessage = IndexOfRecordTypeDelimiter(dataMessage);

                // If there is a EoF Delimiter is after the first element then 
                // it is a complete data message
                completeDataMessage = IndexEofDelimiter(dataMessage, 1) > 0;

                // it is a valid message and complete
                if(completeDataMessage)
                {                    
                    int endOfRecordData = dataMessage.Length - 1;

                    messageSegment.AddData(dataMessage, recordtypeIndexOfDataMessage, 
                        endOfRecordData - recordtypeIndexOfDataMessage);

                    OnMessageSegmentReady(new MessageSegmentReadyEventArgs()
                    {
                        MessageSegment = messageSegment,
                        MessageHeader = messageHeaderString
                    });

                    CancelCurrentMessage();
                    return;
                }

                dataProcessingInProgress = true;

                // copy the data from the RecordTypeIndex to end of dataMessage
                int recordDataLength = dataMessage.Length - recordtypeIndexOfDataMessage;
                messageSegment.AddData(dataMessage, recordtypeIndexOfDataMessage, recordDataLength);

                return;
            }



            indexOfEoFDelimiter = IndexEofDelimiter(dataMessage);

            // Does the data contain the EoFDelimiter
            if (indexOfEoFDelimiter >= 0)
            {
                // this dataMessage segment completes the message

                // check if there is enough space in the segmentPayload to fit the data    
                if(messageSegment.FreeSpace >= indexOfEoFDelimiter)
                {
                    // copy data into message setment and fire OnMessageSegment Ready
                    // and cancel current message
                    messageSegment.AddData(dataMessage, 0, indexOfEoFDelimiter);

                    OnMessageSegmentReady(new MessageSegmentReadyEventArgs()
                    {
                        MessageSegment = messageSegment,
                        MessageHeader = messageHeaderString
                    });

                    CancelCurrentMessage();
                    return;
                }
            }
            else if (messageSegment.FreeSpace > dataMessage.Length)
            {
                messageSegment.AddData(dataMessage, 0, dataMessage.Length);
            }
            else
            {
                // This dataMessage is not the first portion, does not contain the EoF delimiter
                // and the length is greater than the free space we have on the current 
                // messageSegment.  Throw an exception because we expect 
                throw new ArgumentException(
                    string.Format("Processor reached message segment size limit of {0}," + 
                                   " and has not encountered the EndOfFile delimiter."
                                    , messageProcessorConfig.MessageSegmentSize));
            }
        }



        public void CancelCurrentMessage()
        {
            // Empty out array
            Array.Clear(messageHeader, 0, messageHeader.Length);
            
            dataProcessingInProgress = false;
            messageSegment = null;
            messageHeaderString = string.Empty;
            lastRectypeIndex = 0;
        }

        private void ExtractHeader(byte[] data)
        {
            // assume that data has the correct start delimiter move to position 1
            int index = 1;

            int indexFirstRecordTypeDelimiter = Array.IndexOf(data, messageProcessorConfig.RecordTypeDelimiter);

            if(indexFirstRecordTypeDelimiter < 0)
                throw new ArgumentException(String.Format("Invalid message format, record type delimeter not found, expecting {0}", 
                    BitConverter.ToString(new byte[1]{messageProcessorConfig.RecordTypeDelimiter})));
            
            int headerLenght =indexFirstRecordTypeDelimiter - index;

            messageHeader = new byte[headerLenght]; 

            Array.Copy(data, index, messageHeader, 0, headerLenght);

            messageHeaderString = System.Text.Encoding.ASCII.GetString(messageHeader);

        }

        private void ValidateFirstDataSegment(byte[] data)
        {
            // Size of data must be less than MessageSegmentSize we are configured for
            if (data.Length > messageSegment.Length)
                throw new ArgumentOutOfRangeException("dataMessage.Length is greater" +
                                    "than MessageProcessorConfiguration.MessageSegmentSize");

            if (data[0] != messageProcessorConfig.StartDelimiter)
                throw new ArgumentException(String.Format("Invalid message format. Start " +
                    "delimeter not found in first position, expecting {0}", 
                    BitConverter.ToString(new byte[1]{messageProcessorConfig.StartDelimiter})));
            
            // RecordType delimiter expected in first segment of data message
            if (IndexOfRecordTypeDelimiter(data) < 0)
                throw new ArgumentException(String.Format("Invalid message format, record type " +
                    "delimeter not found, expecting {0}",
                BitConverter.ToString(new byte[1] { messageProcessorConfig.RecordTypeDelimiter })));
        }

        private int IndexOfRecordTypeDelimiter(byte[] data)
        {
            return Array.IndexOf(data, messageProcessorConfig.RecordTypeDelimiter);                
        }

        private int IndexEofDelimiter(byte[] data, int startIndex = 0)
        {
            return Array.IndexOf(data, messageProcessorConfig.EndDelimiter, startIndex);
        }


        protected virtual void OnMessageSegmentReady(MessageSegmentReadyEventArgs e)
        {
            EventHandler<MessageSegmentReadyEventArgs> handler = MessageSegmentReady;
            if (handler != null)
            {
                handler(this, e);
            }
        }

    }
}
