using System;
using System.Windows.Forms;
using D2G.Iris.ML.Core.Models;

namespace D2G.Iris.ML.ConfigUI.Controls
{
    public partial class DatabaseSettingsControl : UserControl
    {
        public DatabaseSettingsControl()
        {
            InitializeComponent();
        }

        public void SetConfiguration(DatabaseConfig config)
        {
            if (config == null) return;

            txtServer.Text = config.Server;
            txtDatabase.Text = config.Database;
            txtTableName.Text = config.TableName;
            txtOutputTableName.Text = config.OutputTableName;
            txtWhereClause.Text = config.WhereClause;
        }

        public DatabaseConfig GetConfiguration()
        {
            return new DatabaseConfig
            {
                Server = txtServer.Text,
                Database = txtDatabase.Text,
                TableName = txtTableName.Text,
                OutputTableName = txtOutputTableName.Text,
                WhereClause = txtWhereClause.Text
            };
        }
    


            private void InitializeComponent()
        {
            this.grpDatabase = new System.Windows.Forms.GroupBox();
            this.lblServer = new System.Windows.Forms.Label();
            this.txtServer = new System.Windows.Forms.TextBox();
            this.lblDatabase = new System.Windows.Forms.Label();
            this.txtDatabase = new System.Windows.Forms.TextBox();
            this.lblTableName = new System.Windows.Forms.Label();
            this.txtTableName = new System.Windows.Forms.TextBox();
            this.lblOutputTableName = new System.Windows.Forms.Label();
            this.txtOutputTableName = new System.Windows.Forms.TextBox();
            this.lblWhereClause = new System.Windows.Forms.Label();
            this.txtWhereClause = new System.Windows.Forms.TextBox();
            this.grpDatabase.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpDatabase
            // 
            this.grpDatabase.Controls.Add(this.txtWhereClause);
            this.grpDatabase.Controls.Add(this.lblWhereClause);
            this.grpDatabase.Controls.Add(this.txtOutputTableName);
            this.grpDatabase.Controls.Add(this.lblOutputTableName);
            this.grpDatabase.Controls.Add(this.txtTableName);
            this.grpDatabase.Controls.Add(this.lblTableName);
            this.grpDatabase.Controls.Add(this.txtDatabase);
            this.grpDatabase.Controls.Add(this.lblDatabase);
            this.grpDatabase.Controls.Add(this.txtServer);
            this.grpDatabase.Controls.Add(this.lblServer);
            this.grpDatabase.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpDatabase.Location = new System.Drawing.Point(0, 0);
            this.grpDatabase.Name = "grpDatabase";
            this.grpDatabase.Size = new System.Drawing.Size(492, 283);
            this.grpDatabase.TabIndex = 0;
            this.grpDatabase.TabStop = false;
            this.grpDatabase.Text = "Database Settings";
            // 
            // lblServer
            // 
            this.lblServer.AutoSize = true;
            this.lblServer.Location = new System.Drawing.Point(27, 38);
            this.lblServer.Name = "lblServer";
            this.lblServer.Size = new System.Drawing.Size(42, 15);
            this.lblServer.TabIndex = 0;
            this.lblServer.Text = "Server:";
            // 
            // txtServer
            // 
            this.txtServer.Location = new System.Drawing.Point(180, 35);
            this.txtServer.Name = "txtServer";
            this.txtServer.Size = new System.Drawing.Size(269, 23);
            this.txtServer.TabIndex = 1;
            // 
            // lblDatabase
            // 
            this.lblDatabase.AutoSize = true;
            this.lblDatabase.Location = new System.Drawing.Point(27, 67);
            this.lblDatabase.Name = "lblDatabase";
            this.lblDatabase.Size = new System.Drawing.Size(58, 15);
            this.lblDatabase.TabIndex = 2;
            this.lblDatabase.Text = "Database:";
            // 
            // txtDatabase
            // 
            this.txtDatabase.Location = new System.Drawing.Point(180, 64);
            this.txtDatabase.Name = "txtDatabase";
            this.txtDatabase.Size = new System.Drawing.Size(269, 23);
            this.txtDatabase.TabIndex = 3;
            // 
            // lblTableName
            // 
            this.lblTableName.AutoSize = true;
            this.lblTableName.Location = new System.Drawing.Point(27, 96);
            this.lblTableName.Name = "lblTableName";
            this.lblTableName.Size = new System.Drawing.Size(73, 15);
            this.lblTableName.TabIndex = 4;
            this.lblTableName.Text = "Table Name:";
            // 
            // txtTableName
            // 
            this.txtTableName.Location = new System.Drawing.Point(180, 93);
            this.txtTableName.Name = "txtTableName";
            this.txtTableName.Size = new System.Drawing.Size(269, 23);
            this.txtTableName.TabIndex = 5;
            // 
            // lblOutputTableName
            // 
            this.lblOutputTableName.AutoSize = true;
            this.lblOutputTableName.Location = new System.Drawing.Point(27, 125);
            this.lblOutputTableName.Name = "lblOutputTableName";
            this.lblOutputTableName.Size = new System.Drawing.Size(112, 15);
            this.lblOutputTableName.TabIndex = 6;
            this.lblOutputTableName.Text = "Output Table Name:";
            // 
            // txtOutputTableName
            // 
            this.txtOutputTableName.Location = new System.Drawing.Point(180, 122);
            this.txtOutputTableName.Name = "txtOutputTableName";
            this.txtOutputTableName.Size = new System.Drawing.Size(269, 23);
            this.txtOutputTableName.TabIndex = 7;
            // 
            // lblWhereClause
            // 
            this.lblWhereClause.AutoSize = true;
            this.lblWhereClause.Location = new System.Drawing.Point(27, 154);
            this.lblWhereClause.Name = "lblWhereClause";
            this.lblWhereClause.Size = new System.Drawing.Size(84, 15);
            this.lblWhereClause.TabIndex = 8;
            this.lblWhereClause.Text = "Where Clause:";
            // 
            // txtWhereClause
            // 
            this.txtWhereClause.Location = new System.Drawing.Point(180, 151);
            this.txtWhereClause.Multiline = true;
            this.txtWhereClause.Name = "txtWhereClause";
            this.txtWhereClause.Size = new System.Drawing.Size(269, 70);
            this.txtWhereClause.TabIndex = 9;
            // 
            // DatabaseSettingsControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.grpDatabase);
            this.Name = "DatabaseSettingsControl";
            this.Size = new System.Drawing.Size(492, 283);
            this.grpDatabase.ResumeLayout(false);
            this.grpDatabase.PerformLayout();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.GroupBox grpDatabase;
        private System.Windows.Forms.Label lblServer;
        private System.Windows.Forms.TextBox txtServer;
        private System.Windows.Forms.Label lblDatabase;
        private System.Windows.Forms.TextBox txtDatabase;
        private System.Windows.Forms.Label lblTableName;
        private System.Windows.Forms.TextBox txtTableName;
        private System.Windows.Forms.Label lblOutputTableName;
        private System.Windows.Forms.TextBox txtOutputTableName;
        private System.Windows.Forms.Label lblWhereClause;
        private System.Windows.Forms.TextBox txtWhereClause;
    }
}
