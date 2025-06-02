using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using D2G.Iris.ML.Core.Models;
using D2G.Iris.ML.ConfigUI.Services;

namespace D2G.Iris.ML.ConfigUI.Controls
{
    public partial class InputFieldsControl : UserControl
    {
        private List<InputField> _inputFields = new List<InputField>();
        private readonly DatabaseSchemaLoader _schemaLoader;

        public InputFieldsControl()
        {
            InitializeComponent();
            SetupDataGridView();

            _schemaLoader = new DatabaseSchemaLoader();
        }

        private void SetupDataGridView()
        {
            dgvInputFields.AutoGenerateColumns = false;
            dgvInputFields.AllowUserToAddRows = false;
            dgvInputFields.AllowUserToDeleteRows = false;
            dgvInputFields.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvInputFields.MultiSelect = false;
            dgvInputFields.BackgroundColor = System.Drawing.SystemColors.Window;
            dgvInputFields.BorderStyle = BorderStyle.Fixed3D;

            var nameColumn = new DataGridViewTextBoxColumn
            {
                Name = "Name",
                HeaderText = "Field Name",
                DataPropertyName = "Name",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                ReadOnly = true
            };

            var enabledColumn = new DataGridViewCheckBoxColumn
            {
                Name = "IsEnabled",
                HeaderText = "Enabled",
                DataPropertyName = "IsEnabled",
                Width = 80,
                ReadOnly = false
            };

            dgvInputFields.Columns.Add(nameColumn);
            dgvInputFields.Columns.Add(enabledColumn);

            dgvInputFields.CellValueChanged += DgvInputFields_CellValueChanged;
            dgvInputFields.CurrentCellDirtyStateChanged += DgvInputFields_CurrentCellDirtyStateChanged;
        }

