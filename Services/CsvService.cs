using MassoToolEditor.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace MassoToolEditor.Services
{
    public class CsvService
    {
        public static List<ToolRecord> ImportFromCsv(string filePath, Units units)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File not found: {filePath}");

            var lines = File.ReadAllLines(filePath);
            if (lines.Length < 2)
                throw new InvalidDataException("CSV file must contain at least a header and one data row.");

            var records = new List<ToolRecord>();
            
            // Skip header row
            for (int i = 1; i < lines.Length; i++)
            {
                var fields = ParseCsvLine(lines[i]);
                if (fields.Count < 4)
                    continue; // Skip incomplete rows

                try
                {
                    var record = new ToolRecord();
                    
                    // Parse tool number
                    if (!int.TryParse(fields[0], out int toolNumber) || toolNumber < 1 || toolNumber > 104)
                        continue; // Skip invalid tool numbers
                    
                    record.ToolNumber = toolNumber;
                    
                    // Parse tool name (truncate if too long)
                    record.ToolName = fields[1].Length > 29 ? fields[1].Substring(0, 29) : fields[1];
                    
                    // Parse tool diameter
                    if (float.TryParse(fields[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float diameter))
                    {
                        record.ToolDiameter = units == Units.Inches ? diameter * 25.4f : diameter;
                    }
                    
                    // Parse tool diameter wear
                    if (float.TryParse(fields[3], NumberStyles.Float, CultureInfo.InvariantCulture, out float wear))
                    {
                        record.ToolDiaWear = units == Units.Inches ? wear * 25.4f : wear;
                    }
                    
                    // Z offset defaults to 0 for CSV imports
                    record.ZOffset = 0;
                    
                    records.Add(record);
                }
                catch
                {
                    // Skip rows that can't be parsed
                    continue;
                }
            }

            return records;
        }

        public static void ExportToCsv(string filePath, List<ToolRecord> records, Units units)
        {
            var csv = new StringBuilder();
            
            // Header
            csv.AppendLine("Tool No.,Tool Name,Tool Diameter,Tool Dia Wear");
            
            // Data rows (skip record 0)
            for (int i = 1; i < records.Count; i++)
            {
                var record = records[i];
                
                float diameter = units == Units.Inches ? record.ToolDiameter / 25.4f : record.ToolDiameter;
                float wear = units == Units.Inches ? record.ToolDiaWear / 25.4f : record.ToolDiaWear;
                
                csv.AppendLine($"{record.ToolNumber},{EscapeCsvField(record.ToolName)},{diameter:F3},{wear:F3}");
            }
            
            File.WriteAllText(filePath, csv.ToString());
        }

        private static List<string> ParseCsvLine(string line)
        {
            var fields = new List<string>();
            var field = new StringBuilder();
            bool inQuotes = false;
            bool escapeNext = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (escapeNext)
                {
                    field.Append(c);
                    escapeNext = false;
                }
                else if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        // Escaped quote
                        field.Append('"');
                        i++; // Skip next quote
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    fields.Add(field.ToString().Trim());
                    field.Clear();
                }
                else
                {
                    field.Append(c);
                }
            }

            fields.Add(field.ToString().Trim());
            return fields;
        }

        private static string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field))
                return string.Empty;

            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n") || field.Contains("\r"))
            {
                return "\"" + field.Replace("\"", "\"\"") + "\"";
            }

            return field;
        }
    }
}
