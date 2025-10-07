using System.ComponentModel.DataAnnotations.Schema;
using Pgvector;

namespace UserService.DataAccess.Entities.Postgres;

public partial class SurveyEmbeddingVectorTagFilter
{
    public int SurveyId { get; set; }

    public int FilterTagId { get; set; }

    [Column(TypeName = "vector")]
    public Vector? EmbeddingVector { get; set; }
}
