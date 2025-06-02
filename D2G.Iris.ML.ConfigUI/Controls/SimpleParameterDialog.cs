using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using D2G.Iris.ML.Core.Enums;
using D2G.Iris.ML.Utils;

namespace D2G.Iris.ML.ConfigUI.Controls
{
    public partial class SimpleParameterDialog : Form
    {
        public string ParameterName { get; set; }
        public object ParameterValue { get; set; }

        private readonly string _algorithmName;
        private readonly ModelType _modelType;
        private List<PropertyInfo> _availableParameters;

        public SimpleParameterDialog(string algorithmName, ModelType modelType = ModelType.BinaryClassification)
        {
            InitializeComponent();
            _algorithmName = algorithmName;
            _modelType = modelType;
            LoadParameters();
        }

        private void LoadParameters()
        {
            try
            {
                Type optionsType = AlgorithmRegistry.GetOptionsType(_algorithmName, _modelType);

                if (optionsType == null)
                {
                    lblInfo.Text = $"No parameter mapping found for algorithm: {_algorithmName} with model type: {_modelType}";
                    return;
                }

                var properties = ParameterHelper.GetConfigurableProperties(optionsType);
                var fields = ParameterHelper.GetConfigurableFields(optionsType);

                _availableParameters = new List<PropertyInfo>();
                _availableParameters.AddRange(properties);
                _availableParameters.AddRange(fields.Select(f => new FieldAsProperty(f)));

                PopulateParameterList();
            }
            catch (Exception ex)
            {
                lblInfo.Text = $"Error loading parameters: {ex.Message}";
            }
        }

        private void PopulateParameterList()
        {
            listBoxParameters.Items.Clear();

            foreach (var param in _availableParameters)
            {
                string displayText = ParameterHelper.CreateParameterDisplayText(param.Name, param.PropertyType);
                listBoxParameters.Items.Add(new ParameterItem
                {
                    Property = param,
                    DisplayText = displayText
                });
            }

            listBoxParameters.DisplayMember = "DisplayText";

            if (listBoxParameters.Items.Count > 0)
            {
                lblInfo.Text = $"Found {listBoxParameters.Items.Count} configurable parameters. Select one:";
            }
            else
            {
                lblInfo.Text = "No configurable parameters found for this algorithm.";
            }
        }

        private void listBoxParameters_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxParameters.SelectedItem is ParameterItem selectedItem)
            {
                var prop = selectedItem.Property;
                txtParameterName.Text = prop.Name;
                txtParameterValue.Text = ParameterHelper.GetDefaultValue(prop.PropertyType);
                lblValueHint.Text = ParameterHelper.GetValueHint(prop.PropertyType);
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtParameterName.Text))
            {
                MessageBox.Show("Please select a parameter.", "Validation Error");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtParameterValue.Text))
            {
                MessageBox.Show("Please enter a value.", "Validation Error");
                return;
            }

            ParameterName = txtParameterName.Text;

            try
            {
                var selectedParam = _availableParameters.FirstOrDefault(p => p.Name == ParameterName);
                if (selectedParam != null)
                {
                    ParameterValue = ParameterHelper.ConvertParameterValue(txtParameterValue.Text, selectedParam.PropertyType);
                }
                else
                {
                    ParameterValue = txtParameterValue.Text;
                }

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error converting value: {ex.Message}", "Validation Error");
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void InitializeComponent()
        {
            this.lblInfo = new System.Windows.Forms.Label();
            this.listBoxParameters = new System.Windows.Forms.ListBox();
            this.lblParameterName = new System.Windows.Forms.Label();
            this.txtParameterName = new System.Windows.Forms.TextBox();
            this.lblParameterValue = new System.Windows.Forms.Label();
            this.txtParameterValue = new System.Windows.Forms.TextBox();
            this.lblValueHint = new System.Windows.Forms.Label();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblInfo
            // 
            this.lblInfo.Location = new System.Drawing.Point(12, 9);
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Size = new System.Drawing.Size(460, 20);
            this.lblInfo.TabIndex = 0;
            this.lblInfo.Text = "Loading parameters...";
            // 
            // listBoxParameters
            // 
            this.listBoxParameters.FormattingEnabled = true;
            this.listBoxParameters.ItemHeight = 15;
            this.listBoxParameters.Location = new System.Drawing.Point(12, 32);
            this.listBoxParameters.Name = "listBoxParameters";
            this.listBoxParameters.Size = new System.Drawing.Size(460, 139);
            this.listBoxParameters.TabIndex = 1;
            this.listBoxParameters.SelectedIndexChanged += new System.EventHandler(this.listBoxParameters_SelectedIndexChanged);
            // 
            // lblParameterName
            // 
            this.lblParameterName.AutoSize = true;
            this.lblParameterName.Location = new System.Drawing.Point(12, 185);
            this.lblParameterName.Name = "lblParameterName";
            this.lblParameterName.Size = new System.Drawing.Size(101, 15);
            this.lblParameterName.TabIndex = 2;
            this.lblParameterName.Text = "Parameter Name:";
            // 
            // txtParameterName
            // 
            this.txtParameterName.Location = new System.Drawing.Point(119, 182);
            this.txtParameterName.Name = "txtParameterName";
            this.txtParameterName.ReadOnly = true;
            this.txtParameterName.Size = new System.Drawing.Size(200, 23);
            this.txtParameterName.TabIndex = 3;
            // 
            // lblParameterValue
            // 
            this.lblParameterValue.AutoSize = true;
            this.lblParameterValue.Location = new System.Drawing.Point(12, 214);
            this.lblParameterValue.Name = "lblParameterValue";
            this.lblParameterValue.Size = new System.Drawing.Size(98, 15);
            this.lblParameterValue.TabIndex = 4;
            this.lblParameterValue.Text = "Parameter Value:";
            // 
            // txtParameterValue
            // 
            this.txtParameterValue.Location = new System.Drawing.Point(119, 211);
            this.txtParameterValue.Name = "txtParameterValue";
            this.txtParameterValue.Size = new System.Drawing.Size(200, 23);
            this.txtParameterValue.TabIndex = 5;
            // 
            // lblValueHint
            // 
            this.lblValueHint.ForeColor = System.Drawing.Color.Gray;
            this.lblValueHint.Location = new System.Drawing.Point(119, 237);
            this.lblValueHint.Name = "lblValueHint";
            this.lblValueHint.Size = new System.Drawing.Size(353, 40);
            this.lblValueHint.TabIndex = 6;
            this.lblValueHint.Text = "Select a parameter to see value hints";
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(316, 290);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 7;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(397, 290);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 8;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // SimpleParameterDialog
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(484, 325);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.lblValueHint);
            this.Controls.Add(this.txtParameterValue);
            this.Controls.Add(this.lblParameterValue);
            this.Controls.Add(this.txtParameterName);
            this.Controls.Add(this.lblParameterName);
            this.Controls.Add(this.listBoxParameters);
            this.Controls.Add(this.lblInfo);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SimpleParameterDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Select Algorithm Parameter";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Label lblInfo;
        private System.Windows.Forms.ListBox listBoxParameters;
        private System.Windows.Forms.Label lblParameterName;
        private System.Windows.Forms.TextBox txtParameterName;
        private System.Windows.Forms.Label lblParameterValue;
        private System.Windows.Forms.TextBox txtParameterValue;
        private System.Windows.Forms.Label lblValueHint;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
    }
}