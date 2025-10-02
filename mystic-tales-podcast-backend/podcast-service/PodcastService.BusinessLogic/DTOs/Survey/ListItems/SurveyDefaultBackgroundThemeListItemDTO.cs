using System.Text.Json.Serialization;
using PodcastService.BusinessLogic.DTOs.Survey.JsonConfigs;

namespace PodcastService.BusinessLogic.DTOs.Survey.ListItems
{
    public class SurveyDefaultBackgroundThemeListItemDTO
    {
        public int Id { get; set; }
        public SurveyDefaultBackgroundThemeConfigJsonDTO? ConfigJson { get; set; }
        public string? MainImageUrl { get; set; }
    }
}
