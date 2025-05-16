using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using D2G.Iris.ML.ConfigUI.Controls;
using D2G.Iris.ML.ConfigUI.Models;
using D2G.Iris.ML.ConfigUI.Services;
using D2G.Iris.ML.Core.Enums;

namespace D2G.Iris.ML.ConfigUI
{
    public partial class MainForm : Form
    {
        private readonly ConfigurationService _configService;
        private ModelConfigUI _currentConfig;
        private string _currentFilePath;

        // UI Controls
        private GeneralSettingsControl _generalSettingsControl;
        private DatabaseSettingsControl _databaseSettingsControl;
        private InputFieldsControl _inputFieldsControl;
        private TrainingParametersControl _trainingParametersControl;

        public MainForm()
        {
            InitializeComponent();
            _configService = new ConfigurationService();
            InitializeConfigurationControls();
            SetupTabLayout();
            InitializeMenuItems();
            CreateNewConfiguration();
        }

        private void InitializeConfigurationControls()
        {
            _generalSettingsControl = new GeneralSettingsControl();
            _databaseSettingsControl = new DatabaseSettingsControl();
            _inputFieldsControl = new InputFieldsControl();
            _trainingParametersControl = new TrainingParametersControl();
        }

        private void SetupTabLayout()
        {
            // Setup tabs and add controls
            tabGeneral.Controls.Add(_generalSettingsControl);
            _generalSettingsControl.Dock = DockStyle.Fill;

            tabDatabase.Controls.Add(_databaseSettingsControl);
            _databaseSettingsControl.Dock = DockStyle.Fill;

            tabInputFields.Controls.Add(_inputFieldsControl);
            _inputFieldsControl.Dock = DockStyle.Fill;

            tabTraining.Controls.Add(_trainingParametersControl);
            _trainingParametersControl.Dock = DockStyle.Fill;
        }

        private void InitializeMenuItems()
        {
            // File menu
            newToolStripMenuItem.Click += (s, e) => CreateNewConfiguration();
            openToolStripMenuItem.Click += (s, e) => OpenConfiguration();
            saveToolStripMenuItem.Click += (s, e) => SaveConfiguration();
            saveAsToolStripMenuItem.Click += (s, e) => SaveConfigurationAs();
            exitToolStripMenuItem.Click += (s, e) => Close();
        }

        private void CreateNewConfiguration()
        {
            _currentConfig = new ModelConfigUI
            {
                Author = Environment.UserName,
                Description = "New Model Configuration",
                ModelType = ModelType.BinaryClassification,
                TargetField = "Label",
                Database = new DatabaseConfigUI
                {
                    Server = "localhost",
                    Database = "IrisData",
                    TableName = "DataTable",
                    OutputTableName = "",
                    WhereClause = ""
                },
                TrainingParameters = new TrainingParametersUI
                {
                    Algorithm = "fasttree",
                    TestFraction = 0.2,
                    AlgorithmParameters = new Dictionary<string, object>
                    {
                        { "NumberOfLeaves", 20 }
                    }
                },
                InputFields = new List<InputFieldUI>()
            };

            _currentFilePath = null;
            UpdateFormTitle();
            UpdateUIFromConfig();
        }

        private void OpenConfiguration()
        {
            using (var openFileDialog = new OpenFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                Title = "Open Model Configuration"
            })
            {
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        _currentConfig = _configService.LoadConfiguration(openFileDialog.FileName);
                        _currentFilePath = openFileDialog.FileName;
                        UpdateFormTitle();
                        UpdateUIFromConfig();
                        MessageBox.Show("Configuration loaded successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error loading configuration: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void SaveConfiguration()
        {
            if (string.IsNullOrEmpty(_currentFilePath))
            {
                SaveConfigurationAs();
                return;
            }

            UpdateConfigFromUI();
            try
            {
                _configService.SaveConfiguration(_currentConfig, _currentFilePath);
                MessageBox.Show("Configuration saved successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving configuration: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveConfigurationAs()
        {
            using (var saveFileDialog = new SaveFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                Title = "Save Model Configuration",
                DefaultExt = "json",
                FileName = "modelconfig.json"
            })
            {
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    UpdateConfigFromUI();
                    try
                    {
                        _configService.SaveConfiguration(_currentConfig, saveFileDialog.FileName);
                        _currentFilePath = saveFileDialog.FileName;
                        UpdateFormTitle();
                        MessageBox.Show("Configuration saved successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error saving configuration: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void UpdateFormTitle()
        {
            string fileName = Path.GetFileName(_currentFilePath) ?? "Untitled";
            this.Text = $"Iris ML Config - {fileName}";
        }

        private void UpdateUIFromConfig()
        {
            if (_currentConfig == null) return;

            // Update General Settings
            _generalSettingsControl.SetConfiguration(
                _currentConfig.Author,
                _currentConfig.Description,
                _currentConfig.ModelType,
                _currentConfig.TargetField);

            // Update Database Settings
            _databaseSettingsControl.SetConfiguration(_currentConfig.Database);

            // Update Input Fields
            _inputFieldsControl.SetConfiguration(_currentConfig.InputFields);

            // Update Training Parameters
            _trainingParametersControl.SetConfiguration(_currentConfig.TrainingParameters);
        }

        private void UpdateConfigFromUI()
        {
            // Update from General Settings
            var generalSettings = _generalSettingsControl.GetConfiguration();
            _currentConfig.Author = generalSettings.Author;
            _currentConfig.Description = generalSettings.Description;
            _currentConfig.ModelType = generalSettings.ModelType;
            _currentConfig.TargetField = generalSettings.TargetField;

            // Update from Database Settings
            _currentConfig.Database = _databaseSettingsControl.GetConfiguration();

            // Update from Input Fields
            _currentConfig.InputFields = _inputFieldsControl.GetConfiguration();

            // Update from Training Parameters
            _currentConfig.TrainingParameters = _trainingParametersControl.GetConfiguration();
        }
    }
}