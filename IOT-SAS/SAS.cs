using System;
using System.IO.Ports;
using System.Linq;
using System.Text;
using IOTSAS;
using IOTSAS.API;

namespace IOTSAS
{
    public class SAS //: IDisposable
    {

        IComms Comms;


        public API.SHA       SHA         { get; private set; }
        public API.Utility   Utility     { get; private set; }
        public API.Crypto    Crypto      { get; private set; }
        public API.Ed25519   Ed25519     { get; private set; }


        public SAS(IComms comms)
        {
            Comms   = comms;
            SHA     = new SHA(comms);
            Utility = new Utility(comms);
            Crypto  = new Crypto(comms);
            Ed25519 = new Ed25519(comms);
        }
    }    
}
