
namespace Salton.Bank
{
    partial class BankReconciliation
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
            this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.textBox_Folder = new System.Windows.Forms.TextBox();
            this.btn_SelectFolder = new System.Windows.Forms.Button();
            this.dateTimePicker = new System.Windows.Forms.DateTimePicker();
            this.btn_Run = new System.Windows.Forms.Button();
            this.textBox_Target = new System.Windows.Forms.TextBox();
            this.btn_SelectTarget = new System.Windows.Forms.Button();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.SuspendLayout();
            // 
            // textBox_Folder
            // 
            this.textBox_Folder.Location = new System.Drawing.Point(19, 63);
            this.textBox_Folder.Name = "textBox_Folder";
            this.textBox_Folder.Size = new System.Drawing.Size(640, 23);
            this.textBox_Folder.TabIndex = 0;
            // 
            // btn_SelectFolder
            // 
            this.btn_SelectFolder.Location = new System.Drawing.Point(675, 63);
            this.btn_SelectFolder.Name = "btn_SelectFolder";
            this.btn_SelectFolder.Size = new System.Drawing.Size(102, 23);
            this.btn_SelectFolder.TabIndex = 1;
            this.btn_SelectFolder.Text = "Select Folder";
            this.btn_SelectFolder.UseVisualStyleBackColor = true;
            this.btn_SelectFolder.Click += new System.EventHandler(this.btn_SelectFolder_Click);
            // 
            // dateTimePicker
            // 
            this.dateTimePicker.CustomFormat = "MM/yyyy";
            this.dateTimePicker.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTimePicker.Location = new System.Drawing.Point(19, 12);
            this.dateTimePicker.Name = "dateTimePicker";
            this.dateTimePicker.Size = new System.Drawing.Size(200, 23);
            this.dateTimePicker.TabIndex = 2;
            // 
            // btn_Run
            // 
            this.btn_Run.Location = new System.Drawing.Point(19, 160);
            this.btn_Run.Name = "btn_Run";
            this.btn_Run.Size = new System.Drawing.Size(75, 23);
            this.btn_Run.TabIndex = 3;
            this.btn_Run.Text = "Run";
            this.btn_Run.UseVisualStyleBackColor = true;
            this.btn_Run.Click += new System.EventHandler(this.btn_Run_Click);
            // 
            // textBox_Target
            // 
            this.textBox_Target.Location = new System.Drawing.Point(19, 114);
            this.textBox_Target.Name = "textBox_Target";
            this.textBox_Target.Size = new System.Drawing.Size(640, 23);
            this.textBox_Target.TabIndex = 0;
            // 
            // btn_SelectTarget
            // 
            this.btn_SelectTarget.Location = new System.Drawing.Point(675, 114);
            this.btn_SelectTarget.Name = "btn_SelectTarget";
            this.btn_SelectTarget.Size = new System.Drawing.Size(102, 23);
            this.btn_SelectTarget.TabIndex = 1;
            this.btn_SelectTarget.Text = "Select Target";
            this.btn_SelectTarget.UseVisualStyleBackColor = true;
            this.btn_SelectTarget.Click += new System.EventHandler(this.btn_SelectTarget_Click);
            // 
            // openFileDialog
            // 
            this.openFileDialog.FileName = "*.XLS";
            // 
            // BankReconciliation
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.btn_Run);
            this.Controls.Add(this.dateTimePicker);
            this.Controls.Add(this.btn_SelectTarget);
            this.Controls.Add(this.btn_SelectFolder);
            this.Controls.Add(this.textBox_Target);
            this.Controls.Add(this.textBox_Folder);
            this.Name = "BankReconciliation";
            this.Text = "Select Folder";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
        private System.Windows.Forms.TextBox textBox_Folder;
        private System.Windows.Forms.Button btn_SelectFolder;
        private System.Windows.Forms.DateTimePicker dateTimePicker;
        private System.Windows.Forms.Button btn_Run;
        private System.Windows.Forms.TextBox textBox_Target;
        private System.Windows.Forms.Button btn_SelectTarget;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
    }
}