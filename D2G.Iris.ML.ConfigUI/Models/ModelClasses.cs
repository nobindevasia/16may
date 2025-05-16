using System.Collections.Generic;
using System.Text.Json.Serialization;
using D2G.Iris.ML.Core.Enums;

namespace D2G.Iris.ML.ConfigUI.Models
{
    // UI models for configuration that mirror the actual data models
    // but can have additional UI-specific properties or behaviors

    public class ModelConfigUI
    {
        public string Author { get; set; }
        public string Description { get; set; }
        public ModelType ModelType { get; set; }
        public string TargetField { get; set; }
        public DatabaseConfigUI Database { get; set; }
        public TrainingParametersUI TrainingParameters { get; set; }
        public List<InputFieldUI> InputFields { get; set; } = new List<InputFieldUI>();
    }

    public class DatabaseConfigUI
    {
        public string Server { get; set; }
        public string Database { get; set; }
        public string TableName { get; set; }
        public string OutputTableName { get; set; }
        public string WhereClause { get; set; }
    }

    public class InputFieldUI
    {
        public string Name { get; set; }
        public bool IsEnabled { get; set; }
    }

    public class TrainingParametersUI
    {
        public string Algorithm { get; set; }
        public Dictionary<string, object> AlgorithmParameters { get; set; } = new Dictionary<string, object>();
        public double TestFraction { get; set; }
    }

    // JSON serializable version for storage
    public class SerializableModelConfig
    {
        [JsonPropertyName("modelConfig")]
        public ModelConfigUI ModelConfig { get; set; }
    }

    // Lightweight model for general settings
    public class GeneralSettingsModel
    {
        public string Author { get; set; }
        public string Description { get; set; }
        public ModelType ModelType { get; set; }
        public string TargetField { get; set; }
    }
}