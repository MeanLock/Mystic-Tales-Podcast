using System.Text.Json.Serialization;
using SubscriptionService.BusinessLogic.DTOs.Survey.JsonConfigs;

namespace SubscriptionService.BusinessLogic.DTOs.Survey.ListItems
{
    public class SurveyDefaultBackgroundThemeListItemDTO
    {
        public int Id { get; set; }
        public SurveyDefaultBackgroundThemeConfigJsonDTO? ConfigJson { get; set; }
        public string? MainImageUrl { get; set; }
    }
}
