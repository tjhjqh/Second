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
                var priceUsed = false;
                foreach (var dataRow in nonEmptyDataRows)
                {
                    var itemCell = dataRow.Cell(ItemIndex).CachedValue;
                    if (itemCell == null)
                        continue;
                    var item = itemCell.ToString();
                    var transactionTypeValue = dataRow.Cell(TransactionTypeIndex).Value;
                    if (priceUsed && transactionTypeValue != null && transactionTypeValue.ToString().Equals(ItemReceipt, StringComparison.InvariantCultureIgnoreCase))
                    {
                        currentPrice = null;
                        currentItem = "";
                        priceUsed = false;
                    }
                    var quantity = dataRow.Cell(QuantityIndex).Value;
                    var originalUnit = dataRow.Cell(OriginalUnitIndex).Value;
                    if (
                        item == currentItem &&
                        currentPrice.HasValue && 
                        transactionTypeValue!=null && 
                        transactionTypeValue.ToString().Equals(ItemFulfillment,StringComparison.InvariantCultureIgnoreCase) &&
                        quantity != null && originalUnit !=null 
                        )
                    {
                        var diff = (currentPrice.Value - Utilities.ToDecimal(originalUnit.ToString()) ?? 0) * Utilities.ToDecimal(quantity.ToString()) ?? 0;
                        dataRow.Cell(ResultIndex-1).SetValue(currentPrice);
                        dataRow.Cell(ResultIndex).SetValue(diff);
                        priceUsed = true;
                    }
                    else if (item != currentItem)
                    {
                        var correctionValue = dataRow.Cell(CorrectionIndex).Value;
                        var originalValue = dataRow.Cell(OriginalIndex).Value;
                        if (transactionTypeValue != null && transactionTypeValue.ToString().Equals(ItemReceipt, StringComparison.InvariantCultureIgnoreCase)
                            && correctionValue != null && originalValue != null
                            && !string.IsNullOrEmpty(correctionValue.ToString()) && !string.IsNullOrEmpty(originalValue.ToString())
                            )
                        {
                            if (correctionValue.ToString() != originalValue.ToString())
                            {
                                currentItem = item;
                                priceUsed = false;
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
            decimal? value = null;
            for (int i = 0; i < 20; i++)
            {
                var itemCell = dataRow.Cell(ItemIndex);
                var itemCellValue = itemCell.CachedValue.ToString();
                var itemCellValue1 = sheet.Cell($"{itemCell.Address.ColumnLetter}{itemCell.Address.RowNumber + i + 1}").Value;
                var itemCellValue2 = sheet.Cell($"{itemCell.Address.ColumnLetter}{itemCell.Address.RowNumber + i + 2}").Value;

                var unitPriceCellValue = sheet.Cell($"{currentCorrectionCell.Address.ColumnLetter}{currentCorrectionCell.Address.RowNumber + i}").Value;
                var unitPriceCellValue1 = sheet.Cell($"{currentCorrectionCell.Address.ColumnLetter}{currentCorrectionCell.Address.RowNumber + i + 1}").Value;
                var unitPriceCellValue2 = sheet.Cell($"{currentCorrectionCell.Address.ColumnLetter}{currentCorrectionCell.Address.RowNumber + i + 2}").Value;

                if (itemCellValue == currentItem && 
                    unitPriceCellValue != null && !string.IsNullOrEmpty(unitPriceCellValue.ToString()) 
                    && (unitPriceCellValue1 == null || string.IsNullOrEmpty(unitPriceCellValue1.ToString())) 
                    && (unitPriceCellValue2 == null || string.IsNullOrEmpty(unitPriceCellValue2.ToString()))
                    && (itemCellValue1 == null || string.IsNullOrEmpty(itemCellValue1.ToString()) || itemCellValue1.ToString() == currentItem)
                    && (itemCellValue2 == null || string.IsNullOrEmpty(itemCellValue2.ToString()) || itemCellValue2.ToString() == currentItem)
                    )
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
