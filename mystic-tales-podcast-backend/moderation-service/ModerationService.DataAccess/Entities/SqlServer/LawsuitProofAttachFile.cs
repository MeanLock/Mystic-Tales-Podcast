using System;
using System.Collections.Generic;

namespace ModerationService.DataAccess.Entities.SqlServer;

public partial class LawsuitProofAttachFile
{
    public Guid Id { get; set; }

    public Guid LawsuitProofId { get; set; }

    public string AttachFileKey { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual LawsuitProof LawsuitProof { get; set; } = null!;
}
