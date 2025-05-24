using MassoToolEditor.Models;
using System;
using System.Collections.Generic;
using System.Configuration.Internal;
using System.IO;
using System.Text;

namespace MassoToolEditor.Services
{    public class HtgFileService
    {
        private const int RecordCount = 105;
        private const int RecordSize = 64;
        private const int ExpectedFileSize = RecordCount * RecordSize;
        private const int ToolNameSize = 30;
        
        // Store record 0 as raw bytes - it's never modified
        private static byte[]? _record0Bytes;
        
        enum RecordFieldOffsets
        {
            ToolName = 0,
            Unknown1 = 30, // uint16
            Unknown2 = 32, // uint32
            Unknown3 = 36, // uint32 
            ZOffset = 40, // float
            Unknown4 = 44, // uint32
            ToolDiaWear = 48, // float
            ToolDiameter = 52, // float
            Unknown5 = 56, // uint32
            CRC = 60 // uint32
        }        public static List<ToolRecord> LoadFromFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File not found: {filePath}");

            byte[] fileData = File.ReadAllBytes(filePath);

            if (fileData.Length != ExpectedFileSize)
                throw new InvalidDataException($"Invalid file size. Expected {ExpectedFileSize} bytes, got {fileData.Length} bytes.");

            var records = new List<ToolRecord>();

            // Store record 0 as raw bytes - never modify it
            _record0Bytes = new byte[RecordSize];
            Array.Copy(fileData, 0, _record0Bytes, 0, RecordSize);

            // Validate record 0 has CRC = 0
            uint record0Crc = BitConverter.ToUInt32(_record0Bytes, (int)RecordFieldOffsets.CRC);
            if (record0Crc != 0)
                throw new InvalidDataException($"Record 0 must have CRC = 0, but found CRC = {record0Crc:X8}");

            // Parse records 1-104 only
            for (int i = 1; i < RecordCount; i++)
            {
                int offset = i * RecordSize;
                var record = ParseRecord(fileData, offset, i);

                // Validate unknown fields and CRC for records 1-104
                if (!ValidateRecordCrc(fileData, offset))
                    throw new InvalidDataException($"CRC validation failed for record {i}");

                if (!ValidateUnknownFields(fileData, offset))
                    throw new InvalidDataException($"Invalid / unexpected values in record {i}");

                records.Add(record);
            }

            return records;
        }        public static void SaveToFile(string filePath, List<ToolRecord> records)
        {
            if (records.Count != RecordCount - 1) // Expecting 104 records (1-104)
                throw new ArgumentException($"Expected {RecordCount - 1} records, got {records.Count}");

            if (_record0Bytes == null)
                throw new InvalidOperationException("Record 0 bytes not loaded. Load a file first.");

            byte[] fileData = new byte[ExpectedFileSize];

            // Write record 0 unmodified
            Array.Copy(_record0Bytes, 0, fileData, 0, RecordSize);

            // Write records 1-104
            for (int i = 0; i < records.Count; i++)
            {
                int recordNumber = i + 1; // Records are numbered 1-104
                int offset = recordNumber * RecordSize;
                WriteRecord(fileData, offset, records[i]);
            }

            File.WriteAllBytes(filePath, fileData);
        }private static ToolRecord ParseRecord(byte[] data, int offset, int recordNumber)
        {
            var record = new ToolRecord { ToolNumber = recordNumber };

            // Tool name (30 bytes, ASCII, null-terminated)
            byte[] nameBytes = new byte[ToolNameSize];
            Array.Copy(data, offset + (int)RecordFieldOffsets.ToolName, nameBytes, 0, ToolNameSize);
            int nullIndex = Array.IndexOf(nameBytes, (byte)0);
            if (nullIndex >= 0)
                record.ToolName = Encoding.ASCII.GetString(nameBytes, 0, nullIndex);
            else
                record.ToolName = Encoding.ASCII.GetString(nameBytes).TrimEnd('\0');

            // Skip unknown fields and read the float values
            record.ZOffset = BitConverter.ToSingle(data, offset + (int)RecordFieldOffsets.ZOffset);
            record.ToolDiaWear = BitConverter.ToSingle(data, offset + (int)RecordFieldOffsets.ToolDiaWear);
            record.ToolDiameter = BitConverter.ToSingle(data, offset + (int)RecordFieldOffsets.ToolDiameter);
            
            // CRC is handled internally for validation only

            return record;
        }        private static void WriteRecord(byte[] data, int offset, ToolRecord record)
        {
            // Clear the record area
            Array.Clear(data, offset, RecordSize);

            // Tool name (30 bytes, ASCII, null-terminated)
            byte[] nameBytes = new byte[ToolNameSize];
            if (!string.IsNullOrEmpty(record.ToolName))
            {
                string truncatedName = record.ToolName.Length > 29 ? record.ToolName.Substring(0, 29) : record.ToolName;
                byte[] nameData = Encoding.ASCII.GetBytes(truncatedName);
                Array.Copy(nameData, 0, nameBytes, 0, Math.Min(nameData.Length, 29));
            }
            Array.Copy(nameBytes, 0, data, offset, ToolNameSize);

            // Z offset
            BitConverter.GetBytes(record.ZOffset).CopyTo(data, offset + (int)RecordFieldOffsets.ZOffset);

            // Tool diameter wear (bytes 46-49)
            BitConverter.GetBytes(record.ToolDiaWear).CopyTo(data, offset + (int)RecordFieldOffsets.ToolDiaWear);

            // Tool diameter (bytes 50-53)
            BitConverter.GetBytes(record.ToolDiameter).CopyTo(data, offset + (int)RecordFieldOffsets.ToolDiameter);

            // Calculate and write CRC unless it's all zeroes.
            if (!IsRecordPreCRCAllZeroes(data, offset))
            {
                byte[] recordData = new byte[60];
                Array.Copy(data, offset, recordData, 0, 60);
                uint crc = CrcCalculator.CalculateCrc32(recordData);
                BitConverter.GetBytes(crc).CopyTo(data, offset + (int)RecordFieldOffsets.CRC);
            }
        }

