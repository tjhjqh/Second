using ClosedXML.Excel;
using Newtonsoft.Json;
using Salton.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Salton.Helpers
{
    public class BankReconciliationHelpers
    {
        private static List<StoreData> StoreDatas = new List<StoreData>();
        private static IEnumerable<Store> Stores;
        private static IEnumerable<BankTransaction> BankData;
        private static string BankFileName = "Bank Statement";
        private static string CashAuditFileName = "Cash Audit";
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
            ReadCashPaymentDataSheets(files.First(p => p.Contains(CashAuditFileName, StringComparison.InvariantCultureIgnoreCase)), date);
            foreach (var store in Stores)
            {
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

        private static void ReadCashPaymentDataSheets(string cashFileName, DateTime date)
        {
            using (var wb = new XLWorkbook(cashFileName, XLEventTracking.Disabled))
            {
                foreach (var store in Stores)
                {

                    var ws = wb.Worksheet(store.Name);
                    if (ws == null)
                    {
                        MessageBox.Show($"Could not find Cash Audit Sheet for store: {store.Name}", "Error");
                    }
                    var storeDataList = ReadCashPaymentSheet(ws);
                    var previousMonthDate = date.AddMonths(-1);
                    var storeData = StoreDatas.FirstOrDefault(p => p.Store.Name == store.Name);
                    if (storeData == null)
                    {
                        storeData = new StoreData
                        {
                            Store = store,
                            CurrentMonthData = new List<CashPayment>(),
                            PreviousMonthData = new List<CashPayment>(),
                            BankReconciliationResult = new List<BankPaymentReconciliation>()
                        };
                        StoreDatas.Add(storeData);
                    }
                    if (storeDataList.Any())
                    {
                        storeData.PreviousMonthData.AddRange(storeDataList.Where(p => p.Date.HasValue && p.Date.Value.Month == previousMonthDate.Month && p.Date.Value.Year == previousMonthDate.Year));
                        storeData.CurrentMonthData.AddRange(storeDataList.Where(p => p.Date.HasValue && p.Date.Value.Month == date.Month && p.Date.Value.Year == date.Year));
                    }

                }
            }
        }
        private static void PopulateResult(string target)
        {

            var mapping = ReadBankFileMapping();
            using (var wb = new XLWorkbook(target, XLEventTracking.Disabled))
            {
                PrepareStoreResultSheet(wb, BankFileName);
                PopulateSheet(BankData.Select(p=>new {
                    p.RowNumber,
                    p.Currency,
                    p.Description,
                    p.Debit,
                    p.Credit,
                }),BankFileName,wb);
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
                    var resultSheetName = $"{storeData.Store.Name} Result";
                    PrepareStoreResultSheet(wb, resultSheetName);
                    foreach (var result in storeData.BankReconciliationResult)
                    {
                        PopulateBankReconciliationResult(wb,ws,storeData,result, thisMonthOSRowNumber, lastMonthOSRowNumber, currentMonthRowNumber, resultSheetName);
                    }

                }
                PopulateStoreFee(wb, ws.Name);

                // Prepare the style for the titles
                var titlesStyle = wb.Style;
                titlesStyle.Font.Bold = true;
                titlesStyle.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Format all titles in one shot
                wb.NamedRanges.NamedRange("Titles").Ranges.Style = titlesStyle;

                wb.Save();
            }

        }

        private static void PopulateStoreFee(XLWorkbook wb, string sheetName)
        {
            var list = StoreDatas.Select(p=>new { 
                Name = $"{p.Store.Name} Fee",
                Fee = GetStoreFee(wb,p.Store)
            });
            PopulateList(list, sheetName,2,"Store Fees",wb);
        }

        private static decimal GetStoreFee(XLWorkbook wb, Store store)
        {
            var feeList = BankData
                .Where(p=>
                    p.Description.Contains($"{store.Id}  DIV") &&
                    (p.Description.Contains("FRA") ||
                    p.Description.Contains("FEE"))
                );
            HighLightBankStatementCell(wb, feeList);
            var debit = feeList.Sum(p=>p.Debit??0);
            var credit= feeList.Sum(p => p.Credit??0);
            return credit - debit;
        }

        private static void PrepareStoreResultSheet(XLWorkbook wb, string name)
        {
            if (wb.Worksheets.Any(p => p.Name == name))
            {
                wb.Worksheets.Delete(name);
            }
            wb.AddWorksheet(name);
        }

        private static void PopulateBankReconciliationResult(XLWorkbook wb, IXLWorksheet ws, StoreData storeData, BankPaymentReconciliation result, 
            int thisMonthOSRowNumber, int lastMonthOSRowNumber, int currentMonthRowNumber, string resultSheetName)
        {
            var payment = storeData.Store.Payments.FirstOrDefault(p=>p.Type == result.Type);
            if (payment != null)
            {
                ws.Cell($"{payment.ColumnLetter}{currentMonthRowNumber}").SetValue(result.BankReconciliationResult.CurrentMonthAmount);
                ws.Cell($"{payment.ColumnLetter}{thisMonthOSRowNumber}").SetValue(result.BankReconciliationResult.CurrentMonthOutStanding);
                ws.Cell($"{payment.ColumnLetter}{lastMonthOSRowNumber}").SetValue(result.BankReconciliationResult.PreviousMonthOutStanding);
            }

            PopulateResultSheet(wb,storeData.Store,payment, result,resultSheetName);

        }

        private static void PopulateResultSheet(XLWorkbook wb, Store store, Payment payment, BankPaymentReconciliation result, string resultSheetName)
        {

            PopulateList(result.BankReconciliationResult.BankReconciliationRecords
                .Where(p => p.CashTransaction == null).Select(p => p.BankTransaction), resultSheetName, 5, $"{store.Name} {payment.Type} MissMatched Records", wb);

            PopulateList(result.BankReconciliationResult.PreviousMonthOutStandingRecords, resultSheetName, 4, $"{store.Name} {payment.Type} Previous Month OutStanding Records", wb);

            PopulateList(result.BankReconciliationResult.CurrentMonthOutStandingRecords, resultSheetName, 4, $"{store.Name} {payment.Type} Current Month OutStanding Records", wb);

            PopulateList(result.BankReconciliationResult.BankReconciliationRecords
            .Select(p => new {
                BankTransactionDate = p.BankTransaction.Date,
                BankTransactionDescription = p.BankTransaction.Description,
                BankTransactionDebit = p.BankTransaction.Debit,
                BankTransactionCredit = p.BankTransaction.Credit,
                CashTransactionDate = p.CashTransaction?.Date,
                CashTransactionDebit = p.CashTransaction?.Debit,
                CashTransactionCredit = p.CashTransaction?.Credit,
                CashTransactionMatched = p.CashTransaction?.Matched,
                SumOff = p.BankTransaction.SumOff
            }),
            resultSheetName, 4, $"{store.Name} {payment.Type} BankReconciliation Records", wb);

            HighLightBankStatementCell(wb,
                result.BankReconciliationResult.BankReconciliationRecords.Where(p=>p.CashTransaction != null).Select(p=>p.BankTransaction)
            );
        }

        private static void HighLightBankStatementCell(XLWorkbook wb, IEnumerable<BankTransaction> records)
        {
            var ws = wb.Worksheet(BankFileName);
            foreach (var matchedRecord in records)
            {
                ws.Cell($"D{matchedRecord.RowNumber}").Style.Fill.BackgroundColor = XLColor.Green;
                ws.Cell($"E{matchedRecord.RowNumber}").Style.Fill.BackgroundColor = XLColor.Green;
            }
        }

        private static void PopulateList<T>(IEnumerable<T> data, string resultSheetName, int endIndex, string title, XLWorkbook wb)
        {
            var ws = wb.Worksheet(resultSheetName);
            var row = ws.LastRowUsed();
            var startRow = row == null ? 1 : row.RowNumber() + 2;
            if (data.Any())
            {
                var titleCell = ws.Cell($"A{startRow}");
                titleCell.SetValue(title);
                titleCell.AsRange().AddToNamed("Titles");
                ws.Range(startRow, 1, startRow, endIndex).Merge().AddToNamed("Titles");
                ws.Cell($"A{startRow + 1}").InsertTable(data);
                ws.Columns().AdjustToContents();
            }
        }

        private static void PopulateSheet<T>(IEnumerable<T> data, string resultSheetName, XLWorkbook wb)
        {
            var ws = wb.Worksheet(resultSheetName);
            if (data.Any())
            {
                ws.Cell($"A1").InsertTable(data);
                ws.Columns().AdjustToContents();
            }
        }

        private static void CleanData()
        {
            StoreDatas = new List<StoreData>();
        }

        private static BankReconciliationResult RunPayment(Store store, Payment payment, DateTime date)
        {
            var previousMonthDate = date.AddMonths(-1);
            Regex rgx = new Regex(payment.Expression);
            var bankPaymentData = BankData.Where(p=>rgx.IsMatch(p.Description) && p.Date.HasValue && p.Date.Value.Year == date.Year && p.Date.Value.Month == date.Month).ToList();
            var cashPaymentData = GetCashPaymentData(store, payment).ToList();
            var list = new List<BankReconciliationRecord>();
            foreach (var bankPayment in bankPaymentData)
            {
                var matchCash = cashPaymentData.FirstOrDefault(p => !p.Matched && p.Credit == bankPayment.Credit && p.Debit == bankPayment.Debit && 
                    p.Date.HasValue && p.Date.Value.Month == date.Month && p.Date.Value.Year == date.Year
                );
                if (matchCash == null)
                {
                    matchCash = cashPaymentData.FirstOrDefault(p => !p.Matched && p.Credit == bankPayment.Credit && p.Debit == bankPayment.Debit);
                }
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

            foreach (var cashPayment in cashPaymentData)
            {
                if (!cashPayment.Matched && cashPayment.Date.HasValue && cashPayment.Date.Value.Month == date.Month && cashPayment.Date.Value.Year == date.Year)
                {
                    if (cashPayment.Credit.HasValue)
                    {
                        var sumList = Utilities.GetCombinations(list.Where(p =>p.CashTransaction ==null && !p.BankTransaction.SumOff && p.BankTransaction.Credit.HasValue).Select(p => p.BankTransaction.Credit ?? 0).ToArray(), cashPayment.Credit.Value, "").ToList();
                        if (sumList.Any(p=>!string.IsNullOrEmpty(p)))
                        {
                            foreach (var factor in sumList.FirstOrDefault(p => !string.IsNullOrEmpty(p)).Split(","))
                            {
                                var value = Utilities.ToDecimal(factor);
                                if (value.HasValue)
                                {
                                    var sumOff = list.FirstOrDefault(p => p.CashTransaction == null && !p.BankTransaction.SumOff && p.BankTransaction.Credit.HasValue && p.BankTransaction.Credit.Value == value);
                                    if (sumOff != null)
                                    {
                                        sumOff.BankTransaction.SumOff = true;
                                        sumOff.CashTransaction = cashPayment;
                                    }
                                }
                            }
                            cashPayment.Matched = true;
                        }
                    }
                }

            }


            var currentMonthOutStandingRecords = cashPaymentData
                .Where(p => !p.Matched && p.Date.HasValue && p.Date.Value.Month == date.Month && p.Date.Value.Year == date.Year);
            var currentMonthOutStandingDebit = currentMonthOutStandingRecords.Sum(p => p.Debit ?? 0);
            var currentMonthOutStandingCredit = currentMonthOutStandingRecords.Sum(p => p.Credit ?? 0);


            var previousMonthOutStandingRecords = list
                .Where(p => p.CashTransaction != null && p.CashTransaction.Date.HasValue && p.CashTransaction.Date.Value.Month == previousMonthDate.Month && p.CashTransaction.Date.Value.Year == previousMonthDate.Year)
                .Select(p=>p.CashTransaction);

            var previousMonthOutStandingDebit = previousMonthOutStandingRecords.Sum(p => p.Debit ?? 0);
            var previousMonthOutStandingCredit = previousMonthOutStandingRecords.Sum(p => p.Credit ?? 0);

            var currentMonthAmountRecords = cashPaymentData
                .Where(p => 
                    p.Date.HasValue &&
                    p.Date.Value.Month == date.Month && p.Date.Value.Year == date.Year
                );

            var currentMonthAmountDebit = currentMonthAmountRecords.Sum(p => p.Debit ?? 0);

            var currentMonthAmountCredit = currentMonthAmountRecords.Sum(p => p.Credit ?? 0);

            return new BankReconciliationResult {
                BankReconciliationRecords = list,
                CurrentMonthOutStanding = currentMonthOutStandingCredit -  currentMonthOutStandingDebit,
                PreviousMonthOutStanding = previousMonthOutStandingCredit - previousMonthOutStandingDebit,
                CurrentMonthAmount = currentMonthAmountCredit - currentMonthAmountDebit,
                CurrentMonthOutStandingRecords = currentMonthOutStandingRecords,
                PreviousMonthOutStandingRecords = previousMonthOutStandingRecords,
                CurrentMonthAmountRecords = currentMonthAmountRecords
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
                    case PaymentType.UnionPay:
                        return storeData.PreviousMonthData.Select(p => new CashTransaction
                        {
                            Date = p.Date,
                            Credit = GetCashAmount(p.Union, AmountType.Credit),
                            Debit = GetCashAmount(p.Union, AmountType.Debit),
                        }).Union(storeData.CurrentMonthData.Select(p => new CashTransaction
                        {
                            Date = p.Date,
                            Credit = GetCashAmount(p.Union, AmountType.Credit),
                            Debit = GetCashAmount(p.Union, AmountType.Debit),
                        })).Where(p => p.Credit.HasValue || p.Debit.HasValue);
                    case PaymentType.Cash:
                        return storeData.PreviousMonthData.Select(p => new CashTransaction
                        {
                            Date = p.Date,
                            Credit = GetCashAmount(p.Cash, AmountType.Credit),
                            Debit = GetCashAmount(p.Cash, AmountType.Debit),
                        }).Union(storeData.CurrentMonthData.Select(p => new CashTransaction
                        {
                            Date = p.Date,
                            Credit = GetCashAmount(p.Cash, AmountType.Credit),
                            Debit = GetCashAmount(p.Cash, AmountType.Debit),
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

        private static IEnumerable<CashPayment> ReadCashPaymentSheet(IXLWorksheet ws)
        {
            var CreditNoteIssuedTitle = $"CREDIT NOTE {ws.Cell("L1").Value}";
            var CreditNoteRedeemedTitle = $"CREDIT NOTE {ws.Cell("M1").Value}";
            var GiftCertificateIssuedTitle = $"GIFT CERTIFICATE {ws.Cell("H1").Value}";
            var GiftCertificateRedeemedTitle = $"GIFT CERTIFICATE {ws.Cell("I1").Value}";

            ws.Cell("H1").Value = GiftCertificateIssuedTitle;
            ws.Cell("I1").Value = GiftCertificateRedeemedTitle;
            ws.Cell("L1").Value = CreditNoteIssuedTitle;
            ws.Cell("M1").Value = CreditNoteRedeemedTitle;

            DataTable dataTable = ws.RangeUsed().AsTable().AsNativeDataTable();
            return dataTable.AsEnumerable().Select(p => new CashPayment
            {
                Date = ReadDatetime(p, "Date"),
                Debit = ReadDecimal(p, "Debit"),
                Amex = ReadDecimal(p, "Amex"),
                Visa = ReadDecimal(p, "VISA"),
                MasterCard = ReadDecimal(p, "MC"),
                Cash = ReadDecimal(p, "CASH"),
                CreditNoteIssued = ReadDecimal(p, CreditNoteIssuedTitle),
                CreditNoteRedeemed = ReadDecimal(p, CreditNoteRedeemedTitle),
                Diff = ReadDecimal(p, "DIFF"),
                Discover = ReadDecimal(p, "Discover"),
                FxRate = ReadDecimal(p, "F/X"),
                Gc = ReadDecimal(p, "Gc"),
                GiftCertificateIssued = ReadDecimal(p, GiftCertificateIssuedTitle),
                GiftCertificateRedeemed = ReadDecimal(p, GiftCertificateRedeemedTitle),
                Gross = ReadDecimal(p, "GROSS"),
                GST = ReadDecimal(p, "GST"),
                NET = ReadDecimal(p, "NET"),
                PST = ReadDecimal(p, "PST"),
                Total = ReadDecimal(p, "TOTAL"),
                Union = ReadDecimal(p, "UNION"),
                UnKnow = ReadDecimal(p, "UNKNOWN"),
                Usd = ReadDecimal(p, "USD"),
            }).ToList();

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
            var index = 2;
            foreach (var line in lines)
            {
                if (line[0] == "Account Number")
                {
                    continue;
                }
                list.Add(ReadBankTransaction(line, index));
                index++;
            }
            return list.Where(p=>p.Date.HasValue && p.Date.Value.Month == date.Month && p.Date.Value.Year == date.Year).ToList();
        }

        private static BankTransaction ReadBankTransaction(string[] line, int index)
        {
            return new BankTransaction { 
                RowNumber = index,
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
            var column = row.Table.Columns[columnName];
            if (column != null)
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
