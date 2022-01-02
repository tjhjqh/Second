using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Salton.Helpers
{
    public static class InventoryHelpers
    {
        private static string ItemReceipt = "Item Receipt";
        private static string ItemFulfillment = "Item Fulfillment";
        private static string AdjustmentResult = "Adjustment Result";

        
        
        private static int TransactionTypeIndex = 3;
        private static int ExhangeRateIndex = 11;
        private static int FxRateIndex = 12;
        private static int CorrectionIndex = 14;
        private static int OriginalIndex = 10;
        private static int ItemIndex = 2;
        private static int QuantityIndex = 7;
        private static int OriginalUnitIndex = 9;

        private static int ResultIndex = 16;

        internal static void Adjust(string fileName)
        {
            using (var excelWorkbook = new XLWorkbook(fileName))
            {
                var sheet = excelWorkbook.Worksheet(2);
                var nonEmptyDataRows = sheet.RowsUsed();

                var currentItem = "";
                decimal? currentPrice = null;
                var itemsWithoutPrice = new List<string>();
                foreach (var dataRow in nonEmptyDataRows)
                {
                    var itemCell = dataRow.Cell(ItemIndex).CachedValue;
                    if (itemCell == null)
                        continue;
                    var item = itemCell.ToString();
                    var transactionTypeValue = dataRow.Cell(TransactionTypeIndex).Value;

                    var fxRate = Utilities.ToDecimal(dataRow.Cell(FxRateIndex).Value.ToString());
                    var exhangeRate = Utilities.ToDecimal(dataRow.Cell(ExhangeRateIndex).Value.ToString());

                    if (transactionTypeValue != null)
                    {
                        // item receipt
                        if (transactionTypeValue.ToString().Equals(ItemReceipt, StringComparison.InvariantCultureIgnoreCase))
                        {
                            var correctionValue = dataRow.Cell(CorrectionIndex).Value;
                            var originalValue = dataRow.Cell(OriginalIndex).Value;
                            if (correctionValue != null && originalValue != null
                                && !string.IsNullOrEmpty(correctionValue.ToString()) && !string.IsNullOrEmpty(originalValue.ToString())
                                && (fxRate.HasValue|| (exhangeRate.HasValue && exhangeRate.Value != 1))
                                )
                            {
                                currentItem = item;
                                var correctionUnitPrice = GetCorrectionUnitPrice(currentItem, dataRow, sheet);
                                if (correctionUnitPrice.HasValue)
                                {
                                    currentPrice = correctionUnitPrice;
                                }
                                else
                                {
                                    itemsWithoutPrice.Add(currentItem);
                                    currentPrice = null;
                                }
                            }
                            else if (currentItem != item)
                            {
                                currentItem = "";
                                currentPrice = null;
                            }

                        }

                        var quantity = dataRow.Cell(QuantityIndex).Value;
                        var originalUnit = dataRow.Cell(OriginalUnitIndex).Value;
                        if (transactionTypeValue.ToString().Equals(ItemFulfillment, StringComparison.InvariantCultureIgnoreCase))
                        {
                            if (
                                item == currentItem &&
                                currentPrice.HasValue &&
                                quantity != null && originalUnit != null
                                )
                            {
                                var diff = Math.Round((currentPrice.Value - Utilities.ToDecimal(originalUnit.ToString()) ?? 0) * Utilities.ToDecimal(quantity.ToString()) ?? 0, 2);
                                dataRow.Cell(ResultIndex - 1).SetValue(currentPrice);
                                dataRow.Cell(ResultIndex).SetValue(diff);
                            }

                        }
                    }

                }
                if (itemsWithoutPrice.Any())
                {
                    PrepareStoreResultSheet(excelWorkbook, AdjustmentResult);
                    PopulateList(itemsWithoutPrice, AdjustmentResult,1,"Result", excelWorkbook);
                }
                excelWorkbook.Save();
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

        private static void PrepareStoreResultSheet(XLWorkbook wb, string name)
        {
            if (wb.Worksheets.Any(p => p.Name == name))
            {
                wb.Worksheets.Delete(name);
            }
            wb.AddWorksheet(name);
        }

        private static decimal? GetCorrectionUnitPrice(string currentItem, IXLRow dataRow, IXLWorksheet sheet)
        {
            var currentCorrectionCell = dataRow.Cell(CorrectionIndex);
            var itemCell = dataRow.Cell(ItemIndex);
            var isPriceCell = dataRow.Cell(ResultIndex+1);
            decimal? value = null;
            for (int i = 0; i < 20; i++)
            {
                var itemCellValue = sheet.Cell($"{itemCell.Address.ColumnLetter}{itemCell.Address.RowNumber + i}").CachedValue.ToString();
                var isPriceCellValue = sheet.Cell($"{isPriceCell.Address.ColumnLetter}{isPriceCell.Address.RowNumber + i}").Value.ToString();
                if (!string.IsNullOrEmpty(itemCellValue) && itemCellValue != currentItem)
                {
                    break;
                }
                var unitPriceCellValue = sheet.Cell($"{currentCorrectionCell.Address.ColumnLetter}{currentCorrectionCell.Address.RowNumber + i}").Value;
                if (isPriceCellValue == "Yes")
                {
                    if (decimal.TryParse(unitPriceCellValue.ToString(), out decimal doubleValue))
                    {
                        return doubleValue;
                    }
                }
            }
            return value;
        }
    }
}
