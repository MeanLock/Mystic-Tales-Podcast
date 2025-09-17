using Microsoft.EntityFrameworkCore.Metadata.Internal;
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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Architecture_1.BusinessLogic.Services.DbServices.RequestServices
{
    public class ServiceRequestService
    {
        // LOGGER
        private readonly ILogger<ServiceRequestService> _logger;

        // CONFIG
        public readonly IAppConfig _appConfig;
        public readonly IFilePathConfig _filePathConfig;

        // DB CONTEXT
        private readonly AppDbContext _dbContext;

        // HELPERS
        private readonly FileHelpers _fileHelpers;
        private readonly DateHelpers _dateHelpers;

        // UNIT OF WORK
        private readonly IUnitOfWork _unitOfWork;

        // REPOSITORIES
        private readonly IGenericRepository<ServiceRequest> _serviceRequestRepository;
        private readonly IGenericRepository<Service> _serviceRepository;
        private readonly IGenericRepository<RequestStatus> _requestStatusRepository;

        // AWS SERVICE
        private readonly AWSS3Service _awsS3Service;


        public ServiceRequestService(
            ILogger<ServiceRequestService> logger,
            AppDbContext dbContext,
            FileHelpers fileHelpers,
            IUnitOfWork unitOfWork,
            IFilePathConfig filePathConfig,
            IGenericRepository<ServiceRequest> serviceRequestRepository,
            IGenericRepository<Service> serviceRepository,
            IGenericRepository<RequestStatus> requestStatusRepository,
            DateHelpers dateHelpers,
            AWSS3Service awsS3Service,
            IAppConfig appConfig
            )
        {
            _logger = logger;
            _dbContext = dbContext;
            _fileHelpers = fileHelpers;
            _unitOfWork = unitOfWork;
            _filePathConfig = filePathConfig;
            _serviceRequestRepository = serviceRequestRepository;
            _serviceRepository = serviceRepository;
            _requestStatusRepository = requestStatusRepository;
            _dateHelpers = dateHelpers;
            _awsS3Service = awsS3Service;
            _appConfig = appConfig;
        }

        public string GetDayOfWeek(int day)
        {
            switch (day)
            {
                case 1:
                    return "Sunday";
                case 2:
                    return "Monday";
                case 3:
                    return "Tuesday";
                case 4:
                    return "Wednesday";
                case 5:
                    return "Thursday";
                case 6:
                    return "Friday";
                case 7:
                    return "Saturday";
                default:
                    throw new HttpRequestException("Invalid day of the week: " + day);
            }
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

        public async Task<bool> CheckDateTimeRequestIsValid(int serviceId, string dateRequest, string timeRequest)
        {
            var service = await _unitOfWork.ServiceRepository.FindByIdAsync(serviceId);
            var major = service.FacilityMajor;
            if (major.CloseScheduleDate != null && major.OpenScheduleDate == null && _dateHelpers.IsDateEarlier(dateRequest, major.CloseScheduleDate?.ToString("yyyy-MM-dd")) == false)
            {
                // throw new HttpRequestException("Major sẽ đóng từ ngày " + major.CloseScheduleDate?.ToString("dd/MM/yyyy"));
                throw new HttpRequestException("Failed to create service request, Major id " + major.Id + " will be closed from " + major.CloseScheduleDate?.ToString("dd/MM/yyyy"));
            }
            else if (major.CloseScheduleDate != null && major.OpenScheduleDate != null)
            {

                if (_dateHelpers.IsDateInRange(dateRequest, major.CloseScheduleDate.ToString(), major.OpenScheduleDate?.AddDays(-1).ToString()) == true)
                {
                    // throw new HttpRequestException("Major sẽ đóng từ ngày " + major.CloseScheduleDate?.ToString("dd/MM/yyyy") + " và mở lại vào ngày " + major.OpenScheduleDate?.ToString("dd/MM/yyyy"));
                    throw new HttpRequestException("Failed to create service request, Major id " + major.Id + " will be closed from " + major.CloseScheduleDate?.ToString("dd/MM/yyyy") + " and will be opened again on " + major.OpenScheduleDate?.ToString("dd/MM/yyyy"));
                }
            }

            if (service.CloseScheduleDate != null && service.OpenScheduleDate == null && _dateHelpers.IsDateEarlier(dateRequest, service.CloseScheduleDate?.ToString("yyyy-MM-dd")) == false)
            {
                // throw new HttpRequestException("Dịch vụ sẽ đóng từ ngày " + service.CloseScheduleDate?.ToString("dd/MM/yyyy"));
                throw new HttpRequestException("Failed to create service request, Service id " + service.Id + " will be closed from " + service.CloseScheduleDate?.ToString("dd/MM/yyyy"));
            }
            else if (service.CloseScheduleDate != null && service.OpenScheduleDate != null)
            {
                if (_dateHelpers.IsDateInRange(dateRequest, service.CloseScheduleDate.ToString(), service.OpenScheduleDate?.AddDays(-1).ToString()) == true)
                {
                    // throw new HttpRequestException("Dịch vụ sẽ đóng từ ngày " + service.CloseScheduleDate?.ToString("dd/MM/yyyy") + " và mở lại vào ngày " + service.OpenScheduleDate?.ToString("dd/MM/yyyy"));
                    throw new HttpRequestException("Failed to create service request, Service id " + service.Id + " will be closed from " + service.CloseScheduleDate?.ToString("dd/MM/yyyy") + " and will be opened again on " + service.OpenScheduleDate?.ToString("dd/MM/yyyy"));
                }
            }

            var availabilities = await _unitOfWork.ServiceAvailabilityRepository.FindByServiceId(serviceId);
            bool isBookable = false;
            foreach (var availability in availabilities)
            {
                if (availability.DayOfWeek == _dateHelpers.GetDayOfWeekNumber(dateRequest))
                {

                    string start = availability.StartRequestableTime.ToString();
                    string end = availability.EndRequestableTime.ToString();
                    if (_dateHelpers.IsTimeInRange(timeRequest, start, end))
                    {
                        isBookable = true;
                        break;
                    }
                }
            }
            return isBookable;

        }

        public async Task CancelTimeRequiredServiceRequestByHour()
        {
            var serviceRequests = await _unitOfWork.ServiceRequestRepository.FindByServiceTypeId(2);
            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    foreach (var request in serviceRequests)
                    {
                        DateTime now = DateTime.Now;
                        DateTime? requestDateTime = request.DateRequest?.ToDateTime(request.TimeRequest ?? TimeOnly.MinValue).AddMinutes(-1);

                        if (_dateHelpers.IsDateTimeEarlier(requestDateTime?.ToString(), now.ToString()))
                        {

                            bool IsCancelAutomatically = false;
                            if (request.RequestStatusId == 1)
                            { // Pending
                                request.RequestStatusId = 9;
                                request.IsCancelAutomatically = true;
                                // request.CancelReason = "Yêu cầu không được xử lí";
                                // request.ProgressNote = request.ProgressNote + "\n\n-[*CANCELLED AUTO*] Yêu cầu chưa được duyệt";
                                request.CancelReason = "Request is not processed";
                                request.ProgressNote = request.ProgressNote + "\n\n-[*CANCELLED AUTO*] Request is not approved";
                                IsCancelAutomatically = true;
                            }
                            else if (request.RequestStatusId == 2)
                            { // Assigned
                                request.RequestStatusId = 9;
                                request.IsCancelAutomatically = true;
                                // request.CancelReason = "Yêu cầu không được xử lí";
                                // request.ProgressNote = request.ProgressNote + "\n\n-[*CANCELLED AUTO*] Yêu cầu chưa nhận được phản hồi từ assignee";
                                request.CancelReason = "Request is not processed";
                                request.ProgressNote = request.ProgressNote + "\n\n-[*CANCELLED AUTO*] Request is not responded by assignee";
                                IsCancelAutomatically = true;
                            }
                            else if (request.RequestStatusId == 3)
                            { // RejectedByAssignee
                                request.RequestStatusId = 9;
                                request.IsCancelAutomatically = true;
                                // request.CancelReason = "Yêu cầu không được xử lí";
                                // request.ProgressNote = request.ProgressNote + "\n\n-[*CANCELLED AUTO*] Yêu cầu đã bị từ chối bởi assignee và không được duyệt lại";
                                request.CancelReason = "Request is not processed";
                                request.ProgressNote = request.ProgressNote + "\n\n-[*CANCELLED AUTO*] Request is rejected by assignee and not approved again";
                                IsCancelAutomatically = true;
                            }
                            else if (request.RequestStatusId == 4)
                            { // RejectedByAssigneeDeactivation
                                request.RequestStatusId = 9;
                                request.IsCancelAutomatically = true;
                                // request.CancelReason = "Yêu cầu không được xử lí";
                                // request.ProgressNote = request.ProgressNote + "\n\n-[*CANCELLED AUTO*] Yêu cầu đã bị từ chối bởi vì lí do assignee bị vô hiệu hóa và không được duyệt lại";
                                request.CancelReason = "Request is not processed";
                                request.ProgressNote = request.ProgressNote + "\n\n-[*CANCELLED AUTO*] Request is rejected by assignee deactivation and not approved again";
                                IsCancelAutomatically = true;
                            }
                            else if (request.RequestStatusId == 5)
                            {  // AcceptedByAssignee
                                request.RequestStatusId = 9;
                                request.IsCancelAutomatically = true;
                                // request.CancelReason = "Yêu cầu không được xử lí";
                                // request.ProgressNote = request.ProgressNote + "\n\n-[*CANCELLED AUTO*] Yêu cầu chưa được assignee xử lí xong trong thời gian quy định";
                                request.CancelReason = "Request is not processed";
                                request.ProgressNote = request.ProgressNote + "\n\n-[*CANCELLED AUTO*] Request is not completed by assignee in time";
                                IsCancelAutomatically = true;
                            }
                            else if (request.RequestStatusId == 6)
                            {  // CompletedByAssignee
                                request.RequestStatusId = 9;
                                request.IsCancelAutomatically = true;
                                // request.CancelReason = "Yêu cầu không được xử lí";
                                // request.ProgressNote = request.ProgressNote + "\n\n-[*CANCELLED AUTO*] Yêu cầu chưa được assignee xử lí xong trong thời gian quy định nhưng chưa được duyệt lại";
                                request.CancelReason = "Request is not processed";
                                request.ProgressNote = request.ProgressNote + "\n\n-[*CANCELLED AUTO*] Request is not completed by assignee in time but not approved again";
                                IsCancelAutomatically = true;
                            }

                            if (IsCancelAutomatically == true)
                            {
                                Console.WriteLine("\n[BACKGROUND] Service Request datetime: " + requestDateTime?.ToString("yyyy-MM-dd HH:mm:ss"));
                                Console.WriteLine("Now: " + now.ToString("yyyy-MM-dd HH:mm:ss"));
                                await _serviceRequestRepository.UpdateAsync(request.Id, request);
                                Console.WriteLine("service request id[" + request.Id + "] đã hủy yêu cầu dịch vụ tự động");
                                Console.WriteLine("Lí do: " + request.ProgressNote + "\n\n\n");
                            }

                        }
                    }
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    // throw new HttpRequestException("Lỗi khi hủy yêu cầu dịch vụ: " + ex.Message);
                    throw new HttpRequestException("Failed to cancel service request: " + ex.Message);
                }
            }
        }

        public async Task CancelNonTimeRequiredServiceRequestByHour()
        {
            var service_1_requests = await _unitOfWork.ServiceRequestRepository.FindByServiceTypeId(1);
            var service_3_requests = await _unitOfWork.ServiceRequestRepository.FindByServiceTypeId(3);
            var serviceRequests = service_1_requests.Concat(service_3_requests).ToList();
            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    foreach (var request in serviceRequests)
                    {
                        DateTime now = DateTime.Now.AddMonths(-1);
                        DateTime? requestDateTime = request.CreatedAt;

                        if (_dateHelpers.IsDateTimeEarlier(requestDateTime?.ToString(), now.ToString()))
                        {

                            bool IsCancelAutomatically = false;
                            if (request.RequestStatusId == 1)
                            { // Pending
                                request.RequestStatusId = 9;
                                request.IsCancelAutomatically = true;
                                // request.CancelReason = "Yêu cầu không được xử lí";
                                // request.ProgressNote = request.ProgressNote + "\n\n-[*CANCELLED AUTO*] Yêu cầu chưa được duyệt";
                                request.CancelReason = "Request is not processed";
                                request.ProgressNote = request.ProgressNote + "\n\n-[*CANCELLED AUTO*] Request is not approved";
                                IsCancelAutomatically = true;
                            }
                            else if (request.RequestStatusId == 2)
                            { // Assigned
                                request.RequestStatusId = 9;
                                request.IsCancelAutomatically = true;
                                // request.CancelReason = "Yêu cầu không được xử lí";
                                // request.ProgressNote = request.ProgressNote + "\n\n-[*CANCELLED AUTO*] Yêu cầu chưa nhận được phản hồi từ assignee";
                                request.CancelReason = "Request is not processed";
                                request.ProgressNote = request.ProgressNote + "\n\n-[*CANCELLED AUTO*] Request is not responded by assignee";
                                IsCancelAutomatically = true;
                            }
                            else if (request.RequestStatusId == 3)
                            { // RejectedByAssignee
                                request.RequestStatusId = 9;
                                request.IsCancelAutomatically = true;
                                // request.CancelReason = "Yêu cầu không được xử lí";
                                // request.ProgressNote = request.ProgressNote + "\n\n-[*CANCELLED AUTO*] Yêu cầu đã bị từ chối bởi assignee và không được duyệt lại";
                                request.CancelReason = "Request is not processed";
                                request.ProgressNote = request.ProgressNote + "\n\n-[*CANCELLED AUTO*] Request is rejected by assignee and not approved again";
                                IsCancelAutomatically = true;
                            }
                            else if (request.RequestStatusId == 4)
                            { // RejectedByAssigneeDeactivation
                                request.RequestStatusId = 9;
                                request.IsCancelAutomatically = true;
                                // request.CancelReason = "Yêu cầu không được xử lí";
                                // request.ProgressNote = request.ProgressNote + "\n\n-[*CANCELLED AUTO*] Yêu cầu đã bị từ chối bởi vì lí do assignee bị vô hiệu hóa và không được duyệt lại";
                                request.CancelReason = "Request is not processed";
                                request.ProgressNote = request.ProgressNote + "\n\n-[*CANCELLED AUTO*] Request is rejected by assignee deactivation and not approved again";
                                IsCancelAutomatically = true;
                            }
                            else if (request.RequestStatusId == 5)
                            {  // AcceptedByAssignee
                                request.RequestStatusId = 9;
                                request.IsCancelAutomatically = true;
                                // request.CancelReason = "Yêu cầu không được xử lí";
                                // request.ProgressNote = request.ProgressNote + "\n\n-[*CANCELLED AUTO*] Yêu cầu chưa được assignee xử lí xong trong thời gian quy định";
                                request.CancelReason = "Request is not processed";
                                request.ProgressNote = request.ProgressNote + "\n\n-[*CANCELLED AUTO*] Request is not completed by assignee in time";
                                IsCancelAutomatically = true;
                            }
                            else if (request.RequestStatusId == 6)
                            {  // CompletedByAssignee
                                request.RequestStatusId = 9;
                                request.IsCancelAutomatically = true;
                                // request.CancelReason = "Yêu cầu không được xử lí";
                                // request.ProgressNote = request.ProgressNote + "\n\n-[*CANCELLED AUTO*] Yêu cầu chưa được assignee xử lí xong trong thời gian quy định nhưng chưa được duyệt lại";
                                request.CancelReason = "Request is not processed";
                                request.ProgressNote = request.ProgressNote + "\n\n-[*CANCELLED AUTO*] Request is not completed by assignee in time but not approved again";
                                IsCancelAutomatically = true;
                            }

                            if (IsCancelAutomatically == true)
                            {
                                Console.WriteLine("\n[BACKGROUND] Service Request datetime: " + requestDateTime?.ToString("yyyy-MM-dd HH:mm:ss"));
                                Console.WriteLine("Now: " + now.ToString("yyyy-MM-dd HH:mm:ss"));
                                await _serviceRequestRepository.UpdateAsync(request.Id, request);
                                Console.WriteLine("service request id[" + request.Id + "] đã hủy yêu cầu dịch vụ tự động");
                                Console.WriteLine("Lí do: " + request.ProgressNote + "\n\n\n");
                            }
                        }
                    }
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    // throw new HttpRequestException("Lỗi khi hủy yêu cầu dịch vụ: " + ex.Message);
                    throw new HttpRequestException("Failed to cancel service request: " + ex.Message);
                }
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
        public async Task<JArray> GetMajorHeadServiceRequests(int accountId)
        {
            var account = await _unitOfWork.AccountRepository.FindById(accountId);
            if (account == null)
            {
                // throw new HttpRequestException("Tài khoản không tồn tại");
                throw new HttpRequestException("Account is not exist, id: " + accountId);
            }
            if (account.Role.Id != 2)
            {
                // throw new HttpRequestException("Tài khoản không phải là major head");
                throw new HttpRequestException("Account is not Major Head, id: " + accountId);
            }
            try
            {
                var majorAssignments = await _unitOfWork.AssigneeFacilityMajorAssignmentRepository.FindByAccountId(account.Id);
                var result = new List<object>();
                foreach (var majorAssignment in majorAssignments)
                {
                    var serviceRequests = await _unitOfWork.ServiceRequestRepository.FindByFacilityMajorId(majorAssignment.FacilityMajorId);

                    foreach (var serviceRequest in serviceRequests)
                    {
                        var requester = serviceRequest.Requester;
                        var requestStatus = serviceRequest.RequestStatus;
                        var service = serviceRequest.Service;
                        var major = service.FacilityMajor;
                        result.Add(new
                        {
                            ServiceRequest = new
                            {
                                Id = serviceRequest.Id,
                                ServiceId = serviceRequest.ServiceId,
                                RequesterId = serviceRequest.RequesterId,
                                RequestStatusId = serviceRequest.RequestStatusId,
                                RequestInitDescription = serviceRequest.RequestInitDescription,
                                RequestResultDescription = serviceRequest.RequestResultDescription,
                                AssignedAssigneeId = serviceRequest.AssignedAssigneeId,
                                TimeRequest = serviceRequest.TimeRequest?.ToString("hh:mm"),
                                DateRequest = serviceRequest.DateRequest,
                                IsCancelAutomatically = serviceRequest.IsCancelAutomatically,
                                ProgressNote = serviceRequest.ProgressNote,
                                CancelReason = serviceRequest.CancelReason,
                                CreatedAt = serviceRequest.CreatedAt,
                                UpdatedAt = serviceRequest.UpdatedAt
                            },
                            Requester = new
                            {
                                Id = requester.Id,
                                FullName = requester.FullName,
                                Email = requester.Email,
                                // ImageUrl = await GenerateImageUrl(_filePathConfig.ACCOUNt_IMAGE_PATH, requester.Id.ToString(), "main"),
                                DateOfBirth = requester.DateOfBirth,
                                Phone = requester.Phone,
                                Address = requester.Address,
                                IsDeactivated = requester.IsDeactivated,
                                CreatedAt = requester.CreatedAt
                            },
                            RequestStatus = new
                            {
                                Id = requestStatus.Id,
                                Name = requestStatus.Name
                            },
                            Service = new
                            {
                                Id = service.Id,
                                Name = service.Name,
                                FacilityMajorId = service.FacilityMajorId,
                                IsInitRequestDescriptionRequired = service.IsInitRequestDescriptionRequired,
                                RequestInitHintDescription = service.RequestInitHintDescription,
                                MainDescription = service.MainDescription,
                                WorkShiftsDescription = service.WorkShiftsDescription,
                                IsOpen = service.IsOpen,
                                CloseScheduleDate = service.CloseScheduleDate,
                                OpenScheduleDate = service.OpenScheduleDate,
                                ServiceTypeId = service.ServiceTypeId,
                                // ImageUrl = await GenerateImageUrl(_filePathConfig.SERVICE_IMAGE_PATH, service.Id.ToString(), "main"),
                                IsDeactivated = service.IsDeactivated,
                                CreatedAt = service.CreatedAt
                            },
                            Major = new
                            {
                                Id = major.Id,
                                Name = major.Name,
                                MainDescription = major.MainDescription,
                                WorkShiftsDescription = major.WorkShiftsDescription,
                                FacilityMajorTypeId = major.FacilityMajorTypeId,
                                FacilityId = major.FacilityId,
                                IsOpen = major.IsOpen,
                                CloseScheduleDate = major.CloseScheduleDate,
                                OpenScheduleDate = major.OpenScheduleDate,
                                IsDeactivated = major.IsDeactivated,
                                CreatedAt = major.CreatedAt,
                                // BackgroundImageUrl = await GenerateImageUrl(_filePathConfig.MAJOR_IMAGE_PATH, major.Id.ToString(), "background"),
                                // ImageUrl = await GenerateImageUrl(_filePathConfig.MAJOR_IMAGE_PATH, major.Id.ToString(), "main")
                            }
                        });
                    }
                    return JArray.FromObject(result);
                }

                return JArray.FromObject(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetMajorHeadServiceRequests");
                // throw new HttpRequestException("Lỗi khi lấy danh sách yêu cầu dịch vụ");
                throw new HttpRequestException("Failed to get service request list: " + ex.Message);
            }
        }

        public async Task<JArray> GetMajorServiceRequests(int majorId)
        {
            var major = await _unitOfWork.FacilityMajorRepository.FindByIdAsync(majorId);
            if (major == null)
            {
                // throw new HttpRequestException("Major không tồn tại");
                throw new HttpRequestException("Major is not exist, id: " + majorId);
            }
            try
            {
                var serviceRequests = await _unitOfWork.ServiceRequestRepository.FindByFacilityMajorId(majorId);
                var result = new List<object>();
                foreach (var serviceRequest in serviceRequests)
                {
                    var requester = serviceRequest.Requester;
                    var requestStatus = serviceRequest.RequestStatus;
                    var service = serviceRequest.Service;
                    result.Add(new
                    {
                        ServiceRequest = new
                        {
                            Id = serviceRequest.Id,
                            ServiceId = serviceRequest.ServiceId,
                            RequesterId = serviceRequest.RequesterId,
                            RequestStatusId = serviceRequest.RequestStatusId,
                            RequestInitDescription = serviceRequest.RequestInitDescription,
                            RequestResultDescription = serviceRequest.RequestResultDescription,
                            AssignedAssigneeId = serviceRequest.AssignedAssigneeId,
                            TimeRequest = serviceRequest.TimeRequest?.ToString("hh:mm"),
                            DateRequest = serviceRequest.DateRequest,
                            IsCancelAutomatically = serviceRequest.IsCancelAutomatically,
                            ProgressNote = serviceRequest.ProgressNote,
                            CancelReason = serviceRequest.CancelReason,
                            CreatedAt = serviceRequest.CreatedAt,
                            UpdatedAt = serviceRequest.UpdatedAt
                        },
                        Requester = new
                        {
                            Id = requester.Id,
                            FullName = requester.FullName,
                            Email = requester.Email,
                            // ImageUrl = await GenerateImageUrl(_filePathConfig.ACCOUNt_IMAGE_PATH, requester.Id.ToString(), "main"),
                            DateOfBirth = requester.DateOfBirth,
                            Phone = requester.Phone,
                            Address = requester.Address,
                            IsDeactivated = requester.IsDeactivated,
                            CreatedAt = requester.CreatedAt
                        },
                        RequestStatus = new
                        {
                            Id = requestStatus.Id,
                            Name = requestStatus.Name
                        },
                        Service = new
                        {
                            Id = service.Id,
                            Name = service.Name,
                            FacilityMajorId = service.FacilityMajorId,
                            IsInitRequestDescriptionRequired = service.IsInitRequestDescriptionRequired,
                            RequestInitHintDescription = service.RequestInitHintDescription,
                            MainDescription = service.MainDescription,
                            WorkShiftsDescription = service.WorkShiftsDescription,
                            IsOpen = service.IsOpen,
                            CloseScheduleDate = service.CloseScheduleDate,
                            OpenScheduleDate = service.OpenScheduleDate,
                            ServiceTypeId = service.ServiceTypeId,
                            // ImageUrl = await GenerateImageUrl(_filePathConfig.SERVICE_IMAGE_PATH, service.Id.ToString(), "main"),
                            IsDeactivated = service.IsDeactivated,
                            CreatedAt = service.CreatedAt
                        },
                        Major = new
                        {
                            Id = major.Id,
                            Name = major.Name,
                            MainDescription = major.MainDescription,
                            WorkShiftsDescription = major.WorkShiftsDescription,
                            FacilityMajorTypeId = major.FacilityMajorTypeId,
                            FacilityId = major.FacilityId,
                            IsOpen = major.IsOpen,
                            CloseScheduleDate = major.CloseScheduleDate,
                            OpenScheduleDate = major.OpenScheduleDate,
                            IsDeactivated = major.IsDeactivated,
                            CreatedAt = major.CreatedAt,
                            // BackgroundImageUrl = await GenerateImageUrl(_filePathConfig.MAJOR_IMAGE_PATH, major.Id.ToString(), "background"),
                            // ImageUrl = await GenerateImageUrl(_filePathConfig.MAJOR_IMAGE_PATH, major.Id.ToString(), "main")
                        }
                    });
                }
                return JArray.FromObject(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetMajorServiceRequests");
                Console.WriteLine("\n\n\n" + ex.Message.ToString() + "\n\n\n");
                // throw new HttpRequestException("Lỗi khi lấy danh sách yêu cầu dịch vụ: " + ex.Message);
                throw new HttpRequestException("Failed to get service request list: " + ex.Message);
            }
        }

        public async Task<JArray> GetAssigneeServiceRequests(int accountId)
        {
            var account = await _unitOfWork.AccountRepository.FindById(accountId);
            if (account == null)
            {
                // throw new HttpRequestException("Tài khoản không tồn tại");
                throw new HttpRequestException("Account is not exist, id: " + accountId);
            }
            if (account.Role.Id != 3)
            {
                // throw new HttpRequestException("Tài khoản không phải là assignee");
                throw new HttpRequestException("Account is not Assignee, id: " + accountId);
            }

            try
            {
                var serviceRequests = await _unitOfWork.ServiceRequestRepository.FindByAssignedAssigneeId(accountId);
                var result = new List<object>();
                foreach (var serviceRequest in serviceRequests)
                {
                    var requester = serviceRequest.Requester;
                    var requestStatus = serviceRequest.RequestStatus;
                    var service = serviceRequest.Service;
                    var major = service.FacilityMajor;
                    result.Add(new
                    {
                        ServiceRequest = new
                        {
                            Id = serviceRequest.Id,
                            ServiceId = serviceRequest.ServiceId,
                            RequesterId = serviceRequest.RequesterId,
                            RequestStatusId = serviceRequest.RequestStatusId,
                            RequestInitDescription = serviceRequest.RequestInitDescription,
                            RequestResultDescription = serviceRequest.RequestResultDescription,
                            AssignedAssigneeId = serviceRequest.AssignedAssigneeId,
                            TimeRequest = serviceRequest.TimeRequest?.ToString("hh:mm"),
                            DateRequest = serviceRequest.DateRequest,
                            IsCancelAutomatically = serviceRequest.IsCancelAutomatically,
                            ProgressNote = serviceRequest.ProgressNote,
                            CancelReason = serviceRequest.CancelReason,
                            CreatedAt = serviceRequest.CreatedAt,
                            UpdatedAt = serviceRequest.UpdatedAt
                        },
                        Requester = new
                        {
                            Id = requester.Id,
                            FullName = requester.FullName,
                            Email = requester.Email,
                            ImageUrl = await GenerateImageUrl(_filePathConfig.ACCOUNt_IMAGE_PATH, requester.Id.ToString(), "main"),
                            DateOfBirth = requester.DateOfBirth,
                            Phone = requester.Phone,
                            Address = requester.Address,
                            IsDeactivated = requester.IsDeactivated,
                            CreatedAt = requester.CreatedAt
                        },
                        RequestStatus = new
                        {
                            Id = requestStatus.Id,
                            Name = requestStatus.Name
                        },
                        Service = new
                        {
                            Id = service.Id,
                            Name = service.Name,
                            FacilityMajorId = service.FacilityMajorId,
                            IsInitRequestDescriptionRequired = service.IsInitRequestDescriptionRequired,
                            RequestInitHintDescription = service.RequestInitHintDescription,
                            MainDescription = service.MainDescription,
                            WorkShiftsDescription = service.WorkShiftsDescription,
                            IsOpen = service.IsOpen,
                            CloseScheduleDate = service.CloseScheduleDate,
                            OpenScheduleDate = service.OpenScheduleDate,
                            ServiceTypeId = service.ServiceTypeId,
                            ImageUrl = await GenerateImageUrl(_filePathConfig.SERVICE_IMAGE_PATH, service.Id.ToString(), "main"),
                            IsDeactivated = service.IsDeactivated,
                            CreatedAt = service.CreatedAt
                        },
                        Major = new
                        {
                            Id = major.Id,
                            Name = major.Name,
                            MainDescription = major.MainDescription,
                            WorkShiftsDescription = major.WorkShiftsDescription,
                            FacilityMajorTypeId = major.FacilityMajorTypeId,
                            FacilityId = major.FacilityId,
                            IsOpen = major.IsOpen,
                            CloseScheduleDate = major.CloseScheduleDate,
                            OpenScheduleDate = major.OpenScheduleDate,
                            IsDeactivated = major.IsDeactivated,
                            CreatedAt = major.CreatedAt,
                            BackgroundImageUrl = await GenerateImageUrl(_filePathConfig.MAJOR_IMAGE_PATH, major.Id.ToString(), "background"),
                            ImageUrl = await GenerateImageUrl(_filePathConfig.MAJOR_IMAGE_PATH, major.Id.ToString(), "main")
                        }
                    });
                }
                return JArray.FromObject(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAssigneeServiceRequests");
                Console.WriteLine("\n\n\n" + ex.Message.ToString() + "\n\n\n");
                // throw new HttpRequestException("Lỗi khi lấy danh sách yêu cầu dịch vụ: " + ex.Message);
                throw new HttpRequestException("Failed to get service request list: " + ex.Message);
            }
        }

        public async Task<JArray> GetAssigneeMajorServiceRequests(int accountId, int majorId)
        {
            var account = await _unitOfWork.AccountRepository.FindById(accountId);
            if (account == null)
            {
                // throw new HttpRequestException("Tài khoản không tồn tại");
                throw new HttpRequestException("Account is not exist, id: " + accountId);
            }
            if (account.Role.Id != 3)
            {
                // throw new HttpRequestException("Tài khoản không phải là assignee");
                throw new HttpRequestException("Account is not Assignee, id: " + accountId);
            }
            var majorExist = await _unitOfWork.FacilityMajorRepository.FindByIdAsync(majorId);
            if (majorExist == null)
            {
                // throw new HttpRequestException("Major không tồn tại");
                throw new HttpRequestException("Major is not exist, id: " + majorId);
            }
            try
            {
                var serviceRequests = await _unitOfWork.ServiceRequestRepository.FindByAssignedAssigneeIdAndFacilityMajorId(accountId, majorId);
                var result = new List<object>();
                foreach (var serviceRequest in serviceRequests)
                {
                    var requester = serviceRequest.Requester;
                    var requestStatus = serviceRequest.RequestStatus;
                    var service = serviceRequest.Service;
                    var major = service.FacilityMajor;
                    result.Add(new
                    {
                        ServiceRequest = new
                        {
                            Id = serviceRequest.Id,
                            ServiceId = serviceRequest.ServiceId,
                            RequesterId = serviceRequest.RequesterId,
                            RequestStatusId = serviceRequest.RequestStatusId,
                            RequestInitDescription = serviceRequest.RequestInitDescription,
                            RequestResultDescription = serviceRequest.RequestResultDescription,
                            AssignedAssigneeId = serviceRequest.AssignedAssigneeId,
                            TimeRequest = serviceRequest.TimeRequest?.ToString("hh:mm"),
                            DateRequest = serviceRequest.DateRequest,
                            IsCancelAutomatically = serviceRequest.IsCancelAutomatically,
                            ProgressNote = serviceRequest.ProgressNote,
                            CancelReason = serviceRequest.CancelReason,
                            CreatedAt = serviceRequest.CreatedAt,
                            UpdatedAt = serviceRequest.UpdatedAt
                        },
                        Requester = new
                        {
                            Id = requester.Id,
                            FullName = requester.FullName,
                            Email = requester.Email,
                            // ImageUrl = await GenerateImageUrl(_filePathConfig.ACCOUNt_IMAGE_PATH, requester.Id.ToString(), "main"),
                            DateOfBirth = requester.DateOfBirth,
                            Phone = requester.Phone,
                            Address = requester.Address,
                            IsDeactivated = requester.IsDeactivated,
                            CreatedAt = requester.CreatedAt
                        },
                        RequestStatus = new
                        {
                            Id = requestStatus.Id,
                            Name = requestStatus.Name
                        },
                        Service = new
                        {
                            Id = service.Id,
                            Name = service.Name,
                            FacilityMajorId = service.FacilityMajorId,
                            IsInitRequestDescriptionRequired = service.IsInitRequestDescriptionRequired,
                            RequestInitHintDescription = service.RequestInitHintDescription,
                            MainDescription = service.MainDescription,
                            WorkShiftsDescription = service.WorkShiftsDescription,
                            IsOpen = service.IsOpen,
                            CloseScheduleDate = service.CloseScheduleDate,
                            OpenScheduleDate = service.OpenScheduleDate,
                            ServiceTypeId = service.ServiceTypeId,
                            // ImageUrl = await GenerateImageUrl(_filePathConfig.SERVICE_IMAGE_PATH, service.Id.ToString(), "main"),
                            IsDeactivated = service.IsDeactivated,
                            CreatedAt = service.CreatedAt
                        },
                        Major = new
                        {
                            Id = major.Id,
                            Name = major.Name,
                            MainDescription = major.MainDescription,
                            WorkShiftsDescription = major.WorkShiftsDescription,
                            FacilityMajorTypeId = major.FacilityMajorTypeId,
                            FacilityId = major.FacilityId,
                            IsOpen = major.IsOpen,
                            CloseScheduleDate = major.CloseScheduleDate,
                            OpenScheduleDate = major.OpenScheduleDate,
                            IsDeactivated = major.IsDeactivated,
                            CreatedAt = major.CreatedAt,
                            // BackgroundImageUrl = await GenerateImageUrl(_filePathConfig.MAJOR_IMAGE_PATH, major.Id.ToString(), "background"),
                            // ImageUrl = await GenerateImageUrl(_filePathConfig.MAJOR_IMAGE_PATH, major.Id.ToString(), "main")
                        }

                    });
                }
                return JArray.FromObject(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAssigneeMajorServiceRequests");
                Console.WriteLine("\n\n\n" + ex.Message.ToString() + "\n\n\n");
                // throw new HttpRequestException("Lỗi khi lấy danh sách yêu cầu dịch vụ: " + ex.Message);
                throw new HttpRequestException("Failed to get service request list: " + ex.Message);
            }

        }

        public async Task<JArray> GetRequesterServiceRequests(int accountId)
        {
            var account = await _unitOfWork.AccountRepository.FindById(accountId);
            if (account == null)
            {
                // throw new HttpRequestException("Tài khoản không tồn tại");
                throw new HttpRequestException("Account is not exist, id: " + accountId);
            }
            try
            {
                var serviceRequests = await _unitOfWork.ServiceRequestRepository.FindByRequesterId(accountId);
                var result = new List<object>();
                foreach (var serviceRequest in serviceRequests)
                {
                    var requester = serviceRequest.Requester;
                    var requestStatus = serviceRequest.RequestStatus;
                    var service = serviceRequest.Service;
                    var major = service.FacilityMajor;
                    result.Add(new
                    {
                        ServiceRequest = new
                        {
                            Id = serviceRequest.Id,
                            ServiceId = serviceRequest.ServiceId,
                            RequesterId = serviceRequest.RequesterId,
                            RequestStatusId = serviceRequest.RequestStatusId,
                            RequestInitDescription = serviceRequest.RequestInitDescription,
                            RequestResultDescription = serviceRequest.RequestResultDescription,
                            AssignedAssigneeId = serviceRequest.AssignedAssigneeId,
                            TimeRequest = serviceRequest.TimeRequest?.ToString("hh:mm"),
                            DateRequest = serviceRequest.DateRequest,
                            IsCancelAutomatically = serviceRequest.IsCancelAutomatically,
                            ProgressNote = serviceRequest.ProgressNote,
                            CancelReason = serviceRequest.CancelReason,
                            CreatedAt = serviceRequest.CreatedAt,
                            UpdatedAt = serviceRequest.UpdatedAt
                        },
                        Requester = new
                        {
                            Id = requester.Id,
                            FullName = requester.FullName,
                            Email = requester.Email,
                            // ImageUrl = await GenerateImageUrl(_filePathConfig.ACCOUNt_IMAGE_PATH, requester.Id.ToString(), "main"),
                            DateOfBirth = requester.DateOfBirth,
                            Phone = requester.Phone,
                            Address = requester.Address,
                            IsDeactivated = requester.IsDeactivated,
                            CreatedAt = requester.CreatedAt
                        },
                        RequestStatus = new
                        {
                            Id = requestStatus.Id,
                            Name = requestStatus.Name
                        },
                        Service = new
                        {
                            Id = service.Id,
                            Name = service.Name,
                            FacilityMajorId = service.FacilityMajorId,
                            IsInitRequestDescriptionRequired = service.IsInitRequestDescriptionRequired,
                            RequestInitHintDescription = service.RequestInitHintDescription,
                            MainDescription = service.MainDescription,
                            WorkShiftsDescription = service.WorkShiftsDescription,
                            IsOpen = service.IsOpen,
                            CloseScheduleDate = service.CloseScheduleDate,
                            OpenScheduleDate = service.OpenScheduleDate,
                            ServiceTypeId = service.ServiceTypeId,
                            // ImageUrl = await GenerateImageUrl(_filePathConfig.SERVICE_IMAGE_PATH, service.Id.ToString(), "main"),
                            IsDeactivated = service.IsDeactivated,
                            CreatedAt = service.CreatedAt
                        },
                        Major = new
                        {
                            Id = major.Id,
                            Name = major.Name,
                            MainDescription = major.MainDescription,
                            WorkShiftsDescription = major.WorkShiftsDescription,
                            FacilityMajorTypeId = major.FacilityMajorTypeId,
                            FacilityId = major.FacilityId,
                            IsOpen = major.IsOpen,
                            CloseScheduleDate = major.CloseScheduleDate,
                            OpenScheduleDate = major.OpenScheduleDate,
                            IsDeactivated = major.IsDeactivated,
                            CreatedAt = major.CreatedAt,
                            // BackgroundImageUrl = await GenerateImageUrl(_filePathConfig.MAJOR_IMAGE_PATH, major.Id.ToString(), "background"),
                            // ImageUrl = await GenerateImageUrl(_filePathConfig.MAJOR_IMAGE_PATH, major.Id.ToString(), "main")
                        }
                    });

                }
                return JArray.FromObject(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetRequesterMajorServiceRequests");
                Console.WriteLine("\n\n\n" + ex.Message.ToString() + "\n\n\n");
                // throw new HttpRequestException("Lấy danh sách yêu cầu dịch vụ thất bại: " + ex.Message);
                throw new HttpRequestException("Failed to get service request list: " + ex.Message);
            }
        }

        public async Task<JObject> GetServiceRequestDetail(int requestId)
        {
            var serviceRequest = await _unitOfWork.ServiceRequestRepository.FindByIdAsync(requestId);
            if (serviceRequest == null)
            {
                // throw new HttpRequestException("Yêu cầu dịch vụ không tồn tại");
                throw new HttpRequestException("Service request is not exist, id: " + requestId);
            }
            try
            {
                var requester = serviceRequest.Requester;
                var requestStatus = serviceRequest.RequestStatus;
                var service = serviceRequest.Service;
                var major = service.FacilityMajor;
                return JObject.FromObject(new
                {
                    ServiceRequest = new
                    {
                        Id = serviceRequest.Id,
                        ServiceId = serviceRequest.ServiceId,
                        RequesterId = serviceRequest.RequesterId,
                        RequestStatusId = serviceRequest.RequestStatusId,
                        RequestInitDescription = serviceRequest.RequestInitDescription,
                        RequestResultDescription = serviceRequest.RequestResultDescription,
                        AssignedAssigneeId = serviceRequest.AssignedAssigneeId,
                        TimeRequest = serviceRequest.TimeRequest?.ToString("hh:mm"),
                        DateRequest = serviceRequest.DateRequest,
                        IsCancelAutomatically = serviceRequest.IsCancelAutomatically,
                        ProgressNote = serviceRequest.ProgressNote,
                        CancelReason = serviceRequest.CancelReason,
                        CreatedAt = serviceRequest.CreatedAt,
                        UpdatedAt = serviceRequest.UpdatedAt
                    },
                    Requester = new
                    {
                        Id = requester.Id,
                        FullName = requester.FullName,
                        Email = requester.Email,
                        ImageUrl = await GenerateImageUrl(_filePathConfig.ACCOUNt_IMAGE_PATH, requester.Id.ToString(), "main"),
                        DateOfBirth = requester.DateOfBirth,
                        Phone = requester.Phone,
                        Address = requester.Address,
                        IsDeactivated = requester.IsDeactivated,
                        CreatedAt = requester.CreatedAt
                    },
                    RequestStatus = new
                    {
                        Id = requestStatus.Id,
                        Name = requestStatus.Name
                    },
                    Service = new
                    {
                        Id = service.Id,
                        Name = service.Name,
                        FacilityMajorId = service.FacilityMajorId,
                        IsInitRequestDescriptionRequired = service.IsInitRequestDescriptionRequired,
                        RequestInitHintDescription = service.RequestInitHintDescription,
                        MainDescription = service.MainDescription,
                        WorkShiftsDescription = service.WorkShiftsDescription,
                        IsOpen = service.IsOpen,
                        CloseScheduleDate = service.CloseScheduleDate,
                        OpenScheduleDate = service.OpenScheduleDate,
                        ServiceTypeId = service.ServiceTypeId,
                        ImageUrl = await GenerateImageUrl(_filePathConfig.SERVICE_IMAGE_PATH, service.Id.ToString(), "main"),
                        IsDeactivated = service.IsDeactivated,
                        CreatedAt = service.CreatedAt
                    },
                    Major = new
                    {
                        Id = major.Id,
                        Name = major.Name,
                        MainDescription = major.MainDescription,
                        WorkShiftsDescription = major.WorkShiftsDescription,
                        FacilityMajorTypeId = major.FacilityMajorTypeId,
                        FacilityId = major.FacilityId,
                        IsOpen = major.IsOpen,
                        CloseScheduleDate = major.CloseScheduleDate,
                        OpenScheduleDate = major.OpenScheduleDate,
                        IsDeactivated = major.IsDeactivated,
                        CreatedAt = major.CreatedAt,
                        BackgroundImageUrl = await GenerateImageUrl(_filePathConfig.MAJOR_IMAGE_PATH, major.Id.ToString(), "background"),
                        ImageUrl = await GenerateImageUrl(_filePathConfig.MAJOR_IMAGE_PATH, major.Id.ToString(), "main")
                    }
                }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetServiceRequestDetail");
                Console.WriteLine("\n\n\n" + ex.Message.ToString() + "\n\n\n");
                // throw new HttpRequestException("Lỗi khi lấy chi tiết yêu cầu dịch vụ: " + ex.Message);
                throw new HttpRequestException("Failed to get service request detail: " + ex.Message);
            }
        }

        public async Task UpdateServiceRequest(int requestId, string action, dynamic updateInformations)
        {
            var serviceRequest = await _unitOfWork.ServiceRequestRepository.FindByIdAsync(requestId);
            if (serviceRequest == null)
            {
                // throw new HttpRequestException("Yêu cầu dịch vụ không tồn tại");
                throw new HttpRequestException("Service request is not exist, id: " + requestId);
            }
            try
            {


                switch (action)
                {
                    case "Assigned":
                        Account assignee = await _unitOfWork.AccountRepository.FindById(int.Parse(updateInformations.AssigneeId.ToString()));
                        if (assignee == null)
                        {
                            // throw new HttpRequestException("Assignee không tồn tại");
                            throw new HttpRequestException("Assignee is not exist, id: " + updateInformations.AssigneeId.ToString());
                        }
                        if (assignee.IsDeactivated == true)
                        {
                            // throw new HttpRequestException("Assignee đã bị xoá");
                            throw new HttpRequestException("Assignee is deactivated, id: " + updateInformations.AssigneeId.ToString());
                        }

                        AssigneeFacilityMajorAssignment assigneeFacilityMajorAssignment = assignee.AssigneeFacilityMajorAssignments.FirstOrDefault(afma => afma.FacilityMajorId == serviceRequest.Service.FacilityMajorId);
                        if (assigneeFacilityMajorAssignment == null)
                        {
                            // throw new HttpRequestException("Assignee không được phân công cho major này");
                            throw new HttpRequestException("Assignee is not assigned to this major, id: " + updateInformations.AssigneeId.ToString());
                        }
                        if (assigneeFacilityMajorAssignment.IsHead == true)
                        {
                            // throw new HttpRequestException("Assignee không thể được phân công trong vai trò head");
                            throw new HttpRequestException("Assignee is head, id: " + updateInformations.AssigneeId.ToString());
                        }
                        if (string.IsNullOrEmpty(assigneeFacilityMajorAssignment.WorkDescription))
                        {
                            // throw new HttpRequestException("Assignee chưa có mô tả công việc");
                            throw new HttpRequestException("Assignee has no work description, id: " + updateInformations.AssigneeId.ToString());
                        }


                        serviceRequest.AssignedAssigneeId = updateInformations.AssigneeId;
                        serviceRequest.ProgressNote = updateInformations.ProgressNote;
                        serviceRequest.RequestStatusId = 2;

                        //1. xoá request result description
                        serviceRequest.RequestResultDescription = null;



                        break;
                    case "RejectedByAssignee":
                        serviceRequest.ProgressNote = updateInformations.ProgressNote;
                        serviceRequest.RequestStatusId = 3;
                        break;
                    case "AcceptedByAssignee":
                        serviceRequest.ProgressNote = updateInformations.ProgressNote;
                        serviceRequest.RequestStatusId = 5;
                        break;
                    case "CompletedByAssignee":
                        serviceRequest.RequestResultDescription = updateInformations.RequestResultDescription;
                        serviceRequest.ProgressNote = updateInformations.ProgressNote;
                        serviceRequest.RequestStatusId = 6;
                        break;
                    case "Finished":
                        serviceRequest.RequestResultDescription = updateInformations.RequestResultDescription;
                        serviceRequest.ProgressNote = updateInformations.ProgressNote;
                        serviceRequest.RequestStatusId = 7;
                        break;
                    case "Cancelled":
                        serviceRequest.ProgressNote = updateInformations.ProgressNote;
                        serviceRequest.CancelReason = updateInformations.CancelReason;
                        serviceRequest.RequestStatusId = 8;
                        break;
                    default:
                        // throw new HttpRequestException("Hành động không hợp lệ");
                        throw new HttpRequestException("Invalid action: " + action);
                }
                await _serviceRequestRepository.UpdateAsync(requestId, serviceRequest);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateServiceRequest");
                Console.WriteLine("\n\n\n" + ex.Message.ToString() + "\n\n\n");
                // throw new HttpRequestException("Lỗi khi cập nhật yêu cầu dịch vụ: " + ex.Message);
                throw new HttpRequestException("Failed to update service request: " + ex.Message);
            }
        }

        public async Task<JArray> GetAssignableAssignee(int majorId)
        {
            var major = await _unitOfWork.FacilityMajorRepository.FindByIdAsync(majorId);
            if (major == null)
            {
                // throw new HttpRequestException("Major không tồn tại");
                throw new HttpRequestException("Major is not exist, id: " + majorId);
            }
            try
            {
                var assignees = (await _unitOfWork.AssigneeFacilityMajorAssignmentRepository.FindAssignableAssigneeByMajorId(majorId)).Where(afma => afma.Account.IsDeactivated == false).ToList();
                var result = new List<object>();
                foreach (var assignee in assignees)
                {

                    result.Add(new
                    {
                        Account = new
                        {
                            Id = assignee.Account.Id,
                            FullName = assignee.Account.FullName,
                            Email = assignee.Account.Email,
                            ImageUrl = await GenerateImageUrl(_filePathConfig.ACCOUNt_IMAGE_PATH, assignee.Account.Id.ToString(), "main"),
                            DateOfBirth = assignee.Account.DateOfBirth,
                            Phone = assignee.Account.Phone,
                            Address = assignee.Account.Address,
                            RoleId = assignee.Account.RoleId,
                            JobTypeId = assignee.Account.JobTypeId,
                            IsDeactivated = assignee.Account.IsDeactivated,
                            CreatedAt = assignee.Account.CreatedAt
                        },
                        Role = new
                        {
                            Id = assignee.Account.Role.Id,
                            Name = assignee.Account.Role.Name
                        },
                        JobType = new
                        {
                            Id = assignee.Account.JobType.Id,
                            Name = assignee.Account.JobType.Name
                        }
                    });
                }
                return JArray.FromObject(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAssignableAssignee");
                Console.WriteLine("\n\n\n" + ex.Message.ToString() + "\n\n\n");
                // throw new HttpRequestException("Lỗi khi lấy danh sách assignee: " + ex.Message);
                throw new HttpRequestException("Failed to get assignable assignee list: " + ex.Message);
            }
        }

        public async Task CreateServiceRequest(dynamic serviceRequest)
        {
            var existRequester = await _unitOfWork.AccountRepository.FindById(int.Parse(serviceRequest.RequesterId.ToString()));
            if (existRequester == null)
            {
                // throw new HttpRequestException("Tài khoản không tồn tại");
                throw new HttpRequestException("Account is not exist, id: " + serviceRequest.RequesterId.ToString());
            }
            Service existService = await _serviceRepository.FindByIdAsync(int.Parse(serviceRequest.ServiceId.ToString()));
            if (existService == null)
            {
                // throw new HttpRequestException("Dịch vụ không tồn tại");
                throw new HttpRequestException("Service is not exist, id: " + serviceRequest.ServiceId.ToString());
            }

            if (existService.ServiceTypeId == 2)
            {
                if (!_dateHelpers.IsValidTime(serviceRequest.TimeRequest.ToString()))
                {
                    // throw new HttpRequestException("Thời gian không hợp lệ");
                    throw new HttpRequestException("Time is not valid: " + serviceRequest.TimeRequest.ToString());
                }
                if (!_dateHelpers.IsValidDate(serviceRequest.DateRequest.ToString()))
                {
                    // throw new HttpRequestException("Ngày không hợp lệ");
                    throw new HttpRequestException("Date is not valid: " + serviceRequest.DateRequest.ToString());
                }
                // Kiểm tra thời gian đặt hợp lệ
                bool isBookable = await CheckDateTimeRequestIsValid(int.Parse(serviceRequest.ServiceId.ToString()), serviceRequest.DateRequest.ToString(), serviceRequest.TimeRequest.ToString());
                if (isBookable == false)
                {
                    // throw new HttpRequestException("Không thể đặt dịch vụ vào thời gian này do không nằm trong khung giờ mở cửa của dịch vụ");
                    throw new HttpRequestException("Failed to book service at this time, because it is not in the opening hours of the service: " + serviceRequest.ServiceId.ToString());
                }
            }
            else if (existService.ServiceTypeId == 3) // (***CHƯA LÀM: LOẠI SERVICE CONTACT , NHẬN VÀO THÔNG TIN CONTACT, LẤY CÁC THÔNG TIN NÀY FORMAT LẠI THÀNH ĐOẠN TEXT (MÔ TẢ THÔNG TIN CONTACT) VÀ LƯU VÀO FEILD REQUESTINITDESCRIPTION)
            {
                serviceRequest.RequestInitDescription = serviceRequest.RequestInitDescription;
            }




            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var newServiceRequest = new ServiceRequest
                    {
                        ServiceId = serviceRequest.ServiceId,
                        RequesterId = serviceRequest.RequesterId,
                        RequestStatusId = 1,
                        RequestInitDescription = serviceRequest.RequestInitDescription,
                        ProgressNote = "",
                        TimeRequest = existService.ServiceTypeId == 2 ? TimeOnly.Parse(serviceRequest.TimeRequest.ToString()) : null,
                        DateRequest = existService.ServiceTypeId == 2 ? DateOnly.Parse(serviceRequest.DateRequest.ToString()) : null,
                    };
                    await _serviceRequestRepository.CreateAsync(newServiceRequest);

                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error in CreateServiceRequest");
                    // throw new HttpRequestException("Tạo yêu cầu dịch vụ thất bại");
                    throw new HttpRequestException("Failed to create service request: " + ex.Message);
                }
            }

        }

        public async Task<JArray> GetRequestStatuses()
        {
            try
            {
                var requestStatuses = await _requestStatusRepository.FindAllAsync();
                var result = new List<object>();
                foreach (var requestStatus in requestStatuses)
                {
                    result.Add(new
                    {
                        Id = requestStatus.Id,
                        Name = requestStatus.Name,
                    });
                }
                return JArray.FromObject(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetRequestStatuses");
                Console.WriteLine("\n\n\n" + ex.Message.ToString() + "\n\n\n");
                // throw new HttpRequestException("Lỗi khi lấy danh sách trạng thái yêu cầu: " + ex.Message);
                throw new HttpRequestException("Failed to get service request status list: " + ex.Message);
            }
        }
    }
}