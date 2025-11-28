using BookingManagementService.API.Filters.ExceptionFilters;
using BookingManagementService.BusinessLogic.Enums.Kafka;
using BookingManagementService.BusinessLogic.Helpers.AuthHelpers;
using BookingManagementService.BusinessLogic.Helpers.FileHelpers;
using BookingManagementService.BusinessLogic.Services.CrossServiceServices.QueryServices;
using BookingManagementService.BusinessLogic.Services.DbServices.BookingServices;
using BookingManagementService.BusinessLogic.Services.MessagingServices.interfaces;
using BookingManagementService.Common.AppConfigurations.BusinessSetting.interfaces;
using BookingManagementService.Common.AppConfigurations.FilePath.interfaces;
using BookingManagementService.Infrastructure.Services.Audio.Hls;
using BookingManagementService.Infrastructure.Services.Kafka;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BookingManagementService.API.Controllers.ReportControllers
{
    [Route("api/report/bookings")]
    [ApiController]
    [TypeFilter(typeof(HttpExceptionFilter))]
    [Authorize(Policy = "OptionalAccess")]
    public class BookingStatisticController : ControllerBase
    {
        private readonly GenericQueryService _genericQueryService;
        private readonly HttpServiceQueryClient _httpServiceQueryClient;
        private readonly BookingService _bookingService;
        private readonly JwtHelper _jwtHelper;
        private readonly FileIOHelper _fileIOHelper;
        private readonly IFileValidationConfig _fileValidationConfig;
        private readonly IFilePathConfig _filePathConfig;
        private readonly KafkaProducerService _kafkaProducerService;
        private readonly IMessagingService _messagingService;
        private const string SAGA_TOPIC = KafkaTopicEnum.BookingManagementDomain;
        private readonly FFMpegCoreHlsService _ffMpegCoreHlsService;
        public BookingStatisticController(
            GenericQueryService genericQueryService,
            HttpServiceQueryClient httpServiceQueryClient,
            BookingService bookingService,
            JwtHelper jwtHelper,
            FileIOHelper fileIOHelper,
            IFileValidationConfig fileValidationConfig,
            IFilePathConfig filePathConfig,
            KafkaProducerService kafkaProducerService,
            IMessagingService messagingService,
            FFMpegCoreHlsService ffMpegCoreHlsService
            )
        {
            _genericQueryService = genericQueryService;
            _httpServiceQueryClient = httpServiceQueryClient;
            _bookingService = bookingService;
            _jwtHelper = jwtHelper;
            _fileIOHelper = fileIOHelper;
            _fileValidationConfig = fileValidationConfig;
            _filePathConfig = filePathConfig;
            _kafkaProducerService = kafkaProducerService;
            _messagingService = messagingService;
            _ffMpegCoreHlsService = ffMpegCoreHlsService;
        }

    }
}
