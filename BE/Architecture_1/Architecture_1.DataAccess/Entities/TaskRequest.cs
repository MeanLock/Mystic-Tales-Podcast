using System;
using System.Collections.Generic;

namespace Architecture_1.DataAccess.Entities;

public partial class TaskRequest
{
    public int Id { get; set; }

    public string Description { get; set; } = null!;

    public int RequesterId { get; set; }

    public int FacilityMajorId { get; set; }

    public string? CancelReason { get; set; }

    public int RequestStatusId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual FacilityMajor FacilityMajor { get; set; } = null!;

    public virtual RequestStatus RequestStatus { get; set; } = null!;

    public virtual Account Requester { get; set; } = null!;
}
