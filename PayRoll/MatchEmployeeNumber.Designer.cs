
namespace Salton.PayRoll
{
    partial class MatchEmployeeNumber
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
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.btn_SelectEmployeeFile = new System.Windows.Forms.Button();
            this.btn_LoadNumberFile = new System.Windows.Forms.Button();
            this.missingDataGridView = new System.Windows.Forms.DataGridView();
            this.label1 = new System.Windows.Forms.Label();
            this.btn_SaveEmployeeData = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.missingDataGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // openFileDialog
            // 
            this.openFileDialog.FileName = "*.CSV";
            // 
            // btn_SelectEmployeeFile
            // 
            this.btn_SelectEmployeeFile.Location = new System.Drawing.Point(39, 35);
            this.btn_SelectEmployeeFile.Name = "btn_SelectEmployeeFile";
            this.btn_SelectEmployeeFile.Size = new System.Drawing.Size(275, 23);
            this.btn_SelectEmployeeFile.TabIndex = 1;
            this.btn_SelectEmployeeFile.Text = "Select Employee Name File (No Column)";
            this.btn_SelectEmployeeFile.UseVisualStyleBackColor = true;
            this.btn_SelectEmployeeFile.Click += new System.EventHandler(this.btn_SelectEmployeeFile_Click);
            // 
            // btn_LoadNumberFile
            // 
            this.btn_LoadNumberFile.Location = new System.Drawing.Point(39, 6);
            this.btn_LoadNumberFile.Name = "btn_LoadNumberFile";
            this.btn_LoadNumberFile.Size = new System.Drawing.Size(275, 23);
            this.btn_LoadNumberFile.TabIndex = 2;
            this.btn_LoadNumberFile.Text = "Load Employee Number File (No Column)";
            this.btn_LoadNumberFile.UseVisualStyleBackColor = true;
            this.btn_LoadNumberFile.Click += new System.EventHandler(this.btn_LoadNumberFile_Click);
            // 
            // missingDataGridView
            // 
            this.missingDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.missingDataGridView.Location = new System.Drawing.Point(39, 97);
            this.missingDataGridView.Name = "missingDataGridView";
            this.missingDataGridView.RowTemplate.Height = 25;
            this.missingDataGridView.Size = new System.Drawing.Size(720, 303);
            this.missingDataGridView.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(39, 76);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(173, 15);
            this.label1.TabIndex = 4;
            this.label1.Text = "Fill Missing Employee Numbers";
            // 
            // btn_SaveEmployeeData
            // 
            this.btn_SaveEmployeeData.Location = new System.Drawing.Point(683, 415);
            this.btn_SaveEmployeeData.Name = "btn_SaveEmployeeData";
            this.btn_SaveEmployeeData.Size = new System.Drawing.Size(75, 23);
            this.btn_SaveEmployeeData.TabIndex = 5;
            this.btn_SaveEmployeeData.Text = "Save To File";
            this.btn_SaveEmployeeData.UseVisualStyleBackColor = true;
            this.btn_SaveEmployeeData.Click += new System.EventHandler(this.btn_SaveEmployeeData_Click);
            // 
            // MatchEmployeeNumber
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.btn_SaveEmployeeData);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.missingDataGridView);
            this.Controls.Add(this.btn_LoadNumberFile);
            this.Controls.Add(this.btn_SelectEmployeeFile);
            this.Name = "MatchEmployeeNumber";
            this.Text = "MatchEmployeeNumber";
            ((System.ComponentModel.ISupportInitialize)(this.missingDataGridView)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
        private System.Windows.Forms.Button btn_SelectEmployeeFile;
        private System.Windows.Forms.Button btn_LoadNumberFile;
        private System.Windows.Forms.DataGridView missingDataGridView;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btn_SaveEmployeeData;
    }
}