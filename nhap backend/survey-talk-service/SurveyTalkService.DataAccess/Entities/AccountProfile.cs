using System;
using System.Collections.Generic;

namespace SurveyTalkService.DataAccess.Entities;

public partial class AccountProfile
{
    public int AccountId { get; set; }

    public string? CountryRegion { get; set; }

    public string? MaritalStatus { get; set; }

    public string? AverageIncome { get; set; }

    public string? EducationLevel { get; set; }

    public string? JobField { get; set; }

    public int? ProvinceCode { get; set; }

    public int? DistrictCode { get; set; }

    public int? WardCode { get; set; }

    public virtual Account Account { get; set; } = null!;
}