        private static bool IsRecordPreCRCAllZeroes(byte[] data, int offset)
        {
            for (int i = 0; i < (int)RecordFieldOffsets.CRC; i++)
            {
                if (data[offset + i] != 0)
                    return false; // Found a non-zero byte
            }
            return true; // All bytes are zero
        }

        private static bool ValidateRecordCrc(byte[] data, int offset)
        {
            // First check if entire record is zero
            if (IsRecordPreCRCAllZeroes(data, offset))
                return true; // Empty records have no CRC to validate

            uint storedCrc = BitConverter.ToUInt32(data, offset + (int)RecordFieldOffsets.CRC);

            // Extract the first 60 bytes of the record for CRC calculation
            byte[] recordData = new byte[(int)RecordFieldOffsets.CRC];
            Array.Copy(data, offset, recordData, 0, (int)RecordFieldOffsets.CRC);
            uint calculatedCrc = CrcCalculator.CalculateCrc32(recordData);

            return storedCrc == calculatedCrc;
        }

        private static bool ValidateUnknownFields(byte[] data, int offset)
        {
            // Check unknown1 (bytes 30-31)
            if (BitConverter.ToUInt16(data, offset + (int)RecordFieldOffsets.Unknown1) != 0) return false;
            
            // Check unknown2 (bytes 32-35)
            if (BitConverter.ToUInt32(data, offset + (int)RecordFieldOffsets.Unknown2) != 0) return false;
            
            // Check unknown3 (bytes 36-39)
            if (BitConverter.ToUInt32(data, offset + (int)RecordFieldOffsets.Unknown3) != 0) return false;
            
            // Check unknown4 (bytes 44-47)
            if (BitConverter.ToUInt32(data, offset + (int)RecordFieldOffsets.Unknown4) != 0) return false;
            
            // Check unknown5 (bytes 56-59)
            if (BitConverter.ToUInt32(data, offset + (int)RecordFieldOffsets.Unknown5) != 0) return false;

            return true;
        }
    }
}
