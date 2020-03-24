using System;
namespace IOTSAS.API
{

    internal enum eClass : byte
    {
        CLA_NONE = 0x0,
        CLA_CRYPTO = 0x1,
        CLA_FACTOM = 0xFA,
        CLA_DIAG = 0xFF
    }

    internal enum eP2 : byte
    {
        P2_FINAL = 0x00,
        P2_MORE = 0x01,
        P2_FACTOM_ADDRESS_ID = 0x00,
        P2_FACTOM_ADDRESS_EC = 0x01,
    }

    internal enum eP1 : byte
    {
        P1_CRYPTO_HASH_SHA1 = 0x01,
        P1_CRYPTO_HASH_SHA256 = 0x02,
        P1_CRYPTO_HASH_SHA512 = 0x03,
        P1_CRYPTO_MONOTONIC_COUNTER = 0x00,
        P1_CRYPTO_MONOTONIC_SEED = 0x01,
        P1_FACTOM_PUBKEY = 0x00,
        P1_FACTOM_ADDRESS = 0x01,
    }

    internal enum eFunction : byte
    {
        INS_NONE = 0x00,
        INS_CRYPTO_HASH = 0x01,
        INS_CRYPTO_MONOTONIC_COUNTER = 0x04,
        INS_CRYPTO_ED25519_SIGN_RAW_DATA = 0x05,
        INS_FACTOM_GET_ADDRESS = 0x01,
        INS_FACTOM_SIGN_ENTRY = 0x02,
        INS_DIAG_FW_VERSION = 0x00,
        INS_DIAG_HW_VERSION = 0x01,
        INS_DIAG_TOGGLE_LED = 0x02,
        INS_DIAG_LED_OFF = 0x03,
        INS_DIAG_RNG = 0x04,
        INS_DIAG_RESET = 0xFE,
        INS_DIAG_FIRMWARE_UPDATE = 0xFF
    }

}
