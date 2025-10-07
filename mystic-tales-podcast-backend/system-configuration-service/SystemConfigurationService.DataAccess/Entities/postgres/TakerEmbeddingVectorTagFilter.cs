using System.ComponentModel.DataAnnotations.Schema;
using Pgvector;

namespace SystemConfigurationService.DataAccess.Entities.Postgres;

public partial class TakerEmbeddingVectorTagFilter
{
    public int TakerId { get; set; }

    public int FilterTagId { get; set; }

    [Column(TypeName = "vector")]
    public Vector? EmbeddingVector { get; set; }
}
