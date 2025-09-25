using System.Collections.Generic;

namespace SurveyTalkService.BusinessLogic.DTOs.Survey.Session.V2
{
    public class SurveySessionUpdateTriggerResponseDTO
    {
        public string Message { get; set; }
        public bool IsSuccess { get; set; }

        public SurveySessionUpdateTriggerResponseDTO() { }
        public SurveySessionUpdateTriggerResponseDTO(string message, bool isSuccess)
        {
            Message = message;
            IsSuccess = isSuccess;
        }
    }

}
