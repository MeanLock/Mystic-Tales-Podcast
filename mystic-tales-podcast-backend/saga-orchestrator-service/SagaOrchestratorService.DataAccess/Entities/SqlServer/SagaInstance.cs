using System;
using System.Collections.Generic;

namespace SagaOrchestratorService.DataAccess.Entities.sqlserver;

public partial class SagaInstance
{
    public Guid Id { get; set; }

    public string FlowName { get; set; }

    public string? CurrentStepName { get; set; }

    public string InitialData { get; set; }

    public string? ResultData { get; set; }

    public string FlowStatus { get; set; }

    public string? ErrorStepName { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public virtual ICollection<SagaStepExecution> SagaStepExecutions { get; set; } = new List<SagaStepExecution>();
}
