using System;
using System.Collections.Generic;
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

    }
}
