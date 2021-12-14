using Salton.Helpers;
using System;
using System.IO;
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

                BankReconciliationHelpers.Run(files, textBox_Target.Text,dateTimePicker.Value);

                MessageBox.Show("Done!", "Information");

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }

        }

        private void btn_SelectTarget_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog.ShowDialog();
            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(openFileDialog.FileName))
            {
                textBox_Target.Text = openFileDialog.FileName;
            }

        }
    }
}
