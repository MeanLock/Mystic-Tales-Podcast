using Microsoft.AspNetCore.Mvc;
using UserService.API.Filters.ExceptionFilters;
using UserService.BusinessLogic.Helpers.FileHelpers;
using UserService.Infrastructure.Services.Audio.Hls;
using System.Text;
using UserService.Common.AppConfigurations.Media;
using UserService.Common.AppConfigurations.Media.interfaces;
using UserService.Infrastructure.Models.Audio.Hls;
using UserService.Infrastructure.Models.Audio.Tuning;
using UserService.BusinessLogic.Services.AudioServices;

namespace UserService.API.Controllers.QueryControllers
{
    [Route("api/audio-tuning")]
    [ApiController]
    [TypeFilter(typeof(HttpExceptionFilter))]
    public class AudioTuningController : ControllerBase
    {

        private readonly FileIOHelper _fileIOHelper;
        private readonly FFMegLocalHlsService _ffMegLocalHlsService;
        private readonly FFMpegCoreHlsService _ffMpegCoreHlsService;
        private readonly IMediaTypeConfig _mediaTypeConfig;

        private readonly AudioTuningService _audioTuningService;
        public AudioTuningController(FileIOHelper fileIOHelper, FFMegLocalHlsService ffMegLocalHlsService, FFMpegCoreHlsService ffMpegCoreHlsService, IMediaTypeConfig mediaTypeConfig, AudioTuningService audioTuningService)
        {
            _mediaTypeConfig = mediaTypeConfig;
            _fileIOHelper = fileIOHelper;
            _ffMegLocalHlsService = ffMegLocalHlsService;
            _audioTuningService = audioTuningService;
            _ffMpegCoreHlsService = ffMpegCoreHlsService;
        }

        [HttpPost("tune")]
        [Consumes("multipart/form-data")]
        [RequestFormLimits(MultipartBodyLengthLimit = 200_000_000)]
        [RequestSizeLimit(200_000_000)]
        public async Task<IActionResult> Tune([FromForm] TuneForm form)
        {
            // return Ok(form);
            Console.WriteLine($"[DEBUG] 1111111111111111111111111111111111111111111111111111");
            if (form.File == null || form.File.Length == 0)
                return BadRequest("File missing");

            var tuningProfile = form.TuningProfile ?? new GeneralTuningProfile();

            if (form.BackgroundFile != null && form.BackgroundFile.Length > 0)
            {
                tuningProfile.BackgroundMergeProfile ??= new BackgroundMergeProfile();

                var bgMs = new MemoryStream();
                await form.BackgroundFile.CopyToAsync(bgMs);
                bgMs.Position = 0;
                tuningProfile.BackgroundMergeProfile.FileStream = bgMs;

                if (form.BackgroundVolumeDb.HasValue)
                    tuningProfile.BackgroundMergeProfile.VolumeGainDb = form.BackgroundVolumeDb;
            }

            var mainMs = new MemoryStream();
            await form.File.CopyToAsync(mainMs);
            mainMs.Position = 0;
            var result = await _audioTuningService.ProcessTuning(mainMs, tuningProfile);
            Console.WriteLine($"[DEBUG] 2222222222222222222222222222222222222222222222222222");
            return result;
        }

        public class TuneForm
        {
            public IFormFile File { get; set; } = default!;
            public IFormFile? BackgroundFile { get; set; }
            public double? BackgroundVolumeDb { get; set; }
            public GeneralTuningProfile? TuningProfile { get; set; }
        }

    }

}
