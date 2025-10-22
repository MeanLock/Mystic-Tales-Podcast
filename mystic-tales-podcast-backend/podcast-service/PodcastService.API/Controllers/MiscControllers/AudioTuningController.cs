using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PodcastService.API.Filters.ExceptionFilters;
using PodcastService.BusinessLogic.Helpers.FileHelpers;
using PodcastService.BusinessLogic.Models.CrossService;
using PodcastService.BusinessLogic.Services.CrossServiceServices.QueryServices;
using PodcastService.DataAccess.Data;

namespace PodcastService.API.Controllers.MiscControllers
{
    [Route("api/misc/audio-tuning")]
    [ApiController]
    [TypeFilter(typeof(HttpExceptionFilter))]
    [Authorize(Policy = "OptionalAccess")]
    public class AudioTuningController : ControllerBase
    {
        private readonly FileIOHelper _fileIOHelper;
        public AudioTuningController(FileIOHelper fileIOHelper)
        {
            _fileIOHelper = fileIOHelper;
        }


        



    }
}
