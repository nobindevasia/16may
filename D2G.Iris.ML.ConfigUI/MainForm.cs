using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using D2G.Iris.ML.ConfigUI.Controls;
using D2G.Iris.ML.ConfigUI.Models;
using D2G.Iris.ML.ConfigUI.Services;
using D2G.Iris.ML.Core.Enums;
using D2G.Iris.ML.Configuration;
using D2G.Iris.ML.Data;
using D2G.Iris.ML.Core.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace D2G.Iris.ML.ConfigUI
{
    public partial class MainForm : Form
    {
        // Custom TextWriter to redirect Console output to our RichTextBox
        private class TextBoxWriter : TextWriter
        {
            private RichTextBox _textBox;
            private readonly SynchronizationContext _synchronizationContext;

            public TextBoxWriter(RichTextBox textBox)
            {
                _textBox = textBox;
                _synchronizationContext = SynchronizationContext.Current;
            }

            public override void Write(char value)
            {
                // Use sync context to update UI from any thread
                _synchronizationContext.Post(_ =>
                {
                    _textBox.AppendText(value.ToString());
                    _textBox.ScrollToCaret();
                }, null);
            }

            public override void Write(string value)
            {
                _synchronizationContext.Post(_ =>
                {
                    _textBox.AppendText(value);
                    _textBox.ScrollToCaret();
                }, null);
            }

            public override Encoding Encoding => Encoding.UTF8;
        }

        private readonly ConfigurationService _configService;
        private ModelConfigUI _currentConfig;
        private string _currentFilePath;

        // UI Controls
        private GeneralSettingsControl _generalSettingsControl;
        private DatabaseSettingsControl _databaseSettingsControl;
        private InputFieldsControl _inputFieldsControl;
        private TrainingParametersControl _trainingParametersControl;
        private Button btnLaunchTraining;
        private TabPage tabLogs;
        private RichTextBox txtConsoleOutput;
        private TextWriter _originalConsoleOut;

        public MainForm()
        {
            // Call the designer-generated InitializeComponent method
            InitializeComponent();

            // Add handler to restore console output when form closes
            this.FormClosing += (sender, e) =>
            {
                if (_originalConsoleOut != null)
                {
                    Console.SetOut(_originalConsoleOut);
                }
            };

            _configService = new ConfigurationService();
            InitializeConfigurationControls();
            AddTrainingButton();
            AddConsoleTab();
            SetupTabLayout();
            InitializeMenuItems();

            // Try to load the existing configuration file on startup
            LoadExistingConfigOnStartup();
        }

        private void LoadExistingConfigOnStartup()
        {
            try
            {
                // First look for modelconfig.json in the application's directory
                string appConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "modelconfig.json");

                if (File.Exists(appConfigPath))
                {
                    try
                    {
                        // Try to directly load the configuration using a custom JSON converter
                        string jsonContent = File.ReadAllText(appConfigPath);

                        // Configure JsonSerializerOptions with StringEnumConverter
                        var options = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true,
                            Converters = { new JsonStringEnumConverter() }
                        };

                        // Try to deserialize directly
                        var jsonData = JsonSerializer.Deserialize<Dictionary<string, ModelConfig>>(jsonContent, options);

                        if (jsonData != null && jsonData.ContainsKey("modelConfig"))
                        {
                            var modelConfig = jsonData["modelConfig"];

                            // Convert from Core.Models.ModelConfig to ConfigUI.Models.ModelConfigUI
                            _currentConfig = ConvertFromModelConfig(modelConfig);
                            _currentFilePath = appConfigPath;
                            UpdateFormTitle();
                            UpdateUIFromConfig();

                            AppendToConsole($"Successfully loaded configuration from: {appConfigPath}\r\n", Color.Green);
                            return;
                        }
                    }
                    catch (Exception loadEx)
                    {
                        AppendToConsole($"Error loading configuration: {loadEx.Message}\r\n", Color.Red);

                        // Try to manually fix and load the JSON file
                        try
                        {
                            string jsonContent = File.ReadAllText(appConfigPath);

                            // Make a backup of the original file
                            string backupPath = appConfigPath + ".bak";
                            File.Copy(appConfigPath, backupPath, true);
                            AppendToConsole($"Created backup of original config at: {backupPath}\r\n", Color.Yellow);

                            // Try to parse and fix the JSON
                            var tempPath = Path.Combine(Path.GetTempPath(), "modelconfig_fixed.json");

                            // Perform specific fixes based on the known error pattern
                            if (jsonContent.Contains("\"modelType\":") && !jsonContent.Contains("\"modelType\": 0") &&
                                !jsonContent.Contains("\"modelType\": 1") && !jsonContent.Contains("\"modelType\": 2"))
                            {
                                // Replace string enum values with numeric values
                                if (jsonContent.Contains("\"modelType\": \"BinaryClassification\"") ||
                                    jsonContent.Contains("\"modelType\":\"BinaryClassification\""))
                                {
                                    jsonContent = jsonContent.Replace("\"modelType\": \"BinaryClassification\"", "\"modelType\": 0")
                                                           .Replace("\"modelType\":\"BinaryClassification\"", "\"modelType\": 0");
                                }
                                else if (jsonContent.Contains("\"modelType\": \"MultiClassClassification\"") ||
                                         jsonContent.Contains("\"modelType\":\"MultiClassClassification\""))
                                {
                                    jsonContent = jsonContent.Replace("\"modelType\": \"MultiClassClassification\"", "\"modelType\": 1")
                                                           .Replace("\"modelType\":\"MultiClassClassification\"", "\"modelType\": 1");
                                }
                                else if (jsonContent.Contains("\"modelType\": \"Regression\"") ||
                                         jsonContent.Contains("\"modelType\":\"Regression\""))
                                {
                                    jsonContent = jsonContent.Replace("\"modelType\": \"Regression\"", "\"modelType\": 2")
                                                           .Replace("\"modelType\":\"Regression\"", "\"modelType\": 2");
                                }

                                AppendToConsole("Fixed ModelType issue in JSON file\r\n", Color.Yellow);
                            }

                            // Save the fixed content
                            File.WriteAllText(tempPath, jsonContent);

                            // Try to load the fixed file through the config service
                            _currentConfig = _configService.LoadConfiguration(tempPath);
                            _currentFilePath = appConfigPath; // Still point to the original file
                            UpdateFormTitle();
                            UpdateUIFromConfig();

                            AppendToConsole($"Successfully loaded fixed configuration!\r\n", Color.Green);
                            return;
                        }
                        catch (Exception fixEx)
                        {
                            AppendToConsole($"Could not fix and load configuration: {fixEx.Message}\r\n", Color.Red);
                        }
                    }
                }

                // If we get here, either file doesn't exist or all loading attempts failed
                CreateNewConfiguration();
                AppendToConsole("Created a new configuration.\r\n", Color.Yellow);
            }
            catch (Exception ex)
            {
                // If loading fails, create a new configuration
                CreateNewConfiguration();
                AppendToConsole($"Unexpected error: {ex.Message}\r\n", Color.Red);
                if (ex.InnerException != null)
                {
                    AppendToConsole($"Inner exception: {ex.InnerException.Message}\r\n", Color.Red);
                }
                AppendToConsole("Created a new configuration.\r\n", Color.Yellow);
            }
        }

        private ModelConfigUI ConvertFromModelConfig(ModelConfig modelConfig)
        {
            var configUI = new ModelConfigUI
            {
                Author = modelConfig.Author,
                Description = modelConfig.Description,
                ModelType = modelConfig.ModelType,
                TargetField = modelConfig.TargetField,
                Database = new DatabaseConfigUI
                {
                    Server = modelConfig.Database?.Server ?? "",
                    Database = modelConfig.Database?.Database ?? "",
                    TableName = modelConfig.Database?.TableName ?? "",
                    OutputTableName = modelConfig.Database?.OutputTableName ?? "",
                    WhereClause = modelConfig.Database?.WhereClause ?? ""
                },
                TrainingParameters = new TrainingParametersUI
                {
                    Algorithm = modelConfig.TrainingParameters?.Algorithm ?? "fasttree",
                    TestFraction = modelConfig.TrainingParameters?.TestFraction ?? 0.2,
                    AlgorithmParameters = modelConfig.TrainingParameters?.AlgorithmParameters ??
                        new Dictionary<string, object> { { "NumberOfLeaves", 20 } }
                },
                InputFields = modelConfig.InputFields?.Select(f => new InputFieldUI
                {
                    Name = f.Name,
                    IsEnabled = f.IsEnabled
                }).ToList() ?? new List<InputFieldUI>()
            };

            return configUI;
        }

        private void InitializeConfigurationControls()
        {
            _generalSettingsControl = new GeneralSettingsControl();
            _databaseSettingsControl = new DatabaseSettingsControl();
            _inputFieldsControl = new InputFieldsControl();
            _trainingParametersControl = new TrainingParametersControl();
        }

        private void AddTrainingButton()
        {
            // Create a new button
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

            // Add click event handler
            btnLaunchTraining.Click += BtnLaunchTraining_Click;

            // Add to form
            this.Controls.Add(btnLaunchTraining);
        }

        private void AddConsoleTab()
        {
            // Create a new tab for console output
            tabLogs = new TabPage
            {
                Text = "Training Logs",
                UseVisualStyleBackColor = true
            };

            // Create a rich text box to show console output
            txtConsoleOutput = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BackColor = Color.Black,
                ForeColor = Color.LightGreen,
                Font = new Font("Consolas", 10),
                Multiline = true,
                ScrollBars = RichTextBoxScrollBars.Both,
                WordWrap = false
            };

            // Add to tab and tab control
            tabLogs.Controls.Add(txtConsoleOutput);
            tabControl.TabPages.Add(tabLogs);

            // Redirect console output to our text box
            _originalConsoleOut = Console.Out;
            Console.SetOut(new TextBoxWriter(txtConsoleOutput));

            // Add a welcome message
            AppendToConsole("Welcome to Iris ML Configuration Tool\r\n", Color.White);
            AppendToConsole("Use the tabs to configure your model settings and click 'Launch Training' to start training\r\n", Color.LightGreen);
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

        private async void BtnLaunchTraining_Click(object sender, EventArgs e)
        {
            try
            {
                // First make sure we have a configuration
                if (_currentConfig == null)
                {
                    MessageBox.Show("Please create a configuration first.", "No Configuration",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Update config with latest UI values
                UpdateConfigFromUI();

                // Validate configuration
                if (!_configService.ValidateConfiguration(_currentConfig))
                {
                    MessageBox.Show("Configuration validation failed. Please check all required fields.",
                        "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Save configuration if needed
                if (string.IsNullOrEmpty(_currentFilePath))
                {
                    if (MessageBox.Show("Configuration needs to be saved before training. Save now?",
                        "Save Required", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        SaveConfigurationAs();
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    // Save latest changes
                    _configService.SaveConfiguration(_currentConfig, _currentFilePath);
                }

                // Confirm with user
                if (MessageBox.Show("Are you sure you want to start the training process? This may take some time.",
                    "Confirm Training", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    return;
                }

                // Switch to the logs tab
                tabControl.SelectedTab = tabLogs;

                // Clear previous logs
                txtConsoleOutput.Clear();
                AppendToConsole("Starting training process...\r\n", Color.White);
                AppendToConsole($"Configuration: {_currentFilePath}\r\n", Color.Yellow);

                // Disable UI during training
                EnableUI(false);
                btnLaunchTraining.Text = "Training...";
                btnLaunchTraining.BackColor = Color.DarkOrange;
                Application.DoEvents();

                await Task.Run(() => RunTrainingProcess());

                btnLaunchTraining.Text = "Launch Training";
                btnLaunchTraining.BackColor = Color.Green;
                EnableUI(true);

                AppendToConsole("\r\nTraining process completed!", Color.LightGreen);

                MessageBox.Show("Training process completed! Check the Training Logs tab for details.",
                    "Training Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                AppendToConsole($"\r\nERROR: {ex.Message}", Color.Red);
                if (ex.InnerException != null)
                {
                    AppendToConsole($"\r\nInner exception: {ex.InnerException.Message}", Color.Red);
                }
                AppendToConsole($"\r\nStack trace: {ex.StackTrace}", Color.Red);

                btnLaunchTraining.Text = "Launch Training";
                btnLaunchTraining.BackColor = Color.Green;
                EnableUI(true);
                MessageBox.Show($"Error during training: {ex.Message}", "Training Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AppendToConsole(string text, Color color)
        {
            if (txtConsoleOutput.InvokeRequired)
            {
                txtConsoleOutput.Invoke(new Action(() => AppendToConsole(text, color)));
                return;
            }

            txtConsoleOutput.SelectionStart = txtConsoleOutput.TextLength;
            txtConsoleOutput.SelectionLength = 0;
            txtConsoleOutput.SelectionColor = color;
            txtConsoleOutput.AppendText(text);
            txtConsoleOutput.SelectionColor = txtConsoleOutput.ForeColor;
            txtConsoleOutput.ScrollToCaret();
        }

        private void RunTrainingProcess()
        {
            try
            {
                // Convert ModelConfigUI to ModelConfig
                var modelConfig = ConvertToModelConfig(_currentConfig);

                // Save config to temporary file that the ML process can read
                string tempConfigPath = Path.Combine(Path.GetTempPath(), "modelconfig.json");
                var serializableConfig = new Dictionary<string, ModelConfig>
                {
                    { "modelConfig", modelConfig }
                };

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    Converters = { new JsonStringEnumConverter() }
                };

                string jsonConfig = JsonSerializer.Serialize(serializableConfig, options);
                File.WriteAllText(tempConfigPath, jsonConfig);

                // Run the training process directly
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

                // Clean up
                try { File.Delete(tempConfigPath); } catch { }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in training process: {ex.Message}");
                throw;
            }
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

                        AppendToConsole($"Loaded configuration from: {_currentFilePath}\r\n", Color.Green);
                        MessageBox.Show("Configuration loaded successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        AppendToConsole($"Error loading configuration: {ex.Message}\r\n", Color.Red);
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
                AppendToConsole($"Configuration saved to: {_currentFilePath}\r\n", Color.Green);
                MessageBox.Show("Configuration saved successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                AppendToConsole($"Error saving configuration: {ex.Message}\r\n", Color.Red);
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
                        AppendToConsole($"Configuration saved to: {_currentFilePath}\r\n", Color.Green);
                        MessageBox.Show("Configuration saved successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        AppendToConsole($"Error saving configuration: {ex.Message}\r\n", Color.Red);
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

        private ModelConfig ConvertToModelConfig(ModelConfigUI configUI)
        {
            var modelConfig = new ModelConfig
            {
                Author = configUI.Author,
                Description = configUI.Description,
                ModelType = configUI.ModelType,
                TargetField = configUI.TargetField,
                Database = new Core.Models.DatabaseConfig
                {
                    Server = configUI.Database.Server,
                    Database = configUI.Database.Database,
                    TableName = configUI.Database.TableName,
                    OutputTableName = configUI.Database.OutputTableName,
                    WhereClause = configUI.Database.WhereClause
                },
                TrainingParameters = new Core.Models.TrainingParameters
                {
                    Algorithm = configUI.TrainingParameters.Algorithm,
                    TestFraction = configUI.TrainingParameters.TestFraction,
                    AlgorithmParameters = configUI.TrainingParameters.AlgorithmParameters
                },
                InputFields = configUI.InputFields.Select(f => new Core.Models.InputField
                {
                    Name = f.Name,
                    IsEnabled = f.IsEnabled
                }).ToList(),
                FeatureEngineering = new Core.Models.FeatureEngineeringConfig
                {
                    Method = Core.Enums.FeatureSelectionMethod.None,
                    ExecutionOrder = 2,
                    NumberOfComponents = 3,
                    MaxFeatures = 10,
                    MulticollinearityThreshold = 0.7
                },
                DataBalancing = new Core.Models.DataBalancingConfig
                {
                    Method = Core.Enums.DataBalanceMethod.None,
                    ExecutionOrder = 1,
                    KNeighbors = 5,
                    UndersamplingRatio = 0.9f,
                    MinorityToMajorityRatio = 0.1f
                },
                // Adding AutoML config to ensure compatibility with modelconfig.json
                AutoML = new Core.Models.AutoMLConfig
                {
                    Enabled = false,
                    MaxExperimentTimeInSeconds = 30,
                    OptimizingMetric = "Accuracy"
                }
            };

            return modelConfig;
        }

        private void EnableUI(bool enable)
        {
            tabControl.Enabled = enable;
            menuStrip.Enabled = enable;

            // Keep the logs tab enabled even during training
            if (!enable)
            {
                tabLogs.Enabled = true;
                txtConsoleOutput.Enabled = true;
            }
        }
    }
}