namespace SurveyTalkService.BusinessLogic.DTOs.Survey.JsonConfigs
{
    public class SurveyConfigJsonDTO
    {
        public string? Background { get; set; } // "color_gradient" hoặc "image"
        public bool? IsUseBackgroundImageBase64 { get; set; }
        public bool? IsPause { get; set; }
        public string? BackgroundGradient1Color { get; set; }
        public string? BackgroundGradient2Color { get; set; }
        public string? TitleColor { get; set; }
        public string? ContentColor { get; set; }
        public string? ButtonBackgroundColor { get; set; }
        public string? ButtonContentColor { get; set; }
        public string? Password { get; set; }
        public int? Brightness { get; set; }
        public int? DefaultBackgroundImageId { get; set; }
        public bool? SkipStartPage { get; set; }

    }
}
