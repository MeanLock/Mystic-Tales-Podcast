using System;
using System.Collections.Generic;

namespace Architecture_1.DataAccess.Entities;

public partial class Service
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int FacilityMajorId { get; set; }

    public bool IsInitRequestDescriptionRequired { get; set; }

    public string? RequestInitHintDescription { get; set; }

    public string MainDescription { get; set; } = null!;

    public string? WorkShiftsDescription { get; set; }

    public bool IsOpen { get; set; }

    public DateOnly? CloseScheduleDate { get; set; }

    public DateOnly? OpenScheduleDate { get; set; }

    public int ServiceTypeId { get; set; }

    public bool IsDeactivated { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual FacilityMajor FacilityMajor { get; set; } = null!;

    public virtual ICollection<ServiceAvailability> ServiceAvailabilities { get; set; } = new List<ServiceAvailability>();

    public virtual ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();

    public virtual ServiceType ServiceType { get; set; } = null!;
}
