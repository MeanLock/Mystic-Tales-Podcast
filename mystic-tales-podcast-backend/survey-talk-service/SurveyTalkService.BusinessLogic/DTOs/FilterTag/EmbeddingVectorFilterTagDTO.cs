namespace SurveyTalkService.BusinessLogic.DTOs.FilterTag
{

    public class EmbeddingVectorFilterTagDTO
    {
        public int FilterTagId { get; set; }
        public float[]? EmbeddingVector { get; set; } 

    }
}