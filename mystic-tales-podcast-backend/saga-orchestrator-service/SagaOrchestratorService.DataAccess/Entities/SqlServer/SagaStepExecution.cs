using System;
using System.Collections.Generic;

namespace SagaOrchestratorService.DataAccess.Entities.sqlserver;

public partial class SagaStepExecution
{
    public Guid Id { get; set; }

    public Guid SagaInstanceId { get; set; }

    public string StepName { get; set; }

    public string? TopicName { get; set; }

    public string StepStatus { get; set; }

    public string? RequestData { get; set; }

    public string? ResponseData { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual SagaInstance SagaInstance { get; set; }
}
