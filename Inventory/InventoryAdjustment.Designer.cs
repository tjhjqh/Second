
namespace Salton.Inventory
{
    partial class InventoryAdjustment
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.button_LoadFile = new System.Windows.Forms.Button();
            this.label_file = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // openFileDialog
            // 
            this.openFileDialog.FileName = "Inventory File";
            // 
            // button_LoadFile
            // 
            this.button_LoadFile.Location = new System.Drawing.Point(487, 29);
            this.button_LoadFile.Name = "button_LoadFile";
            this.button_LoadFile.Size = new System.Drawing.Size(247, 23);
            this.button_LoadFile.TabIndex = 0;
            this.button_LoadFile.Text = "Load Inventory File";
            this.button_LoadFile.UseVisualStyleBackColor = true;
            this.button_LoadFile.Click += new System.EventHandler(this.button_LoadFile_Click);
            // 
            // label_file
            // 
            this.label_file.AutoSize = true;
            this.label_file.Location = new System.Drawing.Point(29, 36);
            this.label_file.Name = "label_file";
            this.label_file.Size = new System.Drawing.Size(0, 15);
            this.label_file.TabIndex = 1;
            // 
            // InventoryAdjustment
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.label_file);
            this.Controls.Add(this.button_LoadFile);
            this.Name = "InventoryAdjustment";
            this.Text = "InventoryAdjustment";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.Button button_LoadFile;
        private System.Windows.Forms.Label label_file;
    }
}