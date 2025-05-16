using System;
using System.IO;
using System.Text.Json;
using D2G.Iris.ML.ConfigUI.Models;
using D2G.Iris.ML.Core.Models;

namespace D2G.Iris.ML.ConfigUI.Services
{
    public class ConfigurationService
    {
        private readonly JsonSerializerOptions _serializerOptions;

        public ConfigurationService()
        {
            _serializerOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public ModelConfigUI LoadConfiguration(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Configuration file not found.", filePath);
            }

            try
            {
                string jsonText = File.ReadAllText(filePath);
                var serializableConfig = JsonSerializer.Deserialize<SerializableModelConfig>(jsonText, _serializerOptions);

                if (serializableConfig?.ModelConfig == null)
                {
                    throw new InvalidOperationException("Invalid configuration format.");
                }

                return serializableConfig.ModelConfig;
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"Error parsing configuration file: {ex.Message}", ex);
            }
            catch (Exception ex) when (ex is not FileNotFoundException && ex is not InvalidOperationException)
            {
                throw new InvalidOperationException($"Error loading configuration: {ex.Message}", ex);
            }
        }

        public void SaveConfiguration(ModelConfigUI config, string filePath)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            try
            {
                var serializableConfig = new SerializableModelConfig
                {
                    ModelConfig = config
                };

                string jsonText = JsonSerializer.Serialize(serializableConfig, _serializerOptions);
                File.WriteAllText(filePath, jsonText);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error saving configuration: {ex.Message}", ex);
            }
        }

        // Helper method to validate the configuration
        public bool ValidateConfiguration(ModelConfigUI config)
        {
            if (config == null)
                return false;

            if (string.IsNullOrWhiteSpace(config.TargetField))
                return false;

            if (config.Database == null)
                return false;

            if (string.IsNullOrWhiteSpace(config.Database.Server) ||
                string.IsNullOrWhiteSpace(config.Database.Database) ||
                string.IsNullOrWhiteSpace(config.Database.TableName))
                return false;

            if (config.TrainingParameters == null ||
                string.IsNullOrWhiteSpace(config.TrainingParameters.Algorithm))
                return false;

            return true;
        }
    }
}