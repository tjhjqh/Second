using ClosedXML.Excel;
using Newtonsoft.Json;
using Salton.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Salton.Helpers
{
    public class BankReconciliationHelpers
    {
        private static List<StoreData> StoreDatas = new List<StoreData>();
        private static IEnumerable<Store> Stores;
        private static IEnumerable<BankTransaction> BankData;
        private static string BankFileName = "Bank Statement";
        internal static void Run(string[] files, DateTime date)
        {
            Stores = ReadStoreData();
            if (!files.Any(p=> p.Contains(BankFileName,StringComparison.InvariantCultureIgnoreCase)))
            {
                MessageBox.Show("Bank file Not Found!", "Error");
                return;
            }
            BankData = ReadBankData(files.First(p => p.Contains(BankFileName, StringComparison.InvariantCultureIgnoreCase)), date);
            if (!BankData.Any())
            {
                MessageBox.Show("No Bank Data Found!", "Error");
            }
            foreach (var store in Stores)
            {
                ReadCashPaymentDatas(files,store, date);
                foreach (var payment in store.Payments)
                {
                    RunPayment(store, payment,date);
                }

            }
        }

        private static void RunPayment(Store store, Payment payment, DateTime date)
        {
            Regex rgx = new Regex(payment.Expression);
            var bankPaymentData = BankData.Where(p=>rgx.IsMatch(p.Description)).ToList();
        }

        private static void ReadCashPaymentDatas(string[] files, Store store,DateTime date)
        {
            var storeFileName = $"{store.Name} Cash audit";
            foreach (var file in files.Where(p=>p.Contains(storeFileName,StringComparison.InvariantCultureIgnoreCase)))
            {
                var storeDataList = ReadCashPaymentData(file);
                var previousMonthDate = date.AddMonths(-1);
                var storeData = StoreDatas.FirstOrDefault(p=>p.Store.Name == store.Name);
                if (storeData == null)
                {
                    storeData = new StoreData { 
                        Store = store,
                        CurrentMonthData = new List<CashPayment>(),
                        PreviousMonthData = new List<CashPayment>()
                    };
                    StoreDatas.Add(storeData);
                }
                if (storeDataList.Any())
                {
                    storeData.PreviousMonthData.AddRange(storeDataList.Where(p=>p.Date.HasValue && p.Date.Value.Month == previousMonthDate.Month && p.Date.Value.Year == previousMonthDate.Year));
                    storeData.CurrentMonthData.AddRange(storeDataList.Where(p => p.Date.HasValue && p.Date.Value.Month == date.Month && p.Date.Value.Year == date.Year));
                }
            }
        }

        private static IEnumerable<CashPayment> ReadCashPaymentData(string fileName)
        {
            using (var wb = new XLWorkbook(fileName, XLEventTracking.Disabled))
            {
                var ws = wb.Worksheet(1);
                ws.Cell("H1").Value = $"GIFT CERTIFICATE {ws.Cell("H1").Value}";
                ws.Cell("I1").Value = $"GIFT CERTIFICATE {ws.Cell("I1").Value}";
                ws.Cell("L1").Value = $"CREDIT NOTE {ws.Cell("L1").Value}";
                ws.Cell("M1").Value = $"CREDIT NOTE {ws.Cell("M1").Value}";
                DataTable dataTable = ws.RangeUsed().AsTable().AsNativeDataTable();
                return dataTable.AsEnumerable().Select(p => new CashPayment
                {
                    Date = ReadDatetime(p, "Date"),
                }).ToList();

            }

        }

        private static IEnumerable<Store> ReadStoreData()
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), @"Store\Bank\BankReconciliationTemplate.json");
            var jsonText = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<IEnumerable<Store>>(jsonText);
        }

        private static IEnumerable<BankTransaction> ReadBankData(string fileName, DateTime date)
        {
            if (fileName.EndsWith("CSV", StringComparison.InvariantCultureIgnoreCase))
            {
                return ReadBankCSVData(fileName, date);
            }
            return ReadBankXLSData(fileName, date);

        }

        private static IEnumerable<BankTransaction> ReadBankCSVData(string fileName, DateTime date)
        {
            var list = new List<BankTransaction>();
            var lines = Utilities.ReadCSVLines(fileName);
            foreach (var line in lines)
            {
                if (line[0] == "Account Number")
                {
                    continue;
                }
                list.Add(ReadBankTransaction(line));
            }
            return list.Where(p=>p.Date.HasValue && p.Date.Value.Month == date.Month && p.Date.Value.Year == date.Year).ToList();
        }

        private static BankTransaction ReadBankTransaction(string[] line)
        {
            return new BankTransaction { 
                Currency = line[1],
                Date = Utilities.ToDate(line[3]),
                Description = line[4],
                Debit = Utilities.ToDecimal(line[5]),
                Credit = Utilities.ToDecimal(line[6]),
            };
        }

        private static IEnumerable<BankTransaction> ReadBankXLSData(string fileName, DateTime date)
        {
            using (var wb = new XLWorkbook(fileName, XLEventTracking.Disabled))
            {
                var ws = wb.Worksheet(1);
                DataTable dataTable = ws.RangeUsed().AsTable().AsNativeDataTable();
                return dataTable.AsEnumerable().Select(p => new BankTransaction
                {
                    Currency = p.Field<string>("Currency"),
                    Date = ReadDatetime(p, "Date"),
                    Description = p.Field<string>("Description"),
                    Debit = ReadDecimal(p, "Debit Amount"),
                    Credit = ReadDecimal(p, "Credit Amount"),
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
            if (columnType == typeof(double))
            {
                return DateTime.FromOADate(row.Field<double>(columnName));
            }
            return null;
        }
    }
}
