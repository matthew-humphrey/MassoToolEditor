using MassoToolEditor.Models;
using System.Collections.Generic;

namespace MassoToolEditor.Services
{
    public static class UnitConverter
    {
        private const float MmToInchFactor = 1.0f / 25.4f;
        private const float InchToMmFactor = 25.4f;

        public static void ConvertRecords(List<ToolRecord> records, Units fromUnits, Units toUnits)
        {
            if (fromUnits == toUnits)
                return;

            float conversionFactor = fromUnits == Units.Millimeters ? MmToInchFactor : InchToMmFactor;

            foreach (var record in records)
            {
                if (record.ToolNumber == 0)
                    continue; // Skip record 0

                record.ZOffset *= conversionFactor;
                record.ToolDiameter *= conversionFactor;
                record.ToolDiaWear *= conversionFactor;
            }
        }

        public static float ConvertValue(float value, Units fromUnits, Units toUnits)
        {
            if (fromUnits == toUnits)
                return value;

            return fromUnits == Units.Millimeters ? value * MmToInchFactor : value * InchToMmFactor;
        }

        public static string GetUnitAbbreviation(Units units)
        {
            return units == Units.Millimeters ? "mm" : "in";
        }
    }
}
