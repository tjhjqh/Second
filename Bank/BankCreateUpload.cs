using Salton.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Salton.Bank
{
    public partial class BankCreateUpload : Form
    {
        private string header = "Date (MM/DD/YYYY),Payer/Payee Name,Transaction Id,Transaction Type,Amount,Memo,NS Internal Customer Id,NS Customer Name,Invoice Number(s)";

        public Models.Bank BankType { get; set; }

        public BankCreateUpload()
        {
            InitializeComponent();
        }

        private void InitializeOpenFileDialog()
        {
            // Set the file dialog to filter for graphics files.
            this.openBankFilesDialog.Filter = BankType == Models.Bank.TDNY ?
                "PDF (*.PDF)|*.PDF|" +
                "All files (*.*)|*.*" :
                "CSV (*.CSV)|*.CSV|" +
                "All files (*.*)|*.*"
                ;

            // Allow the user to select multiple images.
            this.openBankFilesDialog.Multiselect = true;
            this.openBankFilesDialog.Title = BankType == Models.Bank.TDNY? "Select TD Bank Files": "Select OTR Bank Files";
            this.openBankFilesDialog.FileName = BankType == Models.Bank.TDNY ? "*.PDF" : "*.CSV";

            this.saveCSVFileDialog.Filter = "CSV Files|*.CSV";
            this.saveCSVFileDialog.Title = "Save an CSV File";
        }
        private void btn_SelectFiles_Click(object sender, EventArgs e)
        {
            InitializeOpenFileDialog();
            DialogResult dr = this.openBankFilesDialog.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                IEnumerable<BankRecord> records = null;
                if (BankType == Models.Bank.TDNY)
                {
                    records = PDFHelper.CreateTDUploadFileEx(openBankFilesDialog.FileNames);
                }
                if (BankType == Models.Bank.OTR)
                {
                    records = PDFHelper.CreateOTRUploadFiles(openBankFilesDialog.FileNames);
                }

                if (records != null)
                {
                    saveCSVFileDialog.ShowDialog();

                    // If the file name is not an empty string open it for saving.
                    if (saveCSVFileDialog.FileName != "")
                    {
                        exportToCSV(saveCSVFileDialog.FileName, records);
                    }
                    MessageBox.Show("Files Created!");
                }
            }
        }

        private void exportToCSV(string filename, IEnumerable<BankRecord> records)
        {
            var recordGroups = records.GroupBy(p => p.Currency).Select(p => new { Currency = p.Key, Records = p.ToList() });

            foreach (var recordGroup in recordGroups)
            {
                string newFilename = AddSuffix(filename, String.Format("({0})", recordGroup.Currency));

                using (Stream stream = File.OpenWrite(newFilename))
                {
                    stream.SetLength(0);
                    using (StreamWriter writer = new StreamWriter(stream))
                    {
                        writer.WriteLine(header);

                        foreach (var record in recordGroup.Records)
                        {
                            var line = $"{record.Date},{record.Name},,{record.TransactionType},{record.Amount},,,,";
                            writer.WriteLine(line);
                        }

                        writer.Flush();
                    }
                };

            }
        }
        private string AddSuffix(string filename, string suffix)
        {
            string fDir = Path.GetDirectoryName(filename);
            string fName = Path.GetFileNameWithoutExtension(filename);
            string fExt = Path.GetExtension(filename);
            return Path.Combine(fDir, String.Concat(fName, suffix, fExt));
        }

        
    }
}
