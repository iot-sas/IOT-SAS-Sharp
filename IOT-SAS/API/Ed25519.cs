using System;
using System.IO;

namespace IOTSAS.API
{
    public class Ed25519
    {

        IComms Comms;
        
        internal Ed25519(IComms comms)
        {
            Comms = comms;
        }
        
        
        public byte[] Sign(byte[] data) { return Sign(data, 0, data.Length); }
        public byte[] Sign(byte[] data, int offset, int count)
        {
            const byte returnBytes = 64;
            
            Comms.WriteReset();

            Comms.WriteByte((byte)eClass.CLA_FACTOM);                   //Class Instruction for Cryptographic Functions
            Comms.WriteByte((byte)eFunction.INS_FACTOM_SIGN_ENTRY);     //Instruction Code for Cryptographic HASH Function
            Comms.WriteByte((byte)eP1.P1_CRYPTO_HASH_SHA256);           //Parameter (P1) for SHA-1 hash function
            Comms.WriteByte((byte)eP2.P2_FACTOM_ADDRESS_ID);            //Parameter (P2) reserved for future use
            
            Comms.WriteEncodedLength((UInt16)count);   // L
            Comms.Write(data, offset, count);   // N bytes of data
            Comms.WriteByte(returnBytes);       // 64 byte hash of expected return
            return Comms.Execute(returnBytes);
        }
        
        public byte[] GetIdPub()
        {
            const byte returnBytes = 55;
            Comms.WriteReset();

            Comms.WriteByte((byte)eClass.CLA_FACTOM);                   //Class Instruction for Cryptographic Functions
            Comms.WriteByte((byte)eFunction.INS_FACTOM_GET_ADDRESS);    //Instruction Code for Encryption Key Request INS_FACTOM_GET_ADDRESS
            Comms.WriteByte((byte)eP1.P1_FACTOM_ADDRESS);               //Parameter (P1) reserved for future use P2_FACTOM_ADDRESS_ID
            Comms.WriteByte((byte)eP2.P2_FACTOM_ADDRESS_ID);            //Parameter (P2) reserved for future use
            
            Comms.WriteByte(returnBytes);   //0x20 32-byte encryption key length of expected return
            return Comms.Execute(returnBytes);
        }
    }
}
