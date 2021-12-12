using ClosedXML.Excel;
using Salton.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Salton.Helpers
{
    public class BankReconciliationHelpers
    {
        private static string BankFileName = "Bank";
        internal static void Run(string[] files, DateTime date)
        {
            if (!files.Any(p=> p.Contains(BankFileName,StringComparison.InvariantCultureIgnoreCase)))
            {
                MessageBox.Show("Bank file Not Found!", "Error");
                return;
            }
            var bankList = ReadBankData(files.First(p => p.Contains(BankFileName, StringComparison.InvariantCultureIgnoreCase)), date);
        }

        private static IEnumerable<BankTransaction> ReadBankData(string fileName, DateTime date)
        {
            using (var wb = new XLWorkbook(fileName, XLEventTracking.Disabled))
            {
                var ws = wb.Worksheet(1);
                DataTable dataTable = ws.RangeUsed().AsTable().AsNativeDataTable();
                return dataTable.AsEnumerable().Select(p=>new BankTransaction { 
                    Date = ReadDatetime(p,"Date"),
                    Description = p.Field<string>("Description"),
                    Debit = ReadDecimal(p, "Debit"),
                    Credit = ReadDecimal(p, "Credit"),
                }).ToList();
            }
        }

        private static decimal? ReadDecimal(DataRow row, string columnName)
        {
            var columnType = row.Table.Columns[columnName].DataType;
            if (columnType == typeof(string) || columnType == typeof(object))
            {
                decimal value;
                if (decimal.TryParse(row[columnName].ToString(), out value))
                {
                    return value;
                }
            }
            if (columnType == typeof(double))
            {
                return (decimal)row.Field<double>(columnName);
            }
            return null;

        }

        private static DateTime? ReadDatetime(DataRow row, string columnName)
        {
            var columnType = row.Table.Columns[columnName].DataType;
            if (columnType == typeof(string) || columnType == typeof(object))
            {
                DateTime dateValue;
                CultureInfo enUS = new CultureInfo("en-US");
                if (DateTime.TryParseExact(row[columnName].ToString(), "MM/dd/yyyy", enUS,
                  DateTimeStyles.None, out dateValue))
                {
                    return dateValue;
                }
            }
            if (columnType == typeof(DateTime))
            {
                return row.Field<DateTime>(columnName);
            }
            return null;
        }
    }
}
