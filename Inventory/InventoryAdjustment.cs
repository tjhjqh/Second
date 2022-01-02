using Salton.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Salton.Inventory
{
    public partial class InventoryAdjustment : Form
    {
        public InventoryAdjustment()
        {
            InitializeComponent();
            InitializeOpenFileDialog();

        }
        private void InitializeOpenFileDialog()
        {
            // Set the file dialog to filter for graphics files.
            this.openFileDialog.Filter =
                "Excel (*.Xlsx)|*.Xlsx|" +
                "All files (*.*)|*.*"
                ;

            // Allow the user to select multiple images.
            this.openFileDialog.Multiselect = false;
            this.openFileDialog.Title = "Select Inventory File";

        }

        private void button_LoadFile_Click(object sender, EventArgs e)
        {
            DialogResult dr = this.openFileDialog.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                InventoryHelpers.Adjust(this.openFileDialog.FileName);
            }
            MessageBox.Show("Done!");



        }
    }
}
