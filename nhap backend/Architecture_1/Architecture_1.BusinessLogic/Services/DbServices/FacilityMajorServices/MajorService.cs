using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Architecture_1.BusinessLogic.Helpers;
using Architecture_1.Common.AppConfigurations.App.interfaces;
using Architecture_1.Common.AppConfigurations.FileStorage.interfaces;
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
using Architecture_1.Infrastructure.Services.AWS.S3;

namespace Architecture_1.BusinessLogic.Services.DbServices.FacilityMajorServices
{
    public class MajorService
    {
        // LOGGER
        private readonly ILogger<MajorService> _logger;

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
        private readonly IGenericRepository<FacilityMajor> _facilityMajorRepository;
        private readonly IGenericRepository<FacilityMajorType> _majorTypeRepository;
        private readonly IGenericRepository<Facility> _facilityRepository;
        private readonly IGenericRepository<Account> _accountRepository;
        private readonly IGenericRepository<AssigneeFacilityMajorAssignment> _assigneeFacilityMajorAssignmentRepository;
        private readonly IGenericRepository<Feedback> _feedbackRepository;
        private readonly IGenericRepository<ReportType> _reportTypeRepository;
        private readonly IGenericRepository<Report> _reportRepository;
        private readonly IGenericRepository<ServiceRequest> _serviceRequestRepository;
        private readonly IGenericRepository<TaskRequest> _taskRequestRepository;

        // AWS SERVICE
        private readonly AWSS3Service _awsS3Service;


        public MajorService(
            ILogger<MajorService> logger,
            AppDbContext dbContext,
            FileHelpers fileHelpers,
            IUnitOfWork unitOfWork,
            IGenericRepository<FacilityMajor> facilityMajorRepository,
            IFilePathConfig filePathConfig,
            IGenericRepository<FacilityMajorType> majorTypeRepository,
            IGenericRepository<Facility> facilityRepository,
            IGenericRepository<Account> accountRepository,
            IGenericRepository<AssigneeFacilityMajorAssignment> assigneeFacilityMajorAssignmentRepository,
            IGenericRepository<Feedback> feedbackRepository,
            IGenericRepository<ReportType> reportTypeRepository,
            IGenericRepository<Report> reportRepository,
            DateHelpers dateHelpers,
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
            _facilityMajorRepository = facilityMajorRepository;
            _filePathConfig = filePathConfig;
            _majorTypeRepository = majorTypeRepository;
            _facilityRepository = facilityRepository;
            _accountRepository = accountRepository;
            _assigneeFacilityMajorAssignmentRepository = assigneeFacilityMajorAssignmentRepository;
            _feedbackRepository = feedbackRepository;
            _reportTypeRepository = reportTypeRepository;
            _reportRepository = reportRepository;
            _dateHelpers = dateHelpers;
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

        public async Task HandleMajorsSchedule()
        {
            var majors = await _unitOfWork.FacilityMajorRepository.FindByAllAsync();
            foreach (var major in majors)
            {
                bool isChanged = false;
                if (major.CloseScheduleDate != null && major.CloseScheduleDate == DateOnly.FromDateTime(DateTime.Now))
                {
                    major.IsOpen = false;
                    await _facilityMajorRepository.UpdateAsync(major.Id, major);
                    isChanged = true;
                }
                if (major.OpenScheduleDate != null && major.OpenScheduleDate == DateOnly.FromDateTime(DateTime.Now))
                {
                    major.IsOpen = true;
                    major.CloseScheduleDate = null;
                    major.OpenScheduleDate = null;

                    isChanged = true;
                }

                if (isChanged == true)
                {
                    await _facilityMajorRepository.UpdateAsync(major.Id, major);
                    Console.WriteLine("\n[BACKGROUND] Major: " + major.Name + " đã được cập nhật trạng thái thành công!");
                    Console.WriteLine("CloseScheduleDate: " + major.CloseScheduleDate?.ToString("yyyy-MM-dd"));
                    Console.WriteLine("OpenScheduleDate: " + major.OpenScheduleDate?.ToString("yyyy-MM-dd"));
                    Console.WriteLine("IsOpen: " + major.IsOpen.ToString());
                }
            }
        }

        ////////////////////////////////////////////////////////////
        public async Task<JArray> GetMajors()
        {
            var majors = await _unitOfWork.FacilityMajorRepository.FindByAllAsync();
            var result = new List<object>();

            foreach (var major in majors)
            {
                string folderPath = _filePathConfig.MAJOR_IMAGE_PATH;
                var majorType = major.FacilityMajorType;
                var facility = major.Facility;
                var serviceRequestCount = await _unitOfWork.ServiceRequestRepository.CountByMajorIdFromThisMonth(major.Id);
                result.Add(new
                {
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
                        // BackgroundImageUrl = await GenerateImageUrl(folderPath, major.Id.ToString(), "background"),
                        ImageUrl = await GenerateImageUrl(folderPath, major.Id.ToString(), "main"),
                    },
                    MajorType = new
                    {
                        Id = majorType.Id,
                        Name = majorType.Name
                    },
                    Facility = new
                    {
                        Id = facility.Id,
                        Name = facility.Name,
                        Description = facility.Description,
                        // ImageUrl = await GenerateImageUrl(_filePathConfig.FACILITY_IMAGE_PATH, facility.Id.ToString(), "main"),
                        IsDeactivated = facility.IsDeactivated,
                        CreatedAt = facility.CreatedAt
                    },
                    ServiceRequestCount = serviceRequestCount


                });
            }

            return JArray.FromObject(result);
        }

        public async Task<JArray> GetAllFeatureMajors()
        {
            var majors = await _unitOfWork.FacilityMajorRepository.FindByAllAsync();
            var temp_result = new List<dynamic>();
            majors = majors.Where(m => m.IsDeactivated == false).ToList();
            foreach (var major in majors)
            {
                string folderPath = _filePathConfig.MAJOR_IMAGE_PATH;
                var majorType = major.FacilityMajorType;
                var facility = major.Facility;
                var serviceRequestCount = await _unitOfWork.ServiceRequestRepository.CountByMajorIdFromThisMonth(major.Id);
                temp_result.Add(new
                {
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
                    },
                    MajorType = new
                    {
                        Id = majorType.Id,
                        Name = majorType.Name
                    },
                    Facility = new
                    {
                        Id = facility.Id,
                        Name = facility.Name,
                        Description = facility.Description,
                        IsDeactivated = facility.IsDeactivated,
                        CreatedAt = facility.CreatedAt
                    },
                    ServiceRequestCount = serviceRequestCount


                });
            }

