using System;
using System.Collections.Generic;

namespace Architecture_1.DataAccess.Entities;

public partial class Account
{
    public int Id { get; set; }

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public int RoleId { get; set; }

    public int JobTypeId { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string? Address { get; set; }

    public string? Phone { get; set; }

    public bool IsDeactivated { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<AssigneeFacilityMajorAssignment> AssigneeFacilityMajorAssignments { get; set; } = new List<AssigneeFacilityMajorAssignment>();

    public virtual ICollection<Chat> ChatFromNavigations { get; set; } = new List<Chat>();

    public virtual ICollection<Chat> ChatToNavigations { get; set; } = new List<Chat>();

    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public virtual JobType JobType { get; set; } = null!;

    public virtual ICollection<Report> Reports { get; set; } = new List<Report>();

    public virtual Role Role { get; set; } = null!;

    public virtual ICollection<ServiceRequest> ServiceRequestAssignedAssignees { get; set; } = new List<ServiceRequest>();

    public virtual ICollection<ServiceRequest> ServiceRequestRequesters { get; set; } = new List<ServiceRequest>();

    public virtual ICollection<TaskRequest> TaskRequests { get; set; } = new List<TaskRequest>();
}
