using System;
using System.Windows.Forms;

namespace D2G.Iris.ML.ConfigUI.Controls
{
    public partial class InputFieldDialog : Form
    {
        public string FieldName { get; set; }
        public bool IsEnabled { get; set; }

        public InputFieldDialog()
        {
            InitializeComponent();
        }

        private void InputFieldDialog_Load(object sender, EventArgs e)
        {
            txtFieldName.Text = FieldName;
            chkEnabled.Checked = IsEnabled;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFieldName.Text))
            {
                MessageBox.Show("Field name cannot be empty.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtFieldName.Focus();
                return;
            }

            FieldName = txtFieldName.Text.Trim();
            IsEnabled = chkEnabled.Checked;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void InitializeComponent()
        {
            lblFieldName = new Label();
            txtFieldName = new TextBox();
            chkEnabled = new CheckBox();
            btnOK = new Button();
            btnCancel = new Button();
            SuspendLayout();
            // 
            // lblFieldName
            // 
            lblFieldName.AutoSize = true;
            lblFieldName.Location = new Point(21, 29);
            lblFieldName.Name = "lblFieldName";
            lblFieldName.Size = new Size(70, 15);
            lblFieldName.TabIndex = 0;
            lblFieldName.Text = "Field Name:";
            // 
            // txtFieldName
            // 
            txtFieldName.Location = new Point(98, 26);
            txtFieldName.Name = "txtFieldName";
            txtFieldName.Size = new Size(255, 23);
            txtFieldName.TabIndex = 1;
            // 
            // chkEnabled
            // 
            chkEnabled.AutoSize = true;
            chkEnabled.Checked = true;
            chkEnabled.CheckState = CheckState.Checked;
            chkEnabled.Location = new Point(98, 66);
            chkEnabled.Name = "chkEnabled";
            chkEnabled.Size = new Size(68, 19);
            chkEnabled.TabIndex = 2;
            chkEnabled.Text = "Enabled";
            chkEnabled.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            btnOK.Location = new Point(157, 100);
            btnOK.Name = "btnOK";
            btnOK.Size = new Size(89, 30);
            btnOK.TabIndex = 3;
            btnOK.Text = "OK";
            btnOK.UseVisualStyleBackColor = true;
            btnOK.Click += btnOK_Click;
            // 
            // btnCancel
            // 
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(265, 100);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(89, 30);
            btnCancel.TabIndex = 4;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // InputFieldDialog
            // 
            AcceptButton = btnOK;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(374, 146);
            Controls.Add(btnCancel);
            Controls.Add(btnOK);
            Controls.Add(chkEnabled);
            Controls.Add(txtFieldName);
            Controls.Add(lblFieldName);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "InputFieldDialog";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Input Field";
            Load += InputFieldDialog_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        private System.Windows.Forms.Label lblFieldName;
        private System.Windows.Forms.TextBox txtFieldName;
        private System.Windows.Forms.CheckBox chkEnabled;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
    }
}