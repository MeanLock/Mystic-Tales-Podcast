using System;
using System.Collections.Generic;

namespace Architecture_1.DataAccess.Entities;

public partial class FacilityMajor
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? MainDescription { get; set; }

    public string? WorkShiftsDescription { get; set; }

    public bool IsOpen { get; set; }

    public DateOnly? CloseScheduleDate { get; set; }

    public DateOnly? OpenScheduleDate { get; set; }

    public int FacilityMajorTypeId { get; set; }

    public int? FacilityId { get; set; }

    public bool IsDeactivated { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<AssigneeFacilityMajorAssignment> AssigneeFacilityMajorAssignments { get; set; } = new List<AssigneeFacilityMajorAssignment>();

    public virtual Facility? Facility { get; set; }

    public virtual ICollection<FacilityItemAssignment> FacilityItemAssignments { get; set; } = new List<FacilityItemAssignment>();

    public virtual FacilityMajorType FacilityMajorType { get; set; } = null!;

    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public virtual ICollection<Report> Reports { get; set; } = new List<Report>();

    public virtual ICollection<Service> Services { get; set; } = new List<Service>();

    public virtual ICollection<TaskRequest> TaskRequests { get; set; } = new List<TaskRequest>();
}
