using System;
using System.Windows.Forms;

namespace D2G.Iris.ML.ConfigUI.Controls
{
    public partial class ParameterDialog : Form
    {
        public string ParameterName { get; set; }
        public object ParameterValue { get; set; }

        public ParameterDialog()
        {
            InitializeComponent();
        }

        private void ParameterDialog_Load(object sender, EventArgs e)
        {
            txtParameterName.Text = ParameterName;

            if (ParameterValue != null)
            {
                txtParameterValue.Text = ParameterValue.ToString();
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtParameterName.Text))
            {
                MessageBox.Show("Parameter name cannot be empty.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtParameterName.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtParameterValue.Text))
            {
                MessageBox.Show("Parameter value cannot be empty.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtParameterValue.Focus();
                return;
            }

            ParameterName = txtParameterName.Text.Trim();

            try
            {
                string value = txtParameterValue.Text.Trim();

                if (bool.TryParse(value.ToLower(), out bool boolValue))
                {
                    ParameterValue = boolValue;
                }
                else if (int.TryParse(value, out int intValue))
                {
                    ParameterValue = intValue;
                }
                else if (double.TryParse(value, out double doubleValue))
                {
                    ParameterValue = doubleValue;
                }
                else
                {
                    ParameterValue = value;
                }

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error parsing value: {ex.Message}", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtParameterValue.Focus();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void InitializeComponent()
        {
            this.lblParameterName = new System.Windows.Forms.Label();
            this.txtParameterName = new System.Windows.Forms.TextBox();
            this.lblParameterValue = new System.Windows.Forms.Label();
            this.txtParameterValue = new System.Windows.Forms.TextBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblParameterName
            // 
            this.lblParameterName.AutoSize = true;
            this.lblParameterName.Location = new System.Drawing.Point(21, 29);
            this.lblParameterName.Name = "lblParameterName";
            this.lblParameterName.Size = new System.Drawing.Size(101, 15);
            this.lblParameterName.TabIndex = 0;
            this.lblParameterName.Text = "Parameter Name:";
            // 
            // txtParameterName
            // 
            this.txtParameterName.Location = new System.Drawing.Point(128, 26);
            this.txtParameterName.Name = "txtParameterName";
            this.txtParameterName.Size = new System.Drawing.Size(225, 23);
            this.txtParameterName.TabIndex = 1;
            // 
            // lblParameterValue
            // 
            this.lblParameterValue.AutoSize = true;
            this.lblParameterValue.Location = new System.Drawing.Point(21, 64);
            this.lblParameterValue.Name = "lblParameterValue";
            this.lblParameterValue.Size = new System.Drawing.Size(98, 15);
            this.lblParameterValue.TabIndex = 2;
            this.lblParameterValue.Text = "Parameter Value:";
            // 
            // txtParameterValue
            // 
            this.txtParameterValue.Location = new System.Drawing.Point(128, 61);
            this.txtParameterValue.Name = "txtParameterValue";
            this.txtParameterValue.Size = new System.Drawing.Size(225, 23);
            this.txtParameterValue.TabIndex = 3;
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(160, 100);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(89, 30);
            this.btnOK.TabIndex = 6;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(264, 100);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(89, 30);
            this.btnCancel.TabIndex = 7;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // ParameterDialog
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(374, 146);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.txtParameterValue);
            this.Controls.Add(this.lblParameterValue);
            this.Controls.Add(this.txtParameterName);
            this.Controls.Add(this.lblParameterName);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ParameterDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Algorithm Parameter";
            this.Load += new System.EventHandler(this.ParameterDialog_Load);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Label lblParameterName;
        private System.Windows.Forms.TextBox txtParameterName;
        private System.Windows.Forms.Label lblParameterValue;
        private System.Windows.Forms.TextBox txtParameterValue;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
    }
}