using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using D2G.Iris.ML.Core.Models;

namespace D2G.Iris.ML.ConfigUI.Controls
{
    public partial class InputFieldsControl : UserControl
    {
        private List<InputField> _inputFields = new List<InputField>();

        public InputFieldsControl()
        {
            InitializeComponent();
            dgvInputFields.AutoGenerateColumns = true;

            dgvInputFields.DataBindingComplete += DgvInputFields_DataBindingComplete;
        }

        public void SetConfiguration(List<InputField> inputFields)
        {
            _inputFields = inputFields?.ToList() ?? new List<InputField>();
            RefreshGridView();
        }

        private void RefreshGridView()
        {
            dgvInputFields.DataSource = null;
            dgvInputFields.DataSource = _inputFields.ToList();
        }

        public List<InputField> GetConfiguration()
        {
            return _inputFields.ToList();
        }

        private void DgvInputFields_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            if (dgvInputFields.Columns.Contains("Name"))
            {
                var nameCol = dgvInputFields.Columns["Name"];
                nameCol.HeaderText = "Field Name";
                nameCol.Width = 250;
            }

            if (dgvInputFields.Columns.Contains("IsEnabled"))
            {
                var enabledCol = dgvInputFields.Columns["IsEnabled"];
                enabledCol.HeaderText = "Enabled";
                enabledCol.Width = 80;
            }
        }

        private void btnAddField_Click(object sender, EventArgs e)
        {
            using (var form = new InputFieldDialog())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    var newField = new InputField
                    {
                        Name = form.FieldName,
                        IsEnabled = form.IsEnabled
                    };

                    if (_inputFields.Any(f => f.Name.Equals(newField.Name, StringComparison.OrdinalIgnoreCase)))
                    {
                        MessageBox.Show($"A field with the name '{newField.Name}' already exists.",
                            "Duplicate Field", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    _inputFields.Add(newField);
                    RefreshGridView();
                }
            }
        }

        private void btnEditField_Click(object sender, EventArgs e)
        {
            if (dgvInputFields.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a field to edit.",
                    "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int index = dgvInputFields.SelectedRows[0].Index;
            var field = _inputFields[index];

            using (var form = new InputFieldDialog())
            {
                form.FieldName = field.Name;
                form.IsEnabled = field.IsEnabled;

                if (form.ShowDialog() == DialogResult.OK)
                {
                    string newName = form.FieldName;
                    if (!newName.Equals(field.Name, StringComparison.OrdinalIgnoreCase) &&
                        _inputFields.Any(f => f.Name.Equals(newName, StringComparison.OrdinalIgnoreCase)))
                    {
                        MessageBox.Show($"A field with the name '{newName}' already exists.",
                            "Duplicate Field", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    field.Name = newName;
                    field.IsEnabled = form.IsEnabled;
                    RefreshGridView();
                }
            }
        }

        private void btnRemoveField_Click(object sender, EventArgs e)
        {
            if (dgvInputFields.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a field to remove.",
                    "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int index = dgvInputFields.SelectedRows[0].Index;
            var field = _inputFields[index];

            var result = MessageBox.Show($"Are you sure you want to remove the field '{field.Name}'?",
                "Confirm Removal", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                _inputFields.RemoveAt(index);
                RefreshGridView();
            }
        }
    


        private void InitializeComponent()
        {
            this.grpInputFields = new System.Windows.Forms.GroupBox();
            this.btnRemoveField = new System.Windows.Forms.Button();
            this.btnEditField = new System.Windows.Forms.Button();
            this.btnAddField = new System.Windows.Forms.Button();
            this.dgvInputFields = new System.Windows.Forms.DataGridView();
            this.grpInputFields.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvInputFields)).BeginInit();
            this.SuspendLayout();
            // 
            // grpInputFields
            // 
            this.grpInputFields.Controls.Add(this.btnRemoveField);
            this.grpInputFields.Controls.Add(this.btnEditField);
            this.grpInputFields.Controls.Add(this.btnAddField);
            this.grpInputFields.Controls.Add(this.dgvInputFields);
            this.grpInputFields.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpInputFields.Location = new System.Drawing.Point(0, 0);
            this.grpInputFields.Name = "grpInputFields";
            this.grpInputFields.Size = new System.Drawing.Size(492, 283);
            this.grpInputFields.TabIndex = 0;
            this.grpInputFields.TabStop = false;
            this.grpInputFields.Text = "Input Fields";
            // 
            // btnRemoveField
            // 
            this.btnRemoveField.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnRemoveField.Location = new System.Drawing.Point(251, 244);
            this.btnRemoveField.Name = "btnRemoveField";
            this.btnRemoveField.Size = new System.Drawing.Size(110, 28);
            this.btnRemoveField.TabIndex = 3;
            this.btnRemoveField.Text = "Remove Field";
            this.btnRemoveField.UseVisualStyleBackColor = true;
            this.btnRemoveField.Click += new System.EventHandler(this.btnRemoveField_Click);
            // 
            // btnEditField
            // 
            this.btnEditField.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnEditField.Location = new System.Drawing.Point(135, 244);
            this.btnEditField.Name = "btnEditField";
            this.btnEditField.Size = new System.Drawing.Size(110, 28);
            this.btnEditField.TabIndex = 2;
            this.btnEditField.Text = "Edit Field";
            this.btnEditField.UseVisualStyleBackColor = true;
            this.btnEditField.Click += new System.EventHandler(this.btnEditField_Click);
            // 
            // btnAddField
            // 
            this.btnAddField.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnAddField.Location = new System.Drawing.Point(19, 244);
            this.btnAddField.Name = "btnAddField";
            this.btnAddField.Size = new System.Drawing.Size(110, 28);
            this.btnAddField.TabIndex = 1;
            this.btnAddField.Text = "Add Field";
            this.btnAddField.UseVisualStyleBackColor = true;
            this.btnAddField.Click += new System.EventHandler(this.btnAddField_Click);
            // 
            // dgvInputFields
            // 
            this.dgvInputFields.AllowUserToAddRows = false;
            this.dgvInputFields.AllowUserToDeleteRows = false;
            this.dgvInputFields.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvInputFields.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvInputFields.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvInputFields.Location = new System.Drawing.Point(19, 22);
            this.dgvInputFields.MultiSelect = false;
            this.dgvInputFields.Name = "dgvInputFields";
            this.dgvInputFields.ReadOnly = true;
            this.dgvInputFields.RowHeadersWidth = 51;
            this.dgvInputFields.RowTemplate.Height = 25;
            this.dgvInputFields.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvInputFields.Size = new System.Drawing.Size(458, 216);
            this.dgvInputFields.TabIndex = 0;
            // 
            // InputFieldsControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.grpInputFields);
            this.Name = "InputFieldsControl";
            this.Size = new System.Drawing.Size(492, 283);
            this.grpInputFields.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvInputFields)).EndInit();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.GroupBox grpInputFields;
        private System.Windows.Forms.DataGridView dgvInputFields;
        private System.Windows.Forms.Button btnAddField;
        private System.Windows.Forms.Button btnEditField;
        private System.Windows.Forms.Button btnRemoveField;
    }
}