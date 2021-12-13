using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salton.Helpers
{
    public static class Utilities
    {
        public static List<string[]> ReadCSVLines(string fileName)
        {
            using (var reader = new StreamReader(fileName))
            {
                List<string[]> lines = new List<string[]>();
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    lines.Add(line.Split(","));
                }
                return lines;
            }
        }

        internal static DateTime? ToDate(string value)
        {
            DateTime dateValue;
            CultureInfo enUS = new CultureInfo("en-US");
            if (DateTime.TryParseExact(value, "MM/d/yyyy", enUS,
              DateTimeStyles.None, out dateValue))
            {
                return dateValue;
            }
            return null;
        }

        internal static decimal? ToDecimal(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                decimal decimalValue;
                if (decimal.TryParse(value, out decimalValue))
                {
                    return decimalValue;
                }
            }
            return null;
        }
    }
}
