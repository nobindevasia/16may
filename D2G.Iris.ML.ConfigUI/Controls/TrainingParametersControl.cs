using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using D2G.Iris.ML.Core.Models;
using D2G.Iris.ML.Core.Enums;
using Microsoft.ML.Trainers;
using Microsoft.ML.Trainers.FastTree;
using Microsoft.ML.Trainers.LightGbm;

namespace D2G.Iris.ML.ConfigUI.Controls
{
    public partial class TrainingParametersControl : UserControl
    {
        private Dictionary<string, object> _algorithmParameters = new Dictionary<string, object>();
        private Dictionary<string, Type> _algorithmOptionTypes = new Dictionary<string, Type>();

        public TrainingParametersControl()
        {
            InitializeComponent();
            InitializeAlgorithmComboBox();
            InitializeAlgorithmOptionTypes();
            SetupEventHandlers();
        }

        private void InitializeAlgorithmComboBox()
        {
            cboAlgorithm.Items.Clear();
            cboAlgorithm.Items.Add("fastforest");
            cboAlgorithm.Items.Add("fasttree");
            cboAlgorithm.Items.Add("lightgbm");
            cboAlgorithm.Items.Add("sdcalogisticregression");
            cboAlgorithm.Items.Add("gam");
            cboAlgorithm.Items.Add("ols");
            cboAlgorithm.SelectedIndex = 0;
        }

        private void InitializeAlgorithmOptionTypes()
        {
            // Map algorithm names to their corresponding options classes
            _algorithmOptionTypes = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
            {
                // Binary Classification
                { "fastforest", typeof(FastForestBinaryTrainer.Options) },
                { "fasttree", typeof(FastTreeBinaryTrainer.Options) },
                { "lightgbm", typeof(LightGbmBinaryTrainer.Options) },
                { "sdcalogisticregression", typeof(SdcaLogisticRegressionBinaryTrainer.Options) },
                { "gam", typeof(GamBinaryTrainer.Options) },
                { "averagedperceptron", typeof(AveragedPerceptronTrainer.Options) },
                { "linearsvm", typeof(LinearSvmTrainer.Options) },
                { "ldsvm", typeof(LdSvmTrainer.Options) },
                { "sdca", typeof(SdcaNonCalibratedBinaryTrainer.Options) },
                { "sgdcalibrated", typeof(SgdCalibratedTrainer.Options) },
                { "symbolicsgdlogisticregression", typeof(SymbolicSgdLogisticRegressionBinaryTrainer.Options) },
                { "fieldawarefactorizationmachine", typeof(FieldAwareFactorizationMachineTrainer.Options) },
                { "lbfgslogisticregression", typeof(LbfgsLogisticRegressionBinaryTrainer.Options) },
                
                // Regression
                { "ols", typeof(OlsTrainer.Options) },
                { "onlinegradientdescent", typeof(OnlineGradientDescentTrainer.Options) },
                { "fasttreetweedie", typeof(FastTreeTweedieTrainer.Options) },
                { "lbfgspoissonregression", typeof(LbfgsPoissonRegressionTrainer.Options) },
                
                // Multi-class
                { "sdcamaximumentropy", typeof(SdcaMaximumEntropyMulticlassTrainer.Options) },
                { "lbfgsmaximumentropy", typeof(LbfgsMaximumEntropyMulticlassTrainer.Options) }
            };

            // Debug: Print all mapped types
            Console.WriteLine("Initialized algorithm option types:");
            foreach (var kvp in _algorithmOptionTypes)
            {
                Console.WriteLine($"  {kvp.Key} -> {kvp.Value.FullName}");
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
            if (!_algorithmOptionTypes.TryGetValue(algorithmName, out Type optionsType))
            {
                // If we don't have the type mapped, clear the parameter suggestions
                ClearParameterSuggestions();
                return;
            }

            try
            {
                // Get all public properties that can be set - more comprehensive approach
                var properties = optionsType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.CanWrite && p.CanRead)
                    .Where(p => !IsExcludedProperty(p.Name))
                    .Where(p => IsUserConfigurableType(p.PropertyType))
                    .OrderBy(p => p.Name)
                    .ToList();

                // Also get public fields that might be configurable
                var fields = optionsType.GetFields(BindingFlags.Public | BindingFlags.Instance)
                    .Where(f => !f.IsInitOnly && !f.IsLiteral)
                    .Where(f => !IsExcludedProperty(f.Name))
                    .Where(f => IsUserConfigurableType(f.FieldType))
                    .OrderBy(f => f.Name)
                    .ToList();

                Console.WriteLine($"Found {properties.Count} properties and {fields.Count} fields for {algorithmName}");

                // Debug: Print all found properties
                foreach (var prop in properties)
                {
                    Console.WriteLine($"  Property: {prop.Name} ({prop.PropertyType.Name})");
                }

                foreach (var field in fields)
                {
                    Console.WriteLine($"  Field: {field.Name} ({field.FieldType.Name})");
                }

                ShowParameterSuggestions(properties, fields);
            }
            catch (Exception ex)
            {
                // If reflection fails, show error in console but don't crash
                Console.WriteLine($"Error loading parameters for {algorithmName}: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                ClearParameterSuggestions();
            }
        }

        private bool IsUserConfigurableType(Type type)
        {
            // Allow nullable types
            Type underlyingType = Nullable.GetUnderlyingType(type) ?? type;

            // Check if it's a basic configurable type
            return underlyingType.IsPrimitive ||
                   underlyingType == typeof(string) ||
                   underlyingType == typeof(decimal) ||
                   underlyingType.IsEnum ||
                   underlyingType == typeof(TimeSpan);
        }

        private bool IsExcludedProperty(string propertyName)
        {
            // Exclude properties that are typically not user-configurable
            var excludedProperties = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "LabelColumnName",
                "FeatureColumnName",
                "ExampleWeightColumnName",
                "RowGroupColumnName",
                "GroupIdColumnName",
                "ScoreColumnName",
                "PredictedLabelColumnName",
                "ProbabilityColumnName"
            };

            return excludedProperties.Contains(propertyName);
        }

