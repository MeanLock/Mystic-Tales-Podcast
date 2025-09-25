// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Http;
// using Microsoft.AspNetCore.Mvc;
// using SurveyTalkService.API.Controllers.UserControllers;
// using SurveyTalkService.API.Filters.ExceptionFilters;
// using SurveyTalkService.BusinessLogic.DTOs.Auth;
// using SurveyTalkService.BusinessLogic.DTOs.Feedback;
// using SurveyTalkService.BusinessLogic.Services.DbServices.MiscServices;
// using SurveyTalkService.BusinessLogic.Services.DbServices.UserServices;
// using SurveyTalkService.DataAccess.Entities;
// using SurveyTalkService.BusinessLogic.Helpers.FileHelpers;

// namespace SurveyTalkService.API.Controllers.ServiceQueryControllers
// {
//     [Route("api/query/batch")]
//     [ApiController]
//     [TypeFilter(typeof(HttpExceptionFilter))]
//     public class ServiceBatchQueryController : ControllerBase
//     {
//         private ILogger<ServiceBatchQueryController> _logger;
//         private readonly FileIOHelper _fileIOHelper;

//         public ServiceBatchQueryController(
//             ILogger<ServiceBatchQueryController> logger,
//             FileIOHelper fileIOHelper)
//         {
//             _logger = logger;
//             _fileIOHelper = fileIOHelper;
//         }



//         ///////////////////////////////////////////////////////////////
//         [HttpGet("s3/base64/get-presigned-url")]
//         public async Task<IActionResult> GetBase64PresignedUrl([FromQuery] string rootFolderPath, [FromQuery] string folderName, [FromQuery] string fileName)
//         {
//             string filePath = FilePathHelper.CombinePaths(rootFolderPath, folderName, fileName);
//             var presignedUrl = await _fileIOHelper.GeneratePresignedUrlAsync(filePath);
//             return Ok(new
//             {
//                 PresignedUrl = presignedUrl
//             });
//         }

//         [HttpGet("s3/binary/get-presigned-url")]
//         public async Task<IActionResult> GetBinaryPresignedUrl([FromQuery] string? rootFolderPath, [FromQuery] string? folderName, [FromQuery] string? fileName)
//         {
//             string filePath = FilePathHelper.CombinePaths(rootFolderPath, folderName, fileName);
//             var presignedUrl = await _fileIOHelper.GeneratePresignedUrlAsync(filePath);
//             return Ok(new
//             {
//                 PresignedUrl = presignedUrl
//             });
//         }

//     }
// }
