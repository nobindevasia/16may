using System;
using System.Windows.Forms;
using D2G.Iris.ML.Core.Models;
using D2G.Iris.ML.Core.Enums;

namespace D2G.Iris.ML.ConfigUI.Controls
{
    public partial class FeatureEngineeringControl : UserControl
    {
        public FeatureEngineeringControl()
        {
            InitializeComponent();
            InitializeMethodComboBox();
            SetupEventHandlers();
            SetInitialControlVisibility();
        }

        private void SetInitialControlVisibility()
        {
            numNumberOfComponents.Visible = false;
            lblNumberOfComponents.Visible = false;

            numMaxFeatures.Visible = false;
            numMulticollinearityThreshold.Visible = false;
            lblMaxFeatures.Visible = false;
            lblMulticollinearityThreshold.Visible = false;
        }

        private void InitializeMethodComboBox()
        {
            cboMethod.Items.Clear();
            cboMethod.Items.Add(FeatureSelectionMethod.None);
            cboMethod.Items.Add(FeatureSelectionMethod.Correlation);
            cboMethod.Items.Add(FeatureSelectionMethod.PCA);
            cboMethod.SelectedIndex = 0;
        }

        private void SetupEventHandlers()
        {
            cboMethod.SelectedIndexChanged += CboMethod_SelectedIndexChanged;
        }

        private void CboMethod_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedMethod = (FeatureSelectionMethod)cboMethod.SelectedItem;
            bool isMethodSelected = selectedMethod != FeatureSelectionMethod.None;

            numExecutionOrder.Enabled = isMethodSelected;
            lblExecutionOrder.Enabled = isMethodSelected;

            bool isPCA = selectedMethod == FeatureSelectionMethod.PCA;
            numNumberOfComponents.Visible = isPCA;
            lblNumberOfComponents.Visible = isPCA;

            bool isCorrelation = selectedMethod == FeatureSelectionMethod.Correlation;
            numMaxFeatures.Visible = isCorrelation;
            numMulticollinearityThreshold.Visible = isCorrelation;
            lblMaxFeatures.Visible = isCorrelation;
            lblMulticollinearityThreshold.Visible = isCorrelation;

            AdjustControlPositions(selectedMethod);

            UpdateDescription(selectedMethod);
        }

        private void AdjustControlPositions(FeatureSelectionMethod method)
        {
            int nextY = 105; 

            if (method == FeatureSelectionMethod.PCA)
            {
                lblNumberOfComponents.Location = new Point(24, nextY);
                numNumberOfComponents.Location = new Point(190, nextY - 2);
                nextY += 35;
            }
            else if (method == FeatureSelectionMethod.Correlation)
            {
                lblMaxFeatures.Location = new Point(24, nextY);
                numMaxFeatures.Location = new Point(190, nextY - 2);
                nextY += 35;

                lblMulticollinearityThreshold.Location = new Point(24, nextY);
                numMulticollinearityThreshold.Location = new Point(190, nextY - 2);
                nextY += 35;
            }

            lblDescription.Location = new Point(24, nextY + 10);
        }

        private void UpdateDescription(FeatureSelectionMethod method)
        {
            string description = method switch
            {
                FeatureSelectionMethod.None => "No feature selection will be applied. All enabled features will be used for training.",
                FeatureSelectionMethod.Correlation => "Correlation-based feature selection removes highly correlated features and selects features with strong correlation to the target variable. Configure the multicollinearity threshold and maximum number of features.",
                FeatureSelectionMethod.PCA => "Principal Component Analysis (PCA) reduces dimensionality by creating new features that are linear combinations of the original features. Specify the number of principal components to retain.",
                _ => ""
            };
            lblDescription.Text = description;
        }

        public void SetConfiguration(FeatureEngineeringConfig config)
        {
            if (config == null)
            {
                cboMethod.SelectedItem = FeatureSelectionMethod.None;
                numExecutionOrder.Value = 2;
                numNumberOfComponents.Value = 3;
                numMaxFeatures.Value = 10;
                numMulticollinearityThreshold.Value = 0.7m;
                SetInitialControlVisibility();
                return;
            }

            cboMethod.SelectedItem = config.Method;
            numExecutionOrder.Value = Math.Max(1, Math.Min(2, config.ExecutionOrder));
            numNumberOfComponents.Value = Math.Max(1, Math.Min(50, config.NumberOfComponents));
            numMaxFeatures.Value = Math.Max(1, Math.Min(100, config.MaxFeatures));
            numMulticollinearityThreshold.Value = (decimal)Math.Max(0.1, Math.Min(1.0, config.MulticollinearityThreshold));


            CboMethod_SelectedIndexChanged(cboMethod, EventArgs.Empty);
        }

