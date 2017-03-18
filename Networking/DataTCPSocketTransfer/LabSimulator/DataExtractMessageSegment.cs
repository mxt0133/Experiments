using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabSimulator
{
    public class DataExtractMessageSegment
    {
        private byte[] buffer;        
        private int bufferIndex;

        public byte[] SegmentData { get { return buffer; } }
        public int FreeSpace { get { return buffer.Length - bufferIndex; } }
        public int Length { get { return buffer.Length; } }

        public DataExtractMessageSegment(int segmentSize)
        {
            buffer = new byte[segmentSize];            
            bufferIndex = 0;
        }
        /// <summary>
        /// Copies data to the MessageSegment
        /// </summary>
        /// <param name="sourceData">Contains data to copy</param>
        /// <param name="sourceIndex">Index in the source data at which copying begins</param>
        /// <param name="lenght">Number of elements to copy</param>
        public void AddData(byte[] sourceData, int sourceIndex, int lenght)
        {
            Array.Copy(sourceData, sourceIndex, buffer, bufferIndex, lenght);
            bufferIndex += lenght;
        }
    }
}