        private void ShowParameterSuggestions(List<PropertyInfo> properties, List<FieldInfo> fields = null)
        {
            // Clear existing parameter suggestions
            ClearParameterSuggestions();

            var totalCount = properties.Count + (fields?.Count ?? 0);

            if (totalCount == 0)
                return;

            // Add a context menu or tooltip to show available parameters
            var toolTip = new ToolTip();
            var parameterNames = new List<string>();

            parameterNames.AddRange(properties.Select(p => $"{p.Name} ({GetFriendlyTypeName(p.PropertyType)})"));

            if (fields != null)
            {
                parameterNames.AddRange(fields.Select(f => $"{f.Name} ({GetFriendlyTypeName(f.FieldType)})"));
            }

            var tooltipText = "Available Parameters:\n" + string.Join("\n", parameterNames.OrderBy(x => x));

            toolTip.SetToolTip(btnAddParameter, tooltipText);
            toolTip.SetToolTip(lvParameters, tooltipText);

            // Update the add parameter button text to indicate suggestions are available
            btnAddParameter.Text = $"Add Parameter ({totalCount} available)";
        }

        private void ClearParameterSuggestions()
        {
            btnAddParameter.Text = "Add Parameter";

            // Clear any existing tooltips
            var toolTip = new ToolTip();
            toolTip.SetToolTip(btnAddParameter, "");
            toolTip.SetToolTip(lvParameters, "");
        }

        private string GetFriendlyTypeName(Type type)
        {
            if (type == typeof(int)) return "int";
            if (type == typeof(double)) return "double";
            if (type == typeof(float)) return "float";
            if (type == typeof(bool)) return "bool";
            if (type == typeof(string)) return "string";
            if (type.IsEnum) return "enum";
            if (Nullable.GetUnderlyingType(type) != null)
            {
                var underlyingType = Nullable.GetUnderlyingType(type);
                return GetFriendlyTypeName(underlyingType) + "?";
            }
            return type.Name;
        }

