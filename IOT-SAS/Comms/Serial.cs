using System;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;

namespace IOTSAS.Comms
{
    public class Serial : SASWriteBuffer, IComms, IDisposable
    {
        

        SerialPort mySerial;
        readonly object lockObject = new object();
        
        const int MaxPacketSize = 1024;
        public int MaxPayloadSize { get; } = MaxPacketSize - 9;


        //Set up serial port.  On a Raspberry Pi, use /dev/serial0
        public Serial(String device, uint baudRate = 115200) : base(MaxPacketSize)
        {

#if DEBUGSERIAL
            Console.WriteLine($"Opening {device} {baudRate}");
#endif        
            mySerial = new SerialPort(device, (int)baudRate, Parity.None, 8, StopBits.One);

            mySerial.Handshake = Handshake.None;
            mySerial.ReadTimeout = 1000;
            mySerial.WriteTimeout = 500;
            
            mySerial.Open();
        }



        public byte[] Execute(int returnBytes)
        {
            UInt16 ResultCode;
            var data = Execute(returnBytes, out ResultCode);
            
            if (ResultCode != (UInt16)eResultCode.IO_SUCCESS)
            {
                throw new SASException(ResultCode);
            }
            return data;
        }
        
        public byte[] Execute(int returnBytes, out UInt16 ResultCode)
        {
        
            lock(lockObject)
            {
                mySerial.DiscardInBuffer();
                mySerial.Write(WriteBufferData, 0, WriteBufferData.Length);
#if DEBUGSERIAL
                Console.WriteLine($"TX: {BitConverter.ToString(WriteBufferData,0,WriteBufferData.Length)}");
#endif 
                return Read(returnBytes, out ResultCode);
            }
        }


        public byte[] Read(int expectedByteCount, out UInt16 ResultCode)
        {
            //Read the IOT-SAS reply
                       
            var data = new byte[expectedByteCount];
            var resultCode = new byte[2];

                
            var count = 0;
            var bytesRx = 0;

#if DEBUGSERIAL
                    Console.WriteLine($"RX: Start RX {expectedByteCount}");
#endif 

            try
            {

                while (count < expectedByteCount)
                {
                    bytesRx = mySerial.Read(data, count, expectedByteCount - count);
#if DEBUGSERIAL
                    Console.WriteLine($"RX: {BitConverter.ToString(data,count,bytesRx)}");
#endif 
                    count += bytesRx;
                }
                
                
                count = 0;
                expectedByteCount = 2;
                while (count < 2)
                {
                    bytesRx = mySerial.Read(resultCode, count, expectedByteCount-count);
                    count += bytesRx;
                }    
                Array.Reverse(resultCode);
                ResultCode = BitConverter.ToUInt16(resultCode, 0);
                
            }
            catch (TimeoutException)
            {

                if (count == 2)
                {
                    Array.Reverse(data, 0, 2);
                    ResultCode = BitConverter.ToUInt16(data, 0);
                    throw new SASException(ResultCode, "Serial port - Timeout, no data");
                }
                else
                {
                    throw new SASException(eResultCode.PORT_NO_DATA, "Serial port - Timeout, no data");
                }
            }

#if DEBUGSERIAL
                Console.WriteLine($"RXE: {BitConverter.ToString(resultCode,0,2)}  ResultCode: {resultCode.ToHexString()}  BytesToRead: {mySerial.BytesToRead}");
                
                if (mySerial.BytesToRead > 0)
                {
                    var b = new byte[mySerial.BytesToRead];
                    bytesRx = mySerial.Read(b, 0, b.Length);
                    Console.WriteLine($"RX Overflow: {BitConverter.ToString(b, 0, b.Length)}");
                }
#endif 

            return data;
            
        }

        public void Dispose()
        {
            mySerial.Dispose();
        }
        
    }
}