        private void DgvInputFields_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dgvInputFields.IsCurrentCellDirty && dgvInputFields.CurrentCell is DataGridViewCheckBoxCell)
            {
                dgvInputFields.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void DgvInputFields_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 1 && e.RowIndex >= 0) 
            {
                var row = dgvInputFields.Rows[e.RowIndex];
                var fieldName = row.Cells["Name"].Value?.ToString();
                var isEnabled = (bool)(row.Cells["IsEnabled"].Value ?? false);

                var field = _inputFields.FirstOrDefault(f => f.Name == fieldName);
                if (field != null)
                {
                    field.IsEnabled = isEnabled;
                }
            }
        }

        public void SetConfiguration(List<InputField> inputFields)
        {
            _inputFields = inputFields?.ToList() ?? new List<InputField>();
            RefreshGridView();
        }

        private void RefreshGridView()
        {
            dgvInputFields.DataSource = null;
            dgvInputFields.DataSource = new BindingList<InputField>(_inputFields);
        }

        public List<InputField> GetConfiguration()
        {
            return _inputFields.ToList();
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

        private async void btnLoadFromDatabase_Click(object sender, EventArgs e)
        {
            try
            {
                var dbConfig = GetDatabaseConfigFromParent();
                var targetField = GetTargetFieldFromParent();

                if (dbConfig == null)
                {
                    MessageBox.Show("Please configure database settings first.",
                        "Database Configuration Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (string.IsNullOrWhiteSpace(dbConfig.TableName))
                {
                    MessageBox.Show("Please specify a table name in database settings.",
                        "Table Name Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                btnLoadFromDatabase.Enabled = false;
                btnLoadFromDatabase.Text = "Loading...";
                Application.DoEvents();

                if (!_schemaLoader.TestConnection(dbConfig))
                {
                    MessageBox.Show("Cannot connect to database. Please check your database settings.",
                        "Connection Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var columnNames = await System.Threading.Tasks.Task.Run(() => _schemaLoader.LoadTableColumns(dbConfig));

                var inputFields = columnNames
                    .Where(name => !string.Equals(name, targetField, StringComparison.OrdinalIgnoreCase))
                    .Select(name => new InputField
                    {
                        Name = name,
                        IsEnabled = true
                    })
                    .OrderBy(f => f.Name)
                    .ToList();

                if (_inputFields.Count > 0)
                {
                    var result = MessageBox.Show(
                        $"This will replace your current {_inputFields.Count} field(s) with {inputFields.Count} fields from the database table '{dbConfig.TableName}'.\n\n" +
                        $"All loaded fields will be enabled by default. You can disable fields you don't want to use.\n\n" +
                        "Do you want to continue?",
                        "Replace Current Fields?",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result != DialogResult.Yes)
                        return;
                }

                SetConfiguration(inputFields);

                MessageBox.Show(
                        $"Successfully loaded {inputFields.Count} fields from database table '{dbConfig.TableName}'.",
                        "Fields Loaded Successfully",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading fields from database:\n\n{ex.Message}",
                    "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnLoadFromDatabase.Enabled = true;
                btnLoadFromDatabase.Text = "Load from Database";
            }
        }

        private DatabaseConfig GetDatabaseConfigFromParent()
        {
            var mainForm = FindMainForm();
            if (mainForm == null) return null;

            try
            {
                var tabControl = FindControlByName(mainForm, "tabControl") as TabControl;
                if (tabControl == null) return null;

                var databaseTab = tabControl.TabPages.Cast<TabPage>()
                    .FirstOrDefault(tab => tab.Name == "tabDatabase" || tab.Text.Contains("Database"));
                if (databaseTab == null) return null;

                var databaseControl = databaseTab.Controls.OfType<DatabaseSettingsControl>().FirstOrDefault();
                return databaseControl?.GetConfiguration();
            }
            catch
            {
                return null;
            }
        }

        private string GetTargetFieldFromParent()
        {
            var mainForm = FindMainForm();
            if (mainForm == null) return null;

            try
            {
                var tabControl = FindControlByName(mainForm, "tabControl") as TabControl;
                if (tabControl == null) return null;

                var generalTab = tabControl.TabPages.Cast<TabPage>()
                    .FirstOrDefault(tab => tab.Name == "tabGeneral" || tab.Text.Contains("General"));
                if (generalTab == null) return null;

                var generalControl = generalTab.Controls.OfType<GeneralSettingsControl>().FirstOrDefault();
                if (generalControl == null) return null;

                var values = generalControl.GetValues();
                return values.TargetField;
            }
            catch
            {
                return null;
            }
        }

        private Form FindMainForm()
        {
            Control current = this;
            while (current != null && !(current is Form))
            {
                current = current.Parent;
            }
            return current as Form;
        }

        private Control FindControlByName(Control parent, string name)
        {
            if (parent.Name == name) return parent;

            foreach (Control child in parent.Controls)
            {
                var found = FindControlByName(child, name);
                if (found != null) return found;
            }

            return null;
        }

        private void InitializeComponent()
        {
            this.grpInputFields = new System.Windows.Forms.GroupBox();
            this.btnLoadFromDatabase = new System.Windows.Forms.Button();
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
            this.grpInputFields.Controls.Add(this.btnLoadFromDatabase);
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
            // btnLoadFromDatabase
            // 
            this.btnLoadFromDatabase.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnLoadFromDatabase.BackColor = System.Drawing.Color.LightBlue;
            this.btnLoadFromDatabase.Location = new System.Drawing.Point(19, 244);
            this.btnLoadFromDatabase.Name = "btnLoadFromDatabase";
            this.btnLoadFromDatabase.Size = new System.Drawing.Size(130, 28);
            this.btnLoadFromDatabase.TabIndex = 4;
            this.btnLoadFromDatabase.Text = "Load from Database";
            this.btnLoadFromDatabase.UseVisualStyleBackColor = false;
            this.btnLoadFromDatabase.Click += new System.EventHandler(this.btnLoadFromDatabase_Click);
            // 
            // btnRemoveField
            // 
            this.btnRemoveField.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRemoveField.Location = new System.Drawing.Point(397, 244);
            this.btnRemoveField.Name = "btnRemoveField";
            this.btnRemoveField.Size = new System.Drawing.Size(80, 28);
            this.btnRemoveField.TabIndex = 3;
            this.btnRemoveField.Text = "Remove";
            this.btnRemoveField.UseVisualStyleBackColor = true;
            this.btnRemoveField.Click += new System.EventHandler(this.btnRemoveField_Click);
            // 
            // btnEditField
            // 
            this.btnEditField.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnEditField.Location = new System.Drawing.Point(325, 244);
            this.btnEditField.Name = "btnEditField";
            this.btnEditField.Size = new System.Drawing.Size(66, 28);
            this.btnEditField.TabIndex = 2;
            this.btnEditField.Text = "Edit";
            this.btnEditField.UseVisualStyleBackColor = true;
            this.btnEditField.Click += new System.EventHandler(this.btnEditField_Click);
            // 
            // btnAddField
            // 
            this.btnAddField.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddField.Location = new System.Drawing.Point(265, 244);
            this.btnAddField.Name = "btnAddField";
            this.btnAddField.Size = new System.Drawing.Size(54, 28);
            this.btnAddField.TabIndex = 1;
            this.btnAddField.Text = "Add";
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
            this.dgvInputFields.BackgroundColor = System.Drawing.SystemColors.Window;
            this.dgvInputFields.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.dgvInputFields.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvInputFields.Location = new System.Drawing.Point(19, 22);
            this.dgvInputFields.MultiSelect = false;
            this.dgvInputFields.Name = "dgvInputFields";
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
        private System.Windows.Forms.Button btnLoadFromDatabase;
    }
}