        public void SetConfiguration(TrainingParameters parameters)
        {
            if (parameters == null) return;

            cboAlgorithm.Text = parameters.Algorithm;
            numTestFraction.Value = (decimal)parameters.TestFraction;
            _algorithmParameters = parameters.AlgorithmParameters ?? new Dictionary<string, object>();
            UpdateParametersListView();

            // Load parameter suggestions for the selected algorithm
            LoadAvailableParameters(parameters.Algorithm?.ToLower() ?? "");
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
            string selectedAlgorithm = cboAlgorithm.Text.ToLower();

            // Use the simple, working parameter dialog
            using (var form = new SimpleParameterDialog(selectedAlgorithm))
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

        private List<PropertyInfo> GetAvailableParametersForAlgorithm(string algorithmName)
        {
            if (!_algorithmOptionTypes.TryGetValue(algorithmName, out Type optionsType))
            {
                return new List<PropertyInfo>();
            }

            try
            {
                return optionsType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.CanWrite && p.CanRead)
                    .Where(p => !IsExcludedProperty(p.Name))
                    .Where(p => IsUserConfigurableType(p.PropertyType))
                    .OrderBy(p => p.Name)
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting parameters for {algorithmName}: {ex.Message}");
                return new List<PropertyInfo>();
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
            this.grpTraining = new System.Windows.Forms.GroupBox();
            this.lblAlgorithm = new System.Windows.Forms.Label();
            this.cboAlgorithm = new System.Windows.Forms.ComboBox();
            this.lblTestFraction = new System.Windows.Forms.Label();
            this.numTestFraction = new System.Windows.Forms.NumericUpDown();
            this.lblParameters = new System.Windows.Forms.Label();
            this.lvParameters = new System.Windows.Forms.ListView();
            this.colParameterName = new System.Windows.Forms.ColumnHeader();
            this.colParameterValue = new System.Windows.Forms.ColumnHeader();
            this.btnAddParameter = new System.Windows.Forms.Button();
            this.btnRemoveParameter = new System.Windows.Forms.Button();
            this.grpTraining.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numTestFraction)).BeginInit();
            this.SuspendLayout();
            // 
            // grpTraining
            // 
            this.grpTraining.Controls.Add(this.btnRemoveParameter);
            this.grpTraining.Controls.Add(this.btnAddParameter);
            this.grpTraining.Controls.Add(this.lvParameters);
            this.grpTraining.Controls.Add(this.lblParameters);
            this.grpTraining.Controls.Add(this.numTestFraction);
            this.grpTraining.Controls.Add(this.lblTestFraction);
            this.grpTraining.Controls.Add(this.cboAlgorithm);
            this.grpTraining.Controls.Add(this.lblAlgorithm);
            this.grpTraining.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpTraining.Location = new System.Drawing.Point(0, 0);
            this.grpTraining.Name = "grpTraining";
            this.grpTraining.Size = new System.Drawing.Size(492, 283);
            this.grpTraining.TabIndex = 0;
            this.grpTraining.TabStop = false;
            this.grpTraining.Text = "Training Parameters";
            // 
            // lblAlgorithm
            // 
            this.lblAlgorithm.AutoSize = true;
            this.lblAlgorithm.Location = new System.Drawing.Point(27, 38);
            this.lblAlgorithm.Name = "lblAlgorithm";
            this.lblAlgorithm.Size = new System.Drawing.Size(64, 15);
            this.lblAlgorithm.TabIndex = 0;
            this.lblAlgorithm.Text = "Algorithm:";
            // 
            // cboAlgorithm
            // 
            this.cboAlgorithm.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboAlgorithm.FormattingEnabled = true;
            this.cboAlgorithm.Location = new System.Drawing.Point(158, 35);
            this.cboAlgorithm.Name = "cboAlgorithm";
            this.cboAlgorithm.Size = new System.Drawing.Size(291, 23);
            this.cboAlgorithm.TabIndex = 1;
            // 
            // lblTestFraction
            // 
            this.lblTestFraction.AutoSize = true;
            this.lblTestFraction.Location = new System.Drawing.Point(27, 67);
            this.lblTestFraction.Name = "lblTestFraction";
            this.lblTestFraction.Size = new System.Drawing.Size(79, 15);
            this.lblTestFraction.TabIndex = 2;
            this.lblTestFraction.Text = "Test Fraction:";
            // 
            // numTestFraction
            // 
            this.numTestFraction.DecimalPlaces = 2;
            this.numTestFraction.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.numTestFraction.Location = new System.Drawing.Point(158, 65);
            this.numTestFraction.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numTestFraction.Name = "numTestFraction";
            this.numTestFraction.Size = new System.Drawing.Size(120, 23);
            this.numTestFraction.TabIndex = 3;
            this.numTestFraction.Value = new decimal(new int[] {
            2,
            0,
            0,
            65536});
            // 
            // lblParameters
            // 
            this.lblParameters.AutoSize = true;
            this.lblParameters.Location = new System.Drawing.Point(27, 103);
            this.lblParameters.Name = "lblParameters";
            this.lblParameters.Size = new System.Drawing.Size(131, 15);
            this.lblParameters.TabIndex = 4;
            this.lblParameters.Text = "Algorithm Parameters:";
            // 
            // lvParameters
            // 
            this.lvParameters.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colParameterName,
            this.colParameterValue});
            this.lvParameters.FullRowSelect = true;
            this.lvParameters.GridLines = true;
            this.lvParameters.HideSelection = false;
            this.lvParameters.Location = new System.Drawing.Point(27, 121);
            this.lvParameters.MultiSelect = false;
            this.lvParameters.Name = "lvParameters";
            this.lvParameters.Size = new System.Drawing.Size(422, 118);
            this.lvParameters.TabIndex = 5;
            this.lvParameters.UseCompatibleStateImageBehavior = false;
            this.lvParameters.View = System.Windows.Forms.View.Details;
            // 
            // colParameterName
            // 
            this.colParameterName.Text = "Parameter Name";
            this.colParameterName.Width = 200;
            // 
            // colParameterValue
            // 
            this.colParameterValue.Text = "Value";
            this.colParameterValue.Width = 200;
            // 
            // btnAddParameter
            // 
            this.btnAddParameter.Location = new System.Drawing.Point(242, 245);
            this.btnAddParameter.Name = "btnAddParameter";
            this.btnAddParameter.Size = new System.Drawing.Size(150, 25);
            this.btnAddParameter.TabIndex = 6;
            this.btnAddParameter.Text = "Add Parameter";
            this.btnAddParameter.UseVisualStyleBackColor = true;
            this.btnAddParameter.Click += new System.EventHandler(this.btnAddParameter_Click);
            // 
            // btnRemoveParameter
            // 
            this.btnRemoveParameter.Location = new System.Drawing.Point(398, 245);
            this.btnRemoveParameter.Name = "btnRemoveParameter";
            this.btnRemoveParameter.Size = new System.Drawing.Size(51, 25);
            this.btnRemoveParameter.TabIndex = 7;
            this.btnRemoveParameter.Text = "Remove";
            this.btnRemoveParameter.UseVisualStyleBackColor = true;
            this.btnRemoveParameter.Click += new System.EventHandler(this.btnRemoveParameter_Click);
            // 
            // TrainingParametersControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.grpTraining);
            this.Name = "TrainingParametersControl";
            this.Size = new System.Drawing.Size(492, 283);
            this.grpTraining.ResumeLayout(false);
            this.grpTraining.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numTestFraction)).EndInit();
            this.ResumeLayout(false);
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