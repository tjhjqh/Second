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

        internal static IEnumerable<string> GetCombinations(decimal[] set, decimal sum, string values)
        {
            for (int i = 0; i < set.Length; i++)
            {
                decimal left = sum - set[i];
                string vals = set[i] + "," + values;
                if (left == 0)
                {
                    yield return vals;
                }
                else
                {
                    decimal[] possible = set.Take(i).Where(n => n <= sum).ToArray();
                    if (possible.Length > 0)
                    {
                        foreach (string s in GetCombinations(possible, left, vals))
                        {
                            yield return s;
                        }
                    }
                }
            }
        }

        internal static DateTime? GetDate(string dateString)
        {
            CultureInfo enUS = new CultureInfo("en-US");
            DateTime dateValue;
            if (DateTime.TryParseExact(dateString, "yyyy/MM/d", enUS,
                  DateTimeStyles.None, out dateValue))
            {
                return dateValue;
            }
            if (DateTime.TryParseExact(dateString, "yyyy/MM/dd", enUS,
                  DateTimeStyles.None, out dateValue))
            {
                return dateValue;
            }
            if (DateTime.TryParseExact(dateString, "MM/dd/yyyy", enUS,
                  DateTimeStyles.None, out dateValue))
            {
                return dateValue;
            }
            if (DateTime.TryParseExact(dateString, "MM/d/yyyy", enUS,
                  DateTimeStyles.None, out dateValue))
            {
                return dateValue;
            }
            return null;

        }
    }
}
