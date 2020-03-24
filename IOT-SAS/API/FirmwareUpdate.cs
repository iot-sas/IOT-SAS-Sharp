using System;
using System.IO;
using System.Text;
using IOTSAS;
using IOTSAS.Comms;

namespace IOTSAS.API
{
    public class FirmwareUpdate : IDisposable
    {
        
        IComms Comms;
        StreamReader streamReader;
        
        public FirmwareUpdate(IComms comms)
        {
            Comms = comms;
        }
        
       

        public int Upload(string fileName)
        {
            var file = File.OpenRead(fileName);
            return Upload(fileName);
        }
        
        public int Upload(Stream stream) //, byte[] sha256hash, byte[] signature)
        {

            streamReader = new StreamReader(stream);
        
            ushort errcode = 0;
        
            byte[] data = new byte[1024];
        
            byte[] hdblock = new byte[128];

            MemoryStream fwblock = new MemoryStream(30000);
                        
            UInt32 hdlen = 0;
            UInt32 fwlen = 0;
            int linenum = 0;
        
            UInt32 extsegmentaddr = 0x00;
            UInt32 extlinearaddr = 0x00;
            
            while (streamReader.Peek() >= 0)
            {
                //parse line
                uint bytecount      = 0;
                uint address        = 0;
                uint record_type    = 0;
                uint checksum       = 0;
        
               
                var line = streamReader.ReadLine();
                
                ++linenum;
        
                if ( line[0] == ':' )
                {
                    bytecount = (line[1].HexToInt() << 4) + line[2].HexToInt();
        
        
                    checksum += bytecount;
                    address = (line[3].HexToInt() << 12)
                            + (line[4].HexToInt() << 8)
                            + (line[5].HexToInt() << 4)
                            +  line[6].HexToInt();
                    checksum += address >> 8;
                    checksum += address & 0xFF;
        
                    record_type = (line[7].HexToInt() << 4) + line[8].HexToInt();
        
                    checksum += record_type;
        
                    for (int i = 0,j=0; i < bytecount*2; i+=2, ++j )
                    {
                        data[j] = (byte)((line[9+i].HexToInt() << 4) + line[9+i+1].HexToInt());
                        checksum += data[j];
                    }
        
                    byte checkchecksum = (byte) ((line[(int)bytecount*2+9].HexToInt() << 4) + line[(int)bytecount*2+1+9].HexToInt());
                    
                    checksum += checkchecksum;
                    if ( (checksum & 0xff) != 0)
                    {
                        throw new Exception($"Checksum failure at line {linenum}, Computed {checksum}, {checkchecksum} != {~(checksum - checkchecksum) + 1}");
                    }
        
                    switch (record_type)
                    {
                        case 0x00: //data
                            if ( hdlen+bytecount <= 0x80 )
                            {
                                for ( int i = 0; i < bytecount; ++i )
                                {
                                    hdblock[i+hdlen] = data[i];
                                }
                                hdlen += bytecount;
                            }
                            else
                            {
                                if ( bytecount % 4 != 0 )  //Dword count check
                                {
                                    //error
                                }
                                fwblock.Write(data,0,(int)bytecount);
                                fwlen += bytecount;
                            }
                            break;
                        case 0x01: //eof
                            Console.WriteLine("Finalize");
                            break;
                        case 0x02: //extended segment address
                            Console.WriteLine($"Extended segment address {extsegmentaddr}");
                            break;
                        case 0x03: //start segment address
                            Console.WriteLine($"Start segment address {extsegmentaddr}");
                            break;
                        case 0x04: //extended linear address
                            ///this is used to set memory offset to use, this is something the bootloader
                            extlinearaddr = (uint)((data[0] << 24) + (data[1] << 16));
                            //if this is not 0xb0000 then this is an error.
            
                            Console.WriteLine($"Extended linear address {extsegmentaddr}");
                            break;
                        case 0x05: //start linear address
                            Console.WriteLine($"Start linear address {extlinearaddr}");
                            break;
                        default:
                            Console.WriteLine($"Invalid record type on line {linenum}");
                            break;
                    };
                }        
            }

            streamReader.Close();
            
            if ( hdlen != 0x80 )
            {
                //error
                throw new SASException(1,"hdlen != 0x80");
            }

            var nblocks = (fwlen/0x40);  //each block is 64 bytes.
            if ( fwlen % 0x40 != 0 )
            {
                throw new SASException(1,$"Invalid header padding");
            }
        
            //number for each transfer is 128 bytes, so 2 transfers per 256 block
            //and since our block is dwords, divide by size of dword, the last 2 64 k blocks are the key header and trailer
            var numapdu = nblocks - 2;//send 64 bytes at a time and the last 2 64 byte chunks are the key header and trailer
            if ( (fwlen + hdlen ) % 0x40 != 0 )
            {
                //incorrect padding
                throw new SASException(1,"Invalid firmware padding, must be 64byte aligned");
            }
        
        
            //do a quick sanity check on header
            //the first 4 bytes should be PHMC (MCHP)
            UInt16 ndatablocks    = (UInt16)(hdblock[0x10] + (hdblock[0x11] << 8));
            UInt32 fwstartaddress = (UInt32)(hdblock[0x08] + (hdblock[0x09] << 8) + (hdblock[0x0A] << 16) + (hdblock[0x0B] << 24));
            byte[] mchp = { hdblock[0x03],hdblock[0x02], hdblock[0x01], hdblock[0x0]};
        
            if (Encoding.ASCII.GetString(mchp) != "MCHP" || ndatablocks != nblocks - 2 || fwstartaddress != extlinearaddr )
            {
                throw new SASException(1,$"Invalid firmware header");
            }
        
            errcode = SASPreUpgrade(hdblock, 0, (int)hdlen);
            
        
            //if the result is not 0x9000, then there is an issue with the header and / or firmware
            if (errcode != (uint)eResultCode.IO_SUCCESS)
            {
                throw new SASException(errcode, $"Error: Cannot enter firmware update mode.  Received error code {errcode}");
            }
        
            for ( int i = 0,tot = 0; i < numapdu; ++i,tot += 0x40)
            {
                Console.WriteLine($"Sending data payload {i+1} of {numapdu}");

                errcode = SASWriteFirmware(fwblock.GetBuffer(), tot, 0x40);
                                
                if (errcode != (uint)eResultCode.IO_SUCCESS)
                {
                    throw new SASException(errcode,$"Error received ({errcode}) on data payload {i} of {numapdu}");
                }
            }
        
            //after last packet is sent, the firmware will be validated and device will be automatically reset.
            //finalize firmware by validating and setting tag boot tag also reboot when done
            Console.WriteLine("Sending final key header and trailer");
    
            errcode = SASPostUpgrade(fwblock.GetBuffer(), (int)numapdu * 0x40, 0x80);
    
            if (errcode != (uint)eResultCode.IO_SUCCESS)
            {
                throw new SASException(errcode,$"Error received ({errcode}) on firmware finalization");
            }
            

            Console.WriteLine("Firmware update complete");
        
            return errcode;
        }
        
