using Salton.Bank;
using Salton.PayRoll;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Salton
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void bank_UploadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var newMDIChild = new BankCreateUpload
            {
                MdiParent = this,
                BankType = Salton.Models.Bank.TDNY
            };
            newMDIChild.Show();
        }

        private void tool_OTR_CSV_StripMenuItem_Click(object sender, EventArgs e)
        {
            var newMDIChild = new BankCreateUpload
            {
                MdiParent = this,
                BankType = Salton.Models.Bank.OTR
            };
            newMDIChild.Show();

        }

        private void matchStripMenuItem_Click(object sender, EventArgs e)
        {
            var newMDIChild = new MatchEmployeeNumber
            {
                MdiParent = this,
            };
            newMDIChild.Show();

        }
    }
}
