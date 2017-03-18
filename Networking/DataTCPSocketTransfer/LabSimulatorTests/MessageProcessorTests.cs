using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LabSimulator;
using System.Collections.Generic;

namespace LabSimulatorTests
{
    [TestClass]
    public class MessageProcessorTests
    {
        const byte Control28 = 0x1C;
        const byte Control11 = 0x0B;

        /// <summary>
        /// Helper method to assert that the content of the processs message segment is 
        /// the expected value and that any data elment beyond the expected value is not set
        /// </summary>
        /// <param name="segmentData"></param>
        /// <param name="expectedSegmentData"></param>
        private void AssertSegmentDataIsEqual(byte[] segmentData, byte[] expectedSegmentData)
        {
            if (segmentData == null)
                throw new ArgumentNullException("segmentData", "byte [] is null");


            if (expectedSegmentData == null)
                throw new ArgumentNullException("expectedSegmentData", "byte [] is null");

            if (segmentData.Length < expectedSegmentData.Length)
                throw new AssertFailedException("segmentData.Length is less than expectedSegmentData");

            byte[] resizedSegmentData = new byte[expectedSegmentData.Length];

            Array.Copy(segmentData, resizedSegmentData, expectedSegmentData.Length);

            CollectionAssert.AreEqual(resizedSegmentData, expectedSegmentData);

            if(segmentData.Length > expectedSegmentData.Length)
            {
                // ensure that remaining data in segmentData is not set
                for(int i = expectedSegmentData.Length; i < segmentData.Length; i++)
                {
                    if (segmentData[i] != 0)
                        throw new AssertFailedException("AssertSegmentDataIsEqual failed, segment data beyond expected segment data was set.");
                }
            }

        }

