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
using System.Collections;

namespace D2G.Iris.ML.ConfigUI.Controls
{
    public partial class TrainingParametersControl : UserControl
    {
        private Dictionary<string, object> _algorithmParameters = new Dictionary<string, object>();
        private Dictionary<string, Type> _algorithmOptionTypes = new Dictionary<string, Type>();
        private Dictionary<string, Dictionary<ModelType, Type>> _algorithmTypeMapping; 
        private ModelType _currentModelType = ModelType.BinaryClassification;


        private readonly Dictionary<ModelType, List<string>> _algorithmsByModelType = new Dictionary<ModelType, List<string>>
        {
            [ModelType.BinaryClassification] = new List<string>
    {
        "FastForest",
        "FastTree",
        "LightGbm",
        "SdcaLogisticRegression",
        "Gam",
        "AveragedPerceptron",
        "LinearSvm",
        "LdSvm",
        "Sdca",
        "SgdCalibrated",
        "SymbolicSgdLogisticRegression",
        "FieldAwareFactorizationMachine",
        "LbfgsLogisticRegression"
    },
            [ModelType.MultiClassClassification] = new List<string>
    {
        "LightGbm",
        "SdcaMaximumEntropy",
        "Sdca",
        "FastTree",
        "FastForest",
        "LbfgsMaximumEntropy"
    },
            [ModelType.Regression] = new List<string>
    {
        "FastForest",
        "FastTree",
        "LightGbm",
        "Ols",
        "OnlineGradientDescent",
        "Gam",
        "Sdca",
        "FastTreeTweedie",
        "LbfgsPoissonRegression"
    }
        };



        private Type GetOptionsTypeForAlgorithm(string algorithm, ModelType modelType)
        {
            string algorithmLower = algorithm.ToLower();

            if (_algorithmTypeMapping != null &&
                _algorithmTypeMapping.TryGetValue(algorithmLower, out var modelTypeMap))
            {
                if (modelTypeMap.TryGetValue(modelType, out var optionsType))
                {
                    return optionsType;
                }
            }
            return _algorithmOptionTypes.TryGetValue(algorithmLower, out var fallbackType)
                ? fallbackType
                : null;
        }

        public TrainingParametersControl()
        {
            InitializeComponent();
            InitializeAlgorithmOptionTypes();
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

            if (_algorithmsByModelType.TryGetValue(_currentModelType, out var algorithms))
            {
                foreach (var algorithm in algorithms)
                {
                    cboAlgorithm.Items.Add(algorithm);
                }

                if (cboAlgorithm.Items.Count > 0)
                {
                    cboAlgorithm.SelectedIndex = 0;
                }
            }
        }

