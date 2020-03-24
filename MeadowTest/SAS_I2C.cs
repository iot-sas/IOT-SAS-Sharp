using System;
using IOTSAS;
using IOTSAS.Comms;
using Meadow.Hardware;

namespace MeadowTest
{
    public class SAS_I2C : SASWriteBuffer, IComms, IDisposable
    {


        II2cBus i2cBus;
        II2cPeripheral sas;
        byte address = 0x50;
        const int MaxPacketSize = 255;
        public int MaxPayloadSize { get; } = MaxPacketSize - 9;


        readonly object lockObject = new object();

        
        public SAS_I2C(Meadow.Hardware.II2cBus i2cBus) : base(MaxPacketSize)
        {

#if DEBUGSERIAL
            Console.WriteLine($"Opening i2c {address} {i2cBus.Frequency}");
#endif

            this.i2cBus = i2cBus;
            sas = new I2cPeripheral(i2cBus, address);
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

                sas.WriteBytes(WriteBufferData);                
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

#if DEBUGSERIAL
            Console.WriteLine($"RX: Start RX {expectedByteCount}");
#endif 

            try
            {

                while (count < expectedByteCount)
                {
                    var datain = sas.ReadBytes((ushort)(expectedByteCount - count));
                    Array.Copy(datain, 0, data, count, datain.Length);                    
#if DEBUGSERIAL
                    Console.WriteLine($"RX: {BitConverter.ToString(datain)}");
#endif 
                    count += datain.Length;
                }
                
                //count = 0;
                //expectedByteCount = 2;
                //while (count < 2)
                //{
                    resultCode = sas.ReadBytes((ushort)(1));
               // }
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

            resultCode = sas.ReadBytes(2);
            Array.Reverse(resultCode);
            ResultCode = BitConverter.ToUInt16(resultCode, 0);
#if DEBUGSERIAL
            Console.WriteLine($"RXE: {BitConverter.ToString(resultCode,0,2)}  ResultCode: {resultCode.ToHexString()}");
            
#endif 
            return data;            
        }

        public void Dispose()
        {
          
        }
        
    }
}
