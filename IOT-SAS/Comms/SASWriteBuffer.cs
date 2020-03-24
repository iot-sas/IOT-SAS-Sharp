using System;
using System.IO;

namespace IOTSAS.Comms
{
    public class SASWriteBuffer
    {

        byte[] byteBuff;
        MemoryStream ms;
    
    
        public SASWriteBuffer(uint buffersize)
        {
            byteBuff = new byte[buffersize];
            ms = new MemoryStream(byteBuff);
        }

        public int WriteBufferLength
        {
            get
            {
                return (int)ms.Length; 
            }
        }

        public byte[] WriteBufferData
        {
            get
            {
                return ms.ToArray();
            }
        }


        public void WriteReset()
        {
            ms.SetLength(0);
        }

        public void WriteByte(byte value)
        {
            ms.WriteByte(value);
        }

        public void Write(byte[] data)
        {
            ms.Write(data,0,data.Length);
        }
        
        public void Write(byte[] data, int offset, int count)
        {
            ms.Write(data,offset,count);
        }
        
        public void WriteEncodedLength(UInt16 len)
        {
            
            if (len <= 255)
            {
                if (len == 0) throw new Exception("Zero data, protocol error");
                ms.WriteByte((byte)len);
            }
            else
            {
                ms.WriteByte(0);
                var length = BitConverter.GetBytes(len);
                ms.Write(length,0,length.Length);
                if (BitConverter.IsLittleEndian) Array.Reverse(byteBuff, (int)ms.Position - 2, 2);
            }
        }
        
    }
}
