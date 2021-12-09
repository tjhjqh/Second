
namespace Salton.Bank
{
    partial class BankCreateUpload
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
            this.btn_SelectFiles = new System.Windows.Forms.Button();
            this.openBankFilesDialog = new System.Windows.Forms.OpenFileDialog();
            this.saveCSVFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.SuspendLayout();
            // 
            // btn_SelectFiles
            // 
            this.btn_SelectFiles.Location = new System.Drawing.Point(12, 12);
            this.btn_SelectFiles.Name = "btn_SelectFiles";
            this.btn_SelectFiles.Size = new System.Drawing.Size(163, 23);
            this.btn_SelectFiles.TabIndex = 0;
            this.btn_SelectFiles.Text = "Select Bank Files";
            this.btn_SelectFiles.UseVisualStyleBackColor = true;
            this.btn_SelectFiles.Click += new System.EventHandler(this.btn_SelectFiles_Click);
            // 
            // openBankFilesDialog
            // 
            this.openBankFilesDialog.FileName = "*.PDF";
            // 
            // saveCSVFileDialog
            // 
            this.saveCSVFileDialog.FileName = "Save CSV File";
            // 
            // BankCreateUpload
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.btn_SelectFiles);
            this.Name = "BankCreateUpload";
            this.Text = "BankCreateUpload";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btn_SelectFiles;
        private System.Windows.Forms.OpenFileDialog openBankFilesDialog;
        private System.Windows.Forms.SaveFileDialog saveCSVFileDialog;
    }
}