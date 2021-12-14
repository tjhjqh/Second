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
        private static string ResultSheetName = "Bank Conciliation Result";
        private static List<StoreData> StoreDatas = new List<StoreData>();
        private static IEnumerable<Store> Stores;
        private static IEnumerable<BankTransaction> BankData;
        private static string BankFileName = "Bank Statement";
        internal static void Run(string[] files, string target, DateTime date)
        {
            CleanData();
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
                    var result = RunPayment(store, payment,date);
                    var storeData = StoreDatas.FirstOrDefault(p => p.Store.Name == store.Name);
                    if (storeData != null)
                    {
                        storeData.BankReconciliationResult.Add(new BankPaymentReconciliation
                        {
                            Type = payment.Type,
                            BankReconciliationResult = result,
                        });
                    }
                }
            }
            PopulateResult(target);
        }

        private static void PopulateResult(string target)
        {

            var mapping = ReadBankFileMapping();
            using (var wb = new XLWorkbook(target, XLEventTracking.Disabled))
            {
                var ws = wb.Worksheet(1);
                var cell = ws.Range("A:A").Search(mapping.ThisMonthOutStanding, CompareOptions.OrdinalIgnoreCase, false).FirstOrDefault();
                if (cell == null)
                {
                    MessageBox.Show("Could not find the 'O/S This Mth.' row in target file ", "Error");
                }
                var thisMonthOSRowNumber = cell.Address.RowNumber;
                var lastMonthOSRowNumber = cell.Address.RowNumber + 1;
                var currentMonthRowNumber = cell.Address.RowNumber -1;

                foreach (var storeData in StoreDatas)
                {
                    foreach (var result in storeData.BankReconciliationResult)
                    {
                        PopulateBankReconciliationResult(wb,ws,storeData,result, thisMonthOSRowNumber, lastMonthOSRowNumber, currentMonthRowNumber);
                    }

                }

                // Prepare the style for the titles
                var titlesStyle = wb.Style;
                titlesStyle.Font.Bold = true;
                titlesStyle.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                titlesStyle.Fill.BackgroundColor = XLColor.Cyan;

                // Format all titles in one shot
                wb.NamedRanges.NamedRange("Titles").Ranges.Style = titlesStyle;

                wb.Save();
            }

        }

        private static void PopulateBankReconciliationResult(XLWorkbook wb, IXLWorksheet ws, StoreData storeData, BankPaymentReconciliation result, 
            int thisMonthOSRowNumber, int lastMonthOSRowNumber, int currentMonthRowNumber)
        {
            var payment = storeData.Store.Payments.FirstOrDefault(p=>p.Type == result.Type);
            if (payment != null)
            {
                ws.Cell($"{payment.ColumnLetter}{currentMonthRowNumber}").SetValue(result.BankReconciliationResult.CurrentMonthAmount);
                ws.Cell($"{payment.ColumnLetter}{thisMonthOSRowNumber}").SetValue(result.BankReconciliationResult.CurrentMonthOutStanding);
                ws.Cell($"{payment.ColumnLetter}{lastMonthOSRowNumber}").SetValue(result.BankReconciliationResult.PreviousMonthOutStanding);
            }
            if (result.BankReconciliationResult.BankReconciliationRecords.Any(p => p.CashTransaction == null))
            {
                PopulateMissMatchedSheet(wb,storeData.Store,payment, result.BankReconciliationResult.BankReconciliationRecords
                .Where(p => p.CashTransaction == null).Select(p => p.BankTransaction));
            }

        }

        private static void PopulateMissMatchedSheet(XLWorkbook wb, Store store, Payment payment, IEnumerable<BankTransaction> data)
        {
            if (!wb.Worksheets.Any(p => p.Name == ResultSheetName))
            {
                wb.AddWorksheet(ResultSheetName);
            }
            var ws = wb.Worksheet(ResultSheetName);
            var row = ws.LastRowUsed();
            var startRow = row == null ? 1 : row.RowNumber() + 1;
            var titleCell = ws.Cell($"A{startRow}");
            titleCell.SetValue($"{store.Name} {payment.Type} MissMatched Records");
            titleCell.AsRange().AddToNamed("Titles");
            ws.Range(startRow, 1, startRow, 5).Merge().AddToNamed("Titles");
            ws.Cell($"A{startRow + 1}").InsertTable(data);
        }

        private static void CleanData()
        {
            StoreDatas = new List<StoreData>();
        }

        private static BankReconciliationResult RunPayment(Store store, Payment payment, DateTime date)
        {
            Regex rgx = new Regex(payment.Expression);
            var bankPaymentData = BankData.Where(p=>rgx.IsMatch(p.Description) && p.Date.HasValue && p.Date.Value.Year == date.Year && p.Date.Value.Month == date.Month).ToList();
            var cashPaymentData = GetCashPaymentData(store, payment).ToList();
            var list = new List<BankReconciliationRecord>();
            foreach (var bankPayment in bankPaymentData)
            {
                var matchCash = cashPaymentData.FirstOrDefault(p => !p.Matched && p.Credit == bankPayment.Credit && p.Debit == bankPayment.Debit);
                if (matchCash != null)
                {
                    matchCash.Matched = true;
                }
                var record = new BankReconciliationRecord { 
                    BankTransaction = bankPayment,
                    CashTransaction = matchCash
                };
                list.Add(record);
            }
            var currentMonthOutStandingDebit = cashPaymentData
                .Where(p => !p.Matched && p.Date.HasValue && p.Date.Value.Month == date.Month && p.Date.Value.Year == date.Year)
                .Sum(p => p.Debit ?? 0);
            var currentMonthOutStandingCredit = cashPaymentData
                .Where(p => !p.Matched && p.Date.HasValue && p.Date.Value.Month == date.Month && p.Date.Value.Year == date.Year)
                .Sum(p => p.Credit ?? 0);

            var previousMonthDate = date.AddMonths(-1);
            var previousMonthOutStandingDebit = list
                .Where(p =>p.CashTransaction!=null && p.CashTransaction.Date.HasValue && p.CashTransaction.Date.Value.Month == previousMonthDate.Month && p.CashTransaction.Date.Value.Year == previousMonthDate.Year)
                .Sum(p => p.CashTransaction.Debit ?? 0);
            var previousMonthOutStandingCredit = list
                .Where(p => p.CashTransaction != null && p.CashTransaction.Date.HasValue && p.CashTransaction.Date.Value.Month == previousMonthDate.Month && p.CashTransaction.Date.Value.Year == previousMonthDate.Year)
                .Sum(p => p.CashTransaction.Credit ?? 0);

            var currentMonthAmountDebit = list
                .Where(p => p.CashTransaction != null &&
                    p.CashTransaction.Date.HasValue && p.BankTransaction.Date.Value.Month == p.CashTransaction.Date.Value.Month && p.BankTransaction.Date.Value.Year == p.CashTransaction.Date.Value.Year &&
                    p.BankTransaction.Date.HasValue && p.BankTransaction.Date.Value.Month == date.Month && p.BankTransaction.Date.Value.Year == date.Year
                )
                .Sum(p => p.BankTransaction.Debit ?? 0);

            var currentMonthAmountCredit = list
                .Where(p => p.CashTransaction != null &&
                    p.CashTransaction.Date.HasValue && p.BankTransaction.Date.Value.Month == p.CashTransaction.Date.Value.Month && p.BankTransaction.Date.Value.Year == p.CashTransaction.Date.Value.Year &&
                    p.BankTransaction.Date.HasValue && p.BankTransaction.Date.Value.Month == date.Month && p.BankTransaction.Date.Value.Year == date.Year
                )
                .Sum(p => p.BankTransaction.Credit ?? 0);

            return new BankReconciliationResult {
                BankReconciliationRecords = list,
                CurrentMonthOutStanding = currentMonthOutStandingCredit -  currentMonthOutStandingDebit,
                PreviousMonthOutStanding = previousMonthOutStandingCredit - previousMonthOutStandingDebit,
                CurrentMonthAmount = currentMonthAmountCredit - currentMonthAmountDebit
            };
        }

        private static IEnumerable<CashTransaction> GetCashPaymentData(Store store, Payment payment)
        {
            var storeData = StoreDatas.FirstOrDefault(p => p.Store.Name == store.Name);
            if (storeData != null)
            {
                switch (payment.Type)
                {
                    case PaymentType.Debit:
                        return storeData.PreviousMonthData.Select(p=>new CashTransaction { 
                            Date = p.Date,
                            Credit = GetCashAmount(p.Debit, AmountType.Credit),
                            Debit = GetCashAmount(p.Debit, AmountType.Debit),
                        }).Union(storeData.CurrentMonthData.Select(p => new CashTransaction
                        {
                            Date = p.Date,
                            Credit = GetCashAmount(p.Debit, AmountType.Credit),
                            Debit = GetCashAmount(p.Debit, AmountType.Debit),
                        })).Where(p=>p.Credit.HasValue || p.Debit.HasValue);
                    case PaymentType.AMEX:
                        return storeData.PreviousMonthData.Select(p => new CashTransaction
                        {
                            Date = p.Date,
                            Credit = GetCashAmount(p.Amex, AmountType.Credit),
                            Debit = GetCashAmount(p.Amex, AmountType.Debit),
                        }).Union(storeData.CurrentMonthData.Select(p => new CashTransaction
                        {
                            Date = p.Date,
                            Credit = GetCashAmount(p.Amex, AmountType.Credit),
                            Debit = GetCashAmount(p.Amex, AmountType.Debit),
                        })).Where(p => p.Credit.HasValue || p.Debit.HasValue);
                    case PaymentType.Visa:
                        return storeData.PreviousMonthData.Select(p => new CashTransaction
                        {
                            Date = p.Date,
                            Credit = GetCashAmount(p.Visa, AmountType.Credit),
                            Debit = GetCashAmount(p.Visa, AmountType.Debit),
                        }).Union(storeData.CurrentMonthData.Select(p => new CashTransaction
                        {
                            Date = p.Date,
                            Credit = GetCashAmount(p.Visa, AmountType.Credit),
                            Debit = GetCashAmount(p.Visa, AmountType.Debit),
                        })).Where(p => p.Credit.HasValue || p.Debit.HasValue);
                    case PaymentType.MasterCard:
                        return storeData.PreviousMonthData.Select(p => new CashTransaction
                        {
                            Date = p.Date,
                            Credit = GetCashAmount(p.MasterCard, AmountType.Credit),
                            Debit = GetCashAmount(p.MasterCard, AmountType.Debit),
                        }).Union(storeData.CurrentMonthData.Select(p => new CashTransaction
                        {
                            Date = p.Date,
                            Credit = GetCashAmount(p.MasterCard, AmountType.Credit),
                            Debit = GetCashAmount(p.MasterCard, AmountType.Debit),
                        })).Where(p => p.Credit.HasValue || p.Debit.HasValue);


                }
            }
            return null;
        }

        private static decimal? GetCashAmount(decimal? debit, AmountType type)
        {
            if (type == AmountType.Credit && debit.HasValue && debit.Value > 0)
            {
                return debit.Value;
            }
            if (type == AmountType.Debit && debit.HasValue && debit.Value < 0)
            {
                return -debit.Value;
            }
            return null;
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
                        PreviousMonthData = new List<CashPayment>(),
                        BankReconciliationResult = new List<BankPaymentReconciliation>()
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
                    Debit = ReadDecimal(p, "Debit"),
                    Amex = ReadDecimal(p, "Amex"),
                    Visa = ReadDecimal(p, "VISA"),
                    MasterCard = ReadDecimal(p, "MC"),
                }).ToList();

            }

        }

        private static BankFileMapping ReadBankFileMapping()
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), @"Store\Bank\Mapping.json");
            var jsonText = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<BankFileMapping>(jsonText);
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
