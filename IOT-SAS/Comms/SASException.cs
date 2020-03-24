using System;
namespace IOTSAS
{

    public enum eResultCode : UInt16
    {
        ERR_NOT_SET = 0,
        IO_SUCCESS = 0x9000,
        IO_TIMEOUT = 0x5000,
        IO_TIMEOUT_WAITING_FOR_CLA = 0x5001,
        IO_TIMEOUT_WAITING_FOR_INS = 0x5002,
        IO_TIMEOUT_WAITING_FOR_P1 = 0x5003,
        IO_TIMEOUT_WAITING_FOR_P2 = 0x5004,
        IO_TIMEOUT_WAITING_FOR_N = 0x5005,
        IO_TIMEOUT_WAITING_FOR_DATA = 0x5006,
        IO_DATA_OVERFLOW = 0x5007,
        IO_DATA_UNDERFLOW = 0x5008,
        IO_DATA_INSUFFICENT_RETURN_AMOUNT = 0x5009,
        PORT_PROTOCOL_ERROR = 10,
        PORT_TIMEOUT = 20,
        PORT_NO_DATA = 30,
        MAX_BUFFER_EXCEEDED = 36865
    }

    public class SASException : Exception
    {
    
        public eResultCode ResultCode { get; private set; }
        public UInt16 ErrorCode { get; private set; }
        public byte[] Data { get; private set; }
    
        public SASException(UInt16 errCode, string message = null, byte[] data = null) : base(message)
        {
            ErrorCode = errCode;            
            if (typeof(eResultCode).IsEnumDefined(errCode)) ResultCode = (eResultCode)errCode;            
            
            Data = data;
            if (String.IsNullOrEmpty(message)) message = $"SAS Comms Error {ErrorCode} {ResultCode}";
        }
        
        public SASException(byte[] errCode, string message = null, byte[] data = null) : base(message)
        {
            ErrorCode = BitConverter.ToUInt16(errCode,0);
            if (typeof(eResultCode).IsEnumDefined(errCode)) ResultCode = (eResultCode)ErrorCode;
            
            if (String.IsNullOrEmpty(message)) message = $"SAS Comms Error {ErrorCode} {ResultCode}";
        }
        
        public SASException(eResultCode errCode, string message = null, byte[] data = null) : base(message)
        {
            ErrorCode = (UInt16)errCode;
            ResultCode = errCode;
            
            if (String.IsNullOrEmpty(message)) message = $"SAS Comms Error {ErrorCode} {ResultCode}";
        }
        
    }
}
