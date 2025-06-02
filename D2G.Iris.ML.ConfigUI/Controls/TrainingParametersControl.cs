using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using D2G.Iris.ML.Core.Models;
using D2G.Iris.ML.Core.Enums;
using D2G.Iris.ML.Utils;

namespace D2G.Iris.ML.ConfigUI.Controls
{
    public partial class TrainingParametersControl : UserControl
    {
        private Dictionary<string, object> _algorithmParameters = new Dictionary<string, object>();
        private ModelType _currentModelType = ModelType.BinaryClassification;

        public TrainingParametersControl()
        {
            InitializeComponent();
            SetupEventHandlers();
            UpdateAlgorithmComboBox();
        }

        public void SetModelType(ModelType modelType)
        {
            if (_currentModelType != modelType)
            {
                _currentModelType = modelType;
                UpdateAlgorithmComboBox();

                _algorithmParameters.Clear();
                UpdateParametersListView();
            }
        }

        private void UpdateAlgorithmComboBox()
        {
            cboAlgorithm.Items.Clear();

            var algorithms = AlgorithmRegistry.GetAlgorithmsForModelType(_currentModelType);
            foreach (var algorithm in algorithms)
            {
                cboAlgorithm.Items.Add(algorithm);
            }

            if (cboAlgorithm.Items.Count > 0)
            {
                cboAlgorithm.SelectedIndex = 0;
            }
        }

        private void SetupEventHandlers()
        {
            cboAlgorithm.SelectedIndexChanged += CboAlgorithm_SelectedIndexChanged;
        }

        private void CboAlgorithm_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedAlgorithm = cboAlgorithm.Text.ToLower();
            LoadAvailableParameters(selectedAlgorithm);
        }

        private void LoadAvailableParameters(string algorithmName)
        {
            Type optionsType = AlgorithmRegistry.GetOptionsType(algorithmName, _currentModelType);

            if (optionsType == null)
            {
                ClearParameterSuggestions();
                return;
            }

            try
            {
                var properties = ParameterHelper.GetConfigurableProperties(optionsType);
                var fields = ParameterHelper.GetConfigurableFields(optionsType);

                ShowParameterSuggestions(properties, fields, optionsType);
            }
            catch (Exception)
            {
                ClearParameterSuggestions();
            }
        }

        private void ShowParameterSuggestions(List<System.Reflection.PropertyInfo> properties, List<System.Reflection.FieldInfo> fields, Type optionsType)
        {
            ClearParameterSuggestions();

            var totalCount = properties.Count + fields.Count;

            if (totalCount == 0)
                return;

            var toolTip = new ToolTip();
            var tooltipText = ParameterHelper.CreateParameterTooltip(optionsType);

            toolTip.SetToolTip(btnAddParameter, tooltipText);
            toolTip.SetToolTip(lvParameters, tooltipText);

            btnAddParameter.Text = $"Add Parameter ({totalCount} available)";
        }

        private void ClearParameterSuggestions()
        {
            btnAddParameter.Text = "Add Parameter";

            var toolTip = new ToolTip();
            toolTip.SetToolTip(btnAddParameter, "");
            toolTip.SetToolTip(lvParameters, "");
        }

        public void SetConfiguration(TrainingParameters parameters)
        {
            if (parameters == null) return;

            string algorithm = parameters.Algorithm?.ToLower();
            if (!string.IsNullOrEmpty(algorithm))
            {
                var availableAlgorithms = AlgorithmRegistry.GetAlgorithmsForModelType(_currentModelType);
                var matchingAlgorithm = availableAlgorithms.FirstOrDefault(a =>
                    string.Equals(a, parameters.Algorithm, StringComparison.OrdinalIgnoreCase));

                if (matchingAlgorithm != null)
                {
                    cboAlgorithm.Text = matchingAlgorithm;
                }
                else if (cboAlgorithm.Items.Count > 0)
                {
                    cboAlgorithm.SelectedIndex = 0;
                }
            }
            else if (cboAlgorithm.Items.Count > 0)
            {
                cboAlgorithm.SelectedIndex = 0;
            }

            numTestFraction.Value = (decimal)parameters.TestFraction;
            _algorithmParameters = parameters.AlgorithmParameters ?? new Dictionary<string, object>();
            UpdateParametersListView();

            LoadAvailableParameters(cboAlgorithm.Text?.ToLower() ?? "");
        }

