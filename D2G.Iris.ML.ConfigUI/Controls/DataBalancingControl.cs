using System;
using System.Windows.Forms;
using D2G.Iris.ML.Core.Models;
using D2G.Iris.ML.Core.Enums;

namespace D2G.Iris.ML.ConfigUI.Controls
{
    public partial class DataBalancingControl : UserControl
    {
        public DataBalancingControl()
        {
            InitializeComponent();
            InitializeMethodComboBox();
            SetupEventHandlers();
        }

        private void InitializeMethodComboBox()
        {
            cboMethod.Items.Clear();
            cboMethod.Items.Add(DataBalanceMethod.None);
            cboMethod.Items.Add(DataBalanceMethod.SMOTE);
            cboMethod.SelectedIndex = 0;
        }

        private void SetupEventHandlers()
        {
            cboMethod.SelectedIndexChanged += CboMethod_SelectedIndexChanged;
        }

        private void CboMethod_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool isSmoteSelected = (DataBalanceMethod)cboMethod.SelectedItem == DataBalanceMethod.SMOTE;

            numExecutionOrder.Enabled = isSmoteSelected;
            numKNeighbors.Enabled = isSmoteSelected;
            numUndersamplingRatio.Enabled = isSmoteSelected;
            numMinorityToMajorityRatio.Enabled = isSmoteSelected;

            lblExecutionOrder.Enabled = isSmoteSelected;
            lblKNeighbors.Enabled = isSmoteSelected;
            lblUndersamplingRatio.Enabled = isSmoteSelected;
            lblMinorityToMajorityRatio.Enabled = isSmoteSelected;

            UpdateControlAppearance(isSmoteSelected);

            UpdateDescription(isSmoteSelected);
        }

        private void UpdateControlAppearance(bool enabled)
        {
            var disabledBackColor = System.Drawing.SystemColors.Control;
            var enabledBackColor = System.Drawing.SystemColors.Window;
            var disabledForeColor = System.Drawing.SystemColors.GrayText;
            var enabledForeColor = System.Drawing.SystemColors.ControlText;

            numExecutionOrder.BackColor = enabled ? enabledBackColor : disabledBackColor;
            numKNeighbors.BackColor = enabled ? enabledBackColor : disabledBackColor;
            numUndersamplingRatio.BackColor = enabled ? enabledBackColor : disabledBackColor;
            numMinorityToMajorityRatio.BackColor = enabled ? enabledBackColor : disabledBackColor;

            lblExecutionOrder.ForeColor = enabled ? enabledForeColor : disabledForeColor;
            lblKNeighbors.ForeColor = enabled ? enabledForeColor : disabledForeColor;
            lblUndersamplingRatio.ForeColor = enabled ? enabledForeColor : disabledForeColor;
            lblMinorityToMajorityRatio.ForeColor = enabled ? enabledForeColor : disabledForeColor;
        }

        private void UpdateDescription(bool isSmoteSelected)
        {
            if (isSmoteSelected)
            {
                lblDescription.Text = "SMOTE (Synthetic Minority Oversampling Technique) generates synthetic samples for the minority class to balance the dataset.";
            }
            else
            {
                lblDescription.Text = "Data balancing is disabled.";
            }
        }

        public void SetConfiguration(DataBalancingConfig config)
        {
            if (config == null)
            {
                cboMethod.SelectedItem = DataBalanceMethod.None;
                numExecutionOrder.Value = 1;
                numKNeighbors.Value = 5;
                numUndersamplingRatio.Value = 0.9m;
                numMinorityToMajorityRatio.Value = 0.1m;
                return;
            }

            cboMethod.SelectedItem = config.Method;
            numExecutionOrder.Value = Math.Max(1, Math.Min(2, config.ExecutionOrder));
            numKNeighbors.Value = Math.Max(1, Math.Min(20, config.KNeighbors));
            numUndersamplingRatio.Value = (decimal)Math.Max(0.1, Math.Min(1.0, config.UndersamplingRatio));
            numMinorityToMajorityRatio.Value = (decimal)Math.Max(0.01, Math.Min(1.0, config.MinorityToMajorityRatio));

            CboMethod_SelectedIndexChanged(cboMethod, EventArgs.Empty);
        }

