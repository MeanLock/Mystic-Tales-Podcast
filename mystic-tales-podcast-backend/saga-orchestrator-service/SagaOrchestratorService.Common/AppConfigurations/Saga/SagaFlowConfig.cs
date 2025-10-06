using SagaOrchestratorService.Common.AppConfigurations.Saga.interfaces;
using Microsoft.Extensions.Configuration;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SagaOrchestratorService.Common.AppConfigurations.Saga
{
    internal class SagaFlowFileModel
    {
        public string Version { get; set; } = "1.0";
        public Dictionary<string, SagaFlowDefinitionModel> Flows { get; set; } = new();
    }

    public class SagaFlowConfig : ISagaFlowConfig
    {
        public bool Loaded { get; private set; }
        public string Version { get; private set; } = "1.0";
        public IReadOnlyDictionary<string, SagaFlowDefinitionModel> Flows => _flows;
        private readonly Dictionary<string, SagaFlowDefinitionModel> _flows = new(StringComparer.OrdinalIgnoreCase);

        public SagaFlowConfig(IConfiguration configuration)
        {
            // Prefer path from configuration. Example in appsettings.json:
            // "Flow": { "YAML": { "Path": "SagaOrchestratorService/SagaFlows/order-processing-flow-new.yaml" } }
            var configuredPath = configuration["Flow:YAML:Path"];

            configuredPath = configuredPath.Replace("\\", Path.DirectorySeparatorChar.ToString());
            string basePath = AppDomain.CurrentDomain.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar);
            string relativePath = configuredPath.TrimStart(Path.DirectorySeparatorChar);

            string yamlPath = Path.Combine(basePath, relativePath);

            Console.WriteLine($"Loading SagaFlowConfig from: {yamlPath}");

            if (!File.Exists(yamlPath))
            {
                Console.WriteLine($"Warning: YAML flow definition not found at: {yamlPath}");
                // Keep defaults; Loaded stays false, but service is alive to avoid startup failure
                return;
            }

            try
            {
                var yaml = File.ReadAllText(yamlPath);
                Console.WriteLine($"YAML file size: {yaml.Length} characters");
                
                // Try with CamelCase naming convention first
                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .IgnoreUnmatchedProperties()
                    .Build();

                Console.WriteLine("Attempting to deserialize YAML with CamelCase naming convention...");
                var file = deserializer.Deserialize<SagaFlowFileModel>(yaml) ?? new SagaFlowFileModel();
                
                Version = string.IsNullOrWhiteSpace(file.Version) ? "1.0" : file.Version;
                Console.WriteLine($"Parsed version: {Version}");
                Console.WriteLine($"Found {file.Flows.Count} flows");

                _flows.Clear();
                foreach (var kv in file.Flows)
                {
                    _flows[kv.Key] = kv.Value ?? new SagaFlowDefinitionModel();
                }

                // Detailed logging
                foreach (var flow in file.Flows)
                {
                    Console.WriteLine($"Flow: {flow.Key}, Description: {flow.Value.Description}");
                    Console.WriteLine($"  Steps count: {flow.Value.Steps.Count}");
                    
                    foreach (var step in flow.Value.Steps)
                    {
                        Console.WriteLine($"  Step: {step.Name}, Topic: {step.Topic}");
                        Console.WriteLine($"    OnSuccess is null: {step.OnSuccess == null}");
                        Console.WriteLine($"    OnFailure is null: {step.OnFailure == null}");
                        
                        if (step.OnSuccess != null)
                        {
                            Console.WriteLine($"    OnSuccess - Emit: {step.OnSuccess.Emit}, Topic: {step.OnSuccess.Topic}");
                            Console.WriteLine($"    OnSuccess - NextSteps count: {step.OnSuccess.NextSteps?.Count ?? 0}");
                            Console.WriteLine($"    OnSuccess - NextFlows count: {step.OnSuccess.NextFlows?.Count ?? 0}");
                        }
                        
                        if (step.OnFailure != null)
                        {
                            Console.WriteLine($"    OnFailure - Emit: {step.OnFailure.Emit}, Topic: {step.OnFailure.Topic}");
                            Console.WriteLine($"    OnFailure - NextSteps count: {step.OnFailure.NextSteps?.Count ?? 0}");
                            Console.WriteLine($"    OnFailure - NextFlows count: {step.OnFailure.NextFlows?.Count ?? 0}");
                        }
                    }
                }
                
                Loaded = true;
                Console.WriteLine($"Successfully loaded YAML flow definition from: {yamlPath}");
            }
            catch (YamlDotNet.Core.YamlException yamlEx)
            {
                Console.WriteLine($"YAML parsing error: {yamlEx.Message}");
                Console.WriteLine($"Line: {yamlEx.Start.Line}, Column: {yamlEx.Start.Column}");
                Console.WriteLine($"Inner exception: {yamlEx.InnerException?.Message}");
                Loaded = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading YAML: {ex.Message}");
                Console.WriteLine($"Exception type: {ex.GetType().Name}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    Console.WriteLine($"Inner exception type: {ex.InnerException.GetType().Name}");
                }
                Loaded = false;
            }
        }
    }
}