        public TrainingParameters GetConfiguration()
        {
            return new TrainingParameters
            {
                Algorithm = cboAlgorithm.Text,
                TestFraction = (double)numTestFraction.Value,
                AlgorithmParameters = new Dictionary<string, object>(_algorithmParameters)
            };
        }

        private void UpdateParametersListView()
        {
            lvParameters.Items.Clear();

            foreach (var param in _algorithmParameters)
            {
                var item = new ListViewItem(param.Key);
                item.SubItems.Add(param.Value?.ToString() ?? "null");
                lvParameters.Items.Add(item);
            }
        }

        private void btnAddParameter_Click(object sender, EventArgs e)
        {
            string selectedAlgorithm = cboAlgorithm.Text;

            using (var form = new SimpleParameterDialog(selectedAlgorithm, _currentModelType))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    string paramName = form.ParameterName;
                    object paramValue = form.ParameterValue;

                    if (_algorithmParameters.ContainsKey(paramName))
                    {
                        var result = MessageBox.Show($"Parameter '{paramName}' already exists. Do you want to update it?",
                            "Duplicate Parameter", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                        if (result != DialogResult.Yes)
                            return;
                    }

                    _algorithmParameters[paramName] = paramValue;
                    UpdateParametersListView();
                }
            }
        }

        private void btnRemoveParameter_Click(object sender, EventArgs e)
        {
            if (lvParameters.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select a parameter to remove.",
                    "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string paramName = lvParameters.SelectedItems[0].Text;

            var result = MessageBox.Show($"Are you sure you want to remove the parameter '{paramName}'?",
                "Confirm Removal", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                _algorithmParameters.Remove(paramName);
                UpdateParametersListView();
            }
        }

        private void InitializeComponent()
        {
            grpTraining = new GroupBox();
            btnRemoveParameter = new Button();
            btnAddParameter = new Button();
            lvParameters = new ListView();
            colParameterName = new ColumnHeader();
            colParameterValue = new ColumnHeader();
            lblParameters = new Label();
            numTestFraction = new NumericUpDown();
            lblTestFraction = new Label();
            cboAlgorithm = new ComboBox();
            lblAlgorithm = new Label();
            grpTraining.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numTestFraction).BeginInit();
            SuspendLayout();
            // 
            // grpTraining
            // 
            grpTraining.Controls.Add(btnRemoveParameter);
            grpTraining.Controls.Add(btnAddParameter);
            grpTraining.Controls.Add(lvParameters);
            grpTraining.Controls.Add(lblParameters);
            grpTraining.Controls.Add(numTestFraction);
            grpTraining.Controls.Add(lblTestFraction);
            grpTraining.Controls.Add(cboAlgorithm);
            grpTraining.Controls.Add(lblAlgorithm);
            grpTraining.Dock = DockStyle.Fill;
            grpTraining.Location = new Point(0, 0);
            grpTraining.Name = "grpTraining";
            grpTraining.Size = new Size(492, 283);
            grpTraining.TabIndex = 0;
            grpTraining.TabStop = false;
            grpTraining.Text = "Training Parameters";
            // 
            // btnRemoveParameter
            // 
            btnRemoveParameter.Location = new Point(378, 245);
            btnRemoveParameter.Name = "btnRemoveParameter";
            btnRemoveParameter.Size = new Size(71, 25);
            btnRemoveParameter.TabIndex = 7;
            btnRemoveParameter.Text = "Remove";
            btnRemoveParameter.UseVisualStyleBackColor = true;
            btnRemoveParameter.Click += btnRemoveParameter_Click;
            // 
            // btnAddParameter
            // 
            btnAddParameter.Location = new Point(244, 245);
            btnAddParameter.Name = "btnAddParameter";
            btnAddParameter.Size = new Size(119, 25);
            btnAddParameter.TabIndex = 6;
            btnAddParameter.Text = "Add Parameter";
            btnAddParameter.UseVisualStyleBackColor = true;
            btnAddParameter.Click += btnAddParameter_Click;
            // 
            // lvParameters
            // 
            lvParameters.Columns.AddRange(new ColumnHeader[] { colParameterName, colParameterValue });
            lvParameters.FullRowSelect = true;
            lvParameters.GridLines = true;
            lvParameters.Location = new Point(27, 121);
            lvParameters.MultiSelect = false;
            lvParameters.Name = "lvParameters";
            lvParameters.Size = new Size(422, 118);
            lvParameters.TabIndex = 5;
            lvParameters.UseCompatibleStateImageBehavior = false;
            lvParameters.View = View.Details;
            // 
            // colParameterName
            // 
            colParameterName.Text = "Parameter Name";
            colParameterName.Width = 200;
            // 
            // colParameterValue
            // 
            colParameterValue.Text = "Value";
            colParameterValue.Width = 200;
            // 
            // lblParameters
            // 
            lblParameters.AutoSize = true;
            lblParameters.Location = new Point(27, 103);
            lblParameters.Name = "lblParameters";
            lblParameters.Size = new Size(126, 15);
            lblParameters.TabIndex = 4;
            lblParameters.Text = "Algorithm Parameters:";
            // 
            // numTestFraction
            // 
            numTestFraction.DecimalPlaces = 2;
            numTestFraction.Increment = new decimal(new int[] { 1, 0, 0, 131072 });
            numTestFraction.Location = new Point(158, 65);
            numTestFraction.Maximum = new decimal(new int[] { 1, 0, 0, 0 });
            numTestFraction.Name = "numTestFraction";
            numTestFraction.Size = new Size(120, 23);
            numTestFraction.TabIndex = 3;
            numTestFraction.Value = new decimal(new int[] { 2, 0, 0, 65536 });
            // 
            // lblTestFraction
            // 
            lblTestFraction.AutoSize = true;
            lblTestFraction.Location = new Point(27, 67);
            lblTestFraction.Name = "lblTestFraction";
            lblTestFraction.Size = new Size(76, 15);
            lblTestFraction.TabIndex = 2;
            lblTestFraction.Text = "Test Fraction:";
            // 
            // cboAlgorithm
            // 
            cboAlgorithm.DropDownStyle = ComboBoxStyle.DropDownList;
            cboAlgorithm.FormattingEnabled = true;
            cboAlgorithm.Location = new Point(158, 35);
            cboAlgorithm.Name = "cboAlgorithm";
            cboAlgorithm.Size = new Size(291, 23);
            cboAlgorithm.TabIndex = 1;
            // 
            // lblAlgorithm
            // 
            lblAlgorithm.AutoSize = true;
            lblAlgorithm.Location = new Point(27, 38);
            lblAlgorithm.Name = "lblAlgorithm";
            lblAlgorithm.Size = new Size(64, 15);
            lblAlgorithm.TabIndex = 0;
            lblAlgorithm.Text = "Algorithm:";
            // 
            // TrainingParametersControl
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(grpTraining);
            Name = "TrainingParametersControl";
            Size = new Size(492, 283);
            grpTraining.ResumeLayout(false);
            grpTraining.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numTestFraction).EndInit();
            ResumeLayout(false);
        }

        private System.Windows.Forms.GroupBox grpTraining;
        private System.Windows.Forms.Label lblAlgorithm;
        private System.Windows.Forms.ComboBox cboAlgorithm;
        private System.Windows.Forms.Label lblTestFraction;
        private System.Windows.Forms.NumericUpDown numTestFraction;
        private System.Windows.Forms.Label lblParameters;
        private System.Windows.Forms.ListView lvParameters;
        private System.Windows.Forms.ColumnHeader colParameterName;
        private System.Windows.Forms.ColumnHeader colParameterValue;
        private System.Windows.Forms.Button btnAddParameter;
        private System.Windows.Forms.Button btnRemoveParameter;
    }
}