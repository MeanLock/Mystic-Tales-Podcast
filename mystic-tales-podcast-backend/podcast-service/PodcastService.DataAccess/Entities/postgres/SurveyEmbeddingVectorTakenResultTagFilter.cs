using System.ComponentModel.DataAnnotations.Schema;
using Pgvector;

namespace PodcastService.DataAccess.Entities.Postgres;

public partial class SurveyEmbeddingVectorTakenResultTagFilter
{
    public int SurveyTakenResultId { get; set; }

    public int AdditionalFilterTagId { get; set; }

    [Column(TypeName = "vector")]
    public Vector? EmbeddingVector { get; set; }
}