        UInt16 SASPreUpgrade(byte[] data, int offset, int count)
        {
            Console.WriteLine("Entering firmware upgrade mode");
            
            Comms.WriteReset();

            Comms.WriteByte((byte)eClass.CLA_DIAG);     //Class Instruction for Firmware upgrade CLA_DIAG
            Comms.WriteByte((byte)eFunction.INS_DIAG_FIRMWARE_UPDATE);     //INS_DIAG_FIRMWARE_UPDATE
            Comms.WriteByte(0x02);     //Instruction Code for Cryptographic HASH Function
            Comms.WriteByte((byte)eP2.P2_FINAL);     //P2_FINAL

            Comms.WriteEncodedLength((UInt16)count);   // L
            Comms.Write(data, offset, count);   //N bytes of data
            
            UInt16 errCode;
            Comms.Execute(0, out errCode);
            return errCode;
        }
        
        UInt16 SASWriteFirmware(byte[] data, int offset, int count)
        {            
            Console.WriteLine($"Sending {count} bytes....");
            Comms.WriteReset();

            Comms.WriteByte((byte)eClass.CLA_DIAG);     //Class Instruction for Firmware upgrade CLA_DIAG
            Comms.WriteByte((byte)eFunction.INS_DIAG_FIRMWARE_UPDATE);     //INS_DIAG_FIRMWARE_UPDATE
            Comms.WriteByte(0x02);     //
            Comms.WriteByte((byte)eP2.P2_MORE);     //P2_MORE
            
            Comms.WriteEncodedLength((UInt16)count);   // L
            Comms.Write(data, offset, count);   //N bytes of data
            UInt16 errCode;
            Comms.Execute(0, out errCode);
            return errCode;
        }


        UInt16 SASPostUpgrade(byte[] data, int offset, int count)
        {
            Console.WriteLine("Sending final key header and trailer");
            
            Comms.WriteReset();

            Comms.WriteByte((byte)eClass.CLA_DIAG);     //Class Instruction for Firmware upgrade CLA_DIAG
            Comms.WriteByte((byte)eFunction.INS_DIAG_FIRMWARE_UPDATE);     //INS_DIAG_FIRMWARE_UPDATE
            Comms.WriteByte(0x02);     //
            Comms.WriteByte((byte)eP2.P2_FINAL);     //P2_FINAL

            Comms.WriteEncodedLength((UInt16)count);   // L
            Comms.Write(data, offset, count);   //N bytes of data
            
            UInt16 errCode;
            Comms.Execute(0, out errCode);
            return errCode;
            
        }

        public void Dispose()
        {
            streamReader?.Dispose();
        }

    }
}
