using System;

namespace MassoToolEditor.Services
{
    public static class CrcCalculator
    {
        private static readonly uint[] CrcTable = new uint[256];

        static CrcCalculator()
        {
            InitializeCrcTable();
        }

        private static void InitializeCrcTable()
        {
            const uint polynomial = 0xEDB88320;
            
            for (uint i = 0; i < 256; i++)
            {
                uint crc = i;
                for (int j = 8; j > 0; j--)
                {
                    if ((crc & 1) == 1)
                        crc = (crc >> 1) ^ polynomial;
                    else
                        crc >>= 1;
                }
                CrcTable[i] = crc;
            }
        }

        public static uint CalculateCrc32(byte[] data)
        {
            uint crc = 0xFFFFFFFF;
            
            foreach (byte b in data)
            {
                crc = (crc >> 8) ^ CrcTable[(crc ^ b) & 0xFF];
            }
            
            return ~crc;
        }

        public static uint CalculateCrc32(byte[] data, int length)
        {
            uint crc = 0xFFFFFFFF;
            
            for (int i = 0; i < length; i++)
            {
                crc = (crc >> 8) ^ CrcTable[(crc ^ data[i]) & 0xFF];
            }
            
            return ~crc;
        }
    }
}
