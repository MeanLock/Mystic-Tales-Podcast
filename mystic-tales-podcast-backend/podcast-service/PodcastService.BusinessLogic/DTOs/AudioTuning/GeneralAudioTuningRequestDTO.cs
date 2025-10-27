using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using PodcastService.BusinessLogic.Helpers.JsonHelpers;
using PodcastService.Infrastructure.Models.Audio.Tuning;

namespace PodcastService.BusinessLogic.DTOs.AudioTuning
{
    public class GeneralAudioTuningRequestDTO
    {
        public string GeneralTuningProfileRequestInfo { get; set; } 
        public IFormFile? AudioFile { get; set; }
    }

    public class GeneralTuningProfileRequestInfo
    {
        public EqualizerProfileRequestInfo? EqualizerProfile { get; set; } = null;
        public BackgroundMergeProfileRequestInfo? BackgroundMergeProfile { get; set; } = null;
        public AITuningProfileRequestInfo? AITuningProfile { get; set; } = null;
    }

    public class EqualizerProfileRequestInfo
    {
        public ExpandEqualizerProfile? ExpandEqualizer { get; set; } = null;
        public BaseEqualizerProfile? BaseEqualizer { get; set; } = null;
    }

    public class BackgroundMergeProfileRequestInfo
    {
        public double? VolumeGainDb { get; set; } = null;
        public string? BackgroundSoundTrackFileKey { get; set; } = null;
    }

    public class AITuningProfileRequestInfo
    {
        public UvrMdxNetMainProfile? UvrMdxNetMainProfile { get; set; } = null;
    }



}