        private void InitializeAlgorithmOptionTypes()
        {
            var algorithmTypeMapping = new Dictionary<string, Dictionary<ModelType, Type>>(StringComparer.OrdinalIgnoreCase)
            {
                ["lightgbm"] = new Dictionary<ModelType, Type>
                {
                    [ModelType.BinaryClassification] = typeof(LightGbmBinaryTrainer.Options),
                    [ModelType.MultiClassClassification] = typeof(LightGbmMulticlassTrainer.Options),
                    [ModelType.Regression] = typeof(LightGbmRegressionTrainer.Options)
                },

                ["fastforest"] = new Dictionary<ModelType, Type>
                {
                    [ModelType.BinaryClassification] = typeof(FastForestBinaryTrainer.Options),
                    [ModelType.Regression] = typeof(FastForestRegressionTrainer.Options)
                },

                ["fasttree"] = new Dictionary<ModelType, Type>
                {
                    [ModelType.BinaryClassification] = typeof(FastTreeBinaryTrainer.Options),
                    [ModelType.Regression] = typeof(FastTreeRegressionTrainer.Options)
                },

                ["gam"] = new Dictionary<ModelType, Type>
                {
                    [ModelType.BinaryClassification] = typeof(GamBinaryTrainer.Options),
                    [ModelType.Regression] = typeof(GamRegressionTrainer.Options)
                },

                ["sdca"] = new Dictionary<ModelType, Type>
                {
                    [ModelType.BinaryClassification] = typeof(SdcaNonCalibratedBinaryTrainer.Options),
                    [ModelType.MultiClassClassification] = typeof(SdcaNonCalibratedMulticlassTrainer.Options),
                    [ModelType.Regression] = typeof(SdcaRegressionTrainer.Options)
                },

                // Binary Classification only algorithms
                ["sdcalogisticregression"] = new Dictionary<ModelType, Type>
                {
                    [ModelType.BinaryClassification] = typeof(SdcaLogisticRegressionBinaryTrainer.Options)
                },

                ["averagedperceptron"] = new Dictionary<ModelType, Type>
                {
                    [ModelType.BinaryClassification] = typeof(AveragedPerceptronTrainer.Options)
                },

                ["linearsvm"] = new Dictionary<ModelType, Type>
                {
                    [ModelType.BinaryClassification] = typeof(LinearSvmTrainer.Options)
                },

                ["ldsvm"] = new Dictionary<ModelType, Type>
                {
                    [ModelType.BinaryClassification] = typeof(LdSvmTrainer.Options)
                },

                ["sgdcalibrated"] = new Dictionary<ModelType, Type>
                {
                    [ModelType.BinaryClassification] = typeof(SgdCalibratedTrainer.Options)
                },

                ["symbolicsgdlogisticregression"] = new Dictionary<ModelType, Type>
                {
                    [ModelType.BinaryClassification] = typeof(SymbolicSgdLogisticRegressionBinaryTrainer.Options)
                },

                ["fieldawarefactorizationmachine"] = new Dictionary<ModelType, Type>
                {
                    [ModelType.BinaryClassification] = typeof(FieldAwareFactorizationMachineTrainer.Options)
                },

                ["lbfgslogisticregression"] = new Dictionary<ModelType, Type>
                {
                    [ModelType.BinaryClassification] = typeof(LbfgsLogisticRegressionBinaryTrainer.Options)
                },

                // Regression only algorithms
                ["ols"] = new Dictionary<ModelType, Type>
                {
                    [ModelType.Regression] = typeof(OlsTrainer.Options)
                },

                ["onlinegradientdescent"] = new Dictionary<ModelType, Type>
                {
                    [ModelType.Regression] = typeof(OnlineGradientDescentTrainer.Options)
                },

                ["fasttreetweedie"] = new Dictionary<ModelType, Type>
                {
                    [ModelType.Regression] = typeof(FastTreeTweedieTrainer.Options)
                },

                ["lbfgspoissonregression"] = new Dictionary<ModelType, Type>
                {
                    [ModelType.Regression] = typeof(LbfgsPoissonRegressionTrainer.Options)
                },

                // Multi-class only algorithms
                ["sdcamaximumentropy"] = new Dictionary<ModelType, Type>
                {
                    [ModelType.MultiClassClassification] = typeof(SdcaMaximumEntropyMulticlassTrainer.Options)
                },

                ["lbfgsmaximumentropy"] = new Dictionary<ModelType, Type>
                {
                    [ModelType.MultiClassClassification] = typeof(LbfgsMaximumEntropyMulticlassTrainer.Options)
                }
            };

            _algorithmTypeMapping = algorithmTypeMapping;

            _algorithmOptionTypes = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
            foreach (var algorithm in algorithmTypeMapping)
            {
                var firstType = algorithm.Value.Values.FirstOrDefault();
                if (firstType != null)
                {
                    _algorithmOptionTypes[algorithm.Key] = firstType;
                }
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
            Type optionsType = GetOptionsTypeForAlgorithm(algorithmName, _currentModelType);

            if (optionsType == null)
            {
                ClearParameterSuggestions();
                return;
            }

            try
            {
                var properties = optionsType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.CanWrite && p.CanRead)
                    .Where(p => !IsExcludedProperty(p.Name))
                    .Where(p => IsUserConfigurableType(p.PropertyType))
                    .OrderBy(p => p.Name)
                    .ToList();

                var fields = optionsType.GetFields(BindingFlags.Public | BindingFlags.Instance)
                    .Where(f => !f.IsInitOnly && !f.IsLiteral)
                    .Where(f => !IsExcludedProperty(f.Name))
                    .Where(f => IsUserConfigurableType(f.FieldType))
                    .OrderBy(f => f.Name)
                    .ToList();

                ShowParameterSuggestions(properties, fields);
            }
            catch (Exception)
            {
                ClearParameterSuggestions();
            }
        }

        private bool IsUserConfigurableType(Type type)
        {
            Type underlyingType = Nullable.GetUnderlyingType(type) ?? type;

            return underlyingType.IsPrimitive ||
                   underlyingType == typeof(string) ||
                   underlyingType == typeof(decimal) ||
                   underlyingType.IsEnum ||
                   underlyingType == typeof(TimeSpan);
        }

        private bool IsExcludedProperty(string propertyName)
        {
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
            ClearParameterSuggestions();

            var totalCount = properties.Count + (fields?.Count ?? 0);

            if (totalCount == 0)
                return;

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

            btnAddParameter.Text = $"Add Parameter ({totalCount} available)";
        }

        private void ClearParameterSuggestions()
        {
            btnAddParameter.Text = "Add Parameter";

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
            string algorithm = parameters.Algorithm?.ToLower();
            if (!string.IsNullOrEmpty(algorithm) &&
                _algorithmsByModelType.TryGetValue(_currentModelType, out var availableAlgorithms) &&
                availableAlgorithms.Contains(algorithm))
            {
                cboAlgorithm.Text = parameters.Algorithm;
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
            string selectedAlgorithm = cboAlgorithm.Text.ToLower();

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