using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IOTSAS;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Leds;

namespace MeadowTest
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        const int pulseDuration = 3000;
        RgbPwmLed rgbPwmLed;

        public MeadowApp()
        {
            rgbPwmLed = new RgbPwmLed(Device,
                       Device.Pins.OnboardLedRed,
                       Device.Pins.OnboardLedGreen,
                       Device.Pins.OnboardLedBlue);


            Task.Run(() => { PulseRgbPwmLed(); });

            try
            {
                //var serial = new SAS_I2C(Device.CreateI2cBus()); I2C NOT WORKING AT THIS TIME
                var serial = new SAS_Serial(Device.CreateSerialPort(Device.SerialPortNames.Com4, 115200));

                Console.WriteLine("Running.....");
                var sas = new IOTSAS.SAS(serial);

                var x = sas.Utility.GetFirmwareRevision();
                Console.WriteLine($"Firmware Revision: {BitConverter.ToString(x)}");

                var y = sas.Ed25519.GetIdPub();
                Console.WriteLine($"Public ID: {Encoding.ASCII.GetString(y)}");

                Console.WriteLine($"True Random Number: {sas.Utility.GetTrueRandomBytes(10).ToHexString()}");

                Console.WriteLine($"EntryCredit Address: {Encoding.ASCII.GetString(sas.Crypto.Factom_GetEntryCreditAddress())}");

                var ThisEXEPath = System.Reflection.Assembly.GetEntryAssembly().Location;
                using (var stream = File.Open(ThisEXEPath, FileMode.Open))
                {
                    Console.WriteLine($"SHA512 Hash of {ThisEXEPath}: {sas.SHA.SHA512(stream).ToHexString()}");
                }
            }
            catch (SASException ex)
            {
                Console.WriteLine($"SAS Error {ex.ResultCode}  {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error {ex.Message}");
            }

        }

        protected void PulseRgbPwmLed()
        {
            while (true)
            {
                Pulse(Color.Red);
                Pulse(Color.Green);
                Pulse(Color.Blue);
            }
        }

        protected void Pulse(Color color)
        {
            rgbPwmLed.StartPulse(color);
            Thread.Sleep(pulseDuration);
            rgbPwmLed.Stop();
        }
    }
}
