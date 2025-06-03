using System;
using System.Windows.Forms;
using D2G.Iris.ML.Core.Models;
using D2G.Iris.ML.Core.Enums;

namespace D2G.Iris.ML.ConfigUI.Controls
{
    public partial class AutoMLSettingsControl : UserControl
    {
        public AutoMLSettingsControl()
        {
            InitializeComponent();
            InitializeOptimizingMetricComboBox();
            SetupEventHandlers();
            ChkEnabled_CheckedChanged(chkEnabled, EventArgs.Empty);
        }

        private void InitializeOptimizingMetricComboBox()
        {
            cboOptimizingMetric.Items.Clear();
            cboOptimizingMetric.Items.Add("Accuracy");
            cboOptimizingMetric.Items.Add("AUC");
            cboOptimizingMetric.Items.Add("F1Score");
            cboOptimizingMetric.Items.Add("MicroAccuracy");
            cboOptimizingMetric.Items.Add("MacroAccuracy");
            cboOptimizingMetric.Items.Add("RSquared");
            cboOptimizingMetric.Items.Add("MeanAbsoluteError");
            cboOptimizingMetric.Items.Add("RootMeanSquaredError");

            cboOptimizingMetric.SelectedIndex = 0;
        }

        private void SetupEventHandlers()
        {
            chkEnabled.CheckedChanged += ChkEnabled_CheckedChanged;
        }

        private void ChkEnabled_CheckedChanged(object sender, EventArgs e)
        {
            bool isEnabled = chkEnabled.Checked;

            numMaxExperimentTime.Enabled = isEnabled;
            cboOptimizingMetric.Enabled = isEnabled;

            lblMaxExperimentTime.Enabled = isEnabled;
            lblOptimizingMetric.Enabled = isEnabled;
            lblTimeUnit.Enabled = isEnabled;
            lblDescription.Enabled = isEnabled;

            UpdateControlAppearance(isEnabled);

            UpdateDescription(isEnabled);
        }

        private void UpdateControlAppearance(bool enabled)
        {
            var disabledBackColor = System.Drawing.SystemColors.Control;
            var enabledBackColor = System.Drawing.SystemColors.Window;
            var disabledForeColor = System.Drawing.SystemColors.GrayText;
            var enabledForeColor = System.Drawing.SystemColors.ControlText;

            numMaxExperimentTime.BackColor = enabled ? enabledBackColor : disabledBackColor;
            numMaxExperimentTime.ForeColor = enabled ? enabledForeColor : disabledForeColor;

            cboOptimizingMetric.BackColor = enabled ? enabledBackColor : disabledBackColor;
            cboOptimizingMetric.ForeColor = enabled ? enabledForeColor : disabledForeColor;

            lblMaxExperimentTime.ForeColor = enabled ? enabledForeColor : disabledForeColor;
            lblOptimizingMetric.ForeColor = enabled ? enabledForeColor : disabledForeColor;
            lblTimeUnit.ForeColor = enabled ? enabledForeColor : disabledForeColor;
        }

        private void UpdateDescription(bool isEnabled)
        {
            if (isEnabled)
            {
                lblDescription.Text = "AutoML will automatically try multiple algorithms and find the best performing model for your data.";
            }
            else
            {
                lblDescription.Text = "AutoML is disabled. Traditional training will be used with the algorithm specified in Training Parameters.";
            }
        }



        public void SetConfiguration(AutoMLConfig config)
        {
            if (config == null)
            {
                chkEnabled.Checked = false;
                numMaxExperimentTime.Value = 30;
                cboOptimizingMetric.SelectedIndex = 0;
                return;
            }
           
            chkEnabled.Checked = config.Enabled;
            numMaxExperimentTime.Value = Math.Max(1, Math.Min(3600, config.MaxExperimentTimeInSeconds));

            string metric = config.OptimizingMetric ?? "Accuracy";
            int index = cboOptimizingMetric.FindStringExact(metric);
            cboOptimizingMetric.SelectedIndex = index >= 0 ? index : 0;

            ChkEnabled_CheckedChanged(chkEnabled, EventArgs.Empty);
        }

        public AutoMLConfig GetConfiguration()
        {
            return new AutoMLConfig
            {
                Enabled = chkEnabled.Checked,
                MaxExperimentTimeInSeconds = (int)numMaxExperimentTime.Value,
                OptimizingMetric = cboOptimizingMetric.Text
            };
        }

