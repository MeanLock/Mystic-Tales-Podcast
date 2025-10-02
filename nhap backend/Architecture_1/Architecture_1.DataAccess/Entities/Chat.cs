using System;
using System.Collections.Generic;

namespace Architecture_1.DataAccess.Entities;

public partial class Chat
{
    public int Id { get; set; }

    public string? Message { get; set; }

    public DateTime? Created { get; set; }

    public string? SenderName { get; set; }

    public int? From { get; set; }

    public int? To { get; set; }

    public virtual Account? FromNavigation { get; set; }

    public virtual Account? ToNavigation { get; set; }
}
