using System.ComponentModel.DataAnnotations.Schema;
using Pgvector;

namespace SurveyTalkService.DataAccess.Entities.postgres;

public partial class SurveyEmbeddingVectorTakenResultTagFilter
{
    public int SurveyTakenResultId { get; set; }

    public int AdditionalFilterTagId { get; set; }

    [Column(TypeName = "vector")]
    public Vector? EmbeddingVector { get; set; }
}
