using Salton.Helpers;
using Salton.Models;
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

namespace Salton.PayRoll
{
    public partial class MatchEmployeeNumber : Form
    {
        private IEnumerable<MatchedEmployee> MissingEmployeeData;
        
        private BindingList<MatchedEmployee> MatchedEmployeeList;
        public MatchEmployeeNumber()
        {
            InitializeComponent();
            InitializeOpenFileDialog();
        }
        private void InitializeOpenFileDialog()
        {
            // Set the file dialog to filter for graphics files.
            this.openFileDialog.Filter =
                "CSV (*.CSV)|*.CSV|" +
                "All files (*.*)|*.*"
                ;

            // Allow the user to select multiple images.
            this.openFileDialog.Multiselect = false;
            this.openFileDialog.Title = "Select Employee Name File";

            this.saveFileDialog.Filter = "CSV Files|*.CSV";
            this.saveFileDialog.Title = "Save an CSV File";
        }

        private void btn_SelectEmployeeFile_Click(object sender, EventArgs e)
        {
            DialogResult dr = this.openFileDialog.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                var records = PayRollHelpers.GetMatchEmployeeData(openFileDialog.FileNames);

                if (records.Any(p => string.IsNullOrEmpty(p.Number)))
                {
                    MissingEmployeeData = records;
                    DisplayMissingDataView(records.Where(p => string.IsNullOrEmpty(p.Number)));
                }
                else
                {
                    SaveEmployeeFile(records);
                }

            }

        }

        private void DisplayMissingDataView(IEnumerable<MatchedEmployee> missMatchedEmployees)
        {

            MatchedEmployeeList = new BindingList<MatchedEmployee>(missMatchedEmployees.ToList());
            missingDataGridView.DataSource = MatchedEmployeeList;
            missingDataGridView.Columns[1].ReadOnly = true;

        }

        private void SaveEmployeeFile(IEnumerable<MatchedEmployee> records)
        {
            if (records != null)
            {
                saveFileDialog.ShowDialog();

                if (saveFileDialog.FileName != "")
                {
                    exportToCSV(saveFileDialog.FileName, records);
                }
                MessageBox.Show("Files Created!");
            }
        }

        private void exportToCSV(string fileName, IEnumerable<MatchedEmployee> records)
        {
            using (Stream stream = File.OpenWrite(fileName))
            {
                stream.SetLength(0);
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    foreach (var record in records)
                    {
                        var line = $"{record.Number},{record.Section},{record.Name}";
                        writer.WriteLine(line);
                    }
                    writer.Flush();
                }
            };


        }

        private void btn_LoadNumberFile_Click(object sender, EventArgs e)
        {
            DialogResult dr = this.openFileDialog.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                PayRollHelpers.LoadEmployeeNumberFile(openFileDialog.FileNames);
                MessageBox.Show("Employee Number Loaded!");
            }

        }

        private void btn_SaveEmployeeData_Click(object sender, EventArgs e)
        {
            var records = UpdateMissingEmployeeData();
            SaveEmployeeFile(records);

        }

        private IEnumerable<MatchedEmployee> UpdateMissingEmployeeData()
        {
            var list = new List<MatchedEmployee>();
            foreach (var employeeData in MissingEmployeeData.Where(p=>string.IsNullOrEmpty(p.Number)))
            {
                var match = MatchedEmployeeList.FirstOrDefault(p=>p.Name.Equals(employeeData.Name,StringComparison.InvariantCultureIgnoreCase));
                if (match != null)
                {
                    employeeData.Number = match.Number;
                    list.Add(new MatchedEmployee { 
                        Name = match.Name,
                        Section = match.Section,
                        Number = match.Number
                    });
                }
            }
            PayRollHelpers.UpdateEmployeeData(list);
            return MissingEmployeeData;
        }
    }
}
