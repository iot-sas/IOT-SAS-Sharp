using System;
using System.Linq;
using System.Numerics;
namespace IOTSAS
{
    static public class Helper
    {
        private const string Base58Digits = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz"; 
        
        /// <summary>
        ///     Convenience function to emulate Java's CopyOfRange
        /// </summary>
        /// <param name="src">The byte array to copfrom</param>
        /// <param name="start">The index to cut from</param>
        /// <param name="end">The index to cut to</param>
        /// <returns></returns>
        public static byte[] CopyOfRange(this byte[] src, int start, int end)
        {
            var len = end - start + 1;
            var dest = new byte[len];
            Array.Copy(src, start, dest, 0, len);
            return dest;
        }
    
        /// <summary>
        ///     Converts byte[] to hex string
        /// </summary>
        /// <param name="byteArray"></param>
        /// <returns></returns>
        public static string ToHexString(this byte[] byteArray)
        {
            var hex = BitConverter.ToString(byteArray);
            return hex.Replace("-", "").ToLower();
        }
        
        private static readonly byte[,] ByteLookup = {
            // low nibble
            {0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f},
            // high nibble
            {0x00, 0x10, 0x20, 0x30, 0x40, 0x50, 0x60, 0x70, 0x80, 0x90, 0xa0, 0xb0, 0xc0, 0xd0, 0xe0, 0xf0}
        };
    
        /// <summary>
        ///     Converts string hex into byte[]
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static byte[] DecodeHexIntoBytes(this string input) {
            var result = new byte[(input.Length + 1) >> 1];
            var lastcell = result.Length - 1;
            var lastchar = input.Length - 1;
            // count up in characters, but inside the loop will
            // reference from the end of the input/output.
            for (var i = 0; i < input.Length; i++) {
                // i >> 1    -  (i / 2) gives the result byte offset from the end
                // i & 1     -  1 if it is high-nibble, 0 for low-nibble.
                result[lastcell - (i >> 1)] |= ByteLookup[i & 1, HexToInt(input[lastchar - i])];
            }
            return result;
        }
    
        public static byte[] ToHexBytes(this byte[] data)
        {
            byte[] bytesOut = new byte[data.Length * 2];
            int b;
            for (int i = 0; i < data.Length; i++)
            {
                b = data[i] >> 4;
                bytesOut[i * 2] = (byte)(87 + b + (((b - 10) >> 31) & -39));
                b = data[i] & 0xF;
                bytesOut[i * 2 + 1] = (byte)(87 + b + (((b - 10) >> 31) & -39));
            }
            return bytesOut;
        }
        
        /// <summary>
        ///     Helper function of Hex functions
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static uint HexToInt( this char c)
        {
            if (c >= '0' && c <= '9')
            {
                return (uint) c - 0x30;
            }
        
            switch (c) {
                case 'a':
                case 'A':
                    return 0xA;
                case 'b':
                case 'B':
                    return 0xB;
                case 'c':
                case 'C':
                    return 0xC;
                case 'd':
                case 'D':
                    return 0xD;
                case 'e':
                case 'E':
                    return 0xE;
                case 'f':
                case 'F':
                    return 0xF;
                default:
                    throw new FormatException("Unrecognized hex char " + c);
            }   
        }
        
            
            
        public static byte[] FromBase58(this string addressString)
        {
            // Decode Base58 string to BigInteger 
            BigInteger intData = 0;
            for (int i = 0; i < addressString.Length; i++)
            {
                int digit = Base58Digits.IndexOf(addressString[i]); //Slow
                if (digit < 0)
                    throw new FormatException(string.Format("Invalid Base58 character `{0}` at position {1}", addressString[i], i));
                intData = intData * 58 + digit;
            }

            int leadingZeroCount = addressString.TakeWhile(c => c == '1').Count();  
            var leadingZeros = Enumerable.Repeat((byte)0, leadingZeroCount);
            var bytesWithoutLeadingZeros =
                intData.ToByteArray()
                .Reverse()// to big endian
                .SkipWhile(b => b == 0);//strip sign byte
            var result = leadingZeros.Concat(bytesWithoutLeadingZeros).ToArray();
            return result;
        }
        
        
        public static string ToBase58(this byte[] data)
        {
            // Decode byte[] to BigInteger
            BigInteger intData = 0;
            for (int i = 0; i < data.Length; i++)
            {
                intData = intData * 256 + data[i];
            }

            // Encode BigInteger to Base58 string
            string result = "";
            while (intData > 0)
            {
                int remainder = (int)(intData % 58);
                intData /= 58;
                result = Base58Digits[remainder] + result;
            }

            // Append `1` for each leading 0 byte
            for (int i = 0; i < data.Length && data[i] == 0; i++)
            {
                result = '1' + result;
            }
            return result;
        }
        
            static public byte[] GetCombinedKey(byte[] KeySecret, byte[] KeyPublic)
            {
                var PrivateKey = new byte[64];           
                Array.Copy(KeySecret,PrivateKey,32);
                Array.Copy(KeyPublic,0,PrivateKey,32,32);
                return PrivateKey;
            }
    }
}
