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
    public partial class BankReconciliation : Form
    {
        public BankReconciliation()
        {
            InitializeComponent();
        }

        private void btn_SelectFolder_Click(object sender, EventArgs e)
        {
            DialogResult result =  folderBrowserDialog.ShowDialog();
            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(folderBrowserDialog.SelectedPath))
            {
                textBox_Folder.Text = folderBrowserDialog.SelectedPath;
            }
        }

        private void btn_Run_Click(object sender, EventArgs e)
        {
            try
            {
                string[] files = Directory.GetFiles(textBox_Folder.Text);

                BankReconciliationHelpers.Run(files, dateTimePicker.Value);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }

        }
    }
}
