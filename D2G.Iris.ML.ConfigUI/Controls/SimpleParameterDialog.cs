using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.ML.Trainers;
using Microsoft.ML.Trainers.FastTree;
using Microsoft.ML.Trainers.LightGbm;

namespace D2G.Iris.ML.ConfigUI.Controls
{
    public partial class SimpleParameterDialog : Form
    {
        public string ParameterName { get; set; }
        public object ParameterValue { get; set; }

        private readonly string _algorithmName;
        private List<PropertyInfo> _availableParameters;

        public SimpleParameterDialog(string algorithmName)
        {
            InitializeComponent();
            _algorithmName = algorithmName;
            LoadParametersDirectly();
        }

        private void LoadParametersDirectly()
        {
            try
            {
                Type optionsType = GetOptionsTypeForAlgorithm(_algorithmName);

                if (optionsType == null)
                {
                    lblInfo.Text = $"No parameter mapping found for algorithm: {_algorithmName}";
                    return;
                }

                var allProperties = optionsType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

                var configurableProperties = allProperties
                    .Where(p => p.CanWrite && p.CanRead)
                    .Where(p => IsConfigurableParameter(p.Name, p.PropertyType))
                    .OrderBy(p => p.Name)
                    .ToList();

                var allFields = optionsType.GetFields(BindingFlags.Public | BindingFlags.Instance);

                var configurableFields = allFields
                    .Where(f => !f.IsInitOnly && !f.IsLiteral)
                    .Where(f => IsConfigurableParameter(f.Name, f.FieldType))
                    .OrderBy(f => f.Name)
                    .ToList();

                _availableParameters = new List<PropertyInfo>();

                _availableParameters.AddRange(configurableProperties);

                foreach (var field in configurableFields)
                {
                    _availableParameters.Add(new FieldAsProperty(field));
                }

                PopulateParameterList();
            }
            catch (Exception ex)
            {
                lblInfo.Text = $"Error loading parameters: {ex.Message}";
            }
        }

        private Type GetOptionsTypeForAlgorithm(string algorithm)
        {
            var algorithmLower = algorithm.ToLower();

            try
            {
                return algorithmLower switch
                {
                    // Binary Classification
                    "fastforest" => typeof(FastForestBinaryTrainer.Options),
                    "fasttree" => typeof(FastTreeBinaryTrainer.Options),
                    "lightgbm" => typeof(LightGbmBinaryTrainer.Options),
                    "sdcalogisticregression" => typeof(SdcaLogisticRegressionBinaryTrainer.Options),
                    "gam" => typeof(GamBinaryTrainer.Options),
                    "averagedperceptron" => typeof(AveragedPerceptronTrainer.Options),
                    "linearsvm" => typeof(LinearSvmTrainer.Options),
                    "ldsvm" => typeof(LdSvmTrainer.Options),
                    "sdca" => typeof(SdcaNonCalibratedBinaryTrainer.Options),
                    "sgdcalibrated" => typeof(SgdCalibratedTrainer.Options),
                    "symbolicsgdlogisticregression" => typeof(SymbolicSgdLogisticRegressionBinaryTrainer.Options),
                    "fieldawarefactorizationmachine" => typeof(FieldAwareFactorizationMachineTrainer.Options),
                    "lbfgslogisticregression" => typeof(LbfgsLogisticRegressionBinaryTrainer.Options),

                    // Regression
                    "ols" => typeof(OlsTrainer.Options),
                    "onlinegradientdescent" => typeof(OnlineGradientDescentTrainer.Options),
                    "fasttreetweedie" => typeof(FastTreeTweedieTrainer.Options),
                    "lbfgspoissonregression" => typeof(LbfgsPoissonRegressionTrainer.Options),

                    // Multi-class
                    "sdcamaximumentropy" => typeof(SdcaMaximumEntropyMulticlassTrainer.Options),
                    "lbfgsmaximumentropy" => typeof(LbfgsMaximumEntropyMulticlassTrainer.Options),

                    _ => TryGetGenericType(algorithmLower)
                };
            }
            catch (Exception)
            {
                return TryGetGenericType(algorithmLower);
            }
        }

        private Type TryGetGenericType(string algorithm)
        {
            try
            {
                switch (algorithm)
                {
                    case "fastforest":
                        // Try regression version if binary didn't work
                        return typeof(FastForestRegressionTrainer.Options);
                    case "fasttree":
                        // Try regression version if binary didn't work  
                        return typeof(FastTreeRegressionTrainer.Options);
                    case "lightgbm":
                        // Try regression version if binary didn't work
                        return typeof(LightGbmRegressionTrainer.Options);
                    case "gam":
                        // Try regression version if binary didn't work
                        return typeof(GamRegressionTrainer.Options);
                    case "sdca":
                        // Try regression version if binary didn't work
                        return typeof(SdcaRegressionTrainer.Options);
                    default:
                        return null;
                }
            }
            catch
            {
                return null;
            }
        }

