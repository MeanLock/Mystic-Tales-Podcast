using System;
using System.Collections.Generic;

namespace ModerationService.DataAccess.Entities.sqlserver;

public partial class Dmcanotice
{
    public Guid Id { get; set; }

    public Guid? PodcastShowId { get; set; }

    public Guid? PodcastEpisodeId { get; set; }

    public int AccountId { get; set; }

    public string AccountEmail { get; set; } = null!;

    public string AccountPhone { get; set; } = null!;

    public string GoodFaithStatement { get; set; } = null!;

    public string WorkClaimed { get; set; } = null!;

    public string Signature { get; set; } = null!;

    public bool? IsValid { get; set; }

    public string? InvalidReason { get; set; }

    public int? ValidatedBy { get; set; }

    public DateTime? ValidatedAt { get; set; }

    public int DmcaAccusationId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Dmcaaccusation DmcaAccusation { get; set; } = null!;

    public virtual ICollection<DmcanoticeAttachFile> DmcanoticeAttachFiles { get; set; } = new List<DmcanoticeAttachFile>();
}
