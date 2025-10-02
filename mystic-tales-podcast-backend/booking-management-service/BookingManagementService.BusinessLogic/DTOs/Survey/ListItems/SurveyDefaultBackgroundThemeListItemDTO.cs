using System.Text.Json.Serialization;
using BookingManagementService.BusinessLogic.DTOs.Survey.JsonConfigs;

namespace BookingManagementService.BusinessLogic.DTOs.Survey.ListItems
{
    public class SurveyDefaultBackgroundThemeListItemDTO
    {
        public int Id { get; set; }
        public SurveyDefaultBackgroundThemeConfigJsonDTO? ConfigJson { get; set; }
        public string? MainImageUrl { get; set; }
    }
}
