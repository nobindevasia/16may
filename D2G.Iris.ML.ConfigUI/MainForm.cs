using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Forms;
using D2G.Iris.ML.ConfigUI.Controls;
using D2G.Iris.ML.ConfigUI.Services;
using D2G.Iris.ML.ConfigUI.Utilities;
using D2G.Iris.ML.Core.Enums;
using D2G.Iris.ML.Core.Models;
using D2G.Iris.ML.Configuration;
using D2G.Iris.ML.Data;

namespace D2G.Iris.ML.ConfigUI
{
    public partial class MainForm : Form
    {
        private readonly ConfigurationService _configService;
        private ModelConfig _currentConfig;
        private string _currentFilePath;

        private GeneralSettingsControl _generalSettingsControl;
        private DatabaseSettingsControl _databaseSettingsControl;
        private InputFieldsControl _inputFieldsControl;
        private TrainingParametersControl _trainingParametersControl;
        private DataBalancingControl _dataBalancingControl;
        private FeatureEngineeringControl _featureEngineeringControl;
        private AutoMLSettingsControl _autoMLSettingsControl;
        private Button btnLaunchTraining;
        private TabPage tabLogs;
        private RichTextBox txtConsoleOutput;
        private TextWriter _originalConsoleOut;

        public MainForm()
        {
            InitializeComponent();

            _configService = new ConfigurationService();
            InitializeConfigurationControls();
            AddTrainingButton();
            AddConsoleTab();
            SetupTabLayout();
            InitializeMenuItems();
            SetupModelTypeChangeHandling(); // Add this line

            this.FormClosing += (sender, e) =>
            {
                ConsoleUtilities.RestoreConsoleOutput(_originalConsoleOut);
            };

            LoadExistingConfigOnStartup();
        }

        private void SetupModelTypeChangeHandling()
        {
            // Subscribe to model type changes from the general settings control
            _generalSettingsControl.ModelTypeChanged += OnModelTypeChanged;
        }

        private void OnModelTypeChanged(ModelType newModelType)
        {
            // Update the training parameters control when model type changes
            _trainingParametersControl.SetModelType(newModelType);
        }

        private void LoadExistingConfigOnStartup()
        {
            try
            {
                string appConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "modelconfig.json");

                if (File.Exists(appConfigPath))
                {
                    try
                    {
                        _currentConfig = _configService.LoadConfiguration(appConfigPath);
                        _currentFilePath = appConfigPath;
                        UpdateFormTitle();
                        UpdateUIFromConfig();

                        ConsoleUtilities.LogMessage(txtConsoleOutput,
                            $"Successfully loaded configuration from: {appConfigPath}", LogLevel.Success);
                        return;
                    }
                    catch (Exception loadEx)
                    {
                        ConsoleUtilities.LogMessage(txtConsoleOutput,
                            $"Error loading configuration: {loadEx.Message}", LogLevel.Error);
                    }
                }

                CreateNewConfiguration();
            }
            catch (Exception ex)
            {
                CreateNewConfiguration();
                ConsoleUtilities.LogMessage(txtConsoleOutput,
                    $"Unexpected error: {ex.Message}", LogLevel.Error);
                if (ex.InnerException != null)
                {
                    ConsoleUtilities.LogMessage(txtConsoleOutput,
                        $"Inner exception: {ex.InnerException.Message}", LogLevel.Error);
                }
                ConsoleUtilities.LogMessage(txtConsoleOutput,
                    "Created a new configuration.", LogLevel.Warning);
            }
        }

        private void InitializeConfigurationControls()
        {
            _generalSettingsControl = new GeneralSettingsControl();
            _databaseSettingsControl = new DatabaseSettingsControl();
            _inputFieldsControl = new InputFieldsControl();
            _trainingParametersControl = new TrainingParametersControl();
            _dataBalancingControl = new DataBalancingControl();
            _featureEngineeringControl = new FeatureEngineeringControl();
            _autoMLSettingsControl = new AutoMLSettingsControl();
        }