        public FeatureEngineeringConfig GetConfiguration()
        {
            return new FeatureEngineeringConfig
            {
                Method = (FeatureSelectionMethod)cboMethod.SelectedItem,
                ExecutionOrder = (int)numExecutionOrder.Value,
                NumberOfComponents = (int)numNumberOfComponents.Value,
                MaxFeatures = (int)numMaxFeatures.Value,
                MulticollinearityThreshold = (double)numMulticollinearityThreshold.Value
            };
        }

        private void InitializeComponent()
        {
            grpFeatureEngineering = new GroupBox();
            lblMethod = new Label();
            cboMethod = new ComboBox();
            lblExecutionOrder = new Label();
            numExecutionOrder = new NumericUpDown();
            lblNumberOfComponents = new Label();
            numNumberOfComponents = new NumericUpDown();
            lblMaxFeatures = new Label();
            numMaxFeatures = new NumericUpDown();
            lblMulticollinearityThreshold = new Label();
            numMulticollinearityThreshold = new NumericUpDown();
            lblDescription = new Label();
            grpFeatureEngineering.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numExecutionOrder).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numNumberOfComponents).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numMaxFeatures).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numMulticollinearityThreshold).BeginInit();
            SuspendLayout();
            // 
            // grpFeatureEngineering
            // 
            grpFeatureEngineering.Controls.Add(lblDescription);
            grpFeatureEngineering.Controls.Add(numMulticollinearityThreshold);
            grpFeatureEngineering.Controls.Add(lblMulticollinearityThreshold);
            grpFeatureEngineering.Controls.Add(numMaxFeatures);
            grpFeatureEngineering.Controls.Add(lblMaxFeatures);
            grpFeatureEngineering.Controls.Add(numNumberOfComponents);
            grpFeatureEngineering.Controls.Add(lblNumberOfComponents);
            grpFeatureEngineering.Controls.Add(numExecutionOrder);
            grpFeatureEngineering.Controls.Add(lblExecutionOrder);
            grpFeatureEngineering.Controls.Add(cboMethod);
            grpFeatureEngineering.Controls.Add(lblMethod);
            grpFeatureEngineering.Dock = DockStyle.Fill;
            grpFeatureEngineering.Location = new Point(0, 0);
            grpFeatureEngineering.Name = "grpFeatureEngineering";
            grpFeatureEngineering.Size = new Size(492, 283);
            grpFeatureEngineering.TabIndex = 0;
            grpFeatureEngineering.TabStop = false;
            grpFeatureEngineering.Text = "Feature Engineering Settings";
            // 
            // lblMethod
            // 
            lblMethod.AutoSize = true;
            lblMethod.Location = new Point(24, 35);
            lblMethod.Name = "lblMethod";
            lblMethod.Size = new Size(52, 15);
            lblMethod.TabIndex = 0;
            lblMethod.Text = "Method:";
            // 
            // cboMethod
            // 
            cboMethod.DropDownStyle = ComboBoxStyle.DropDownList;
            cboMethod.FormattingEnabled = true;
            cboMethod.Location = new Point(190, 32);
            cboMethod.Name = "cboMethod";
            cboMethod.Size = new Size(200, 23);
            cboMethod.TabIndex = 1;
            // 
            // lblExecutionOrder
            // 
            lblExecutionOrder.AutoSize = true;
            lblExecutionOrder.Location = new Point(24, 70);
            lblExecutionOrder.Name = "lblExecutionOrder";
            lblExecutionOrder.Size = new Size(90, 15);
            lblExecutionOrder.TabIndex = 2;
            lblExecutionOrder.Text = "Execution Order:";
            // 
            // numExecutionOrder
            // 
            numExecutionOrder.Location = new Point(190, 68);
            numExecutionOrder.Maximum = new decimal(new int[] { 2, 0, 0, 0 });
            numExecutionOrder.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numExecutionOrder.Name = "numExecutionOrder";
            numExecutionOrder.Size = new Size(80, 23);
            numExecutionOrder.TabIndex = 3;
            numExecutionOrder.Value = new decimal(new int[] { 2, 0, 0, 0 });
            // 
            // lblNumberOfComponents
            // 
            lblNumberOfComponents.AutoSize = true;
            lblNumberOfComponents.Location = new Point(24, 105);
            lblNumberOfComponents.Name = "lblNumberOfComponents";
            lblNumberOfComponents.Size = new Size(138, 15);
            lblNumberOfComponents.TabIndex = 4;
            lblNumberOfComponents.Text = "Number of Components:";
            // 
            // numNumberOfComponents
            // 
            numNumberOfComponents.Location = new Point(190, 103);
            numNumberOfComponents.Maximum = new decimal(new int[] { 50, 0, 0, 0 });
            numNumberOfComponents.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numNumberOfComponents.Name = "numNumberOfComponents";
            numNumberOfComponents.Size = new Size(80, 23);
            numNumberOfComponents.TabIndex = 5;
            numNumberOfComponents.Value = new decimal(new int[] { 3, 0, 0, 0 });
            // 
            // lblMaxFeatures
            // 
            lblMaxFeatures.AutoSize = true;
            lblMaxFeatures.Location = new Point(24, 140);
            lblMaxFeatures.Name = "lblMaxFeatures";
            lblMaxFeatures.Size = new Size(80, 15);
            lblMaxFeatures.TabIndex = 6;
            lblMaxFeatures.Text = "Max Features:";
            // 
            // numMaxFeatures
            // 
            numMaxFeatures.Location = new Point(190, 138);
            numMaxFeatures.Maximum = new decimal(new int[] { 100, 0, 0, 0 });
            numMaxFeatures.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numMaxFeatures.Name = "numMaxFeatures";
            numMaxFeatures.Size = new Size(80, 23);
            numMaxFeatures.TabIndex = 7;
            numMaxFeatures.Value = new decimal(new int[] { 10, 0, 0, 0 });
            // 
            // lblMulticollinearityThreshold
            // 
            lblMulticollinearityThreshold.AutoSize = true;
            lblMulticollinearityThreshold.Location = new Point(24, 175);
            lblMulticollinearityThreshold.Name = "lblMulticollinearityThreshold";
            lblMulticollinearityThreshold.Size = new Size(148, 15);
            lblMulticollinearityThreshold.TabIndex = 8;
            lblMulticollinearityThreshold.Text = "Multicollinearity Threshold:";
            // 
            // numMulticollinearityThreshold
            // 
            numMulticollinearityThreshold.DecimalPlaces = 2;
            numMulticollinearityThreshold.Increment = new decimal(new int[] { 1, 0, 0, 131072 });
            numMulticollinearityThreshold.Location = new Point(190, 173);
            numMulticollinearityThreshold.Maximum = new decimal(new int[] { 1, 0, 0, 0 });
            numMulticollinearityThreshold.Minimum = new decimal(new int[] { 1, 0, 0, 131072 });
            numMulticollinearityThreshold.Name = "numMulticollinearityThreshold";
            numMulticollinearityThreshold.Size = new Size(80, 23);
            numMulticollinearityThreshold.TabIndex = 9;
            numMulticollinearityThreshold.Value = new decimal(new int[] { 7, 0, 0, 65536 });
            // 
            // lblDescription
            // 
            lblDescription.ForeColor = Color.DarkBlue;
            lblDescription.Location = new Point(24, 210);
            lblDescription.Name = "lblDescription";
            lblDescription.Size = new Size(440, 65);
            lblDescription.TabIndex = 10;
            lblDescription.Text = "No feature selection will be applied. All enabled features will be used for training.";
            // 
            // FeatureEngineeringControl
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(grpFeatureEngineering);
            Name = "FeatureEngineeringControl";
            Size = new Size(492, 283);
            grpFeatureEngineering.ResumeLayout(false);
            grpFeatureEngineering.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numExecutionOrder).EndInit();
            ((System.ComponentModel.ISupportInitialize)numNumberOfComponents).EndInit();
            ((System.ComponentModel.ISupportInitialize)numMaxFeatures).EndInit();
            ((System.ComponentModel.ISupportInitialize)numMulticollinearityThreshold).EndInit();
            ResumeLayout(false);
        }

        private GroupBox grpFeatureEngineering;
        private Label lblMethod;
        private ComboBox cboMethod;
        private Label lblExecutionOrder;
        private NumericUpDown numExecutionOrder;
        private Label lblNumberOfComponents;
        private NumericUpDown numNumberOfComponents;
        private Label lblMaxFeatures;
        private NumericUpDown numMaxFeatures;
        private Label lblMulticollinearityThreshold;
        private NumericUpDown numMulticollinearityThreshold;
        private Label lblDescription;
    }
}