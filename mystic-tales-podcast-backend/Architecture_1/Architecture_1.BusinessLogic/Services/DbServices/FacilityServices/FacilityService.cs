using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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

namespace Architecture_1.BusinessLogic.Services.DbServices.FacilityServices
{
    public class FacilityService
    {
        // LOGGER
        private readonly ILogger<FacilityService> _logger;

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
        public readonly IGenericRepository<Facility> _facilityRepository;
        public readonly IGenericRepository<ServiceRequest> _serviceRequestRepository;
        public readonly IGenericRepository<TaskRequest> _taskRequestRepository;

        // AWS SERVICE
        private readonly AWSS3Service _awsS3Service;



        public FacilityService(
            ILogger<FacilityService> logger,
            AppDbContext dbContext,
            FileHelpers fileHelpers,
            IUnitOfWork unitOfWork,
            IGenericRepository<Facility> facilityRepository,
            IFilePathConfig filePathConfig,
            IGenericRepository<ServiceRequest> serviceRequestRepository,
            IGenericRepository<TaskRequest> taskRequestRepository,
            AWSS3Service awsS3Service,
            IAppConfig appConfig
            )
        {
            _logger = logger;
            _dbContext = dbContext;
            _fileHelpers = fileHelpers;
            _unitOfWork = unitOfWork;
            _facilityRepository = facilityRepository;
            _filePathConfig = filePathConfig;
            _serviceRequestRepository = serviceRequestRepository;
            _taskRequestRepository = taskRequestRepository;
            _awsS3Service = awsS3Service;
            _appConfig = appConfig;
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
    }
}
