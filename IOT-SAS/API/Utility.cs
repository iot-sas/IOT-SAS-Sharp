using System;
using System.IO;

namespace IOTSAS.API
{
    public class Utility
    
    {
        IComms Comms;
        
        public Utility(IComms comms)
        {
            Comms = comms;
        }

        public byte[] GetFirmwareRevision()
        {
        
            const byte returnBytes = 3;
            
            Comms.WriteReset();

            Comms.WriteByte((byte)eClass.CLA_DIAG);                //Class Instruction for Utility Functions
            Comms.WriteByte((byte)eFunction.INS_DIAG_FW_VERSION);  //Instruction Code for Revision Request
            Comms.WriteByte(0x00);                                 //Parameter (P1) reserved for future use
            Comms.WriteByte(0x00);                                 //Parameter (P2) reserved for future use
            Comms.WriteByte(returnBytes);                          //3 bytes representing firmware major, minor, and revision version numbers
            return Comms.Execute(returnBytes);

        }
        
        
        public byte[] GetHardwareRevision()
        {
        
            const byte returnBytes = 3;
            
            Comms.WriteReset();

            Comms.WriteByte((byte)eClass.CLA_DIAG);                 //Class Instruction for Utility Functions
            Comms.WriteByte((byte)eFunction.INS_DIAG_HW_VERSION);   //Instruction Code for Revision Request INS_DIAG_HW_VERSION
            Comms.WriteByte(0x01);                                  //Parameter (P1) reserved for future use
            Comms.WriteByte(0x00);                                  //Parameter (P2) reserved for future use
            Comms.WriteByte(returnBytes);                           //3 bytes representing firmware major, minor, and revision version numbers
            return Comms.Execute(returnBytes);

        }
        
        
        public byte[] GetTrueRandomBytes(ushort bytesToReturn)
        {
            if (bytesToReturn > Comms.MaxPayloadSize)
            {
                throw new SASException(eResultCode.MAX_BUFFER_EXCEEDED);
            }
        
            Comms.WriteReset();

            Comms.WriteByte((byte)eClass.CLA_DIAG);         //Class Instruction for Utility Functions
            Comms.WriteByte((byte)eFunction.INS_DIAG_RNG);  //Instruction Code for Revision Request
            Comms.WriteByte(0x04);                          //Parameter (P1) 
            Comms.WriteByte(0x00);                          //Parameter (P2) reserved for future use
            Comms.WriteEncodedLength(bytesToReturn);
            
            return Comms.Execute(bytesToReturn);
        }
        
        
        public void IncrementMonotonicCounter(byte[] upperSeed = null)
        {
            const byte returnBytes = 0x40;
            
            Comms.WriteReset();

            Comms.WriteByte((byte)eClass.CLA_DIAG);                         //Class Instruction for Utility Functions
            Comms.WriteByte((byte)eFunction.INS_CRYPTO_MONOTONIC_COUNTER);  //Instruction Code for Monotonic Counter
            Comms.WriteByte(0x00);  //Parameter (P1) reserved for future use
            Comms.WriteByte(0x00);  //Parameter (P2) reserved for future use

            if (upperSeed != null)
            {
                Comms.WriteByte(0x04);  // R
                Comms.Write(upperSeed);
            }
            
            Comms.WriteByte(returnBytes);  // this is the size of the returned 64-bit monotonic counter
            
        }
        
       
        
        public byte[] FlashLED(bool output)
        {
            Comms.WriteReset();

            Comms.WriteByte((byte)eClass.CLA_DIAG);                //Class Instruction for Utility Functions
            Comms.WriteByte((byte)eFunction.INS_DIAG_TOGGLE_LED);  //Instruction Code for Revision Request
            Comms.WriteByte((byte)(output ? 0x1 : 0x0));           //Parameter (P1) 
            Comms.WriteByte(0x00);                                 //Parameter (P2)
            
            return Comms.Execute(0);
        }
        
    }
}
