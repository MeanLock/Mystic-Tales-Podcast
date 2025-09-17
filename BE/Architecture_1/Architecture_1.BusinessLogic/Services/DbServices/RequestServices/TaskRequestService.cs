using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Architecture_1.BusinessLogic.Helpers;
using Architecture_1.Infrastructure.Services.AWS.S3;
using Architecture_1.Common.AppConfigurations.App.interfaces;
using Architecture_1.Common.AppConfigurations.FilePath.interfaces;
using Architecture_1.Common.AppConfigurations.Jwt;
using Architecture_1.DataAccess.Data;
using Architecture_1.DataAccess.Entities;
using Architecture_1.DataAccess.Repositories.interfaces;
using Architecture_1.DataAccess.UOW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Architecture_1.BusinessLogic.Services.DbServices.RequestServices
{
    public class TaskRequestService
    {
        // LOGGER
        private readonly ILogger<TaskRequestService> _logger;

        // CONFIG
        public readonly IAppConfig _appConfig;
        public readonly IFilePathConfig _filePathConfig;

        // DB CONTEXT
        private readonly AppDbContext _dbContext;

        // HELPERS
        private readonly FileHelpers _fileHelpers;

        // UNIT OF WORK
        private readonly IUnitOfWork _unitOfWork;

        // REPOSITORIES
        public readonly IGenericRepository<TaskRequest> _taskRequestRepository;
        public readonly IGenericRepository<RequestStatus> _requestStatusRepository;

        // AWS SERVICE
        private readonly AWSS3Service _awsS3Service;


        public TaskRequestService(
            ILogger<TaskRequestService> logger,
            AppDbContext dbContext,
            FileHelpers fileHelpers,
            IUnitOfWork unitOfWork,
            IFilePathConfig filePathConfig,
            IGenericRepository<TaskRequest> taskRequestRepository,
            IGenericRepository<RequestStatus> requestStatusRepository,
            AWSS3Service awsS3Service,
            IAppConfig appConfig
            )
        {
            _logger = logger;
            _dbContext = dbContext;
            _fileHelpers = fileHelpers;
            _unitOfWork = unitOfWork;
            _filePathConfig = filePathConfig;
            _taskRequestRepository = taskRequestRepository;
            _requestStatusRepository = requestStatusRepository;
            _awsS3Service = awsS3Service;
            _appConfig = appConfig;
        }

        public int MapRequestStatus(string status)
        {
            switch (status)
            {
                case "Pending":
                    return 1;
                case "Assigned":
                    return 2;
                case "RejectedByAssignee":
                    return 3;
                case "RejectedByAssigneeDeactivation":
                    return 4;
                case "AcceptedByAssignee":
                    return 5;
                case "CompletedByAssignee":
                    return 6;
                case "Finished":
                    return 7;
                case "Cancelled":
                    return 8;
                case "CancelledAuto":
                    return 9;
                default:
                    return 1;
            }
        }

        public async Task<string> GenerateImageUrl(string rootFolderPath, string folderName, string fileName)
        {
            if (_appConfig.IMAGE_SRC == "awsS3")
            {
                return await _awsS3Service.GeneratePresignedUrlAsync(rootFolderPath, folderName, fileName);
            }
            return await _fileHelpers.GetImageUrl(rootFolderPath, folderName, fileName);
            // return await _awsS3Service.GeneratePresignedUrlAsync(rootFolderPath, folderName, fileName);
        }

        public async Task SaveBase64File(string base64Data, string folderPath, string fileName)
        {
            if (_appConfig.IMAGE_SRC == "awsS3")
            {
                await _awsS3Service.UploadBase64FileAsync(base64Data, folderPath, fileName);
                return;
            }
            await _fileHelpers.SaveBase64File(base64Data, folderPath, fileName);
            // await _awsS3Service.UploadBase64FileAsync(base64Data, folderPath, fileName);
        }

        public async Task CopyFile(string sourceFolder, string sourceFileName, string destinationFolder, string destinationFileName)
        {
            if (_appConfig.IMAGE_SRC == "awsS3")
            {
                await _awsS3Service.CopyFileAsync(sourceFolder, sourceFileName, destinationFolder, destinationFileName);
                return;
            }
            await _fileHelpers.CopyFile(sourceFolder, sourceFileName, destinationFolder, destinationFileName);
            // await _awsS3Service.CopyFileAsync(sourceFolder, sourceFileName, destinationFolder, destinationFileName);
        }

        public async Task DeleteFolder(string folderPath)
        {
            if (_appConfig.IMAGE_SRC == "awsS3")
            {
                await _awsS3Service.DeleteFolderAsync(folderPath);
                return;
            }
            await _fileHelpers.DeleteFolder(folderPath);
            // await _awsS3Service.DeleteFolderAsync(folderPath);
        }

        /////////////////////////////////////////////////////////////////////
        public async Task<JArray> GetTaskRequests()
        {
            try
            {
                var taskRequests = await _unitOfWork.TaskRequestRepository.FindAllAsync();
                var result = new List<object>();
                foreach (var taskRequest in taskRequests)
                {
                    var facilityMajor = taskRequest.FacilityMajor;
                    var requestStatus = taskRequest.RequestStatus;
                    result.Add(new
                    {
                        TaskRequest = new
                        {
                            Id = taskRequest.Id,
                            Description = taskRequest.Description,
                            RequesterId = taskRequest.RequesterId,
                            FacilityMajorId = taskRequest.FacilityMajorId,
                            CancelReason = taskRequest.CancelReason,
                            RequestStatusId = taskRequest.RequestStatusId,
                            CreatedAt = taskRequest.CreatedAt,
                            UpdatedAt = taskRequest.UpdatedAt
                        },
                        RequestStatus = new
                        {
                            Id = requestStatus.Id,
                            Name = requestStatus.Name
                        },
                        Major = new
                        {
                            Id = facilityMajor.Id,
                            Name = facilityMajor.Name,
                            MainDescription = facilityMajor.MainDescription,
                            WorkShiftsDescription = facilityMajor.WorkShiftsDescription,
                            FacilityMajorTypeId = facilityMajor.FacilityMajorTypeId,
                            FacilityId = facilityMajor.FacilityId,
                            IsOpen = facilityMajor.IsOpen,
                            CloseScheduleDate = facilityMajor.CloseScheduleDate,
                            OpenScheduleDate = facilityMajor.OpenScheduleDate,
                            IsDeactivated = facilityMajor.IsDeactivated,
                            CreatedAt = facilityMajor.CreatedAt,
                            BackgroundImageUrl = await GenerateImageUrl(_filePathConfig.MAJOR_IMAGE_PATH, facilityMajor.Id.ToString(), "background"),
                            ImageUrl = await GenerateImageUrl(_filePathConfig.MAJOR_IMAGE_PATH, facilityMajor.Id.ToString(), "main")
                        }
                    });
                }
                return JArray.FromObject(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetTaskRequests");
                Console.WriteLine("\n\n\n" + ex.Message.ToString() + "\n\n\n");
                // throw new HttpRequestException("Lấy yêu cầu công việc thất bại: " + ex.Message);
                throw new HttpRequestException("Failed to retrieve task requests: " + ex.Message);
            }
        }

        public async Task CreateTaskRequest(dynamic taskRequest)
        {
            var requester = await _unitOfWork.AccountRepository.FindById(int.Parse(taskRequest.RequesterId.ToString()));
            if (requester == null)
            {
                // throw new HttpRequestException("Tài khoản tao yêu cầu không tồn tại");
                throw new HttpRequestException("Requester account is not exist, id: " + taskRequest.RequesterId.ToString());
            }
            if (requester.RoleId != 1)
            {
                // throw new HttpRequestException("Tài khoản tạo yêu cầu không phải là campus manager");
                throw new HttpRequestException("Requester account is not a campus manager, id: " + taskRequest.RequesterId.ToString());
            }
            var facilityMajor = await _unitOfWork.FacilityMajorRepository.FindByIdAsync(int.Parse(taskRequest.FacilityMajorId.ToString()));
            if (facilityMajor == null)
            {
                // throw new HttpRequestException("Major không tồn tại");
                throw new HttpRequestException("Major is not exist, id: " + taskRequest.FacilityMajorId.ToString());
            }
            var requestStatus = await _requestStatusRepository.FindByIdAsync(1);
            if (requestStatus == null)
            {
                // throw new HttpRequestException("Trạng thái yêu cầu không tồn tại");
                throw new HttpRequestException("Request status is not exist, id: 1");
            }

            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var newTaskRequest = new TaskRequest
                    {
                        Description = taskRequest.Description,
                        RequesterId = taskRequest.RequesterId,
                        FacilityMajorId = taskRequest.FacilityMajorId,
                        RequestStatusId = 1,
                    };

                    await _taskRequestRepository.CreateAsync(newTaskRequest);
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in CreateTaskRequest");
                    Console.WriteLine("\n\n\n" + ex.Message.ToString() + "\n\n\n");
                    // throw new HttpRequestException("Tạo yêu cầu công việc thất bại: " + ex.Message);
                    throw new HttpRequestException("Failed to create task request: " + ex.Message);
                }
            }
        }

        public async Task<JArray> GetMajorHeadTaskRequests(int accountId)
        {
            var account = await _unitOfWork.AccountRepository.FindById(accountId);
            if (account == null)
            {
                // throw new HttpRequestException("Tài khoản không tồn tại");
                throw new HttpRequestException("Account is not exist, id: " + accountId.ToString());
            }
            if (account.RoleId != 2)
            {
                // throw new HttpRequestException("Tài khoản không phải là major head");
                throw new HttpRequestException("Account is not a major head, id: " + accountId.ToString());
            }
            try
            {
                var majorAssignments = await _unitOfWork.AssigneeFacilityMajorAssignmentRepository.FindByAccountId(accountId);
                var result = new List<object>();
                foreach (var majorAssignment in majorAssignments)
                {
                    var facilityMajor = majorAssignment.FacilityMajor;
                    var taskRequests = await _unitOfWork.TaskRequestRepository.FindByFacilityMajorId(facilityMajor.Id);
                    foreach (var taskRequest in taskRequests)
                    {
                        var requestStatus = taskRequest.RequestStatus;
                        result.Add(new
                        {
                            TaskRequest = new
                            {
                                Id = taskRequest.Id,
                                Description = taskRequest.Description,
                                RequesterId = taskRequest.RequesterId,
                                FacilityMajorId = taskRequest.FacilityMajorId,
                                CancelReason = taskRequest.CancelReason,
                                RequestStatusId = taskRequest.RequestStatusId,
                                CreatedAt = taskRequest.CreatedAt,
                                UpdatedAt = taskRequest.UpdatedAt
                            },
                            RequestStatus = new
                            {
                                Id = requestStatus.Id,
                                Name = requestStatus.Name
                            },
                            Major = new
                            {
                                Id = facilityMajor.Id,
                                Name = facilityMajor.Name,
                                MainDescription = facilityMajor.MainDescription,
                                WorkShiftsDescription = facilityMajor.WorkShiftsDescription,
                                FacilityMajorTypeId = facilityMajor.FacilityMajorTypeId,
                                FacilityId = facilityMajor.FacilityId,
                                IsOpen = facilityMajor.IsOpen,
                                CloseScheduleDate = facilityMajor.CloseScheduleDate,
                                OpenScheduleDate = facilityMajor.OpenScheduleDate,
                                IsDeactivated = facilityMajor.IsDeactivated,
                                CreatedAt = facilityMajor.CreatedAt,
                                BackgroundImageUrl = await GenerateImageUrl(_filePathConfig.MAJOR_IMAGE_PATH, facilityMajor.Id.ToString(), "background"),
                                ImageUrl = await GenerateImageUrl(_filePathConfig.MAJOR_IMAGE_PATH, facilityMajor.Id.ToString(), "main")
                            }
                        });
                    }
                }
                return JArray.FromObject(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetMajorHeadTaskRequests");
                Console.WriteLine("\n\n\n" + ex.Message.ToString() + "\n\n\n");
                // throw new HttpRequestException("Lấy yêu cầu công việc theo major head thất bại: " + ex.Message);
                throw new HttpRequestException("Failed to retrieve task requests by major head: " + ex.Message);
            }
        }

        public async Task<JArray> GetTaskRequestsByMajorId(int majorId)
        {
            var facilityMajor = await _unitOfWork.FacilityMajorRepository.FindByIdAsync(majorId);
            if (facilityMajor == null)
            {
                // throw new HttpRequestException("Major không tồn tại");
                throw new HttpRequestException("Major is not exist, id: " + majorId.ToString());
            }
            try
            {
                var taskRequests = await _unitOfWork.TaskRequestRepository.FindByFacilityMajorId(majorId);
                var result = new List<object>();
                foreach (var taskRequest in taskRequests)
                {
                    var requestStatus = taskRequest.RequestStatus;
                    result.Add(new
                    {
                        TaskRequest = new
                        {
                            Id = taskRequest.Id,
                            Description = taskRequest.Description,
                            RequesterId = taskRequest.RequesterId,
                            FacilityMajorId = taskRequest.FacilityMajorId,
                            CancelReason = taskRequest.CancelReason,
                            RequestStatusId = taskRequest.RequestStatusId,
                            CreatedAt = taskRequest.CreatedAt,
                            UpdatedAt = taskRequest.UpdatedAt
                        },
                        RequestStatus = new
                        {
                            Id = requestStatus.Id,
                            Name = requestStatus.Name
                        },
                        Major = new
                        {
                            Id = facilityMajor.Id,
                            Name = facilityMajor.Name,
                            MainDescription = facilityMajor.MainDescription,
                            WorkShiftsDescription = facilityMajor.WorkShiftsDescription,
                            FacilityMajorTypeId = facilityMajor.FacilityMajorTypeId,
                            FacilityId = facilityMajor.FacilityId,
                            IsOpen = facilityMajor.IsOpen,
                            CloseScheduleDate = facilityMajor.CloseScheduleDate,
                            OpenScheduleDate = facilityMajor.OpenScheduleDate,
                            IsDeactivated = facilityMajor.IsDeactivated,
                            CreatedAt = facilityMajor.CreatedAt,
                            BackgroundImageUrl = await GenerateImageUrl(_filePathConfig.MAJOR_IMAGE_PATH, facilityMajor.Id.ToString(), "background"),
                            ImageUrl = await GenerateImageUrl(_filePathConfig.MAJOR_IMAGE_PATH, facilityMajor.Id.ToString(), "main")
                        }
                    });
                }
                return JArray.FromObject(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetTaskRequestsByMajorId");
                Console.WriteLine("\n\n\n" + ex.Message.ToString() + "\n\n\n");
                // throw new HttpRequestException("Lấy yêu cầu công việc theo major thất bại: " + ex.Message);
                throw new HttpRequestException("Failed to retrieve task requests by major: " + ex.Message);
            }
        }

        public async Task<JObject> GetTaskRequestDetail(int requestId)
        {
            var taskRequest = await _unitOfWork.TaskRequestRepository.FindByIdAsync(requestId);
            if (taskRequest == null)
            {
                // throw new HttpRequestException("Yêu cầu công việc không tồn tại");
                throw new HttpRequestException("Task request is not exist, id: " + requestId.ToString());
            }
            try
            {

                var facilityMajor = taskRequest.FacilityMajor;
                var requestStatus = taskRequest.RequestStatus;

                return JObject.FromObject(new
                {
                    TaskRequest = new
                    {
                        Id = taskRequest.Id,
                        Description = taskRequest.Description,
                        RequesterId = taskRequest.RequesterId,
                        FacilityMajorId = taskRequest.FacilityMajorId,
                        CancelReason = taskRequest.CancelReason,
                        RequestStatusId = taskRequest.RequestStatusId,
                        CreatedAt = taskRequest.CreatedAt,
                        UpdatedAt = taskRequest.UpdatedAt
                    },
                    RequestStatus = new
                    {
                        Id = requestStatus.Id,
                        Name = requestStatus.Name
                    },
                    Major = new
                    {
                        Id = facilityMajor.Id,
                        Name = facilityMajor.Name,
                        MainDescription = facilityMajor.MainDescription,
                        WorkShiftsDescription = facilityMajor.WorkShiftsDescription,
                        FacilityMajorTypeId = facilityMajor.FacilityMajorTypeId,
                        FacilityId = facilityMajor.FacilityId,
                        IsOpen = facilityMajor.IsOpen,
                        CloseScheduleDate = facilityMajor.CloseScheduleDate,
                        OpenScheduleDate = facilityMajor.OpenScheduleDate,
                        IsDeactivated = facilityMajor.IsDeactivated,
                        CreatedAt = facilityMajor.CreatedAt,
                        BackgroundImageUrl = await GenerateImageUrl(_filePathConfig.MAJOR_IMAGE_PATH, facilityMajor.Id.ToString(), "background"),
                        ImageUrl = await GenerateImageUrl(_filePathConfig.MAJOR_IMAGE_PATH, facilityMajor.Id.ToString(), "main")
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetTaskRequestDetail");
                Console.WriteLine("\n\n\n" + ex.Message.ToString() + "\n\n\n");
                // throw new HttpRequestException("Lấy thông tin yêu cầu công việc thất bại: " + ex.Message);
                throw new HttpRequestException("Failed to retrieve task request detail: " + ex.Message);
            }

        }

        public async Task UpdateRequestStatus(int requestId, string action, dynamic updateInfo)
        {
            var taskRequest = await _unitOfWork.TaskRequestRepository.FindByIdAsync(requestId);
            if (taskRequest == null)
            {
                // throw new HttpRequestException("Yêu cầu công việc không tồn tại");
                throw new HttpRequestException("Task request is not exist, id: " + requestId.ToString());
            }
            int requestStatusId = MapRequestStatus(action);
            var requestStatus = await _requestStatusRepository.FindByIdAsync(requestStatusId);
            if (requestStatus == null)
            {
                // throw new HttpRequestException("Trạng thái yêu cầu không tồn tại");
                throw new HttpRequestException("Request status is not exist, id: " + requestStatusId.ToString());
            }
            try
            {
                if(requestStatusId == 7)
                {
                    taskRequest.RequestStatusId = requestStatusId;
                    await _taskRequestRepository.UpdateAsync(requestId, taskRequest);
                }else if(requestStatusId == 8 || requestStatusId == 9)
                {
                    taskRequest.RequestStatusId = requestStatusId;
                    taskRequest.CancelReason = updateInfo.CancelReason;
                    await _taskRequestRepository.UpdateAsync(requestId, taskRequest);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateTaskRequest");
                Console.WriteLine("\n\n\n" + ex.Message.ToString() + "\n\n\n");
                // throw new HttpRequestException("Cập nhật trạng thái yêu cầu công việc thất bại: " + ex.Message);
                throw new HttpRequestException("Failed to update task request status: " + ex.Message);
            }
        }
    }
}