        private void AddTrainingButton()
        {
            btnLaunchTraining = new Button
            {
                Text = "Launch Training",
                BackColor = Color.Green,
                ForeColor = Color.White,
                Font = new Font(Font.FontFamily, 10, FontStyle.Bold),
                Width = 150,
                Height = 40,
                Dock = DockStyle.Bottom,
                Margin = new Padding(10),
                Cursor = Cursors.Hand
            };

            btnLaunchTraining.Click += BtnLaunchTraining_Click;

            this.Controls.Add(btnLaunchTraining);
        }

        private void AddConsoleTab()
        {
            tabLogs = new TabPage
            {
                Text = "Training Logs",
                UseVisualStyleBackColor = true
            };

            txtConsoleOutput = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BackColor = Color.White,
                ForeColor = Color.DarkGreen,
                Font = new Font("Consolas", 10),
                Multiline = true,
                ScrollBars = RichTextBoxScrollBars.Both,
                WordWrap = false
            };

            tabLogs.Controls.Add(txtConsoleOutput);
            tabControl.TabPages.Add(tabLogs);

            _originalConsoleOut = ConsoleUtilities.RedirectConsoleOutput(txtConsoleOutput);

            ConsoleUtilities.LogMessage(txtConsoleOutput,
                "Welcome to Iris ML Configuration Tool", LogLevel.Info);
            ConsoleUtilities.LogMessage(txtConsoleOutput,
                "Use the tabs to configure your model settings and click 'Launch Training' to start training", LogLevel.Info);
        }

        private void SetupTabLayout()
        {
            tabGeneral.Controls.Add(_generalSettingsControl);
            _generalSettingsControl.Dock = DockStyle.Fill;

            tabDatabase.Controls.Add(_databaseSettingsControl);
            _databaseSettingsControl.Dock = DockStyle.Fill;

            tabInputFields.Controls.Add(_inputFieldsControl);
            _inputFieldsControl.Dock = DockStyle.Fill;

            tabTraining.Controls.Add(_trainingParametersControl);
            _trainingParametersControl.Dock = DockStyle.Fill;

            tabDataBalancing.Controls.Add(_dataBalancingControl);
            _dataBalancingControl.Dock = DockStyle.Fill;

            tabFeatureEngineering.Controls.Add(_featureEngineeringControl);
            _featureEngineeringControl.Dock = DockStyle.Fill;

            tabAutoML.Controls.Add(_autoMLSettingsControl);
            _autoMLSettingsControl.Dock = DockStyle.Fill;
        }

        private async void BtnLaunchTraining_Click(object sender, EventArgs e)
        {
            try
            {
                if (_currentConfig == null)
                {
                    MessageBox.Show("Please create a configuration first.", "No Configuration",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                UpdateConfigFromUI();

                if (!_configService.ValidateConfiguration(_currentConfig))
                {
                    MessageBox.Show("Configuration validation failed. Please check all required fields.",
                        "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string confirmationMessage = _currentConfig.AutoML?.Enabled == true
                    ? $"Are you sure you want to start AutoML training? This will run for up to {_currentConfig.AutoML.MaxExperimentTimeInSeconds} seconds."
                    : "Are you sure you want to start the training process?";

                if (string.IsNullOrEmpty(_currentFilePath))
                {
                    if (MessageBox.Show("Configuration needs to be saved before training. Save now?",
                        "Save Required", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        SaveConfiguration();
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    _configService.SaveConfiguration(_currentConfig, _currentFilePath);
                }

                if (MessageBox.Show(confirmationMessage,
                    "Confirm Training", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    return;
                }

                tabControl.SelectedTab = tabLogs;

                ConsoleUtilities.ClearConsole(txtConsoleOutput);

                EnableUI(false);
                btnLaunchTraining.BackColor = Color.DarkOrange;
                btnLaunchTraining.Text = _currentConfig.AutoML?.Enabled == true ? "Running AutoML..." : "Training...";

                Application.DoEvents();

                await ConsoleUtilities.RunWithProgressAsync(
                    txtConsoleOutput,
                    () => Task.Run(() => RunTrainingProcess()),
                    _currentConfig.AutoML?.Enabled == true ? "Initializing AutoML training process..." : "Initializing training process...",
                    "Training process completed!",
                    "Error during training:"
                );

                btnLaunchTraining.Text = "Launch Training";
                btnLaunchTraining.BackColor = Color.Green;
                EnableUI(true);

                string logPath = Path.Combine(
                    Path.GetDirectoryName(_currentFilePath) ?? Environment.CurrentDirectory,
                    $"Training_Log_{DateTime.Now:yyyyMMdd_HHmmss}.txt");

                MessageBox.Show("Training process completed! Check the Training Logs tab for details.",
                    "Training Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                ConsoleUtilities.LogMessage(txtConsoleOutput, $"ERROR: {ex.Message}", LogLevel.Error);
                if (ex.InnerException != null)
                {
                    ConsoleUtilities.LogMessage(txtConsoleOutput, $"Inner exception: {ex.InnerException.Message}", LogLevel.Error);
                }
                ConsoleUtilities.LogMessage(txtConsoleOutput, $"Stack trace: {ex.StackTrace}", LogLevel.Error);

                btnLaunchTraining.Text = "Launch Training";
                btnLaunchTraining.BackColor = Color.Green;
                EnableUI(true);
                MessageBox.Show($"Error during training: {ex.Message}", "Training Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RunTrainingProcess()
        {
            try
            {
                string tempConfigPath = Path.Combine(Path.GetTempPath(), "modelconfig.json");
                var serializableConfig = new Dictionary<string, ModelConfig>
                {
                    { "modelConfig", _currentConfig }
                };

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    Converters = { new JsonStringEnumConverter() }
                };

                string jsonConfig = JsonSerializer.Serialize(serializableConfig, options);
                File.WriteAllText(tempConfigPath, jsonConfig);

                var configManager = new ConfigManager();
                var config = configManager.LoadConfiguration(tempConfigPath);

                var sqlHandler = new SqlHandler(config.Database.TableName);
                sqlHandler.Connect(config.Database);

                var enabledFields = config.InputFields
                    .Where(f => f.IsEnabled)
                    .Select(f => f.Name)
                    .ToArray();

                var dataLoader = new DatabaseDataLoader();
                var rawData = dataLoader.LoadDataFromSql(
                    sqlHandler.GetConnectionString(),
                    config.Database.TableName,
                    enabledFields,
                    config.ModelType,
                    config.TargetField,
                    config.Database.WhereClause);

                var mlContext = new Microsoft.ML.MLContext(seed: 42);

                var dataProcessor = new DataProcessor(sqlHandler);
                var processedData = dataProcessor.ProcessData(
                    mlContext,
                    rawData,
                    enabledFields,
                    config).GetAwaiter().GetResult();

                var modelTrainerFactory = new Training.ModelTrainerFactory(mlContext);
                var modelTrainer = modelTrainerFactory.CreateTrainer(config.ModelType);

                modelTrainer.TrainModel(
                    mlContext,
                    processedData.Data,
                    processedData.FeatureNames,
                    config,
                    processedData).GetAwaiter().GetResult();

                try { File.Delete(tempConfigPath); } catch { }
            }
            catch (Exception ex)
            {
                ConsoleUtilities.LogMessage(txtConsoleOutput, $"Error in training process: {ex.Message}", LogLevel.Error);
                throw;
            }
        }

        private void InitializeMenuItems()
        {
            newToolStripMenuItem.Click += (s, e) => CreateNewConfiguration();
            openToolStripMenuItem.Click += (s, e) => OpenConfiguration();
            saveToolStripMenuItem.Click += (s, e) => SaveConfiguration();
            exitToolStripMenuItem.Click += (s, e) => Close();
        }

        private void CreateNewConfiguration()
        {
            _currentConfig = new ModelConfig
            {
                Author = Environment.UserName,
                Description = "New Model Configuration",
                ModelType = ModelType.BinaryClassification,
                TargetField = "Label",
                Database = new DatabaseConfig
                {
                    Server = "localhost",
                    Database = "IrisData",
                    TableName = "DataTable",
                    OutputTableName = "",
                    WhereClause = ""
                },
                TrainingParameters = new TrainingParameters
                {
                    Algorithm = "fasttree",
                    TestFraction = 0.2,
                    AlgorithmParameters = new Dictionary<string, object>
                    {
                        { "NumberOfLeaves", 20 }
                    }
                },
                InputFields = new List<InputField>(),
                FeatureEngineering = new FeatureEngineeringConfig
                {
                    Method = FeatureSelectionMethod.None,
                    ExecutionOrder = 2,
                    NumberOfComponents = 3,
                    MaxFeatures = 10,
                    MulticollinearityThreshold = 0.7
                },
                DataBalancing = new DataBalancingConfig
                {
                    Method = DataBalanceMethod.None,
                    ExecutionOrder = 1,
                    KNeighbors = 5,
                    UndersamplingRatio = 0.9f,
                    MinorityToMajorityRatio = 0.1f
                },
                AutoML = new AutoMLConfig
                {
                    Enabled = false,
                    MaxExperimentTimeInSeconds = 30,
                    MaxModels = 10,
                    OptimizingMetric = "Accuracy"
                }
            };

            _currentFilePath = null;
            UpdateFormTitle();
            UpdateUIFromConfig();

            ConsoleUtilities.LogMessage(txtConsoleOutput, "Created new configuration", LogLevel.Info);
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

                        ConsoleUtilities.LogMessage(txtConsoleOutput,
                            $"Loaded configuration from: {_currentFilePath}", LogLevel.Success);
                        MessageBox.Show("Configuration loaded successfully.", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        ConsoleUtilities.LogMessage(txtConsoleOutput,
                            $"Error loading configuration: {ex.Message}", LogLevel.Error);
                        MessageBox.Show($"Error loading configuration: {ex.Message}", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void SaveConfiguration()
        {
            UpdateConfigFromUI();

            if (string.IsNullOrEmpty(_currentFilePath))
            {
                using (var saveFileDialog = new SaveFileDialog
                {
                    Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                    Title = "Save Model Configuration",
                    DefaultExt = "json",
                    FileName = "modelconfig.json"
                })
                {
                    if (saveFileDialog.ShowDialog() != DialogResult.OK)
                    {
                        return;
                    }

                    _currentFilePath = saveFileDialog.FileName;
                }
            }

            try
            {
                _configService.SaveConfiguration(_currentConfig, _currentFilePath);
                UpdateFormTitle();
                ConsoleUtilities.LogMessage(txtConsoleOutput,
                    $"Configuration saved to: {_currentFilePath}", LogLevel.Success);
                MessageBox.Show("Configuration saved successfully.", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                ConsoleUtilities.LogMessage(txtConsoleOutput,
                    $"Error saving configuration: {ex.Message}", LogLevel.Error);
                MessageBox.Show($"Error saving configuration: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
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

            _generalSettingsControl.SetConfiguration(
                _currentConfig.Author,
                _currentConfig.Description,
                _currentConfig.ModelType,
                _currentConfig.TargetField);

            _databaseSettingsControl.SetConfiguration(_currentConfig.Database);

            _inputFieldsControl.SetConfiguration(_currentConfig.InputFields);

            _trainingParametersControl.SetConfiguration(_currentConfig.TrainingParameters);

            _dataBalancingControl.SetConfiguration(_currentConfig.DataBalancing);

            _featureEngineeringControl.SetConfiguration(_currentConfig.FeatureEngineering);

            _autoMLSettingsControl.SetConfiguration(_currentConfig.AutoML);
        }

        private void UpdateConfigFromUI()
        {
            var (author, description, modelType, targetField) = _generalSettingsControl.GetValues();
            _currentConfig.Author = author;
            _currentConfig.Description = description;
            _currentConfig.ModelType = modelType;
            _currentConfig.TargetField = targetField;

            _currentConfig.Database = _databaseSettingsControl.GetConfiguration();

            _currentConfig.InputFields = _inputFieldsControl.GetConfiguration();

            _currentConfig.TrainingParameters = _trainingParametersControl.GetConfiguration();

            _currentConfig.DataBalancing = _dataBalancingControl.GetConfiguration();

            _currentConfig.FeatureEngineering = _featureEngineeringControl.GetConfiguration();

            _currentConfig.AutoML = _autoMLSettingsControl.GetConfiguration();
        }

        private void EnableUI(bool enable)
        {
            tabControl.Enabled = enable;
            menuStrip.Enabled = enable;

            if (!enable)
            {
                tabLogs.Enabled = true;
                txtConsoleOutput.Enabled = true;
            }
        }
    }
}