            temp_result = temp_result.OrderByDescending(m => m.ServiceRequestCount).Take(6).ToList();
            var result = new List<object>();
            foreach (var major in temp_result)
            {
                var majorType = major.MajorType;
                var facility = major.Facility;
                result.Add(new
                {
                    Major = new
                    {
                        Id = major.Major.Id,
                        Name = major.Major.Name,
                        MainDescription = major.Major.MainDescription,
                        WorkShiftsDescription = major.Major.WorkShiftsDescription,
                        FacilityMajorTypeId = major.Major.FacilityMajorTypeId,
                        FacilityId = major.Major.FacilityId,
                        IsOpen = major.Major.IsOpen,
                        CloseScheduleDate = major.Major.CloseScheduleDate,
                        OpenScheduleDate = major.Major.OpenScheduleDate,
                        IsDeactivated = major.Major.IsDeactivated,
                        CreatedAt = major.Major.CreatedAt,
                        BackgroundImageUrl = await GenerateImageUrl(_filePathConfig.MAJOR_IMAGE_PATH, major.Major.Id.ToString(), "background"),
                        ImageUrl = await GenerateImageUrl(_filePathConfig.MAJOR_IMAGE_PATH, major.Major.Id.ToString(), "main"),
                    },
                    MajorType = new
                    {
                        Id = majorType.Id,
                        Name = majorType.Name
                    },
                    Facility = new
                    {
                        Id = facility.Id,
                        Name = facility.Name,
                        Description = facility.Description,
                        ImageUrl = await GenerateImageUrl(_filePathConfig.FACILITY_IMAGE_PATH, facility.Id.ToString(), "main"),
                        IsDeactivated = facility.IsDeactivated,
                        CreatedAt = facility.CreatedAt
                    },
                    ServiceRequestCount = major.ServiceRequestCount


                });
            }



            return JArray.FromObject(result);
        }