        private void InitializeComponent()
        {
            grpAutoML = new GroupBox();
            cboOptimizingMetric = new ComboBox();
            lblOptimizingMetric = new Label();
            lblTimeUnit = new Label();
            numMaxExperimentTime = new NumericUpDown();
            lblMaxExperimentTime = new Label();
            chkEnabled = new CheckBox();
            lblDescription = new Label();
            grpAutoML.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numMaxExperimentTime).BeginInit();
            SuspendLayout();
            // 
            // grpAutoML
            // 
            grpAutoML.Controls.Add(cboOptimizingMetric);
            grpAutoML.Controls.Add(lblOptimizingMetric);
            grpAutoML.Controls.Add(lblTimeUnit);
            grpAutoML.Controls.Add(numMaxExperimentTime);
            grpAutoML.Controls.Add(lblMaxExperimentTime);
            grpAutoML.Controls.Add(chkEnabled);
            grpAutoML.Controls.Add(lblDescription);
            grpAutoML.Dock = DockStyle.Fill;
            grpAutoML.Location = new Point(0, 0);
            grpAutoML.Name = "grpAutoML";
            grpAutoML.Size = new Size(492, 283);
            grpAutoML.TabIndex = 0;
            grpAutoML.TabStop = false;
            grpAutoML.Text = "AutoML Settings";
            // 
            // cboOptimizingMetric
            // 
            cboOptimizingMetric.DropDownStyle = ComboBoxStyle.DropDownList;
            cboOptimizingMetric.FormattingEnabled = true;
            cboOptimizingMetric.Location = new Point(190, 120);
            cboOptimizingMetric.Name = "cboOptimizingMetric";
            cboOptimizingMetric.Size = new Size(200, 23);
            cboOptimizingMetric.TabIndex = 8;
            // 
            // lblOptimizingMetric
            // 
            lblOptimizingMetric.AutoSize = true;
            lblOptimizingMetric.Location = new Point(24, 128);
            lblOptimizingMetric.Name = "lblOptimizingMetric";
            lblOptimizingMetric.Size = new Size(106, 15);
            lblOptimizingMetric.TabIndex = 7;
            lblOptimizingMetric.Text = "Optimizing Metric:";
            // 
            // lblTimeUnit
            // 
            lblTimeUnit.AutoSize = true;
            lblTimeUnit.Location = new Point(276, 71);
            lblTimeUnit.Name = "lblTimeUnit";
            lblTimeUnit.Size = new Size(50, 15);
            lblTimeUnit.TabIndex = 4;
            lblTimeUnit.Text = "seconds";
            // 
            // numMaxExperimentTime
            // 
            numMaxExperimentTime.Location = new Point(190, 63);
            numMaxExperimentTime.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            numMaxExperimentTime.Minimum = new decimal(new int[] { 10, 0, 0, 0 });
            numMaxExperimentTime.Name = "numMaxExperimentTime";
            numMaxExperimentTime.Size = new Size(80, 23);
            numMaxExperimentTime.TabIndex = 3;
            numMaxExperimentTime.Value = new decimal(new int[] { 30, 0, 0, 0 });
            // 
            // lblMaxExperimentTime
            // 
            lblMaxExperimentTime.AutoSize = true;
            lblMaxExperimentTime.Location = new Point(24, 63);
            lblMaxExperimentTime.Name = "lblMaxExperimentTime";
            lblMaxExperimentTime.Size = new Size(125, 15);
            lblMaxExperimentTime.TabIndex = 2;
            lblMaxExperimentTime.Text = "Max Experiment Time:";
            // 
            // chkEnabled
            // 
            chkEnabled.AutoSize = true;
            chkEnabled.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            chkEnabled.Location = new Point(24, 25);
            chkEnabled.Name = "chkEnabled";
            chkEnabled.Size = new Size(109, 19);
            chkEnabled.TabIndex = 1;
            chkEnabled.Text = "Enable AutoML";
            chkEnabled.UseVisualStyleBackColor = true;
            // 
            // lblDescription
            // 
            lblDescription.ForeColor = Color.DarkBlue;
            lblDescription.Location = new Point(24, 160);
            lblDescription.Name = "lblDescription";
            lblDescription.Size = new Size(440, 65);
            lblDescription.TabIndex = 0;
            lblDescription.Text = "AutoML is disabled. Traditional training will be used with the algorithm specified in Training Parameters.";
            // 
            // AutoMLSettingsControl
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(grpAutoML);
            Name = "AutoMLSettingsControl";
            Size = new Size(492, 283);
            grpAutoML.ResumeLayout(false);
            grpAutoML.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numMaxExperimentTime).EndInit();
            ResumeLayout(false);
        }

        private System.Windows.Forms.GroupBox grpAutoML;
        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.CheckBox chkEnabled;
        private System.Windows.Forms.Label lblMaxExperimentTime;
        private System.Windows.Forms.NumericUpDown numMaxExperimentTime;
        private System.Windows.Forms.Label lblTimeUnit;
        private System.Windows.Forms.Label lblOptimizingMetric;
        private System.Windows.Forms.ComboBox cboOptimizingMetric;
    }
}