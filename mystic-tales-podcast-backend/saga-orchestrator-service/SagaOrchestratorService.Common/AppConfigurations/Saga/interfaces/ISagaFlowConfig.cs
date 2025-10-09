using System.Collections.Generic;

namespace SagaOrchestratorService.Common.AppConfigurations.Saga.interfaces
{
    public interface ISagaFlowConfig
    {
        bool Loaded { get; }
        string Version { get; }
        IReadOnlyDictionary<string, SagaFlowDefinitionModel> Flows { get; }
    }

    public class SagaFlowDefinitionModel
    {
        public string Topic { get; set; } = string.Empty;
        public string HandleService { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string SaveFile { get; set; } = string.Empty;
        public List<SagaStepDefinitionModel> Steps { get; set; } = new();
    }

    public class SagaStepDefinitionModel
    {
        public string Name { get; set; } = string.Empty;
        public string Topic { get; set; } = string.Empty;
        public string HandleService { get; set; } = string.Empty;
        public string? Description { get; set; }
        // public List<string> Parameters { get; set; } = new();
        // public List<string> NextRequestData { get; set; } = new();
        // public List<string> ResponseData { get; set; } = new();
        public SagaOutcomeDefinitionModel? OnSuccess { get; set; }
        public SagaOutcomeDefinitionModel? OnFailure { get; set; }
    }

    public class SagaOutcomeDefinitionModel
    {
        public string Emit { get; set; } = string.Empty;
        public string Topic { get; set; } = string.Empty;
        public string HandleService { get; set; } = string.Empty;
        public List<SagaStepRefModel> NextSteps { get; set; } = new();

        // Changed to support multiple flows like nextSteps
        public List<SagaStepRefModel> NextFlows { get; set; } = new();
    }

    public class SagaStepRefModel
    {
        public string Name { get; set; } = string.Empty;
        public string Topic { get; set; } = string.Empty;
        public string HandleService { get; set; } = string.Empty;
    }
}