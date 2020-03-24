using System;
namespace IOTSAS
{

    /// 
    /// Class for calculating CRC8/ITU checksums...
    /// 
    static public class CRC8ITU
    {
        static private byte[] table = null;
        const byte Polynomial = 0x7;  //ITU
        const byte XOROut = 0x55;
        


        static public byte CRC8ITUChecksum(this byte[] data)
        {
            if (table == null) table = GenerateTable();
            
            byte c = 0;

            foreach (byte b in data)
            {
                c = table[c ^ b];
            }

            return (byte)(c ^ XOROut);
        }


        static private byte[] GenerateTable()
        {
        
            byte[] csTable = new byte[256];

            for (int i = 0; i < 256; ++i)
            {
                int curr = i;

                for (int j = 0; j < 8; ++j)
                {
                    if ((curr & 0x80) != 0)
                    {
                        curr = (curr << 1) ^ (int)Polynomial;
                    }
                    else
                    {
                        curr <<= 1;
                    }
                }

                csTable[i] = (byte)curr;
            }

            return csTable;
        }


    }
}