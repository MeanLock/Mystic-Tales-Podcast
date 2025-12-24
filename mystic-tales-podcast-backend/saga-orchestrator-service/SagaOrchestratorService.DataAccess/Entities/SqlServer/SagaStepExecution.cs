using System;
using System.Collections.Generic;

namespace SagaOrchestratorService.DataAccess.Entities.SqlServer;

public partial class SagaStepExecution
{
    public Guid Id { get; set; }

    public Guid SagaInstanceId { get; set; }

    public string StepName { get; set; } = null!;

    public string TopicName { get; set; } = null!;

    public string StepStatus { get; set; } = null!;

    public string? RequestData { get; set; }

    public string? ResponseData { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual SagaInstance SagaInstance { get; set; } = null!;
}
