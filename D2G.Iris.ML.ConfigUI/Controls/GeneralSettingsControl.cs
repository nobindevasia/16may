using System;
using System.Windows.Forms;
using D2G.Iris.ML.Core.Enums;
using D2G.Iris.ML.Core.Models;

namespace D2G.Iris.ML.ConfigUI.Controls
{
    public partial class GeneralSettingsControl : UserControl
    {
        // Add event for model type changes
        public event Action<ModelType> ModelTypeChanged;

        public GeneralSettingsControl()
        {
            InitializeComponent();
            InitializeModelTypeComboBox();
            SetupEventHandlers(); // Add this line
        }

        private void SetupEventHandlers()
        {
            // Subscribe to model type selection changes
            cboModelType.SelectedIndexChanged += CboModelType_SelectedIndexChanged;
        }

        private void CboModelType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboModelType.SelectedItem is ModelType selectedModelType)
            {
                // Raise the event to notify other controls
                ModelTypeChanged?.Invoke(selectedModelType);
            }
        }

        private void InitializeModelTypeComboBox()
        {
            cboModelType.Items.Clear();
            cboModelType.Items.Add(ModelType.BinaryClassification);
            cboModelType.Items.Add(ModelType.MultiClassClassification);
            cboModelType.Items.Add(ModelType.Regression);
            cboModelType.SelectedIndex = 0;
        }

        public void SetConfiguration(string author, string description, ModelType modelType, string targetField)
        {
            txtAuthor.Text = author;
            txtDescription.Text = description;

            // Temporarily remove event handler to prevent unwanted events during setup
            cboModelType.SelectedIndexChanged -= CboModelType_SelectedIndexChanged;
            cboModelType.SelectedItem = modelType;
            cboModelType.SelectedIndexChanged += CboModelType_SelectedIndexChanged;

            txtTargetField.Text = targetField;

            // Raise the event to ensure other controls are synchronized
            ModelTypeChanged?.Invoke(modelType);
        }

        public (string Author, string Description, ModelType ModelType, string TargetField) GetValues()
        {
            return (
                txtAuthor.Text,
                txtDescription.Text,
                (ModelType)cboModelType.SelectedItem,
                txtTargetField.Text
            );
        }

        private void InitializeComponent()
        {
            this.grpGeneral = new System.Windows.Forms.GroupBox();
            this.txtTargetField = new System.Windows.Forms.TextBox();
            this.lblTargetField = new System.Windows.Forms.Label();
            this.cboModelType = new System.Windows.Forms.ComboBox();
            this.lblModelType = new System.Windows.Forms.Label();
            this.txtDescription = new System.Windows.Forms.TextBox();
            this.lblDescription = new System.Windows.Forms.Label();
            this.txtAuthor = new System.Windows.Forms.TextBox();
            this.lblAuthor = new System.Windows.Forms.Label();
            this.grpGeneral.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpGeneral
            // 
            this.grpGeneral.Controls.Add(this.txtTargetField);
            this.grpGeneral.Controls.Add(this.lblTargetField);
            this.grpGeneral.Controls.Add(this.cboModelType);
            this.grpGeneral.Controls.Add(this.lblModelType);
            this.grpGeneral.Controls.Add(this.txtDescription);
            this.grpGeneral.Controls.Add(this.lblDescription);
            this.grpGeneral.Controls.Add(this.txtAuthor);
            this.grpGeneral.Controls.Add(this.lblAuthor);
            this.grpGeneral.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpGeneral.Location = new System.Drawing.Point(0, 0);
            this.grpGeneral.Name = "grpGeneral";
            this.grpGeneral.Size = new System.Drawing.Size(492, 270);
            this.grpGeneral.TabIndex = 0;
            this.grpGeneral.TabStop = false;
            this.grpGeneral.Text = "General Settings";
            // 
            // txtTargetField
            // 
            this.txtTargetField.Location = new System.Drawing.Point(180, 190);
            this.txtTargetField.Name = "txtTargetField";
            this.txtTargetField.Size = new System.Drawing.Size(269, 23);
            this.txtTargetField.TabIndex = 7;
            // 
            // lblTargetField
            // 
            this.lblTargetField.AutoSize = true;
            this.lblTargetField.Location = new System.Drawing.Point(27, 193);
            this.lblTargetField.Name = "lblTargetField";
            this.lblTargetField.Size = new System.Drawing.Size(71, 15);
            this.lblTargetField.TabIndex = 6;
            this.lblTargetField.Text = "Target Field:";
            // 
            // cboModelType
            // 
            this.cboModelType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboModelType.FormattingEnabled = true;
            this.cboModelType.Location = new System.Drawing.Point(180, 146);
            this.cboModelType.Name = "cboModelType";
            this.cboModelType.Size = new System.Drawing.Size(269, 23);
            this.cboModelType.TabIndex = 5;
            // 
            // lblModelType
            // 
            this.lblModelType.AutoSize = true;
            this.lblModelType.Location = new System.Drawing.Point(27, 149);
            this.lblModelType.Name = "lblModelType";
            this.lblModelType.Size = new System.Drawing.Size(74, 15);
            this.lblModelType.TabIndex = 4;
            this.lblModelType.Text = "Model Type:";
            // 
            // txtDescription
            // 
            this.txtDescription.Location = new System.Drawing.Point(180, 74);
            this.txtDescription.Multiline = true;
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.Size = new System.Drawing.Size(269, 56);
            this.txtDescription.TabIndex = 3;
            // 
            // lblDescription
            // 
            this.lblDescription.AutoSize = true;
            this.lblDescription.Location = new System.Drawing.Point(27, 77);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(70, 15);
            this.lblDescription.TabIndex = 2;
            this.lblDescription.Text = "Description:";
            // 
            // txtAuthor
            // 
            this.txtAuthor.Location = new System.Drawing.Point(180, 35);
            this.txtAuthor.Name = "txtAuthor";
            this.txtAuthor.Size = new System.Drawing.Size(269, 23);
            this.txtAuthor.TabIndex = 1;
            // 
            // lblAuthor
            // 
            this.lblAuthor.AutoSize = true;
            this.lblAuthor.Location = new System.Drawing.Point(27, 38);
            this.lblAuthor.Name = "lblAuthor";
            this.lblAuthor.Size = new System.Drawing.Size(47, 15);
            this.lblAuthor.TabIndex = 0;
            this.lblAuthor.Text = "Author:";
            // 
            // GeneralSettingsControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.grpGeneral);
            this.Name = "GeneralSettingsControl";
            this.Size = new System.Drawing.Size(492, 270);
            this.grpGeneral.ResumeLayout(false);
            this.grpGeneral.PerformLayout();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.GroupBox grpGeneral;
        private System.Windows.Forms.TextBox txtAuthor;
        private System.Windows.Forms.Label lblAuthor;
        private System.Windows.Forms.TextBox txtDescription;
        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.ComboBox cboModelType;
        private System.Windows.Forms.Label lblModelType;
        private System.Windows.Forms.TextBox txtTargetField;
        private System.Windows.Forms.Label lblTargetField;
    }
}