        public async Task CreateMajor(dynamic major)
        {
            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var newMajor = new FacilityMajor
                    {
                        Name = major.Name,
                        MainDescription = major.MainDescription,
                        WorkShiftsDescription = major.WorkShiftsDescription,
                        FacilityMajorTypeId = major.FacilityMajorTypeId,
                        FacilityId = major.FacilityId,

                        // [*] Mặc định 1: đóng cửa từ lúc mới tạo 
                        // CloseScheduleDate = DateOnly.Parse(DateTime.Now.ToString("yyyy-MM-dd")),

                        // [*] Mặc định 2: mở từ cửa lúc mới tạo
                        IsOpen = true,
                    };

                    await _facilityMajorRepository.CreateAsync(newMajor);

                    var folderPath = _filePathConfig.MAJOR_IMAGE_PATH + "\\" + newMajor.Id;
                    if (major.BackgroundImage != null && major.BackgroundImage != "")
                    {
                        string fileName = "background";
                        string base64Data = major.BackgroundImage;

                        await SaveBase64File(base64Data, folderPath, fileName);
                    }
                    else
                    {
                        await CopyFile(_filePathConfig.MAJOR_IMAGE_PATH, "unknown", folderPath, "background");
                    }

                    if (major.Image != null && major.Image != "")
                    {
                        string fileName = "main";
                        string base64Data = major.Image;

                        await SaveBase64File(base64Data, folderPath, fileName);
                    }
                    else
                    {
                        await CopyFile(_filePathConfig.MAJOR_IMAGE_PATH, "unknown", folderPath, "main");
                    }


                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine("\n\n\n" + ex.Message.ToString() + "\n\n\n");
                    // throw new HttpRequestException("Tạo major thất bại: " + ex.Message);
                    throw new HttpRequestException("Failed to create major: " + ex.Message);
                }
            }

        }

        public async Task<JArray> GetMajorHeadMajors(int accountId)
        {
            var account = await _unitOfWork.AccountRepository.FindById(accountId);
            if (account == null)
            {
                // throw new HttpRequestException("Account không tồn tại");
                throw new HttpRequestException("Account is not exist, id: " + accountId);
            }
            if (account.Role.Id != 2)
            {
                // throw new HttpRequestException("Account không phải là Head");
                throw new HttpRequestException("Account is not Major Head, id: " + accountId);
            }

            var assigneeFacilityMajorAssignments = await _unitOfWork.AssigneeFacilityMajorAssignmentRepository.FindByAccountId(accountId);
            var result = new List<object>();
            foreach (var afma in assigneeFacilityMajorAssignments)
            {
                var major = afma.FacilityMajor;
                result.Add(new
                {
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
                        ImageUrl = await GenerateImageUrl(_filePathConfig.MAJOR_IMAGE_PATH, major.Id.ToString(), "main"),
                    },
                    MajorType = new
                    {
                        Id = major.FacilityMajorType.Id,
                        Name = major.FacilityMajorType.Name,
                    },
                    Facility = new
                    {
                        Id = major.Facility.Id,
                        Name = major.Facility.Name,
                        Description = major.Facility.Description,
                        // ImageUrl = await GenerateImageUrl(_filePathConfig.FACILITY_IMAGE_PATH, major.Facility.Id.ToString(), "main"),
                        IsDeactivated = major.Facility.IsDeactivated,
                        CreatedAt = major.Facility.CreatedAt
                    }

                });
            }

            return JArray.FromObject(
                result
                , new JsonSerializer
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });
        }

        public async Task<JArray> GetAssigneeMajors(int accountId)
        {
            var account = await _unitOfWork.AccountRepository.FindById(accountId);
            if (account == null)
            {
                // throw new HttpRequestException("Account không tồn tại");
                throw new HttpRequestException("Account is not exist, id: " + accountId);
            }

            var assigneeFacilityMajorAssignments = await _unitOfWork.AssigneeFacilityMajorAssignmentRepository.FindByAccountId(accountId);
            var result = new List<object>();
            foreach (var afma in assigneeFacilityMajorAssignments)
            {
                var major = afma.FacilityMajor;
                result.Add(new
                {
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
                        ImageUrl = await GenerateImageUrl(_filePathConfig.MAJOR_IMAGE_PATH, major.Id.ToString(), "main"),
                    },
                    MajorType = new
                    {
                        Id = major.FacilityMajorType.Id,
                        Name = major.FacilityMajorType.Name,
                    },
                    Facility = new
                    {
                        Id = major.Facility.Id,
                        Name = major.Facility.Name,
                        Description = major.Facility.Description,
                        // ImageUrl = await GenerateImageUrl(_filePathConfig.FACILITY_IMAGE_PATH, major.Facility.Id.ToString(), "main"),
                        IsDeactivated = major.Facility.IsDeactivated,
                        CreatedAt = major.Facility.CreatedAt
                    }

                });
            }

            return JArray.FromObject(
                result
                , new JsonSerializer
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });
        }

        public async Task<JArray> GetMajorHeadRequesters(int accountId)
        {
            var account = await _unitOfWork.AccountRepository.FindById(accountId);
            if (account == null)
            {
                // throw new HttpRequestException("Account không tồn tại");
                throw new HttpRequestException("Account is not exist, id: " + accountId);
            }
            if (account.Role.Id != 2)
            {
                // throw new HttpRequestException("Account không phải là Head");
                throw new HttpRequestException("Account is not Major Head, id: " + accountId);
            }
            try
            {
                var assigneeFacilityMajorAssignments = account.AssigneeFacilityMajorAssignments;
                var result = new List<dynamic>();
                foreach (var afma in assigneeFacilityMajorAssignments)
                {
                    var serviceRequests = await _unitOfWork.ServiceRequestRepository.FindByFacilityMajorId(afma.FacilityMajorId);
                    foreach (var serviceRequest in serviceRequests)
                    {
                        var accountRequester = serviceRequest.Requester;

                        result.Add(new
                        {
                            Account = new
                            {
                                Id = accountRequester.Id,
                                FullName = accountRequester.FullName,
                                Email = accountRequester.Email,
                                DateOfBirth = accountRequester.DateOfBirth,
                                Phone = accountRequester.Phone,
                                Address = accountRequester.Address,
                                RoleId = accountRequester.RoleId,
                                JobTypeId = accountRequester.JobTypeId,
                                ImageUrl = await GenerateImageUrl(_filePathConfig.ACCOUNt_IMAGE_PATH, accountRequester.Id.ToString(), "main"),
                                IsDeactivated = accountRequester.IsDeactivated,
                                CreatedAt = accountRequester.CreatedAt
                            },
                            Role = new
                            {
                                Id = accountRequester.Role.Id,
                                Name = accountRequester.Role.Name
                            },
                            JobType = new
                            {
                                Id = accountRequester.JobType.Id,
                                Name = accountRequester.JobType.Name
                            }
                        });
                    }
                }

                // Lọc lại các requester trùng nhau
                result = result.GroupBy(r => r.Account.Id).Select(r => r.First()).ToList();
                return JArray.FromObject(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n\n\n" + ex.ToString() + "\n\n\n");
                // throw new HttpRequestException("Lấy requesters thất bại: " + ex.Message);
                throw new HttpRequestException("Failed to get requesters: " + ex.Message);
            }
        }

        public async Task<JObject> GetMajorDetail(int majorId)
        {
            var major = await _unitOfWork.FacilityMajorRepository.FindByIdAsync(majorId);
            if (major == null)
            {
                // throw new HttpRequestException("Major không tồn tại");
                throw new HttpRequestException("Major is not exist, id: " + majorId);
            }

            //             [
            //     {
            //       "Item": {
            //         "Id": 0,
            //         "Name": "string",
            //         "InUseCount": 0,
            //         "Count": 0,
            //         "ImageUrl": "string",
            //         "CreatedAt": "string",
            //         "UpdatedAt": "string"
            //       },
            //       "FacilityItemAssignment": {
            //         "FacilityItemId": 0,
            //         "FacilityMajorId": 0,
            //         "ItemCount": 0,
            //         "Created": "string"
            //       }
            //     }
            //   ]

            // var items = new List<object>();
            // foreach (var fia in major.FacilityItemAssignments)
            // {
            //     int inUseCount = await _unitOfWork.FacilityItemAssignmentRepository.GetInUseItemCount(facilityItem.Id);
            //     var item = new
            //     {
            //         Id = fia.FacilityItem.Id,
            //         Name = fia.FacilityItem.Name,
            //         InUseCount = fia.FacilityItem.Count - fia.ItemCount,
            //         Count = fia.FacilityItem.Count,
            //         ImageUrl = await GenerateImageUrl(_filePathConfig.ITEM_IMAGE_PATH, fia.FacilityItem.Id.ToString(), "main"),
            //         CreatedAt = fia.FacilityItem.CreatedAt,
            //         UpdatedAt = fia.FacilityItem.UpdatedAt
            //     };
            //     var facilityItemAssignment = new
            //     {
            //         FacilityItemId = fia.FacilityItemId,
            //         FacilityMajorId = fia.FacilityMajorId,
            //         ItemCount = fia.ItemCount,
            //         Created = fia.CreatedAt
            //     };
            //     items.Add(new { Item = item, FacilityItemAssignment = facilityItemAssignment });
            // }

            var items = major.FacilityItemAssignments.Select(fia => new
            {
                Item = new
                {
                    Id = fia.FacilityItem.Id,
                    Name = fia.FacilityItem.Name,
                    Count = fia.FacilityItem.Count,
                    ImageUrl = GenerateImageUrl(_filePathConfig.ITEM_IMAGE_PATH, fia.FacilityItem.Id.ToString(), "main"),
                    CreatedAt = fia.FacilityItem.CreatedAt,
                    UpdatedAt = fia.FacilityItem.UpdatedAt
                },
                FacilityItemAssignment = new
                {
                    FacilityItemId = fia.FacilityItemId,
                    FacilityMajorId = fia.FacilityMajorId,
                    ItemCount = fia.ItemCount,
                    Created = fia.CreatedAt
                }
            }).ToList();
            string folderPath = _filePathConfig.MAJOR_IMAGE_PATH;
            var result = new
            {
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
                    BackgroundImageUrl = await GenerateImageUrl(folderPath, major.Id.ToString(), "background"),
                    ImageUrl = await GenerateImageUrl(folderPath, major.Id.ToString(), "main"),
                },
                MajorType = new
                {
                    Id = major.FacilityMajorType.Id,
                    Name = major.FacilityMajorType.Name
                },
                Facility = new
                {
                    Id = major.Facility.Id,
                    Name = major.Facility.Name,
                    Description = major.Facility.Description,
                    ImageUrl = await GenerateImageUrl(_filePathConfig.FACILITY_IMAGE_PATH, major.Facility.Id.ToString(), "main"),
                    IsDeactivated = major.Facility.IsDeactivated,
                    CreatedAt = major.Facility.CreatedAt
                },
                Items = await Task.WhenAll(major.FacilityItemAssignments
                .Select(async fia => new
                {
                    Item = new
                    {
                        Id = fia.FacilityItem.Id,
                        Name = fia.FacilityItem.Name,
                        Count = fia.FacilityItem.Count,
                        ImageUrl = await GenerateImageUrl(_filePathConfig.ITEM_IMAGE_PATH, fia.FacilityItem.Id.ToString(), "main"),
                        CreatedAt = fia.FacilityItem.CreatedAt,
                        UpdatedAt = fia.FacilityItem.UpdatedAt
                    },
                    FacilityItemAssignment = new
                    {
                        FacilityItemId = fia.FacilityItemId,
                        FacilityMajorId = fia.FacilityMajorId,
                        ItemCount = fia.ItemCount,
                        Created = fia.CreatedAt
                    }
                }).ToList())

            };

            return JObject.FromObject(result);

        }

        public async Task UpdateMajor(int majorId, dynamic major)
        {
            var existingMajor = await _unitOfWork.FacilityMajorRepository.FindByIdAsync(majorId);
            if (existingMajor == null)
            {
                // throw new HttpRequestException("Major không tồn tại");
                throw new HttpRequestException("Major is not exist, id: " + majorId);
            }

            _dateHelpers.IsValidCloseScheduleAndOpenSchedule(major.CloseScheduleDate.ToString(), major.OpenScheduleDate.ToString(), existingMajor.CloseScheduleDate?.ToString("yyyy-MM-dd"));

            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var services = existingMajor.Services;
                    if (major.CloseScheduleDate != null)
                    {
                        foreach (var service in services)
                        {
                            var existingService = await _unitOfWork.ServiceRepository.FindByIdAsync(service.Id);
                            if (existingService.ServiceTypeId == 2)
                            {
                                if (major.OpenScheduleDate != null)
                                {
                                    string closeScheduleDate = major.CloseScheduleDate.ToString();
                                    string openScheduleDate = DateOnly.Parse(major.OpenScheduleDate.ToString()).AddDays(-1).ToString(); // Trừ 1 ngày để so sánh với ngày yêu cầu
                                    foreach (var serviceRequest in existingService.ServiceRequests)
                                    {
                                        if (_dateHelpers.IsDateInRange(serviceRequest.DateRequest.ToString(), major.CloseScheduleDate.ToString(), openScheduleDate) && serviceRequest.RequestStatusId != 7 && serviceRequest.RequestStatusId != 8 && serviceRequest.RequestStatusId != 9)
                                        {
                                            serviceRequest.RequestStatusId = 9;
                                            serviceRequest.IsCancelAutomatically = true;
                                            // serviceRequest.CancelReason = "Huỷ vì lí do major tạm thời đóng";
                                            // serviceRequest.ProgressNote = serviceRequest.ProgressNote + "\n\n-[*CANCELLED AUTO*] Major sẽ tạm thời đóng vào thời gian đặt của yêu cầu";
                                            serviceRequest.CancelReason = "Cancelled because the major is temporarily closed";
                                            serviceRequest.ProgressNote = serviceRequest.ProgressNote + "\n\n-[*CANCELLED AUTO*] Major will be temporarily closed at the time of request";
                                            await _serviceRequestRepository.UpdateAsync(serviceRequest.Id, serviceRequest);
                                        }
                                    }
                                }
                                else if (major.OpenScheduleDate == null)
                                {
                                    DateOnly closeScheduleDate = DateOnly.Parse(major.CloseScheduleDate.ToString());
                                    foreach (var serviceRequest in existingService.ServiceRequests)
                                    {
                                        if (_dateHelpers.IsDateEarlier(closeScheduleDate.AddDays(-1).ToString("yyyy-MM-dd"), serviceRequest.DateRequest?.ToString("yyyy-MM-dd")) && serviceRequest.RequestStatusId != 7 && serviceRequest.RequestStatusId != 8 && serviceRequest.RequestStatusId != 9)
                                        {
                                            serviceRequest.RequestStatusId = 9;
                                            serviceRequest.IsCancelAutomatically = true;
                                            // serviceRequest.CancelReason = "Huỷ vì lí do major tạm thời đóng";
                                            // serviceRequest.ProgressNote = serviceRequest.ProgressNote + "\n\n-[*CANCELLED AUTO*] Major sẽ tạm thời đóng vào thời gian đặt của yêu cầu";
                                            serviceRequest.CancelReason = "Cancelled because the major is temporarily closed";
                                            serviceRequest.ProgressNote = serviceRequest.ProgressNote + "\n\n-[*CANCELLED AUTO*] Major will be temporarily closed at the time of request";
                                            await _serviceRequestRepository.UpdateAsync(serviceRequest.Id, serviceRequest);
                                        }
                                        {
                                            serviceRequest.RequestStatusId = 9;
                                            serviceRequest.IsCancelAutomatically = true;
                                            // serviceRequest.CancelReason = "Huỷ vì lí do major tạm thời đóng";
                                            // serviceRequest.ProgressNote = serviceRequest.ProgressNote + "\n\n-[*CANCELLED AUTO*] Major sẽ tạm thời đóng vào thời gian đặt của yêu cầu";
                                            serviceRequest.CancelReason = "Cancelled because the major is temporarily closed";
                                            serviceRequest.ProgressNote = serviceRequest.ProgressNote + "\n\n-[*CANCELLED AUTO*] Major will be temporarily closed at the time of request";
                                            await _serviceRequestRepository.UpdateAsync(serviceRequest.Id, serviceRequest);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    existingMajor.Name = major.Name;
                    existingMajor.MainDescription = major.MainDescription;
                    existingMajor.WorkShiftsDescription = major.WorkShiftsDescription;
                    existingMajor.FacilityMajorTypeId = major.FacilityMajorTypeId;
                    existingMajor.FacilityId = major.FacilityId;
                    existingMajor.CloseScheduleDate = major.CloseScheduleDate != null ? DateOnly.Parse(major.CloseScheduleDate.ToString()) : null;
                    existingMajor.OpenScheduleDate = major.OpenScheduleDate != null ? DateOnly.Parse(major.OpenScheduleDate.ToString()) : null;

                    await _facilityMajorRepository.UpdateAsync(majorId, existingMajor);

                    var folderPath = _filePathConfig.MAJOR_IMAGE_PATH + "\\" + existingMajor.Id;
                    if (major.BackgroundImage != null && major.BackgroundImage != "")
                    {
                        string fileName = "background";
                        string base64Data = major.BackgroundImage;

                        await SaveBase64File(base64Data, folderPath, fileName);
                    }

                    if (major.Image != null && major.Image != "")
                    {
                        string fileName = "main";
                        string base64Data = major.Image;

                        await SaveBase64File(base64Data, folderPath, fileName);
                    }

                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine("\n\n\n" + ex.Message.ToString() + "\n\n\n");
                    // throw new HttpRequestException("Cập nhật major thất bại: " + ex.Message);
                    throw new HttpRequestException("Failed to update major: " + ex.Message);
                }
            }
        }

        public async Task DeactivateMajor(int majorId, bool deactivate)
        {
            var major = await _unitOfWork.FacilityMajorRepository.FindByIdAsync(majorId);
            if (major == null)
            {
                // throw new HttpRequestException("Major không tồn tại");
                throw new HttpRequestException("Major is not exist, id: " + majorId);
            }

            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    await _unitOfWork.FacilityMajorRepository.Deactivate(majorId, deactivate);
                    foreach (var service in major.Services)
                    {
                        foreach (var serviceRequest in service.ServiceRequests)
                        {
                            if (serviceRequest.RequestStatusId != 7 && serviceRequest.RequestStatusId != 8 && serviceRequest.RequestStatusId != 9)
                            {
                                serviceRequest.RequestStatusId = 9;
                                serviceRequest.IsCancelAutomatically = true;
                                // serviceRequest.CancelReason = "Huỷ vì lí do major không còn tồn tại";
                                // serviceRequest.ProgressNote = serviceRequest.ProgressNote + "\n\n-[*CANCELLED AUTO*] Major đã bị deactivate";
                                serviceRequest.CancelReason = "Cancelled because the major is not exist anymore";
                                serviceRequest.ProgressNote = serviceRequest.ProgressNote + "\n\n-[*CANCELLED AUTO*] Major has been deactivated";

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

                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine("\n\n\n" + ex.Message.ToString() + "\n\n\n");
                    // throw new HttpRequestException("Xoá major thất bại: " + ex.Message);
                    throw new HttpRequestException("Failed to deactivate major: " + ex.Message);
                }
            }
        }

        public async Task CreateFeedback(int majorId, int accountId, dynamic feedback)
        {
            var major = await _unitOfWork.FacilityMajorRepository.FindByIdAsync(majorId);
            if (major == null)
            {
                // throw new HttpRequestException("Major không tồn tại");
                throw new HttpRequestException("Major is not exist, id: " + majorId);
            }

            var account = await _unitOfWork.AccountRepository.FindById(accountId);
            if (account == null)
            {
                // throw new HttpRequestException("Account không tồn tại");
                throw new HttpRequestException("Account is not exist, id: " + accountId);
            }
            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var newFeedback = new Feedback
                    {
                        FacilityMajorId = majorId,
                        AccountId = accountId,
                        Content = feedback.Content,
                        Rate = feedback.Rate
                    };

                    await _feedbackRepository.CreateAsync(newFeedback);

                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine("\n\n\n" + ex.Message.ToString() + "\n\n\n");
                    // throw new HttpRequestException("Tạo feedback thất bại: " + ex.Message);
                    throw new HttpRequestException("Failed to create feedback: " + ex.Message);
                }
            }
        }

        public async Task<JArray> GetAllFeedbacks()
        {
            try
            {
                var feedbacks = await _unitOfWork.FeedbackRepository.FindAllAsync();
                var result = new List<object>();

                foreach (var feedback in feedbacks)
                {
                    var major = feedback.FacilityMajor;
                    var account = feedback.Account;
                    result.Add(new
                    {
                        Feedback = new
                        {
                            Id = feedback.Id,
                            AccountId = feedback.AccountId,
                            FacilityMajorId = feedback.FacilityMajorId,
                            Content = feedback.Content,
                            Rate = feedback.Rate,
                            IsDeactivated = feedback.IsDeactivated,
                            CreatedAt = feedback.CreatedAt
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
                            ImageUrl = await GenerateImageUrl(_filePathConfig.MAJOR_IMAGE_PATH, major.Id.ToString(), "main"),
                        },
                        Account = new
                        {
                            Id = account.Id,
                            Email = account.Email,
                            Phone = account.Phone,
                            FullName = account.FullName,
                            RoleId = account.RoleId,
                            JobTypeId = account.JobTypeId,
                            ImageUrl = await GenerateImageUrl(_filePathConfig.ACCOUNt_IMAGE_PATH, account.Id.ToString(), "main"),
                            IsDeactivated = account.IsDeactivated,
                            CreatedAt = account.CreatedAt
                        }
                    });
                }

                return JArray.FromObject(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n\n\n" + ex.Message.ToString() + "\n\n\n");
                // throw new HttpRequestException("Lấy feedback thất bại: " + ex.Message);
                throw new HttpRequestException("Failed to get feedbacks: " + ex.Message);
            }

        }

        public async Task<JArray> GetMajorHeadFeedbacks(int accountId)
        {
            var account = await _unitOfWork.AccountRepository.FindById(accountId);
            if (account == null)
            {
                // throw new HttpRequestException("Account không tồn tại");
                throw new HttpRequestException("Account is not exist, id: " + accountId);
            }
            if (account.Role.Id != 2)
            {
                // throw new HttpRequestException("Account không phải là Head");
                throw new HttpRequestException("Account is not Major Head, id: " + accountId);
            }

            try
            {
                var assigneeFacilityMajorAssignments = await _unitOfWork.AssigneeFacilityMajorAssignmentRepository.FindByAccountId(accountId);
                var result = new List<object>();
                foreach (var afma in assigneeFacilityMajorAssignments)
                {
                    var major = afma.FacilityMajor;
                    var feedbacks = await _unitOfWork.FeedbackRepository.FindByMajorId(major.Id);
                    foreach (var feedback in feedbacks)
                    {
                        var accountFeedback = feedback.Account;
                        result.Add(new
                        {
                            Feedback = new
                            {
                                Id = feedback.Id,
                                AccountId = feedback.AccountId,
                                FacilityMajorId = feedback.FacilityMajorId,
                                Content = feedback.Content,
                                Rate = feedback.Rate,
                                IsDeactivated = feedback.IsDeactivated,
                                CreatedAt = feedback.CreatedAt
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
                                ImageUrl = await GenerateImageUrl(_filePathConfig.MAJOR_IMAGE_PATH, major.Id.ToString(), "main"),
                            },
                            Account = new
                            {
                                Id = accountFeedback.Id,
                                Email = accountFeedback.Email,
                                Phone = accountFeedback.Phone,
                                FullName = accountFeedback.FullName,
                                RoleId = accountFeedback.RoleId,
                                JobTypeId = accountFeedback.JobTypeId,
                                ImageUrl = await GenerateImageUrl(_filePathConfig.ACCOUNt_IMAGE_PATH, accountFeedback.Id.ToString(), "main"),
                                IsDeactivated = accountFeedback.IsDeactivated,
                                CreatedAt = accountFeedback.CreatedAt
                            }

                        });
                    }

                }
                return JArray.FromObject(
                        result
                        , new JsonSerializer
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        });
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n\n\n" + ex.Message.ToString() + "\n\n\n");
                // throw new HttpRequestException("Lấy feedback thất bại: " + ex.Message);
                throw new HttpRequestException("Failed to get feedbacks: " + ex.Message);
            }
        }

        public async Task<JArray> GetFeedbacksByMajor(int majorId)
        {
            var major = await _unitOfWork.FacilityMajorRepository.FindByIdAsync(majorId);
            if (major == null)
            {
                // throw new HttpRequestException("Major không tồn tại");
                throw new HttpRequestException("Major is not exist, id: " + majorId);
            }

            try
            {
                var feedbacks = await _unitOfWork.FeedbackRepository.FindByMajorId(majorId);
                var result = new List<object>();
                foreach (var feedback in feedbacks)
                {
                    var account = feedback.Account;
                    result.Add(new
                    {
                        Feedback = new
                        {
                            Id = feedback.Id,
                            AccountId = feedback.AccountId,
                            FacilityMajorId = feedback.FacilityMajorId,
                            Content = feedback.Content,
                            Rate = feedback.Rate,
                            IsDeactivated = feedback.IsDeactivated,
                            CreatedAt = feedback.CreatedAt
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
                            ImageUrl = await GenerateImageUrl(_filePathConfig.MAJOR_IMAGE_PATH, major.Id.ToString(), "main"),
                        },
                        MajorType = new
                        {
                            Id = major.FacilityMajorType.Id,
                            Name = major.FacilityMajorType.Name
                        },
                        Account = new
                        {
                            Id = account.Id,
                            Email = account.Email,
                            Phone = account.Phone,
                            FullName = account.FullName,
                            RoleId = account.RoleId,
                            JobTypeId = account.JobTypeId,
                            ImageUrl = await GenerateImageUrl(_filePathConfig.ACCOUNt_IMAGE_PATH, account.Id.ToString(), "main"),
                            IsDeactivated = account.IsDeactivated,
                            CreatedAt = account.CreatedAt
                        }

                    });
                }
                return JArray.FromObject(
                        result
                        , new JsonSerializer
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        });
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n\n\n" + ex.Message.ToString() + "\n\n\n");
                // throw new HttpRequestException("Lấy feedback thất bại: " + ex.Message);
                throw new HttpRequestException("Failed to get feedbacks: " + ex.Message);
            }
        }

        public async Task DeactivateFeedback(int feedbackId, bool deactivate)
        {
            var feedback = await _feedbackRepository.FindByIdAsync(feedbackId);
            if (feedback == null)
            {
                // throw new HttpRequestException("Feedback không tồn tại");
                throw new HttpRequestException("Feedback is not exist, id: " + feedbackId);
            }

            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    await _unitOfWork.FeedbackRepository.Deactivate(feedbackId, deactivate);

                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine("\n\n\n" + ex.Message.ToString() + "\n\n\n");
                    // throw new HttpRequestException("Xoá feedback thất bại: " + ex.Message);
                    throw new HttpRequestException("Failed to deactivate feedback: " + ex.Message);
                }
            }
        }

        public async Task<JArray> GetAllReports()
        {
            try
            {
                var reports = await _unitOfWork.ReportRepository.FindAllAsync();
                var result = new List<object>();
                foreach (var report in reports)
                {
                    var account = report.Account;
                    var facilityMajor = report.FacilityMajor;
                    var facility = facilityMajor.Facility;
                    var reportType = report.ReportType;
                    result.Add(new
                    {
                        Report = new
                        {
                            Id = report.Id,
                            Content = report.Content,
                            IsResolved = report.IsResolved,
                            ReportTypeId = report.ReportTypeId,
                            AccountId = report.AccountId,
                            FacilityMajorId = report.FacilityMajorId,
                            CreatedAt = report.CreatedAt
                        },
                        Account = new
                        {
                            FullName = account.FullName,
                            Email = account.Email,
                            Password = account.Password,
                            Address = account.Address,
                            DateOfBirth = account.DateOfBirth,
                            Phone = account.Phone,
                            RoleId = account.RoleId,
                            JobTypeId = account.JobTypeId,
                            ImageUrl = await GenerateImageUrl(_filePathConfig.ACCOUNt_IMAGE_PATH, account.Id.ToString(), "main"),
                            IsDeactivated = account.IsDeactivated,
                            CreatedAt = account.CreatedAt
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
                            Created = facilityMajor.CreatedAt,
                            BackgroundImageUrl = await GenerateImageUrl(_filePathConfig.MAJOR_IMAGE_PATH, facilityMajor.Id.ToString(), "background"),
                            ImageUrl = await GenerateImageUrl(_filePathConfig.MAJOR_IMAGE_PATH, facilityMajor.Id.ToString(), "main")
                        },
                        ReportType = new
                        {
                            Id = reportType.Id,
                            Name = reportType.Name
                        },
                        MajorType = new
                        {
                            Id = facilityMajor.FacilityMajorType.Id,
                            Name = facilityMajor.FacilityMajorType.Name
                        }
                    });
                }
                return JArray.FromObject(result);

            }
            catch (Exception ex)
            {
                Console.WriteLine("\n\n\n" + ex.Message.ToString() + "\n\n\n");
                // throw new HttpRequestException("Lấy report thất bại: " + ex.Message);
                throw new HttpRequestException("Failed to get reports: " + ex.Message);
            }
        }

        public async Task CreateReport(int majorId, int accountId, dynamic report)
        {
            var account = await _unitOfWork.AccountRepository.FindById(accountId);
            if (account == null)
            {
                // throw new HttpRequestException("Account không tồn tại");
                throw new HttpRequestException("Account is not exist, id: " + accountId);
            }

            var facilityMajor = await _unitOfWork.FacilityMajorRepository.FindByIdAsync(majorId);
            if (facilityMajor == null)
            {
                // throw new HttpRequestException("Major không tồn tại");
                throw new HttpRequestException("Major is not exist, id: " + majorId);
            }
            Console.WriteLine("\n\n\n" + report.ReportTypeId + "\n\n\n");
            var reportType = await _reportTypeRepository.FindByIdAsync(int.Parse(report.ReportTypeId.ToString()));
            if (reportType == null)
            {
                // throw new HttpRequestException("ReportType không tồn tại");
                throw new HttpRequestException("ReportType is not exist, id: " + report.ReportTypeId);
            }

            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var newReport = new Report
                    {
                        ReportTypeId = report.ReportTypeId,
                        AccountId = accountId,
                        FacilityMajorId = majorId,
                        Content = report.Content
                    };

                    await _reportRepository.CreateAsync(newReport);

                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine("\n\n\n" + ex.Message.ToString() + "\n\n\n");
                    // throw new HttpRequestException("Tạo report thất bại: " + ex.Message);
                    throw new HttpRequestException("Failed to create report: " + ex.Message);
                }
            }
        }

        public async Task<JArray> GetMajorHeadReports(int accountId)
        {
            var account = await _unitOfWork.AccountRepository.FindById(accountId);
            if (account == null)
            {
                // throw new HttpRequestException("Account không tồn tại");
                throw new HttpRequestException("Account is not exist, id: " + accountId);
            }
            if (account.Role.Id != 2)
            {
                // throw new HttpRequestException("Account không phải là Head");
                throw new HttpRequestException("Account is not Major Head, id: " + accountId);
            }

            try
            {
                var assigneeFacilityMajorAssignments = await _unitOfWork.AssigneeFacilityMajorAssignmentRepository.FindByAccountId(accountId);
                var result = new List<object>();
                foreach (var afma in assigneeFacilityMajorAssignments)
                {
                    var major = afma.FacilityMajor;
                    var reports = await _unitOfWork.ReportRepository.FindByMajorId(major.Id);
                    foreach (var report in reports)
                    {
                        var accountReport = report.Account;
                        var facilityMajor = report.FacilityMajor;
                        var reportType = report.ReportType;
                        result.Add(new
                        {
                            Report = new
                            {
                                Id = report.Id,
                                Content = report.Content,
                                IsResolved = report.IsResolved,
                                ReportTypeId = report.ReportTypeId,
                                AccountId = report.AccountId,
                                FacilityMajorId = report.FacilityMajorId,
                                CreatedAt = report.CreatedAt
                            },
                            Account = new
                            {
                                FullName = accountReport.FullName,
                                Email = accountReport.Email,
                                Password = accountReport.Password,
                                Address = accountReport.Address,
                                DateOfBirth = accountReport.DateOfBirth,
                                Phone = accountReport.Phone,
                                RoleId = accountReport.RoleId,
                                JobTypeId = accountReport.JobTypeId,
                                ImageUrl = await GenerateImageUrl(_filePathConfig.ACCOUNt_IMAGE_PATH, accountReport.Id.ToString(), "main"),
                                IsDeactivated = accountReport.IsDeactivated,
                                CreatedAt = accountReport.CreatedAt
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
                                Created = facilityMajor.CreatedAt,
                                BackgroundImageUrl = await GenerateImageUrl(_filePathConfig.MAJOR_IMAGE_PATH, facilityMajor.Id.ToString(), "background"),
                                ImageUrl = await GenerateImageUrl(_filePathConfig.MAJOR_IMAGE_PATH, facilityMajor.Id.ToString(), "main")
                            },
                            MajorType = new
                            {
                                Id = facilityMajor.FacilityMajorType.Id,
                                Name = facilityMajor.FacilityMajorType.Name
                            },
                            ReportType = new
                            {
                                Id = reportType.Id,
                                Name = reportType.Name
                            }
                        });
                    }
                }
                return JArray.FromObject(
                        result
                        , new JsonSerializer
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        });
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n\n\n" + ex.Message.ToString() + "\n\n\n");
                // throw new HttpRequestException("Lấy report thất bại: " + ex.Message);
                throw new HttpRequestException("Failed to get reports: " + ex.Message);
            }
        }

        public async Task<JArray> GetReportsByMajor(int majorId)
        {
            var major = await _unitOfWork.FacilityMajorRepository.FindByIdAsync(majorId);
            if (major == null)
            {
                // throw new HttpRequestException("Major không tồn tại");
                throw new HttpRequestException("Major is not exist, id: " + majorId);
            }

            try
            {
                var reports = await _unitOfWork.ReportRepository.FindByMajorId(majorId);
                var result = new List<object>();
                foreach (var report in reports)
                {
                    var account = report.Account;
                    var reportType = report.ReportType;
                    result.Add(new
                    {
                        Report = new
                        {
                            Id = report.Id,
                            Content = report.Content,
                            IsResolved = report.IsResolved,
                            ReportTypeId = report.ReportTypeId,
                            AccountId = report.AccountId,
                            FacilityMajorId = report.FacilityMajorId,
                            CreatedAt = report.CreatedAt
                        },
                        Account = new
                        {
                            FullName = account.FullName,
                            Email = account.Email,
                            Password = account.Password,
                            Address = account.Address,
                            DateOfBirth = account.DateOfBirth,
                            Phone = account.Phone,
                            RoleId = account.RoleId,
                            JobTypeId = account.JobTypeId,
                            ImageUrl = await GenerateImageUrl(_filePathConfig.ACCOUNt_IMAGE_PATH, account.Id.ToString(), "main"),
                            IsDeactivated = account.IsDeactivated,
                            CreatedAt = account.CreatedAt
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
                            Created = major.CreatedAt,
                            BackgroundImageUrl = await GenerateImageUrl(_filePathConfig.MAJOR_IMAGE_PATH, major.Id.ToString(), "background"),
                            ImageUrl = await GenerateImageUrl(_filePathConfig.MAJOR_IMAGE_PATH, major.Id.ToString(), "main")
                        },
                        MajorType = new
                        {
                            Id = major.FacilityMajorType.Id,
                            Name = major.FacilityMajorType.Name
                        },
                        ReportType = new
                        {
                            Id = reportType.Id,
                            Name = reportType.Name
                        }
                    });
                }
                return JArray.FromObject(
                        result
                        , new JsonSerializer
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        });
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n\n\n" + ex.Message.ToString() + "\n\n\n");
                // throw new HttpRequestException("Lấy report thất bại: " + ex.Message);
                throw new HttpRequestException("Failed to get reports: " + ex.Message);
            }
        }

        public async Task<JObject> GetReportDetail(int reportId)
        {
            var report = await _unitOfWork.ReportRepository.FindByIdAsync(reportId);
            if (report == null)
            {
                // throw new HttpRequestException("Report không tồn tại");
                throw new HttpRequestException("Report is not exist, id: " + reportId);
            }

            try
            {
                var account = report.Account;
                var facilityMajor = report.FacilityMajor;
                var reportType = report.ReportType;
                return JObject.FromObject(new
                {
                    Report = new
                    {
                        Id = report.Id,
                        Content = report.Content,
                        IsResolved = report.IsResolved,
                        ReportTypeId = report.ReportTypeId,
                        AccountId = report.AccountId,
                        FacilityMajorId = report.FacilityMajorId,
                        CreatedAt = report.CreatedAt
                    },
                    Account = new
                    {
                        FullName = account.FullName,
                        Email = account.Email,
                        Password = account.Password,
                        Address = account.Address,
                        DateOfBirth = account.DateOfBirth,
                        Phone = account.Phone,
                        RoleId = account.RoleId,
                        JobTypeId = account.JobTypeId,
                        ImageUrl = await GenerateImageUrl(_filePathConfig.ACCOUNt_IMAGE_PATH, account.Id.ToString(), "main"),
                        IsDeactivated = account.IsDeactivated,
                        CreatedAt = account.CreatedAt
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
                        Created = facilityMajor.CreatedAt,
                        BackgroundImageUrl = await GenerateImageUrl(_filePathConfig.MAJOR_IMAGE_PATH, facilityMajor.Id.ToString(), "background"),
                        ImageUrl = await GenerateImageUrl(_filePathConfig.MAJOR_IMAGE_PATH, facilityMajor.Id.ToString(), "main")
                    },
                    MajorType = new
                    {
                        Id = facilityMajor.FacilityMajorType.Id,
                        Name = facilityMajor.FacilityMajorType.Name
                    },
                    ReportType = new
                    {
                        Id = reportType.Id,
                        Name = reportType.Name
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n\n\n" + ex.Message.ToString() + "\n\n\n");
                // throw new HttpRequestException("Lấy report thất bại: " + ex.Message);
                throw new HttpRequestException("Failed to get report: " + ex.Message);
            }
        }

        public async Task ResovleReport(int reportId)
        {
            var report = await _unitOfWork.ReportRepository.FindByIdAsync(reportId);
            if (report == null)
            {
                // throw new HttpRequestException("Report không tồn tại");
                throw new HttpRequestException("Report is not exist, id: " + reportId);
            }

            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {

                    await _unitOfWork.ReportRepository.Resolve(reportId, true);

                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine("\n\n\n" + ex.Message.ToString() + "\n\n\n");
                    // throw new HttpRequestException("Giải quyết report thất bại: " + ex.Message);
                    throw new HttpRequestException("Failed to resolve report: " + ex.Message);
                }
            }
        }

        public async Task<JArray> GetReportTypes()
        {
            var reportTypes = await _reportTypeRepository.FindAllAsync();
            return JArray.FromObject(reportTypes);
        }

        public async Task<JArray> GetFacilityMajorTypes()
        {
            var majorTypes = await _majorTypeRepository.FindAllAsync();
            return JArray.FromObject(majorTypes);
        }
    }
}