        private List<byte[]> PartitionDataMessage(byte[] dataMessage, int partitionSize)
        {
            List<byte[]> dataSegments = new List<byte[]>();
            int messageDataIndex = 0;

            while (messageDataIndex < dataMessage.Length)
            {
                byte[] dataSegment = new byte[partitionSize];

                // copy data only up to what is left
                int lenght = (dataMessage.Length - messageDataIndex) > partitionSize ? partitionSize :
                        dataMessage.Length - messageDataIndex;

                Array.Copy(dataMessage, messageDataIndex, dataSegment, 0, lenght);
                dataSegments.Add(dataSegment);
                messageDataIndex += partitionSize;
            }

            return dataSegments;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), 
            "Invalid message format. Start delimeter not found in first position, expecting 1C")]
        public void ValidationTest_InvalidStartingDelimiter()
        {
            // Assemble

            var messageSegmentsGenerated = new List<DataExtractMessageSegment>();

            MessageProcessorConfiguration config = new MessageProcessorConfiguration();

            config.StartDelimiter = Control28;
            config.EndDelimiter = Control28;
            config.RecordTypeDelimiter = Control11;
            config.MessageSegmentSize = 1024;

            List<string> recordTypes = new List<string>() { "001", "002", "003" };

            // Create a message with an invalid message delimiter 
            LabMessageGenerator generator = new LabMessageGenerator(0x01, Control11, recordTypes);

            string header = "v1,HFH,b0004,,00";

            string recordData = "ResultID,";

            byte[] messageData = generator.GenerateCompleteMessage(recordData, header, 1, 1);

            // Act

            MessageProcessor processor = new MessageProcessor(config);

            processor.MessageSegmentReady += (_, args) =>
            {
                messageSegmentsGenerated.Add(args.MessageSegment);
            };
            
            processor.NewMessage(messageData);

            // Assert
            // Exception should be thrown
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException),
            "Specified argument was out of the range of valid values.\n\rParameter name: dataMessage.Length is greater than MessageProcessorConfiguration.MessageSegmentSize")]
        public void ValidationTest_InvalidDataMessageLength()
        {

            // Assemble
            var messageSegmentsGenerated = new List<byte[]>();

            MessageProcessorConfiguration config = new MessageProcessorConfiguration();

            config.StartDelimiter = Control28;
            config.EndDelimiter = Control28;
            config.RecordTypeDelimiter = Control11;
            config.MessageSegmentSize = 1024;

            List<string> recordTypes = new List<string>(){"001", "002", "003"};

            LabMessageGenerator generator = new LabMessageGenerator(Control28, Control11, recordTypes);

            string header = "v1,HFH,b0004,,00";

            string recordData = "ResultID,ResultDateTime,ResultComponent,Data,Test, 1144333,w34344554,454542434,452452454,45242445454545,423453454544,2454354444,test";

            byte[] messageData = generator.GenerateCompleteMessage(recordData, header, 1, 10);

            // Act

            MessageProcessor processor = new MessageProcessor(config);

            processor.NewMessage(messageData);

            // Assert
            // Nothing to assert exception is expected to be thrown
        }
        
        [TestMethod]
        public void ValidateHeader_OneSegement()
        {

            // Assemble

            var messageHeaderSegmentsGenerated = new List<string>();

            MessageProcessorConfiguration config = new MessageProcessorConfiguration();

            config.StartDelimiter = Control28;
            config.EndDelimiter = Control28;
            config.RecordTypeDelimiter = Control11;
            config.MessageSegmentSize = 1024;

            List<string> recordTypes = new List<string>() { "001", "002", "003" };

            LabMessageGenerator generator = new LabMessageGenerator(Control28, Control11, recordTypes);

            string header = "v1,HFH,b0004,,00";
            string recordData = "ResultID,";
            byte[] messageData = generator.GenerateCompleteMessage(recordData, header, 1, 1);

            MessageProcessor processor = new MessageProcessor(config);

            processor.MessageSegmentReady += (_, args) =>
            {
                messageHeaderSegmentsGenerated.Add(args.MessageHeader);
            };

            // Act
            processor.NewMessage(messageData);

            // Assert
            Assert.AreEqual<int>(messageHeaderSegmentsGenerated.Count, 1);
            Assert.AreEqual<string>(messageHeaderSegmentsGenerated[0], header);
        }

        [TestMethod]
        public void OneRecType_OneSegement()
        {

            // Assemble
            var messageSegmentsGenerated = new List<DataExtractMessageSegment>();

            MessageProcessorConfiguration config = new MessageProcessorConfiguration();

            config.StartDelimiter = Control28;
            config.EndDelimiter = Control28;
            config.RecordTypeDelimiter = Control11;
            config.MessageSegmentSize = 1024;

            List<string> recordTypes = new List<string>() { "001", "002", "003" };

            LabMessageGenerator generator = new LabMessageGenerator(Control28, Control11, recordTypes);

            string header = "v1,HFH,b0004,,00";
            string recordData = "ResultID,";
            byte[] messageData = generator.GenerateCompleteMessage(recordData, header, 1, 1);
            byte[] recordTypeDataSegment = generator.GenerateRecordTypeDataSegment(recordData, 1, 1);

            MessageProcessor processor = new MessageProcessor(config);

            processor.MessageSegmentReady += (_, args) =>
            {
                messageSegmentsGenerated.Add(args.MessageSegment);
            };

            // Act
            processor.NewMessage(messageData);

            // Assert
            Assert.AreEqual<int>(messageSegmentsGenerated.Count, 1);
            AssertSegmentDataIsEqual(messageSegmentsGenerated[0].SegmentData, recordTypeDataSegment);
        }

        [TestMethod]
        public void OneRecType_DataSegement_EqualsSegmentSize()
        {

            // Assemble
            var messageSegmentsGenerated = new List<DataExtractMessageSegment>();

            MessageProcessorConfiguration config = new MessageProcessorConfiguration();

            config.StartDelimiter = Control28;
            config.EndDelimiter = Control28;
            config.RecordTypeDelimiter = Control11;
            config.MessageSegmentSize = 1366;

            List<string> recordTypes = new List<string>() { "001", "002", "003" };

            LabMessageGenerator generator = new LabMessageGenerator(Control28, Control11, recordTypes);

            string header = "v1,HFH,b0004,,00";
            string recordData = "ResultID,ResultDateTime,ResultComponent,Data,Test, 1144333,w34344554,454542434,452452454,45242445454545,423453454544,2454354444,test";

            byte[] messageData = generator.GenerateCompleteMessage(recordData, header, 2, 4);
            byte[] recordTypeDataSegment = generator.GenerateRecordTypeDataSegment(recordData, 2, 4);

            MessageProcessor processor = new MessageProcessor(config);

            processor.MessageSegmentReady += (_, args) =>
            {
                messageSegmentsGenerated.Add(args.MessageSegment);
            };

            // Act
            processor.NewMessage(messageData);

            // Assert
            Assert.AreEqual<int>(messageSegmentsGenerated.Count, 1);
            AssertSegmentDataIsEqual(messageSegmentsGenerated[0].SegmentData, recordTypeDataSegment);
        }


        [TestMethod]
        public void OneRecType_TwoPartitions()
        {
            // Assemble
            var messageSegmentsGenerated = new List<DataExtractMessageSegment>();

            MessageProcessorConfiguration config = new MessageProcessorConfiguration();

            config.StartDelimiter = Control28;
            config.EndDelimiter = Control28;
            config.RecordTypeDelimiter = Control11;
            config.MessageSegmentSize = 10240;

            List<string> recordTypes = new List<string>() { "001", "002", "003" };

            LabMessageGenerator generator = new LabMessageGenerator(Control28, Control11, recordTypes);

            string header = "v1,HFH,b0004,,00";

            string recordData = "ResultID,ResultDateTime,ResultComponent,Data,Test, 1144333,w34344554,454542434,452452454,45242445454545,423453454544,2454354444,test";

            byte[] messageData = generator.GenerateCompleteMessage(recordData, header, 2, 4);
            byte[] recordTypeDataSegment = generator.GenerateRecordTypeDataSegment(recordData, 2, 4);

            int dataSegmentSize = 1024;

            byte[] dataSegment1 = new byte[dataSegmentSize];
            byte[] dataSegment2 = new byte[messageData.Length - dataSegmentSize];

            Array.Copy(messageData, dataSegment1, dataSegment1.Length);
            Array.Copy(messageData, dataSegmentSize, dataSegment2, 0, dataSegment2.Length);
            
            MessageProcessor processor = new MessageProcessor(config);

            processor.MessageSegmentReady += (_, args) =>
            {
                messageSegmentsGenerated.Add(args.MessageSegment);
            };

            // Act
            processor.NewMessage(dataSegment1);
            processor.NewMessage(dataSegment2);
            
            // Assert
            // Nothing to assert exception is expected to be thrown
            Assert.AreEqual<int>(messageSegmentsGenerated.Count, 1);
            AssertSegmentDataIsEqual(messageSegmentsGenerated[0].SegmentData, recordTypeDataSegment);

        }

        [TestMethod]
        public void OneRecType_MultiplePartitions()
        {
            // Assemble
            var messageSegmentsGenerated = new List<DataExtractMessageSegment>();

            MessageProcessorConfiguration config = new MessageProcessorConfiguration();

            config.StartDelimiter = Control28;
            config.EndDelimiter = Control28;
            config.RecordTypeDelimiter = Control11;
            config.MessageSegmentSize = 2048;

            List<string> recordTypes = new List<string>() { "001", "002", "003" };
            LabMessageGenerator generator = new LabMessageGenerator(Control28, Control11, recordTypes);            

            string header = "v1,HFH,b0004,,00";
            string recordData = "ResultID,ResultDateTime,ResultComponent,Data,Test, 1144333,w34344554," + 
                                "454542434,452452454,45242445454545,423453454544,2454354444,test";

            byte[] messageData = generator.GenerateCompleteMessage(recordData, header, 2, 4);
            byte[] recordTypeDataSegment = generator.GenerateRecordTypeDataSegment(recordData, 2, 4);

            int partitionSize = 256;
            List<byte[]> dataSegments = PartitionDataMessage(messageData, partitionSize);
            
            MessageProcessor processor = new MessageProcessor(config);

            processor.MessageSegmentReady += (_, args) =>
            {
                messageSegmentsGenerated.Add(args.MessageSegment);
            };

            // Act
            dataSegments.ForEach(segment => processor.NewMessage(segment));
            
            // Assert
            // Nothing to assert exception is expected to be thrown
            Assert.AreEqual<int>(messageSegmentsGenerated.Count, 1);
            AssertSegmentDataIsEqual(messageSegmentsGenerated[0].SegmentData, recordTypeDataSegment);

        }

        [TestMethod]
        public void OneRecType_DataSegement_EqualsSegmentSize_Paritioned()
        {

            // Assemble
            var messageSegmentsGenerated = new List<DataExtractMessageSegment>();

            List<string> recordTypes = new List<string>() { "001", "002", "003" };

            LabMessageGenerator generator = new LabMessageGenerator(Control28, Control11, recordTypes);

            string header = "v1,HFH,b0004,,00";
            string recordData = "ResultID,ResultDateTime,ResultComponent,Data,Test, 1144333,w34344554,454542434,452452454,45242445454545,423453454544,2454354444,test";

            byte[] messageData = generator.GenerateCompleteMessage(recordData, header, 2, 4);
            byte[] recordTypeDataSegment = generator.GenerateRecordTypeDataSegment(recordData, 2, 4);

            MessageProcessorConfiguration config = new MessageProcessorConfiguration();

            config.StartDelimiter = Control28;
            config.EndDelimiter = Control28;
            config.RecordTypeDelimiter = Control11;
            config.MessageSegmentSize = recordTypeDataSegment.Length;

            int partitionSize = 256;
            List<byte[]> dataSegments = PartitionDataMessage(messageData, partitionSize);

            MessageProcessor processor = new MessageProcessor(config);

            processor.MessageSegmentReady += (_, args) =>
            {
                messageSegmentsGenerated.Add(args.MessageSegment);
            };

            // Act
            dataSegments.ForEach(segment => processor.NewMessage(segment));

            // Assert
            Assert.AreEqual<int>(messageSegmentsGenerated.Count, 1);
            AssertSegmentDataIsEqual(messageSegmentsGenerated[0].SegmentData, recordTypeDataSegment);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), 
            "Processor reached message segment size limit of 1048, " + 
            "and has not encountered the EndOfFile delimiter.")]
        public void RecordDataLenght_GreaterThanSegmentSize()
        {
            // Assemble
            var messageSegmentsGenerated = new List<DataExtractMessageSegment>();

            List<string> recordTypes = new List<string>() { "001", "002", "003" };

            LabMessageGenerator generator = new LabMessageGenerator(Control28, Control11, recordTypes);

            string header = "v1,HFH,b0004,,00";

            string recordData = "ResultID,ResultDateTime,ResultComponent,Data,Test, 1144333,w34344554,454542434,452452454,45242445454545,423453454544,2454354444,test";

            byte[] messageData = generator.GenerateCompleteMessage(recordData, header, 2, 5);
            byte[] recordTypeDataSegment = generator.GenerateRecordTypeDataSegment(recordData, 2, 4);

            MessageProcessorConfiguration config = new MessageProcessorConfiguration();

            config.StartDelimiter = Control28;
            config.EndDelimiter = Control28;
            config.RecordTypeDelimiter = Control11;
            config.MessageSegmentSize = recordTypeDataSegment.Length-10;

            int dataSegmentSize = 256;
            List<byte[]> dataSegments = new List<byte[]>();
            int messageDataIndex = 0;

            while (messageDataIndex < messageData.Length)
            {
                byte[] dataSegment = new byte[dataSegmentSize];

                // copy data only up to what is left
                int lenght = (messageData.Length - messageDataIndex) > dataSegmentSize ? dataSegmentSize :
                        messageData.Length - messageDataIndex;

                Array.Copy(messageData, messageDataIndex, dataSegment, 0, lenght);
                dataSegments.Add(dataSegment);
                messageDataIndex += dataSegmentSize;
            }

            MessageProcessor processor = new MessageProcessor(config);

            processor.MessageSegmentReady += (_, args) =>
            {
                messageSegmentsGenerated.Add(args.MessageSegment);
            };

            // Act
            dataSegments.ForEach(segment => processor.NewMessage(segment));

            // Assert
            // Nothing to assert exception is expected to be thrown
            Assert.AreEqual<int>(messageSegmentsGenerated.Count, 1);
        }
        
        [TestMethod]
        [ExpectedException(typeof(ArgumentException),
            "Processor reached message segment size limit of 1048, " +
            "and has not encountered the EndOfFile delimiter.")]
        public void RecordDataLenght_GreaterThanSegmentSize_ByOne()
        {
            // Assemble
            var messageSegmentsGenerated = new List<DataExtractMessageSegment>();

            List<string> recordTypes = new List<string>() { "001", "002", "003" };

            LabMessageGenerator generator = new LabMessageGenerator(Control28, Control11, recordTypes);

            string header = "v1,HFH,b0004,,00";

            string recordData = "ResultID,ResultDateTime,ResultComponent,Data,Test, 1144333,w34344554,454542434,452452454,45242445454545,423453454544,2454354444,test";

            byte[] messageData = generator.GenerateCompleteMessage(recordData, header, 2, 5);
            byte[] recordTypeDataSegment = generator.GenerateRecordTypeDataSegment(recordData, 2, 4);

            MessageProcessorConfiguration config = new MessageProcessorConfiguration();

            config.StartDelimiter = Control28;
            config.EndDelimiter = Control28;
            config.RecordTypeDelimiter = Control11;
            config.MessageSegmentSize = recordTypeDataSegment.Length - 1;

            int dataSegmentSize = 256;
            List<byte[]> dataSegments = new List<byte[]>();
            int messageDataIndex = 0;

            while (messageDataIndex < messageData.Length)
            {
                byte[] dataSegment = new byte[dataSegmentSize];

                // copy data only up to what is left
                int lenght = (messageData.Length - messageDataIndex) > dataSegmentSize ? dataSegmentSize :
                        messageData.Length - messageDataIndex;

                Array.Copy(messageData, messageDataIndex, dataSegment, 0, lenght);
                dataSegments.Add(dataSegment);
                messageDataIndex += dataSegmentSize;
            }

            MessageProcessor processor = new MessageProcessor(config);

            processor.MessageSegmentReady += (_, args) =>
            {
                messageSegmentsGenerated.Add(args.MessageSegment);
            };

            // Act
            dataSegments.ForEach(segment => processor.NewMessage(segment));

            // Assert
            // Nothing to assert exception is expected to be thrown
            Assert.AreEqual<int>(messageSegmentsGenerated.Count, 1);

        }
    }
}
