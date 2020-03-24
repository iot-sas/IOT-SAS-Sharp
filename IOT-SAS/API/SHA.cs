using System;
using System.IO;

namespace IOTSAS.API
{
    public class SHA
    {
        IComms Comms;
        //StreamReader streamReader;
        
        public SHA(IComms comms)
        {
            Comms = comms;
        }


        public byte[] SHA1(Stream stream)
        {
            byte[] buffer = new byte[Comms.MaxPayloadSize];
            byte[] hash = null;
            int countRX;

            while (stream.Position < stream.Length)
            {
                countRX = stream.Read(buffer, 0, buffer.Length);
                hash = SHA1(buffer, 0, countRX, stream.Position >= stream.Length);
            }
            return hash;
        }
                
        public  byte[] SHA1(byte[] data) { return SHA1(data, 0, data.Length); }
        public  byte[] SHA1(byte[] data, int offset, int count){return SHA1(data, 0, data.Length,true);}
        private byte[] SHA1(byte[] data, int offset, int count, bool final)
        {
            byte returnBytes = final ? (byte)20 : (byte)0;
            
            Comms.WriteReset();

            Comms.WriteByte((byte)eClass.CLA_CRYPTO);     //Class Instruction for Cryptographic Functions
            Comms.WriteByte((byte)eFunction.INS_CRYPTO_HASH);     //Instruction Code for Cryptographic HASH Function
            Comms.WriteByte((byte)eP1.P1_CRYPTO_HASH_SHA1);     //Parameter (P1) for SHA-1 hash function
            Comms.WriteByte(final ? (byte)eP2.P2_FINAL : (byte)eP2.P2_MORE );     //Parameter (P1) for SHA-1 hash function

            Comms.WriteEncodedLength((UInt16)count);   // L
            Comms.Write(data, offset, count);   //N bytes of data
            Comms.WriteByte(returnBytes);  //0x14 20-byte hash of expected return
            return Comms.Execute(returnBytes);
        }


        public byte[] SHA256(Stream stream)
        {
            byte[] buffer = new byte[Comms.MaxPayloadSize];
            byte[] hash = null;
            int countRX;

            while (stream.Position < stream.Length)
            {
                countRX = stream.Read(buffer, 0, buffer.Length);
                hash = SHA256(buffer, 0, countRX, stream.Position >= stream.Length);
            }
            return hash;
        }

        public  byte[] SHA256(byte[] data) { return SHA256(data, 0, data.Length); }
        public  byte[] SHA256(byte[] data, int offset, int count){return SHA256(data, 0, data.Length);}
        private byte[] SHA256(byte[] data, int offset, int count, bool final)
        {
            byte returnBytes = final ? (byte)32 : (byte)0;
            
            Comms.WriteReset();

            Comms.WriteByte((byte)eClass.CLA_CRYPTO);     //Class Instruction for Cryptographic Functions
            Comms.WriteByte((byte)eFunction.INS_CRYPTO_HASH);     //Instruction Code for Cryptographic HASH Function
            Comms.WriteByte((byte)eP1.P1_CRYPTO_HASH_SHA256);     //Parameter (P1) for SHA-1 hash function
            Comms.WriteByte(final ? (byte)eP2.P2_FINAL : (byte)eP2.P2_MORE );     //Parameter (P1) for SHA-1 hash function

            Comms.WriteEncodedLength((UInt16)count);     // L
            Comms.Write(data, offset, count);  //N bytes of data
            Comms.WriteByte(returnBytes);     //0x20 32-byte hash of expected return
            return Comms.Execute(returnBytes);
        }




        public byte[] SHA512(Stream stream)
        {
            byte[] buffer = new byte[Comms.MaxPayloadSize];
            byte[] hash = null;
            int countRX;

            while (stream.Position < stream.Length)
            {
                countRX = stream.Read(buffer, 0, buffer.Length);
                hash = SHA512(buffer, 0, countRX, stream.Position >= stream.Length);
            }
            return hash;
        }

        public  byte[] SHA512(byte[] data) { return SHA512(data, 0, data.Length); }
        public  byte[] SHA512(byte[] data, int offset, int count){return SHA512(data, 0, data.Length);}
        private byte[] SHA512(byte[] data, int offset, int count, bool final)
        {
            byte returnBytes = final ? (byte)0x40 : (byte)0;
            
            Comms.WriteReset();

            Comms.WriteByte((byte)eClass.CLA_CRYPTO);     //Class Instruction for Cryptographic Functions
            Comms.WriteByte((byte)eFunction.INS_CRYPTO_HASH);     //Instruction Code for Cryptographic HASH Function
            Comms.WriteByte((byte)eP1.P1_CRYPTO_HASH_SHA512);     //Parameter (P1) for SHA-1 hash function
            Comms.WriteByte(final ? (byte)eP2.P2_FINAL : (byte)eP2.P2_MORE );     //Parameter (P1) for SHA-1 hash function

            Comms.WriteEncodedLength((UInt16)count);     // L
            Comms.Write(data, offset, count);  //N bytes of data
            Comms.WriteByte(returnBytes);     //64-byte hash of expected return
            return Comms.Execute(returnBytes);
        }

    }
}
