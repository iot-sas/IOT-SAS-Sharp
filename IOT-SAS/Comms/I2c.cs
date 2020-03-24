using System;
namespace IOTSAS.Comms
{
    public class I2c : IComms
    {
        readonly object lockObject = null;
        
        public I2c()
        {
        NOTE: COMPILE OFF
        }
        
        
        public byte[] Execute(int returnBytes)
        {
            return Execute(WriteBufferData, returnBytes);
        }
        
        public byte[] Execute(byte[] data, int returnBytes)
        {
            lock(lockObject)
            {
                mySerial.Write(data, 0, data.Length);
                return Read(returnBytes);
            }
        }
        
    }
}