        public DataBalancingConfig GetConfiguration()
        {
            return new DataBalancingConfig
            {
                Method = (DataBalanceMethod)cboMethod.SelectedItem,
                ExecutionOrder = (int)numExecutionOrder.Value,
                KNeighbors = (int)numKNeighbors.Value,
                UndersamplingRatio = (float)numUndersamplingRatio.Value,
                MinorityToMajorityRatio = (float)numMinorityToMajorityRatio.Value
            };
        }

        private void InitializeComponent()
        {
            grpDataBalancing = new GroupBox();
            lblMethod = new Label();
            cboMethod = new ComboBox();
            lblExecutionOrder = new Label();
            numExecutionOrder = new NumericUpDown();
            lblKNeighbors = new Label();
            numKNeighbors = new NumericUpDown();
            lblUndersamplingRatio = new Label();
            numUndersamplingRatio = new NumericUpDown();
            lblMinorityToMajorityRatio = new Label();
            numMinorityToMajorityRatio = new NumericUpDown();
            lblDescription = new Label();
            grpDataBalancing.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numExecutionOrder).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numKNeighbors).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numUndersamplingRatio).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numMinorityToMajorityRatio).BeginInit();
            SuspendLayout();
            // 
            // grpDataBalancing
            // 
            grpDataBalancing.Controls.Add(lblDescription);
            grpDataBalancing.Controls.Add(numMinorityToMajorityRatio);
            grpDataBalancing.Controls.Add(lblMinorityToMajorityRatio);
            grpDataBalancing.Controls.Add(numUndersamplingRatio);
            grpDataBalancing.Controls.Add(lblUndersamplingRatio);
            grpDataBalancing.Controls.Add(numKNeighbors);
            grpDataBalancing.Controls.Add(lblKNeighbors);
            grpDataBalancing.Controls.Add(numExecutionOrder);
            grpDataBalancing.Controls.Add(lblExecutionOrder);
            grpDataBalancing.Controls.Add(cboMethod);
            grpDataBalancing.Controls.Add(lblMethod);
            grpDataBalancing.Dock = DockStyle.Fill;
            grpDataBalancing.Location = new Point(0, 0);
            grpDataBalancing.Name = "grpDataBalancing";
            grpDataBalancing.Size = new Size(492, 283);
            grpDataBalancing.TabIndex = 0;
            grpDataBalancing.TabStop = false;
            grpDataBalancing.Text = "Data Balancing Settings";
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
            numExecutionOrder.Value = new decimal(new int[] { 1, 0, 0, 0 });
            numExecutionOrder.Enabled = false; 
            // 
            // lblKNeighbors
            // 
            lblKNeighbors.AutoSize = true;
            lblKNeighbors.Location = new Point(24, 105);
            lblKNeighbors.Name = "lblKNeighbors";
            lblKNeighbors.Size = new Size(73, 15);
            lblKNeighbors.TabIndex = 4;
            lblKNeighbors.Text = "K Neighbors:";
            lblKNeighbors.Enabled = false; 
            // 
            // numKNeighbors
            // 
            numKNeighbors.Location = new Point(190, 103);
            numKNeighbors.Maximum = new decimal(new int[] { 20, 0, 0, 0 });
            numKNeighbors.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numKNeighbors.Name = "numKNeighbors";
            numKNeighbors.Size = new Size(80, 23);
            numKNeighbors.TabIndex = 5;
            numKNeighbors.Value = new decimal(new int[] { 5, 0, 0, 0 });
            numKNeighbors.Enabled = false;
            // 
            // lblUndersamplingRatio
            // 
            lblUndersamplingRatio.AutoSize = true;
            lblUndersamplingRatio.Location = new Point(24, 140);
            lblUndersamplingRatio.Name = "lblUndersamplingRatio";
            lblUndersamplingRatio.Size = new Size(116, 15);
            lblUndersamplingRatio.TabIndex = 6;
            lblUndersamplingRatio.Text = "Undersampling Ratio:";
            lblUndersamplingRatio.Enabled = false;
            // 
            // numUndersamplingRatio
            // 
            numUndersamplingRatio.DecimalPlaces = 2;
            numUndersamplingRatio.Increment = new decimal(new int[] { 1, 0, 0, 131072 });
            numUndersamplingRatio.Location = new Point(190, 138);
            numUndersamplingRatio.Maximum = new decimal(new int[] { 1, 0, 0, 0 });
            numUndersamplingRatio.Minimum = new decimal(new int[] { 1, 0, 0, 131072 });
            numUndersamplingRatio.Name = "numUndersamplingRatio";
            numUndersamplingRatio.Size = new Size(80, 23);
            numUndersamplingRatio.TabIndex = 7;
            numUndersamplingRatio.Value = new decimal(new int[] { 9, 0, 0, 65536 });
            numUndersamplingRatio.Enabled = false;
            // 
            // lblMinorityToMajorityRatio
            // 
            lblMinorityToMajorityRatio.AutoSize = true;
            lblMinorityToMajorityRatio.Location = new Point(24, 175);
            lblMinorityToMajorityRatio.Name = "lblMinorityToMajorityRatio";
            lblMinorityToMajorityRatio.Size = new Size(160, 15);
            lblMinorityToMajorityRatio.TabIndex = 8;
            lblMinorityToMajorityRatio.Text = "Minority to Majority Ratio:";
            lblMinorityToMajorityRatio.Enabled = false;
            // 
            // numMinorityToMajorityRatio
            // 
            numMinorityToMajorityRatio.DecimalPlaces = 2;
            numMinorityToMajorityRatio.Increment = new decimal(new int[] { 1, 0, 0, 131072 });
            numMinorityToMajorityRatio.Location = new Point(190, 173);
            numMinorityToMajorityRatio.Maximum = new decimal(new int[] { 1, 0, 0, 0 });
            numMinorityToMajorityRatio.Minimum = new decimal(new int[] { 1, 0, 0, 131072 });
            numMinorityToMajorityRatio.Name = "numMinorityToMajorityRatio";
            numMinorityToMajorityRatio.Size = new Size(80, 23);
            numMinorityToMajorityRatio.TabIndex = 9;
            numMinorityToMajorityRatio.Value = new decimal(new int[] { 1, 0, 0, 65536 });
            numMinorityToMajorityRatio.Enabled = false;
            // 
            // lblDescription
            // 
            lblDescription.ForeColor = Color.DarkBlue;
            lblDescription.Location = new Point(24, 210);
            lblDescription.Name = "lblDescription";
            lblDescription.Size = new Size(440, 65);
            lblDescription.TabIndex = 10;
            lblDescription.Text = "Data balancing is disabled.";
            // 
            // DataBalancingControl
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(grpDataBalancing);
            Name = "DataBalancingControl";
            Size = new Size(492, 283);
            grpDataBalancing.ResumeLayout(false);
            grpDataBalancing.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numExecutionOrder).EndInit();
            ((System.ComponentModel.ISupportInitialize)numKNeighbors).EndInit();
            ((System.ComponentModel.ISupportInitialize)numUndersamplingRatio).EndInit();
            ((System.ComponentModel.ISupportInitialize)numMinorityToMajorityRatio).EndInit();
            ResumeLayout(false);
        }

        private GroupBox grpDataBalancing;
        private Label lblMethod;
        private ComboBox cboMethod;
        private Label lblExecutionOrder;
        private NumericUpDown numExecutionOrder;
        private Label lblKNeighbors;
        private NumericUpDown numKNeighbors;
        private Label lblUndersamplingRatio;
        private NumericUpDown numUndersamplingRatio;
        private Label lblMinorityToMajorityRatio;
        private NumericUpDown numMinorityToMajorityRatio;
        private Label lblDescription;
    }
}