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
                string typeStr = ParameterValue.GetType().Name.ToLower();

                if (typeStr.Contains("int") || typeStr.Contains("int32") || typeStr.Contains("int64"))
                {
                    cboValueType.SelectedIndex = 0; // Integer
                    txtParameterValue.Text = ParameterValue.ToString();
                }
                else if (typeStr.Contains("double") || typeStr.Contains("float") ||
                        typeStr.Contains("single") || typeStr.Contains("decimal"))
                {
                    cboValueType.SelectedIndex = 1; // Float
                    txtParameterValue.Text = ParameterValue.ToString();
                }
                else if (typeStr.Contains("bool"))
                {
                    cboValueType.SelectedIndex = 2; // Boolean
                    bool value = (bool)ParameterValue;
                    txtParameterValue.Text = value ? "true" : "false";
                }
                else
                {
                    cboValueType.SelectedIndex = 3; // String
                    txtParameterValue.Text = ParameterValue.ToString();
                }
            }
            else
            {
                cboValueType.SelectedIndex = 0; // Default to Integer
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
                // Convert string to appropriate type
                switch (cboValueType.SelectedIndex)
                {
                    case 0: // Integer
                        ParameterValue = int.Parse(txtParameterValue.Text);
                        break;
                    case 1: // Float
                        ParameterValue = double.Parse(txtParameterValue.Text);
                        break;
                    case 2: // Boolean
                        string boolVal = txtParameterValue.Text.ToLower();
                        if (boolVal == "true" || boolVal == "1" || boolVal == "yes")
                            ParameterValue = true;
                        else if (boolVal == "false" || boolVal == "0" || boolVal == "no")
                            ParameterValue = false;
                        else
                            throw new FormatException("Boolean must be true/false, 1/0, or yes/no");
                        break;
                    case 3: // String
                        ParameterValue = txtParameterValue.Text;
                        break;
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
            this.lblValueType = new System.Windows.Forms.Label();
            this.cboValueType = new System.Windows.Forms.ComboBox();
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
            this.lblParameterValue.Location = new System.Drawing.Point(21, 100);
            this.lblParameterValue.Name = "lblParameterValue";
            this.lblParameterValue.Size = new System.Drawing.Size(98, 15);
            this.lblParameterValue.TabIndex = 2;
            this.lblParameterValue.Text = "Parameter Value:";
            // 
            // txtParameterValue
            // 
            this.txtParameterValue.Location = new System.Drawing.Point(128, 97);
            this.txtParameterValue.Name = "txtParameterValue";
            this.txtParameterValue.Size = new System.Drawing.Size(225, 23);
            this.txtParameterValue.TabIndex = 3;
            // 
            // lblValueType
            // 
            this.lblValueType.AutoSize = true;
            this.lblValueType.Location = new System.Drawing.Point(21, 64);
            this.lblValueType.Name = "lblValueType";
            this.lblValueType.Size = new System.Drawing.Size(68, 15);
            this.lblValueType.TabIndex = 4;
            this.lblValueType.Text = "Value Type:";
            // 
            // cboValueType
            // 
            this.cboValueType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboValueType.FormattingEnabled = true;
            this.cboValueType.Items.AddRange(new object[] {
            "Integer",
            "Float",
            "Boolean",
            "String"});
            this.cboValueType.Location = new System.Drawing.Point(128, 61);
            this.cboValueType.Name = "cboValueType";
            this.cboValueType.Size = new System.Drawing.Size(225, 23);
            this.cboValueType.TabIndex = 5;
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(160, 140);
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
            this.btnCancel.Location = new System.Drawing.Point(264, 140);
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
            this.ClientSize = new System.Drawing.Size(374, 186);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.cboValueType);
            this.Controls.Add(this.lblValueType);
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
        private System.Windows.Forms.Label lblValueType;
        private System.Windows.Forms.ComboBox cboValueType;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
    }
}