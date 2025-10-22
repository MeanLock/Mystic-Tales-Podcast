using Microsoft.Extensions.Logging;
using Architecture_1.BusinessLogic.Services.DbServices.FacilityServices;
using Architecture_1.DataAccess.Repositories.interfaces;
using Architecture_1.DataAccess.Entities;
using Architecture_1.DataAccess.UOW;
using Architecture_1.DataAccess.Data;
using Architecture_1.BusinessLogic.Helpers;
using Architecture_1.Common.AppConfigurations.FileStorage.interfaces;
using Architecture_1.Infrastructure.Services.AWS.S3;
using Architecture_1.Common.AppConfigurations.App.interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Architecture_1.BusinessLogic.Services.DbServices.FacilityServices
{
    public class FacilityMessagingService
    {
        private readonly ILogger<FacilityMessagingService> _logger;
        private readonly IGenericRepository<Facility> _facilityRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly AppDbContext _dbContext;
        private readonly FileHelpers _fileHelpers;
        private readonly IFilePathConfig _filePathConfig;
        private readonly AWSS3Service _awsS3Service;
        private readonly IAppConfig _appConfig;
        private readonly IGenericRepository<ServiceRequest> _serviceRequestRepository;
        private readonly IGenericRepository<TaskRequest> _taskRequestRepository;

        public FacilityMessagingService(
            ILogger<FacilityMessagingService> logger,
            IGenericRepository<Facility> facilityRepository,
            IUnitOfWork unitOfWork,
            AppDbContext dbContext,
            FileHelpers fileHelpers,
            IFilePathConfig filePathConfig,
            AWSS3Service awsS3Service,
            IAppConfig appConfig,
            IGenericRepository<ServiceRequest> serviceRequestRepository,
            IGenericRepository<TaskRequest> taskRequestRepository)
        {
            _logger = logger;
            _facilityRepository = facilityRepository;
            _unitOfWork = unitOfWork;
            _dbContext = dbContext;
            _fileHelpers = fileHelpers;
            _filePathConfig = filePathConfig;
            _awsS3Service = awsS3Service;
            _appConfig = appConfig;
            _serviceRequestRepository = serviceRequestRepository;
            _taskRequestRepository = taskRequestRepository;
        }

        #region Helper Methods

        public async Task<string> GenerateImageUrl(string rootFolderPath, string folderName, string fileName)
        {
            if (_appConfig.IMAGE_SRC == "awsS3")
            {
                return await _awsS3Service.GeneratePresignedUrlAsync(rootFolderPath, folderName, fileName);
            }
            return await _fileHelpers.GetImageUrl(rootFolderPath, folderName, fileName);
        }

        public async Task SaveBase64File(string base64Data, string folderPath, string fileName)
        {
            if (_appConfig.IMAGE_SRC == "awsS3")
            {
                await _awsS3Service.UploadBase64FileAsync(base64Data, folderPath, fileName);
                return;
            }
            await _fileHelpers.SaveBase64File(base64Data, folderPath, fileName);
        }

        public async Task CopyFile(string sourceFolder, string sourceFileName, string destinationFolder, string destinationFileName)
        {
            if (_appConfig.IMAGE_SRC == "awsS3")
            {
                await _awsS3Service.CopyFileAsync(sourceFolder, sourceFileName, destinationFolder, destinationFileName);
                return;
            }
            await _fileHelpers.CopyFile(sourceFolder, sourceFileName, destinationFolder, destinationFileName);
        }

        public async Task DeleteFolder(string folderPath)
        {
            if (_appConfig.IMAGE_SRC == "awsS3")
            {
                await _awsS3Service.DeleteFolderAsync(folderPath);
                return;
            }
            await _fileHelpers.DeleteFolder(folderPath);
        }

        #endregion

        #region Main Facility Methods (giống hệt FacilityService)

        public async Task<JArray> GetFacilities()
        {
            var facilities = await _facilityRepository.FindAllAsync();
            var result = await Task.WhenAll(facilities.Select(async facility =>
            {
                string folderPath = _filePathConfig.FACILITY_IMAGE_PATH;

                return new
                {
                    Facility = new
                    {
                        Id = facility.Id,
                        Name = facility.Name,
                        Description = facility.Description,
                        ImageUrl = await GenerateImageUrl(folderPath, facility.Id.ToString(), "main"),
                        CreatedAt = facility.CreatedAt,
                        IsDeactivated = facility.IsDeactivated
                    }

                };
            }));

            return JArray.FromObject(result);
        }

        public async Task CreateFacility(dynamic facility)
        {
            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var newFacility = new Facility
                    {
                        Name = facility.Name,
                        Description = facility.Description,
                        CreatedAt = DateTime.Now,
                        IsDeactivated = false
                    };
                    await _facilityRepository.CreateAsync(newFacility);
                    string folderPath = _filePathConfig.FACILITY_IMAGE_PATH + "\\" + newFacility.Id;
                    if (facility.Image != null && facility.Image != "")
                    {
                        string fileName = "main";
                        string base64Data = facility.Image;

                        await SaveBase64File(base64Data, folderPath, fileName);
                    }
                    else
                    {
                        await CopyFile(_filePathConfig.FACILITY_IMAGE_PATH, "unknown", folderPath, "main");
                    }

                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine("\n\n\n" + ex.Message.ToString() + "\n\n\n");
                    throw new HttpRequestException("Failed to create facility: " + ex.Message);
                }
            }
        }

        public async Task<JObject> GetFacilityDetail(int facilityId)
        {
            var facility = await _facilityRepository.FindByIdAsync(facilityId);
            if (facility == null)
            {
                throw new HttpRequestException("Facility is not exist, id: " + facilityId);
            }

            string folderPath = _filePathConfig.FACILITY_IMAGE_PATH;
            var result = new
            {
                Id = facility.Id,
                Name = facility.Name,
                Description = facility.Description,
                ImageUrl = await GenerateImageUrl(folderPath, facility.Id.ToString(), "main"),
                CreatedAt = facility.CreatedAt,
                IsDeactivated = facility.IsDeactivated
            };

            var majors = await Task.WhenAll((await _unitOfWork.FacilityMajorRepository.FindByFacilityId(facilityId)).Select(async fm =>
            {
                string majorFolderPath = _filePathConfig.MAJOR_IMAGE_PATH;
                return new
                {
                    Id = fm.Id,
                    Name = fm.Name,
                    MainDescription = fm.MainDescription,
                    WorkShiftsDescription = fm.WorkShiftsDescription,
                    FacilityMajorTypeId = fm.FacilityMajorTypeId,
                    FacilityId = fm.FacilityId,
                    IsOpen = fm.IsOpen,
                    CloseScheduleDate = fm.CloseScheduleDate,
                    OpenScheduleDate = fm.OpenScheduleDate,
                    IsDeactivated = fm.IsDeactivated,
                    CreatedAt = fm.CreatedAt,
                    BackgroundImageUrl = await GenerateImageUrl(majorFolderPath, fm.Id.ToString(), "background"),
                    ImageUrl = await GenerateImageUrl(majorFolderPath, fm.Id.ToString(), "main")

                };
            }));

            return JObject.FromObject(new
            {
                Facility = result,
                Majors = majors
            }, new JsonSerializer
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        }

        public async Task UpdateFacility(int facilityId, dynamic facility)
        {
            var existingFacility = await _facilityRepository.FindByIdAsync(facilityId);
            if (existingFacility == null)
            {
                throw new HttpRequestException("Facility is not exist, id: " + facilityId);
            }

            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    existingFacility.Name = facility.Name;
                    existingFacility.Description = facility.Description;

                    await _facilityRepository.UpdateAsync(facilityId, existingFacility);

                    if (!string.IsNullOrEmpty(facility.Image?.ToString()))
                    {
                        string folderPath = _filePathConfig.FACILITY_IMAGE_PATH + "\\" + existingFacility.Id;
                        string fileName = "main";
                        string base64Data = facility.Image;

                        await SaveBase64File(base64Data, folderPath, fileName);
                    }

                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine("\n\n\n" + ex.Message.ToString() + "\n\n\n");
                    throw new HttpRequestException("Failed to update facility: " + ex.Message);
                }
            }

        }

        public async Task DeactivateFacility(int facilityId)
        {
            var existingFacility = await _unitOfWork.FacilityRepository.FindByIdAsync(facilityId);
            if (existingFacility == null)
            {
                throw new HttpRequestException("Facility is not exist, id: " + facilityId);
            }

            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    foreach (var major in existingFacility.FacilityMajors)
                    {
                        await _unitOfWork.FacilityMajorRepository.Deactivate(major.Id, true);
                        foreach (var service in major.Services)
                        {
                            foreach (var serviceRequest in service.ServiceRequests)
                            {
                                if (serviceRequest.RequestStatusId != 7 && serviceRequest.RequestStatusId != 8 && serviceRequest.RequestStatusId != 9)
                                {
                                    serviceRequest.RequestStatusId = 9;
                                    serviceRequest.IsCancelAutomatically = true;
                                    // serviceRequest.CancelReason = "Huỷ vì lí do major không còn tồn tại";
                                    // serviceRequest.ProgressNote = serviceRequest.ProgressNote + "\n\n-[*CANCELLED AUTO*] Facility đã bị deactivate";
                                    serviceRequest.CancelReason = "Cancelled because the major is not exist anymore";
                                    serviceRequest.ProgressNote = serviceRequest.ProgressNote + "\n\n-[*CANCELLED AUTO*] Facility has been deactivated";

                                    await _serviceRequestRepository.UpdateAsync(serviceRequest.Id, serviceRequest);
                                }
                            }
                        }
                        foreach (var taskRequest in major.TaskRequests)
                        {
                            if (taskRequest.RequestStatusId != 7 && taskRequest.RequestStatusId != 8 && taskRequest.RequestStatusId != 9)
                            {
                                taskRequest.RequestStatusId = 9;
                                // taskRequest.CancelReason = "[*CANCELLED AUTO*] Huỷ vì lí do major không còn tồn tại";
                                taskRequest.CancelReason = "Cancelled because the major is not exist anymore";
                                await _taskRequestRepository.UpdateAsync(taskRequest.Id, taskRequest);
                            }
                        }
                    }
                    await _unitOfWork.FacilityRepository.Deactivate(facilityId, true);
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine("\n\n\n" + ex.Message.ToString() + "\n\n\n");
                    throw new HttpRequestException("Failed to delete facility: " + ex.Message);
                }
            }

        }

        #endregion

        #region Messaging Event Processing Methods

        public async Task ProcessFacilityCreatedEventAsync(int facilityId)
        {
            _logger.LogInformation("Processing facility created event for facility: {FacilityId}", facilityId);
            
            try
            {
                // Sử dụng GetFacilityDetail để verify facility đã được tạo thành công
                var facilityDetail = await GetFacilityDetail(facilityId);
                _logger.LogInformation("Facility created event processed successfully: {FacilityDetail}", facilityDetail.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing facility created event for facility: {FacilityId}", facilityId);
                throw;
            }
        }

        public async Task ProcessFacilityUpdatedEventAsync(int facilityId)
        {
            _logger.LogInformation("Processing facility updated event for facility: {FacilityId}", facilityId);
            
            try
            {
                // Sử dụng GetFacilityDetail để verify facility đã được update thành công
                var facilityDetail = await GetFacilityDetail(facilityId);
                _logger.LogInformation("Facility updated event processed successfully: {FacilityDetail}", facilityDetail.ToString());
                
                // Additional post-update verification
                var facilities = await GetFacilities();
                _logger.LogInformation("Updated facility appears in facilities list: {FacilitiesCount} total facilities", 
                    facilities.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing facility updated event for facility: {FacilityId}", facilityId);
                throw;
            }
        }

        public async Task ProcessFacilityDeletedEventAsync(int facilityId)
        {
            _logger.LogInformation("Processing facility deleted event for facility: {FacilityId}", facilityId);
            
            try
            {
                // Sử dụng chính xác DeactivateFacility method từ FacilityService
                await DeactivateFacility(facilityId);
                _logger.LogInformation("Facility deleted event processed successfully using DeactivateFacility for facility: {FacilityId}", facilityId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing facility deleted event for facility: {FacilityId}", facilityId);
                throw;
            }
        }

        #endregion
    }
}