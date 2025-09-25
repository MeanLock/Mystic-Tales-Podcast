using System.Globalization;
using HotChocolate.Data;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Architecture_1.BusinessLogic.Helpers;
using Architecture_1.Infrastructure.Services.AWS.S3;
using Architecture_1.Common.AppConfigurations.App.interfaces;
using Architecture_1.Common.AppConfigurations.FileStorage.interfaces;
using Architecture_1.Common.AppConfigurations.Jwt;
using Architecture_1.DataAccess.Data;
using Architecture_1.DataAccess.Entities;
using Architecture_1.DataAccess.Repositories.interfaces;
using Architecture_1.DataAccess.UOW;

namespace Architecture_1.BusinessLogic.Services.DbServices.FacilityMajorServices
{
    public class MajorServicesService
    {
        // LOGGER
        private readonly ILogger<MajorServicesService> _logger;

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
        public readonly IGenericRepository<Service> _serviceRepository;
        public readonly IGenericRepository<ServiceType> _serviceTypeRepository;
        public readonly IGenericRepository<ServiceAvailability> _serviceAvailabilityRepository;
        public readonly IGenericRepository<ServiceRequest> _serviceRequestRepository;

        // AWS SERVICE
        private readonly AWSS3Service _awsS3Service;

        // RECORDS
        public record DateRange
        {
            public DateOnly? Start { get; set; }
            public DateOnly? End { get; set; }
        }

        public MajorServicesService(
            ILogger<MajorServicesService> logger,
            AppDbContext dbContext,
            FileHelpers fileHelpers,
            IUnitOfWork unitOfWork,
            IFilePathConfig filePathConfig,
            IGenericRepository<Service> serviceRepository,
            IGenericRepository<ServiceType> serviceTypeRepository,
            IGenericRepository<ServiceAvailability> serviceAvailabilityRepository,
            DateHelpers dateHelpers,
            IGenericRepository<ServiceRequest> serviceRequestRepository,
            AWSS3Service awsS3Service,
            IAppConfig appConfig
            )
        {
            _logger = logger;
            _dbContext = dbContext;
            _fileHelpers = fileHelpers;
            _unitOfWork = unitOfWork;
            _filePathConfig = filePathConfig;
            _serviceRepository = serviceRepository;
            _serviceTypeRepository = serviceTypeRepository;
            _serviceAvailabilityRepository = serviceAvailabilityRepository;
            _dateHelpers = dateHelpers;
            _serviceRequestRepository = serviceRequestRepository;
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
                    throw new HttpRequestException("Invalid day of week: " + day);
            }
        }

        public List<string> GetValidDates(int dateLimit, DateOnly? closeDate, DateOnly? openDate, List<int> validDays)
        {
            var result = new List<string>();
            var date = DateOnly.FromDateTime(DateTime.Today.AddDays(1));

            if (closeDate == null)
            {
                while (result.Count < dateLimit)
                {
                    int dayOfWeek = ((int)date.DayOfWeek) + 1;

                    if (validDays.Contains(dayOfWeek))
                    {
                        result.Add(date.ToString("yyyy-MM-dd"));
                    }

                    date = date.AddDays(1);
                }
                return result;
            }

            if (closeDate != null && openDate == null)
            {
                while (date < closeDate)
                {
                    int dayOfWeek = ((int)date.DayOfWeek) + 1;

                    if (validDays.Contains(dayOfWeek))
                    {
                        result.Add(date.ToString("yyyy-MM-dd"));
                    }

                    date = date.AddDays(1);
                }
                return result;
            }
            while (result.Count < dateLimit)
            {
                int dayOfWeek = ((int)date.DayOfWeek) + 1;

                if (validDays.Contains(dayOfWeek))
                {
                    if (date >= openDate || date < closeDate)
                    {
                        result.Add(date.ToString("yyyy-MM-dd"));
                    }
                }

                date = date.AddDays(1);
            }

            return result;
        }

