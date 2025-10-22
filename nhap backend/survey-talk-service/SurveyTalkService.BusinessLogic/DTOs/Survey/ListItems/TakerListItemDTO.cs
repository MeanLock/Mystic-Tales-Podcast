using System;

namespace SurveyTalkService.BusinessLogic.DTOs.Survey.ListItems
{
    public class TakerListItemDTO
    {
        public int Id { get; set; }

        public string? FullName { get; set; }

        public DateOnly? Dob { get; set; }

        public string? Gender { get; set; }

        public string? MainImageUrl { get; set; }
    }
}
