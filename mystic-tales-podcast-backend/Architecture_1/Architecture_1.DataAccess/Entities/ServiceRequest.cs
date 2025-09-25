using System;
using System.Collections.Generic;

namespace Architecture_1.DataAccess.Entities;

public partial class ServiceRequest
{
    public int Id { get; set; }

    public int ServiceId { get; set; }

    public int RequesterId { get; set; }

    public int RequestStatusId { get; set; }

    public string? RequestInitDescription { get; set; }

    public string? RequestResultDescription { get; set; }

    public int? AssignedAssigneeId { get; set; }

    public TimeOnly? TimeRequest { get; set; }

    public DateOnly? DateRequest { get; set; }

    public bool IsCancelAutomatically { get; set; }

    public string? ProgressNote { get; set; }

    public string? CancelReason { get; set; }

    public bool IsSeen { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Account? AssignedAssignee { get; set; }

    public virtual RequestStatus RequestStatus { get; set; } = null!;

    public virtual Account Requester { get; set; } = null!;

    public virtual Service Service { get; set; } = null!;
}
