using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using Salton.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salton.Helpers
{
    public class BankRecord
    {
        public string Date { get; set; }
        public string Name { get; set; }
        public string TransactionType { get; set; }
        public decimal Amount { get; set; }
        public Currency Currency { get; set; }
    }
    public class TDBankSection {
        public string Name { get; set; }
        public string Type { get; set; }
    }
    public static class PDFHelper
    {
        private const int currencyIndex = 1;
        private const int dateIndex = 3;
        private const int descriptionIndex = 4;
        private const int debitIndex = 5;
        private const int creditIndex = 6;

        private const string StatementPeriod = "Statement Period:";
        private const string ChecksPaid = "Checks Paid";
        private const string Debit = "Debit";
        private const string Credit = "Credit";
        private static TDBankSection[] Sections = new[] {
                new TDBankSection { Name = "Electronic Deposits",Type = Credit },
                new TDBankSection { Name = ChecksPaid,Type = Debit },
                new TDBankSection{ Name = "Electronic Payments",Type = Debit },
                new TDBankSection{ Name = "Other Withdrawals",Type = Debit },
                new TDBankSection{ Name = "Other Credits",Type = Credit }
            };

        internal static IEnumerable<BankRecord> CreateTDUploadFileEx(string[] fileNames)
        {
            var records = new List<BankRecord>();
            foreach (var fileName in fileNames)
            {
                records.AddRange(ReadTDBankRecords(fileName));
            }
            return records;
        }

        private static IEnumerable<BankRecord> ReadTDBankRecords(string fileName)
        {
            var text = ReadTDBankRecordText(fileName);
            return CreateTDBankRecords(text);
        }

        internal static IEnumerable<BankRecord> CreateOTRUploadFiles(string[] fileNames)
        {
            var records = new List<BankRecord>();
            foreach (var fileName in fileNames)
            {
                records.AddRange(ReadOTRBankRecords(fileName));
            }
            return records;
        }

        private static IEnumerable<BankRecord> ReadOTRBankRecords(string fileName)
        {
            var lines = Utilities.ReadCSVLines(fileName);
            return CreateOTRBankRecords(lines);

        }

        private static IEnumerable<BankRecord> CreateOTRBankRecords(List<string[]> lines)
        {
            var list = new List<BankRecord>();
            foreach (var line in lines)
            {
                if (line.Any(p => p.Equals("Account Number", StringComparison.InvariantCultureIgnoreCase)))
                {
                    continue;
                }
                if (line.Count() >= 7)
                {
                    var currency = line[currencyIndex];
                    DateTime? dateValue = Utilities.GetDate(line[dateIndex]);
                    if (dateValue.HasValue)
                    {
                        var description = line[descriptionIndex];
                        var debit = line[debitIndex];
                        var credit = line[creditIndex];
                        var transactionType = string.IsNullOrEmpty(debit) ? Credit : Debit;

                        decimal amount = 0;
                        if (transactionType == Debit)
                        {
                            amount = -decimal.Parse(debit);
                        }
                        else
                        {
                            amount = decimal.Parse(credit);
                        }

                        CultureInfo enUS = new CultureInfo("en-US");

                        list.Add(new BankRecord
                        {
                            Date = dateValue.Value.ToString("MM/dd/yyyy", enUS),
                            Currency = currency.Equals("USD", StringComparison.InvariantCultureIgnoreCase) ? Currency.USD : Currency.CAD,
                            TransactionType = transactionType,
                            Name = description,
                            Amount = amount
                        }) ;

                    }

                }
            }
            return list;
        }

        private static string ReadTDBankRecordText(string fileName)
        {
            var pageText = new StringBuilder();
            using (PdfDocument pdfDocument = new PdfDocument(new PdfReader(fileName)))
            {
                var pageNumbers = pdfDocument.GetNumberOfPages();
                for (int i = 1; i <= pageNumbers; i++)
                {
                    LocationTextExtractionStrategy strategy = new LocationTextExtractionStrategy();
                    PdfCanvasProcessor parser = new PdfCanvasProcessor(strategy);
                    parser.ProcessPageContent(pdfDocument.GetPage(i));
                    pageText.Append(strategy.GetResultantText());
                }
            }
            return pageText.ToString();
        }
        private static IEnumerable<BankRecord> CreateTDBankRecords(string text)
        {
            var records = new List<BankRecord>();

            string[] stringSeparators = new string[] { "\r\n","\n" };
            string[] lines = text.Split(stringSeparators, StringSplitOptions.None);

            var date = GetStatementDate(lines);
            foreach (var section  in Sections)
            {
                records.AddRange(ReadSectionRecord(lines, date,section));
            }
            records.AddRange(ReadCheckPaidSection(lines, date));
            return records;

        }

        private static IEnumerable<BankRecord> ReadCheckPaidSection(string[] lines, DateTime date)
        {
            var records = new List<BankRecord>();
            var index = GetSectionLineIndex(lines, "DAILY ACCOUNT ACTIVITY");
            for (int i = index; i < lines.Length - 1; i++)
            {
                if (lines[i].Contains(ChecksPaid, StringComparison.InvariantCultureIgnoreCase))
                {
                    for (int j = i+2; j < lines.Length - 1; j++)
                    {
                        if (lines[j].Contains("Subtotal", StringComparison.InvariantCultureIgnoreCase))
                        {
                            return records;
                        }
                        records.Add(CreateTDBankRecord(lines[j], date.Year, Debit));
                    }


                }
            }
            return records;
        }

        private static IEnumerable<BankRecord> ReadSectionRecord(string[] lines, DateTime date, TDBankSection section)
        {
            var records = new List<BankRecord>();
            var index = GetSectionLineIndex(lines, section.Name) + 2; // skip header line
            for (int i = index; i < lines.Length - 1; i++)
            {
                if (lines[i].Contains("Subtotal", StringComparison.InvariantCultureIgnoreCase))
                {
                    return records;
                }
                records.Add(CreateTDBankRecord(lines[i], date.Year, section.Type));
            }
            return records;

        }

        private static BankRecord CreateTDBankRecord(string line, int year, string type)
        {
            var words = line.Split(" ");
            return new BankRecord
            {
                Amount = decimal.Parse(words[words.Length - 1]) * (type == Credit ? 1 : -1),
                Date = $"{words[0]}/{year}",
                TransactionType = type,
                Name = GetName(words),
                Currency = Salton.Models.Currency.USD
            };
        }

        private static string GetName(string[] words)
        {
            var names = words.Skip(1).SkipLast(1);
            return string.Join(' ', names).Replace(",", string.Empty);
        }

        private static int GetSectionLineIndex(string[] lines, string name)
        {
            var index = 0;
            foreach (var line in lines)
            {
                if (line.Trim().Equals(name, StringComparison.InvariantCultureIgnoreCase))
                {
                    return index;
                }
                index++;
            }
            return index;
        }

        private static DateTime GetStatementDate(string[] lines)
        {
            var periodLine = lines.FirstOrDefault(p=>p.Contains(StatementPeriod));
            var dateString = periodLine.Replace(StatementPeriod, string.Empty).Split("-")[1].Trim();
            CultureInfo enUS = new CultureInfo("en-US");
            DateTime dateValue;
            if (DateTime.TryParseExact(dateString, "MMM dd yyyy", enUS,
              DateTimeStyles.None, out dateValue))
            {
                return dateValue;
            }
            return DateTime.Now;

        }
    }
}