        private bool IsConfigurableParameter(string name, Type type)
        {
            var excludedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase) {
                "LabelColumnName", "FeatureColumnName", "ExampleWeightColumnName",
                "RowGroupColumnName", "GroupIdColumnName", "ScoreColumnName",
                "PredictedLabelColumnName", "ProbabilityColumnName"
            };

            if (excludedNames.Contains(name))
            {
                return false;
            }

            var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

            bool isConfigurable = underlyingType.IsPrimitive ||
                   underlyingType == typeof(string) ||
                   underlyingType == typeof(decimal) ||
                   underlyingType.IsEnum ||
                   underlyingType == typeof(TimeSpan);

            return isConfigurable;
        }

        private void PopulateParameterList()
        {
            listBoxParameters.Items.Clear();

            foreach (var param in _availableParameters)
            {
                string displayText = $"{param.Name} ({GetTypeName(param.PropertyType)})";
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

        private string GetTypeName(Type type)
        {
            var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

            return underlyingType.Name switch
            {
                "Int32" => "int",
                "Double" => "double",
                "Single" => "float",
                "Boolean" => "bool",
                "String" => "string",
                _ => underlyingType.IsEnum ? "enum" : underlyingType.Name
            };
        }

        private void listBoxParameters_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxParameters.SelectedItem is ParameterItem selectedItem)
            {
                var prop = selectedItem.Property;
                txtParameterName.Text = prop.Name;

                var type = prop.PropertyType;
                var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

                if (underlyingType == typeof(int))
                    txtParameterValue.Text = "0";
                else if (underlyingType == typeof(double))
                    txtParameterValue.Text = "0.0";
                else if (underlyingType == typeof(float))
                    txtParameterValue.Text = "0.0f";
                else if (underlyingType == typeof(bool))
                    txtParameterValue.Text = "true";
                else if (underlyingType.IsEnum)
                {
                    var enumValues = Enum.GetNames(underlyingType);
                    txtParameterValue.Text = enumValues.Length > 0 ? enumValues[0] : "";
                }
                else
                    txtParameterValue.Text = "";

                UpdateValueHint(prop);
            }
        }

        private void UpdateValueHint(PropertyInfo prop)
        {
            var type = prop.PropertyType;
            var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

            if (underlyingType.IsEnum)
            {
                var enumValues = Enum.GetNames(underlyingType);
                lblValueHint.Text = $"Valid values: {string.Join(", ", enumValues)}";
            }
            else if (underlyingType == typeof(bool))
            {
                lblValueHint.Text = "Valid values: true, false";
            }
            else if (underlyingType == typeof(int))
            {
                lblValueHint.Text = "Enter an integer value";
            }
            else if (underlyingType == typeof(double) || underlyingType == typeof(float))
            {
                lblValueHint.Text = "Enter a decimal value";
            }
            else
            {
                lblValueHint.Text = "Enter a value";
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
                    ParameterValue = ConvertValue(txtParameterValue.Text, selectedParam.PropertyType);
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

        private object ConvertValue(string value, Type targetType)
        {
            var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

            if (underlyingType == typeof(int))
                return int.Parse(value);
            else if (underlyingType == typeof(double))
                return double.Parse(value);
            else if (underlyingType == typeof(float))
                return float.Parse(value.Replace("f", ""));
            else if (underlyingType == typeof(bool))
                return bool.Parse(value);
            else if (underlyingType.IsEnum)
                return Enum.Parse(underlyingType, value, true);
            else
                return value;
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

    public class ParameterItem
    {
        public PropertyInfo Property { get; set; }
        public string DisplayText { get; set; }
    }

    // Helper class to treat fields as properties for uniform handling
    public class FieldAsProperty : PropertyInfo
    {
        private readonly FieldInfo _field;

        public FieldAsProperty(FieldInfo field)
        {
            _field = field;
        }

        public override string Name => _field.Name;
        public override Type PropertyType => _field.FieldType;
        public override bool CanWrite => !_field.IsInitOnly && !_field.IsLiteral;
        public override bool CanRead => true;

        public override object GetValue(object obj, BindingFlags invokeAttr, Binder binder, object[] index, System.Globalization.CultureInfo culture)
        {
            return _field.GetValue(obj);
        }

        public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, System.Globalization.CultureInfo culture)
        {
            _field.SetValue(obj, value);
        }

        // Required overrides for abstract class
        public override PropertyAttributes Attributes => PropertyAttributes.None;
        public override Type DeclaringType => _field.DeclaringType;
        public override Type ReflectedType => _field.ReflectedType;

        public override MethodInfo GetGetMethod(bool nonPublic) => null;
        public override MethodInfo GetSetMethod(bool nonPublic) => null;
        public override MethodInfo[] GetAccessors(bool nonPublic) => new MethodInfo[0];

        public override ParameterInfo[] GetIndexParameters() => new ParameterInfo[0];
        public override object[] GetCustomAttributes(Type attributeType, bool inherit) => _field.GetCustomAttributes(attributeType, inherit);
        public override object[] GetCustomAttributes(bool inherit) => _field.GetCustomAttributes(inherit);
        public override bool IsDefined(Type attributeType, bool inherit) => _field.IsDefined(attributeType, inherit);
    }
}