# IOT-SAS Signed at Source™ microcontroller  
## .NET DLL Driver for Serial/USB & Meadow  
### IoT Data Integrity from its origin https://iot-sas.tech
  
Availabe as a nuget package.

Simple example of signing a jpeg image from onboard camera, and proving it originated from this device:
  
  
**Connect to SAS**

    var serial = new IOTSAS.Comms.Serial("/dev/ttyUSB0"))
    var sas = new IOTSAS.SAS(serial);  

**Get unique public key of this SAS**

    var PublicID = sas.Ed25519.GetIdPub();  


**Get SHA512 Hash of an onboard camera image**

    using (var stream = File.Open("/images/cctv.jpeg", FileMode.Open))  
    {  
        var SHA512_Hash = sas.SHA.SHA512(stream);  
    }  

**Sign the above image Hash, using internal private key unique to this SAS device.**

    var SignedMessage = sas.Ed25519.Sign(SHA512_Hash);  


Save those outputs, and store with the jpeg.  Consider adding GPS & Time as metadata.