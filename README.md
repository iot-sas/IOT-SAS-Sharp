# IOT-SAS Signed at Source™ microcontroller  
## .NET DLL Driver for Serial/USB & Meadow  
### IoT Data Integrity from its origin https://iot-sas.tech
  
  
  
Connect to SAS  

    var serial = new IOTSAS.Comms.Serial("/dev/ttyUSB0"))
    var sas = new IOTSAS.SAS(serial);  

  Get unique public key of this SAS  

    var PublicID = sas.Ed25519.GetIdPub();  

Sign a message, using internal private key unique to this SAS device.

    var SignedMessage = sas.Ed25519.Sign(Encoding.UTF8.GetBytes("Data to Sign")");  

Get 10 bytes of True Random Numbers

    var TrueRandomBytes = sas.Utility.GetTrueRandomNumber(10);

Get SHA512 Hash of a file

    using (var stream = File.Open("/SomeFile.bin", FileMode.Open))  
    {  
        var SHA512_Hash = sas.SHA.SHA512(stream);  
    }  
