using System;
namespace IOTSAS
{
    public interface IComms 
    {


        int MaxPayloadSize { get; }

        /// <summary>
        /// Execute WriteBuffer. Throw SASException if SAS returns an error
        /// </summary>
        /// <returns>SAS data reply.</returns>
        /// <param name="returnBytes">Number of bytes expected from SAS.</param>
        byte[] Execute(int returnBytes);
        
        
        /// <summary>
        /// Execute WriteBuffer. WITHOUT SASException.
        /// </summary>
        /// <returns>SAS data reply.</returns>
        /// <param name="returnBytes">Number of bytes expected from SAS.</param>        
        byte[] Execute(int returnBytes, out UInt16 errCode);

        //SAS WriteBuffer
        void WriteReset();
        void WriteByte(byte value);
        void Write(byte[] data);
        void Write(byte[] data, int offset, int count);
        void WriteEncodedLength(UInt16 len);
        int  WriteBufferLength { get; }
        byte[] WriteBufferData { get; }

        void Dispose();
        
    }
}