        public List<string> GetTimeListByDateOfWeek(IEnumerable<ServiceAvailability> serviceAvailabilities, int dayOfWeek)
        {
            var result = new List<string>();
            var availabilities = serviceAvailabilities.Where(x => x.DayOfWeek == dayOfWeek).ToList();
            foreach (var availability in availabilities)
            {
                var startTime = availability.StartRequestableTime.ToString("HH:mm");
                var endTime = availability.EndRequestableTime.ToString("HH:mm");

                if (_dateHelpers.IsTimeEarlier(startTime, endTime) == false)
                {
                    // throw new HttpRequestException("Thời gian đã đặt trong dịch vụ không hợp lệ: " + startTime + " - " + endTime);
                    Console.WriteLine("Invalid time range: " + startTime + " - " + endTime);
                }

                var startDateTime = DateTime.ParseExact(startTime, "HH:mm", CultureInfo.InvariantCulture);
                var endDateTime = DateTime.ParseExact(endTime, "HH:mm", CultureInfo.InvariantCulture);

                while (startDateTime <= endDateTime)
                {
                    result.Add(startDateTime.ToString("HH:mm"));
                    startDateTime = startDateTime.AddHours(1);
                }
            }
            return result;
        }


        public async Task HandleMajorServicesSchedule()
        {
            var services = await _unitOfWork.ServiceRepository.FindAllAsync();
            foreach (var service in services)
            {
                bool isChanged = false;
                if (service.CloseScheduleDate != null && service.CloseScheduleDate == DateOnly.FromDateTime(DateTime.Now))
                {
                    service.IsOpen = false;
                    await _serviceRepository.UpdateAsync(service.Id, service);
                    isChanged = true;
                }
                if (service.OpenScheduleDate != null && service.OpenScheduleDate == DateOnly.FromDateTime(DateTime.Now))
                {
                    service.IsOpen = true;
                    service.CloseScheduleDate = null;
                    service.OpenScheduleDate = null;

                    isChanged = true;
                }

                if (isChanged == true)
                {
                    await _serviceRepository.UpdateAsync(service.Id, service);
                    Console.WriteLine("\n[BACKGROUND] Service: " + service.Name + " đã được cập nhật trạng thái thành công!");
                    Console.WriteLine("CloseScheduleDate: " + service.CloseScheduleDate?.ToString("yyyy-MM-dd"));
                    Console.WriteLine("OpenScheduleDate: " + service.OpenScheduleDate?.ToString("yyyy-MM-dd"));
                    Console.WriteLine("IsOpen: " + service.IsOpen.ToString());
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

        ////////////////////////////////////////////////////////////
        
        public async Task<JArray> GetAllServices(){
            var services = await _unitOfWork.ServiceRepository.FindAllAsync();
            var result = new List<object>();

            foreach (var service in services)
            {
                // format giống với fomat của hàm GetMajorHeadServices
                var serviceType = service.ServiceType;
                var major = service.FacilityMajor;

                result.Add(new
                {
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
                    },
                    ServiceType = new
                    {
                        Id = serviceType.Id,
                        Name = serviceType.Name
                    }
                });

            }

            return JArray.FromObject(result);
        }
        
        public async Task<JArray> GetMajorHeadServices(int accountId)
        {
            var account = await _unitOfWork.AccountRepository.FindById(accountId);
            if (account == null)
            {
                // throw new HttpRequestException("Account không tồn tại");
                throw new HttpRequestException("Account is not exist");
            }
            if (account.RoleId != 2)
            {
                // throw new HttpRequestException("Account không phải là Major Head");
                throw new HttpRequestException("Account is not Major Head");
            }
            try
            {
                var majorAssignments = await _unitOfWork.AssigneeFacilityMajorAssignmentRepository.FindByAccountId(accountId);
                var result = new List<object>();

                foreach (var majorAssignment in majorAssignments)
                {

                    var services = await _unitOfWork.ServiceRepository.FindByFacilityMajorId(majorAssignment.FacilityMajorId);
                    foreach (var service in services)
                    {
                        result.Add(new
                        {
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
                                Id = majorAssignment.FacilityMajor.Id,
                                Name = majorAssignment.FacilityMajor.Name,
                                MainDescription = majorAssignment.FacilityMajor.MainDescription,
                                WorkShiftsDescription = majorAssignment.FacilityMajor.WorkShiftsDescription,
                                FacilityMajorTypeId = majorAssignment.FacilityMajor.FacilityMajorTypeId,
                                FacilityId = majorAssignment.FacilityMajor.FacilityId,
                                IsOpen = majorAssignment.FacilityMajor.IsOpen,
                                CloseScheduleDate = majorAssignment.FacilityMajor.CloseScheduleDate,
                                OpenScheduleDate = majorAssignment.FacilityMajor.OpenScheduleDate,
                                IsDeactivated = majorAssignment.FacilityMajor.IsDeactivated,
                                CreatedAt = majorAssignment.FacilityMajor.CreatedAt,
                                BackgroundImageUrl = await GenerateImageUrl(_filePathConfig.MAJOR_IMAGE_PATH, majorAssignment.FacilityMajor.Id.ToString(), "background"),
                                ImageUrl = await GenerateImageUrl(_filePathConfig.MAJOR_IMAGE_PATH, majorAssignment.FacilityMajor.Id.ToString(), "main")
                            },
                            ServiceType = new
                            {
                                Id = service.ServiceType.Id,
                                Name = service.ServiceType.Name
                            }

                        });
                    }

                }

                return JArray.FromObject(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error getting Major Head services: " + ex.Message);
                Console.WriteLine("\n\n\n" + ex.Message.ToString() + "\n\n\n");
                // throw new HttpRequestException("Lấy danh sách dịch vụ của Major Head thất bại: " + ex.Message);
                throw new HttpRequestException("Failed to get Major Head services: " + ex.Message);
            }

        }

        public async Task<JArray> GetMajorServices(int majorId)
        {
            var majorExist = await _unitOfWork.FacilityMajorRepository.FindByIdAsync(majorId);
            if (majorExist == null)
            {
                // throw new HttpRequestException("Major không tồn tại");
                throw new HttpRequestException("Major is not exist, id: " + majorId);
            }

            try
            {
                var result = new List<object>();

                var services = await _unitOfWork.ServiceRepository.FindByFacilityMajorId(majorId);
                foreach (var service in services)
                {
                    var serviceType = service.ServiceType;
                    var major = service.FacilityMajor;

                    result.Add(new
                    {
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
                        },
                        ServiceType = new
                        {
                            Id = serviceType.Id,
                            Name = serviceType.Name
                        }
                    });
                }
                return JArray.FromObject(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error getting Major services: " + ex.Message);
                Console.WriteLine("\n\n\n" + ex.Message.ToString() + "\n\n\n");
                // throw new HttpRequestException("Lấy danh sách dịch vụ của Major thất bại: " + ex.Message);
                throw new HttpRequestException("Failed to get Major services: " + ex.Message);
            }

        }

        public async Task<JObject> GetMajorServiceDetail(int serviceId)
        {
            var service = await _serviceRepository.FindByIdAsync(serviceId);
            if (service == null)
            {
                // throw new HttpRequestException("Dịch vụ không tồn tại");
                throw new HttpRequestException("Service is not exist, id: " + serviceId);
            }

            var major = await _unitOfWork.FacilityMajorRepository.FindByIdAsync(service.FacilityMajorId);
            if (major == null)
            {
                // throw new HttpRequestException("Major không tồn tại");
                throw new HttpRequestException("Major is not exist, id: " + service.FacilityMajorId);
            }

            var serviceType = await _serviceTypeRepository.FindByIdAsync(service.ServiceTypeId);
            if (serviceType == null)
            {
                // throw new HttpRequestException("Loại dịch vụ không tồn tại");
                throw new HttpRequestException("Service type is not exist, id: " + service.ServiceTypeId);
            }
            var result = new
            {
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
                },
                ServiceType = new
                {
                    Id = serviceType.Id,
                    Name = serviceType.Name
                }
            };

            return JObject.FromObject(result);
        }

        public async Task UpdateMajorService(int serviceId, dynamic service)
        {
            var existingService = await _unitOfWork.ServiceRepository.FindByIdAsync(serviceId);
            if (existingService == null)
            {
                // throw new HttpRequestException("Dịch vụ không tồn tại");
                throw new HttpRequestException("Service is not exist, id: " + serviceId);
            }

            var serviceType = await _serviceTypeRepository.FindByIdAsync(int.Parse(service.ServiceTypeId.ToString()));
            if (serviceType == null)
            {
                // throw new HttpRequestException("Loại dịch vụ không tồn tại");
                throw new HttpRequestException("Service type is not exist, id: " + service.ServiceTypeId);
            }
            _dateHelpers.IsValidCloseScheduleAndOpenSchedule(service.CloseScheduleDate.ToString(), service.OpenScheduleDate.ToString(), existingService.CloseScheduleDate?.ToString("yyyy-MM-dd"));

            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    if (existingService.ServiceTypeId == 2 && service.CloseScheduleDate != null)
                    {

                        if (service.OpenScheduleDate != null)
                        {
                            string closeScheduleDate = service.CloseScheduleDate.ToString();
                            string openScheduleDate = DateOnly.Parse(service.OpenScheduleDate.ToString()).AddDays(-1).ToString();
                            foreach (var serviceRequest in existingService.ServiceRequests)
                            {
                                if (_dateHelpers.IsDateInRange(serviceRequest.DateRequest.ToString(), closeScheduleDate, openScheduleDate) && serviceRequest.RequestStatusId != 7 && serviceRequest.RequestStatusId != 8 && serviceRequest.RequestStatusId != 9)
                                {
                                    serviceRequest.RequestStatusId = 9;
                                    serviceRequest.IsCancelAutomatically = true;
                                    // serviceRequest.CancelReason = "Huỷ vì lí do dịch vụ tạm thời đóng";
                                    // serviceRequest.ProgressNote = serviceRequest.ProgressNote + "\n\n-[*CANCELLED AUTO*] Dịch vụ sẽ tạm thời đóng vào thời gian đặt của yêu cầu";
                                    serviceRequest.CancelReason = "Cancelled because the service is temporarily closed";
                                    serviceRequest.ProgressNote = serviceRequest.ProgressNote + "\n\n-[*CANCELLED AUTO*] The service will be temporarily closed at the request time";
                                    await _serviceRequestRepository.UpdateAsync(serviceRequest.Id, serviceRequest);
                                }
                            }
                        }
                        else if (service.OpenScheduleDate == null)
                        {
                            DateOnly closeScheduleDate = DateOnly.Parse(service.CloseScheduleDate.ToString());
                            foreach (var serviceRequest in existingService.ServiceRequests)
                            {
                                if (_dateHelpers.IsDateEarlier(closeScheduleDate.AddDays(-1).ToString("yyyy-MM-dd"), serviceRequest.DateRequest?.ToString("yyyy-MM-dd")) && serviceRequest.RequestStatusId != 7 && serviceRequest.RequestStatusId != 8 && serviceRequest.RequestStatusId != 9)
                                {
                                    serviceRequest.RequestStatusId = 9;
                                    serviceRequest.IsCancelAutomatically = true;
                                    // serviceRequest.CancelReason = "Huỷ vì lí do dịch vụ tạm thời đóng";
                                    // serviceRequest.ProgressNote = serviceRequest.ProgressNote + "\n\n-[*CANCELLED AUTO*] Dịch vụ sẽ tạm thời đóng vào thời gian đặt của yêu cầu";
                                    serviceRequest.CancelReason = "Cancelled because the service is temporarily closed";
                                    serviceRequest.ProgressNote = serviceRequest.ProgressNote + "\n\n-[*CANCELLED AUTO*] The service will be temporarily closed at the request time";
                                    await _serviceRequestRepository.UpdateAsync(serviceRequest.Id, serviceRequest);
                                }
                            }
                        }
                    }



                    existingService.Name = service.Name;
                    existingService.FacilityMajorId = service.FacilityMajorId;
                    existingService.IsInitRequestDescriptionRequired = service.IsInitRequestDescriptionRequired;
                    existingService.RequestInitHintDescription = service.RequestInitHintDescription;
                    existingService.MainDescription = service.MainDescription;
                    existingService.WorkShiftsDescription = service.WorkShiftsDescription;
                    existingService.CloseScheduleDate = service.CloseScheduleDate != null ? DateOnly.Parse(service.CloseScheduleDate.ToString()) : null;
                    existingService.OpenScheduleDate = service.OpenScheduleDate != null ? DateOnly.Parse(service.OpenScheduleDate.ToString()) : null;
                    existingService.ServiceTypeId = service.ServiceTypeId;


                    await _serviceRepository.UpdateAsync(serviceId, existingService);

                    var folderPath = _filePathConfig.SERVICE_IMAGE_PATH + "\\" + existingService.Id;
                    if (service.Image != null && service.Image != "")
                    {
                        string fileName = "main";
                        string base64Data = service.Image;

                        await SaveBase64File(base64Data, folderPath, fileName);
                    }

                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine("\n\n\n" + ex.Message.ToString() + "\n\n\n");
                    // throw new HttpRequestException("Cập nhật dịch vụ thất bại: " + ex.Message);
                    throw new HttpRequestException("Failed to update service: " + ex.Message);
                }
            }
        }

        public async Task DeactivateMajorService(int serviceId,bool deactivate)
        {
            var existingService = await _unitOfWork.ServiceRepository.FindByIdAsync(serviceId);
            if (existingService == null)
            {
                // throw new HttpRequestException("Dịch vụ không tồn tại");
                throw new HttpRequestException("Service is not exist, id: " + serviceId);
            }

            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    existingService.IsDeactivated = true;

                    await _unitOfWork.ServiceRepository.Deactivate(serviceId, deactivate);
                    foreach (var serviceRequest in existingService.ServiceRequests)
                    {
                        if (serviceRequest.RequestStatusId != 7 && serviceRequest.RequestStatusId != 8 && serviceRequest.RequestStatusId != 9)
                        {
                            serviceRequest.RequestStatusId = 9;
                            serviceRequest.IsCancelAutomatically = true;
                            // serviceRequest.CancelReason = "Huỷ vì lí do dịch vụ không còn tồn tại";
                            // serviceRequest.ProgressNote = serviceRequest.ProgressNote + "\n\n-[*CANCELLED AUTO*] Dịch vụ đã bị xoá";
                            serviceRequest.CancelReason = "Cancelled because the service is not exist anymore";
                            serviceRequest.ProgressNote = serviceRequest.ProgressNote + "\n\n-[*CANCELLED AUTO*] The service has been deactivated";
                            await _serviceRequestRepository.UpdateAsync(serviceRequest.Id, serviceRequest);
                        }
                    }
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine("\n\n\n" + ex.Message.ToString() + "\n\n\n");
                    // throw new HttpRequestException("Xoá dịch vụ thất bại: " + ex.Message);
                    throw new HttpRequestException("Failed to deactivate service: " + ex.Message);
                }
            }
        }

        public async Task CreateMajorService(int majorId, dynamic service)
        {
            var major = await _unitOfWork.FacilityMajorRepository.FindByIdAsync(majorId);
            if (major == null)
            {
                // throw new HttpRequestException("Major không tồn tại");
                throw new HttpRequestException("Major is not exist, id: " + majorId);
            }

            var serviceType = await _serviceTypeRepository.FindByIdAsync(int.Parse(service.ServiceTypeId.ToString()));
            if (serviceType == null)
            {
                // throw new HttpRequestException("Loại dịch vụ không tồn tại");
                throw new HttpRequestException("Service type is not exist, id: " + service.ServiceTypeId);
            }

            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var newService = new Service
                    {
                        Name = service.Name,
                        FacilityMajorId = majorId,
                        IsInitRequestDescriptionRequired = service.IsInitRequestDescriptionRequired,
                        RequestInitHintDescription = service.RequestInitHintDescription,
                        MainDescription = service.MainDescription,
                        WorkShiftsDescription = service.WorkShiftsDescription,
                        ServiceTypeId = service.ServiceTypeId,

                        // [*] Mặc định 1: đóng cửa từ lúc mới tạo 
                        // CloseScheduleDate = DateOnly.Parse(DateTime.Now.ToString("yyyy-MM-dd")),

                        // [*] Mặc định 2: mở từ cửa lúc mới tạo
                        IsOpen = true,
                    };
                    await _serviceRepository.CreateAsync(newService);

                    var folderPath = _filePathConfig.SERVICE_IMAGE_PATH + "\\" + newService.Id;
                    if (service.Image != null && service.Image != "")
                    {
                        string fileName = "main";
                        string base64Data = service.Image;

                        await SaveBase64File(base64Data, folderPath, fileName);
                    }
                    else
                    {
                        await CopyFile(_filePathConfig.SERVICE_IMAGE_PATH, "unknown", folderPath, "main");
                    }

                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine("\n\n\n" + ex.Message.ToString() + "\n\n\n");
                    // throw new HttpRequestException("Tạo dịch vụ của Major thất bại: " + ex.Message);
                    throw new HttpRequestException("Failed to create Major service: " + ex.Message);
                }
            }
        }

        public async Task<JArray> GetMajorServiceAvailability(int serviceId)
        {
            var service = await _serviceRepository.FindByIdAsync(serviceId);
            if (service == null)
            {
                // throw new HttpRequestException("Dịch vụ không tồn tại");
                throw new HttpRequestException("Service is not exist, id: " + serviceId);
            }

            try
            {
                var result = new List<object>();

                var availability = await _unitOfWork.ServiceAvailabilityRepository.FindByServiceId(serviceId);
                foreach (var item in availability)
                {
                    result.Add(new
                    {
                        Schedule = new
                        {
                            ServiceId = item.ServiceId,
                            DayOfWeek = this.GetDayOfWeek(item.DayOfWeek),
                            StartRequestableTime = item.StartRequestableTime.ToString("HH:mm"),
                            EndRequestableTime = item.EndRequestableTime.ToString("HH:mm")
                        }
                    });
                }
                return JArray.FromObject(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error getting Major Service availability: " + ex.Message);
                Console.WriteLine("\n\n\n" + ex.Message.ToString() + "\n\n\n");
                // throw new HttpRequestException("Lấy lịch trực dịch vụ thất bại: " + ex.Message);
                throw new HttpRequestException("Failed to get Major Service availability: " + ex.Message);
            }

        }

        

        public async Task<JArray> GetMajorServiceBookableSchedules(int serviceId)
        {
            var service = await _unitOfWork.ServiceRepository.FindByIdAsync(serviceId);
            if (service == null)
            {
                // throw new HttpRequestException("Dịch vụ không tồn tại");
                throw new HttpRequestException("Service is not exist, id: " + serviceId);
            }

            try
            {
                int dateLimit = 14;
                var result = new List<object>();

                // DateOnly? major_close_date = major["CloseScheduleDate"] != null ? DateOnly.FromDateTime(DateTime.Parse(major["CloseScheduleDate"].ToString())) : null;
                // DateOnly? major_open_date = major["OpenScheduleDate"] != null ? DateOnly.FromDateTime(DateTime.Parse(major["OpenScheduleDate"].ToString())) : null;
                // DateOnly? service_close_date = service["CloseScheduleDate"] != null ? DateOnly.FromDateTime(DateTime.Parse(service["CloseScheduleDate"].ToString())) : null;
                // DateOnly? service_open_date = service["OpenScheduleDate"] != null ? DateOnly.FromDateTime(DateTime.Parse(service["OpenScheduleDate"].ToString())) : null;
                DateOnly? major_close_date = service.FacilityMajor.CloseScheduleDate;
                DateOnly? major_open_date = service.FacilityMajor.OpenScheduleDate;
                DateOnly? service_close_date = service.CloseScheduleDate;
                DateOnly? service_open_date = service.OpenScheduleDate;

                var serviceAvailabilities = await _unitOfWork.ServiceAvailabilityRepository.FindByServiceId(serviceId);



                List<int> daysOfWeeks = serviceAvailabilities.Select(x =>(int)x.DayOfWeek).ToList();

                if (major_close_date != null && major_open_date != null && service_close_date != null && service_open_date != null)
                {
                    if (!(major_open_date <= service_close_date || service_open_date < major_close_date))
                    {
                        if (major_close_date < service_close_date)
                        {
                            service_close_date = major_close_date;
                        }
                        else
                        {
                            major_close_date = service_close_date;
                        }

                        if (major_open_date < service_open_date)
                        {
                            major_open_date = service_open_date;
                        }
                        else
                        {
                            service_open_date = major_open_date;
                        }
                    }
                }


                List<string> majorAvailableDates = this.GetValidDates(dateLimit, major_close_date, major_open_date, daysOfWeeks);
                List<string> serviceAvailableDates = this.GetValidDates(dateLimit, service_close_date, service_open_date, daysOfWeeks);



                if (major_close_date == null && service_close_date != null)
                {
                    majorAvailableDates = serviceAvailableDates;
                }
                else if (major_close_date != null && service_close_date == null)
                {
                    serviceAvailableDates = majorAvailableDates;
                }


                List<string> availableDates = majorAvailableDates.Intersect(serviceAvailableDates).ToList();

                if (availableDates.Count > dateLimit)
                {
                    availableDates = availableDates.GetRange(0, dateLimit);
                }
                else if (availableDates.Count < dateLimit && major_open_date != null && service_open_date != null)
                {
                    DateOnly? date = DateOnly.Parse(majorAvailableDates[majorAvailableDates.Count - 1]) > DateOnly.Parse(serviceAvailableDates[serviceAvailableDates.Count - 1]) ? DateOnly.Parse(majorAvailableDates[majorAvailableDates.Count - 1]) : DateOnly.Parse(serviceAvailableDates[serviceAvailableDates.Count - 1]);

                    if (major_close_date > service_open_date)
                    {
                        date = service_open_date;
                    }
                    else if (service_close_date > major_open_date)
                    {
                        date = major_open_date;
                    }
                    else if (major_open_date > service_open_date)
                    {
                        if (DateOnly.Parse(majorAvailableDates[majorAvailableDates.Count - 1]) >= major_open_date)
                        {
                            date = major_open_date;
                        }
                    }
                    else if (service_open_date > major_open_date)
                    {
                        if (DateOnly.Parse(serviceAvailableDates[serviceAvailableDates.Count - 1]) >= service_open_date)
                        {
                            date = service_open_date;
                        }
                    }

                    if (availableDates.Contains(date?.ToString("yyyy-MM-dd")))
                    {
                        date = date?.AddDays(1);
                    }

                    while (availableDates.Count < dateLimit)
                    {
                        int dayOfWeek = ((int)date?.DayOfWeek) + 1;
                        if (daysOfWeeks.Contains(dayOfWeek) && !_dateHelpers.IsDateInRange(date.ToString(), major_close_date.ToString(), major_open_date?.AddDays(-1).ToString()) && !_dateHelpers.IsDateInRange(date.ToString(), service_close_date.ToString(), service_open_date?.AddDays(-1).ToString()) && !availableDates.Contains(date?.ToString("yyyy-MM-dd")))
                        {
                            Console.WriteLine(true);
                            availableDates.Add(date?.ToString("yyyy-MM-dd"));
                        }
                        date = date?.AddDays(1);
                    }
                }

                foreach (var date in availableDates)
                {
                    var dayOfWeek = _dateHelpers.GetDayOfWeekNumber(date.ToString());
                    var timeList = this.GetTimeListByDateOfWeek(serviceAvailabilities, dayOfWeek);
                    result.Add(new
                    {
                        Date = date,
                        Times = timeList
                    });
                }

                return JArray.FromObject(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error getting Major Service bookable schedules: " + ex.Message);
                Console.WriteLine("\n\n\n" + ex.Message.ToString() + "\n\n\n");
                // throw new HttpRequestException("Lấy lịch đặt dịch vụ thất bại: " + ex.Message);
                throw new HttpRequestException("Failed to get Major Service bookable schedules: " + ex.Message);
            }
        }

        public async Task AddMajorServiceAvailability(int serviceId, dynamic availability)
        {
            var service = await _serviceRepository.FindByIdAsync(serviceId);
            if (service == null)
            {
                // throw new HttpRequestException("Dịch vụ không tồn tại");
                throw new HttpRequestException("Service is not exist, id: " + serviceId);
            }
            this.GetDayOfWeek(int.Parse(availability.DayOfWeek.ToString()));

            if (_dateHelpers.IsValidTime(availability.StartRequestableTime.ToString()) == false || _dateHelpers.IsValidTime(availability.EndRequestableTime.ToString()) == false)
            {
                // throw new HttpRequestException("Thời gian không hợp lệ");
                throw new HttpRequestException("Invalid time format, required: HH:mm");
            }
            if (_dateHelpers.IsTimeEarlier(availability.StartRequestableTime.ToString(), availability.EndRequestableTime.ToString()) == false)
            {
                // throw new HttpRequestException("Thời gian bắt đầu phải trước thời gian kết thúc");
                throw new HttpRequestException("Start time must be earlier than end time, "+ availability.StartRequestableTime.ToString() + " - " + availability.EndRequestableTime.ToString());
            }

            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {   
                    var existingAvailabilities = await _unitOfWork.ServiceAvailabilityRepository.FindByServiceId(serviceId);
                    var exist = existingAvailabilities.FirstOrDefault(x => x.DayOfWeek == int.Parse(availability.DayOfWeek.ToString()) && (_dateHelpers.IsTimeInRange(availability.StartRequestableTime.ToString(), x.StartRequestableTime.ToString(), x.EndRequestableTime.ToString()) || _dateHelpers.IsTimeInRange(availability.EndRequestableTime.ToString(), x.StartRequestableTime.ToString(), x.EndRequestableTime.ToString()) ) );
                    if (exist != null){
                        // throw new HttpRequestException("Lịch đã tồn tại hoặc bị trùng");
                        throw new HttpRequestException("Schedule already exists or overlaps, " + availability.StartRequestableTime.ToString() + " - " + availability.EndRequestableTime.ToString());
                    }

                    var newAvailability = new ServiceAvailability
                    {
                        ServiceId = serviceId,
                        DayOfWeek = availability.DayOfWeek,
                        StartRequestableTime = TimeOnly.Parse(availability.StartRequestableTime.ToString()),
                        EndRequestableTime = TimeOnly.Parse(availability.EndRequestableTime.ToString())
                    };
                    await _serviceAvailabilityRepository.CreateAsync(newAvailability);

                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine("\n\n\n" + ex.Message.ToString() + "\n\n\n");
                    // throw new HttpRequestException("Thêm lịch trực dịch vụ thất bại vì lí do lịch trùng hoặc không hợp lệ, " + ex.Message);
                    throw new HttpRequestException("Failed to add Major Service availability because of overlapping or invalid schedule: " + ex.Message);
                }
            }
        }

        public async Task DeleteMajorServiceAvailability(int serviceId, dynamic availability)
        {
            var service = await _serviceRepository.FindByIdAsync(serviceId);
            if (service == null)
            {
                // throw new HttpRequestException("Dịch vụ không tồn tại");
                throw new HttpRequestException("Service is not exist, id: " + serviceId);
            }

            var existingAvailability = await _unitOfWork.ServiceAvailabilityRepository.FindByAvailability(new ServiceAvailability
            {
                ServiceId = serviceId,
                DayOfWeek = availability.DayOfWeek,
                StartRequestableTime = TimeOnly.Parse(availability.StartRequestableTime.ToString()),
                EndRequestableTime = TimeOnly.Parse(availability.EndRequestableTime.ToString())
            });

            if (existingAvailability == null)
            {
                // throw new HttpRequestException("Lịch không tồn tại");
                throw new HttpRequestException("Availability is not exist, " + availability.StartRequestableTime.ToString() + " - " + availability.EndRequestableTime.ToString());
            }

            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    await _unitOfWork.ServiceAvailabilityRepository.DeleteAsync(existingAvailability);

                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine("\n\n\n" + ex.Message.ToString() + "\n\n\n");
                    // throw new HttpRequestException("Xóa lịch trực dịch vụ thất bại: " + ex.Message);
                    throw new HttpRequestException("Failed to delete Major Service availability: " + ex.Message);
                }
            }
        }

        public async Task<JArray> GetServiceTypes()
        {
            try
            {
                var result = new List<object>();

                var serviceTypes = await _serviceTypeRepository.FindAllAsync();
                foreach (var item in serviceTypes)
                {
                    result.Add(new
                    {
                        Id = item.Id,
                        Name = item.Name

                    });
                }
                return JArray.FromObject(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error getting Service Types: " + ex.Message);
                Console.WriteLine("\n\n\n" + ex.Message.ToString() + "\n\n\n");
                // throw new HttpRequestException("Lấy danh sách loại dịch vụ thất bại: " + ex.Message);
                throw new HttpRequestException("Failed to get Service Types: " + ex.Message);
            }
        }
    }
}
