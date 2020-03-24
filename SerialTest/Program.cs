using System;
using System.IO;
using System.Text;
using IOTSAS;
using IOTSAS.API;

namespace TestSerial
{
    class MainClass
    {
        public static void Main(string[] args)
        {

            try
            {

                string comms = "/dev/ttyUSB0";
                
                //Check commandline args for alternative port
                foreach (var arg in args)
                {
                    if (arg.Contains("tty") || arg.Contains("/dev/") || arg.Contains("COM"))
                    {
                        comms = arg;
                        break;
                    }
                }


                using (var serial = new IOTSAS.Comms.Serial(comms))
                {
                    var sas = new IOTSAS.SAS(serial);
                    Console.WriteLine($"Opened {comms}");


                    var x = sas.Utility.GetFirmwareRevision();
                    Console.WriteLine($"Firmware Revision: {BitConverter.ToString(x)}");
                    
                    //Check commandline args for firmware update file
                    foreach (var arg in args)
                    {
                        if (arg.Contains(".sas"))
                        {
                            FirmwareUpdate(serial,arg);
                            return;
                        }
                    }
                    

                    var y = sas.Ed25519.GetIdPub();
                    Console.WriteLine($"Public ID: {Encoding.ASCII.GetString(y)}");
                    
                    Console.WriteLine($"True Random Number: {sas.Utility.GetTrueRandomBytes(10).ToHexString()}");

                    Console.WriteLine($"EC Address: {Encoding.ASCII.GetString(sas.Crypto.Factom_GetEntryCreditAddress())}");
                    
                    Console.WriteLine($"Signed message: {sas.Ed25519.Sign(Encoding.UTF8.GetBytes("Data to Sign")).ToHexString()}");                    
                    
                    Console.WriteLine($"EntryCredit Address: {Encoding.ASCII.GetString(sas.Crypto.Factom_GetEntryCreditAddress())}");

                    var ThisEXEPath = System.Reflection.Assembly.GetEntryAssembly().Location;
                    using (var stream = File.Open(ThisEXEPath, FileMode.Open))
                    {
                        Console.WriteLine($"SHA512 Hash of {ThisEXEPath}: {sas.SHA.SHA512(stream).ToHexString()}");
                    }
                }

            }
            catch (SASException ex)
            {
                Console.WriteLine($"SAS Error {ex.ResultCode}  {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            
        }


        static public void FirmwareUpdate(IOTSAS.Comms.Serial serial, string filePath)
        {
            using (var firmware = new FirmwareUpdate(serial))
            {
                var file = File.OpenRead(filePath);
                try
                {
                    firmware.Upload(file);
                }
                catch (SASException ex)
                {
                    Console.WriteLine($"Error: {ex.ResultCode} {ex.Message}");
                }
            }
        }
        
    }
}
