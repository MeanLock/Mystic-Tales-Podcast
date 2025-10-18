using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Architecture_1.BusinessLogic.Helpers;
using Architecture_1.Common.AppConfigurations.App.interfaces;
using Architecture_1.Common.AppConfigurations.FileStorage.interfaces;
using Architecture_1.Common.AppConfigurations.Jwt;
using Architecture_1.DataAccess.Data;
using Architecture_1.DataAccess.Entities;
using Architecture_1.DataAccess.Repositories.interfaces;
using Architecture_1.DataAccess.UOW;
using Architecture_1.Infrastructure.Services.AWS.S3;

namespace Architecture_1.BusinessLogic.Services.DbServices.FacilityMajorServices
{
    public class MajorAssignmentService
    {
        // LOGGER
        private readonly ILogger<MajorAssignmentService> _logger;

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
        public readonly IGenericRepository<AssigneeFacilityMajorAssignment> _assigneeFacilityMajorAssignmentRepository;
        private readonly IGenericRepository<ServiceRequest> _serviceRequestRepository;

        // AWS SERVICE
        private readonly AWSS3Service _awsS3Service;

        public MajorAssignmentService(
            ILogger<MajorAssignmentService> logger,
            AppDbContext dbContext,
            FileHelpers fileHelpers,
            IUnitOfWork unitOfWork,
            IFilePathConfig filePathConfig,
            IGenericRepository<AssigneeFacilityMajorAssignment> assigneeFacilityMajorAssignmentRepository,
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
            _assigneeFacilityMajorAssignmentRepository = assigneeFacilityMajorAssignmentRepository;
            _serviceRequestRepository = serviceRequestRepository;
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

        public async Task<JArray> GetMajorHeadStaffs(int accountId)
        {
            var accountExist = await _unitOfWork.AccountRepository.FindById(accountId);
            if (accountExist == null)
            {
                throw new HttpRequestException("Account is not exist, id: " + accountId);
            }
            // if (accountExist.Role.Id != 2)
            // {
            //     throw new HttpRequestException("Account is not Facility Head, id: " + accountId);
            // }
            try
            {
                var majors = await _unitOfWork.AssigneeFacilityMajorAssignmentRepository.FindByAccountId(accountId);
                var result = new List<object>();
                foreach (var major in majors)
                {
                    var majorAssignment = major.FacilityMajor;
                    var headStaffs = (await _unitOfWork.AssigneeFacilityMajorAssignmentRepository.FindByFacilityMajorId(major.FacilityMajorId)).Where(a => a.IsHead == false);
                    var headStaffsResult = new List<object>();
                    foreach (var headStaff in headStaffs)
                    {

                        headStaffsResult.Add(new
                        {
                            Account = new
                            {
                                Id = headStaff.Account.Id,
                                FullName = headStaff.Account.FullName,
                                Email = headStaff.Account.Email,
                                DateOfBirth = headStaff.Account.DateOfBirth,
                                Phone = headStaff.Account.Phone,
                                Address = headStaff.Account.Address,
                                RoleId = headStaff.Account.RoleId,
                                JobTypeId = headStaff.Account.JobTypeId,
                                ImageUrl = await GenerateImageUrl(_filePathConfig.ACCOUNt_IMAGE_PATH, headStaff.Account.Id.ToString(), "main"),
                                IsDeactivated = headStaff.Account.IsDeactivated,
                                CreatedAt = headStaff.Account.CreatedAt
                            },
                            Role = new
                            {
                                Id = headStaff.Account.Role.Id,
                                Name = headStaff.Account.Role.Name
                            },
                            JobType = new
                            {
                                Id = headStaff.Account.JobType.Id,
                                Name = headStaff.Account.JobType.Name
                            },
                            MajorAssignment = new
                            {
                                AccountId = headStaff.AccountId,
                                FacilityMajorId = headStaff.FacilityMajorId,
                                IsHead = headStaff.IsHead,
                                WorkDescription = headStaff.WorkDescription
                            }
                        });

                    }
                    result.Add(new
                    {
                        Major = new
                        {
                            Id = majorAssignment.Id,
                            Name = majorAssignment.Name,
                            MainDescription = majorAssignment.MainDescription,
                            WorkShiftsDescription = majorAssignment.WorkShiftsDescription,
                            FacilityMajorTypeId = majorAssignment.FacilityMajorTypeId,
                            FacilityId = majorAssignment.FacilityId,
                            IsOpen = majorAssignment.IsOpen,
                            CloseScheduleDate = majorAssignment.CloseScheduleDate,
                            OpenScheduleDate = majorAssignment.OpenScheduleDate,
                            IsDeactivated = majorAssignment.IsDeactivated,
                            CreatedAt = majorAssignment.CreatedAt,
                            BackgroundImageUrl = await GenerateImageUrl(_filePathConfig.MAJOR_IMAGE_PATH, majorAssignment.Id.ToString(), "background"),
                            ImageUrl = await GenerateImageUrl(_filePathConfig.MAJOR_IMAGE_PATH, majorAssignment.Id.ToString(), "main"),
                        },
                        Accounts = headStaffsResult
                    });


                }

                return JArray.FromObject(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetMajorHeadStaffs");
                Console.WriteLine("\n\n\n" + ex.Message.ToString() + "\n\n\n");
                // throw new HttpRequestException("Lấy danh sách Staff thất bại: " + ex.Message);
                throw new HttpRequestException("Failed to get list of Staff: " + ex.Message);
            }
        }

        public async Task<JArray> GetMajorStaffs(int majorId, bool isHead)
        {
            var major = await _unitOfWork.FacilityMajorRepository.FindByIdAsync(majorId);
            if (major == null)
            {
                // throw new HttpRequestException("Major không tồn tại");
                throw new HttpRequestException("Major is not exist, id: " + majorId);
            }
            try
            {
                var staffs = (await _unitOfWork.AssigneeFacilityMajorAssignmentRepository.FindByFacilityMajorId(majorId)).Where(a => a.IsHead == isHead);
                var result = new List<object>();
                foreach (var staff in staffs)
                {
                    result.Add(new
                    {
                        Account = new
                        {
                            Id = staff.Account.Id,
                            FullName = staff.Account.FullName,
                            Email = staff.Account.Email,
                            DateOfBirth = staff.Account.DateOfBirth,
                            Phone = staff.Account.Phone,
                            Address = staff.Account.Address,
                            RoleId = staff.Account.RoleId,
                            JobTypeId = staff.Account.JobTypeId,
                            ImageUrl = await GenerateImageUrl(_filePathConfig.ACCOUNt_IMAGE_PATH, staff.Account.Id.ToString(), "main"),
                            IsDeactivated = staff.Account.IsDeactivated,
                            CreatedAt = staff.Account.CreatedAt
                        },
                        Role = new
                        {
                            Id = staff.Account.Role.Id,
                            Name = staff.Account.Role.Name
                        },
                        JobType = new
                        {
                            Id = staff.Account.JobType.Id,
                            Name = staff.Account.JobType.Name
                        },
                        MajorAssignment = new
                        {
                            AccountId = staff.AccountId,
                            FacilityMajorId = staff.FacilityMajorId,
                            IsHead = staff.IsHead,
                            WorkDescription = staff.WorkDescription
                        }
                    });

                }
                return JArray.FromObject(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetMajorStaffs");
                Console.WriteLine("\n\n\n" + ex.Message.ToString() + "\n\n\n");
                // throw new HttpRequestException("Lấy danh sách Staff thất bại: " + ex.Message);
                throw new HttpRequestException("Failed to get list of Staff: " + ex.Message);
            }
        }

        public async Task<JArray> GetStaffAssignments(int accountId)
        {
            var accountExist = await _unitOfWork.AccountRepository.FindById(accountId);
            if (accountExist == null)
            {
                // throw new HttpRequestException("Account không tồn tại");
                throw new HttpRequestException("Account is not exist, id: " + accountId);
            }
            try
            {
                var assignments = await _unitOfWork.AssigneeFacilityMajorAssignmentRepository.FindByAccountId(accountId);
                var result = new List<object>();
                foreach (var assignment in assignments)
                {
                    var major = assignment.FacilityMajor;
                    result.Add(new
                    {
                        MajorAssignment = new
                        {
                            AccountId = assignment.AccountId,
                            FacilityMajorId = assignment.FacilityMajorId,
                            IsHead = assignment.IsHead,
                            WorkDescription = assignment.WorkDescription
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
                        }
                    });
                }
                return JArray.FromObject(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetStaffAssignments");
                Console.WriteLine("\n\n\n" + ex.Message.ToString() + "\n\n\n");
                // throw new HttpRequestException("Lấy danh sách major assignment của staff thất bại: " + ex.Message);
                throw new HttpRequestException("Failed to get list of major assignment of staff: " + ex.Message);
            }
        }

        public async Task AssignStaff(int accountId, List<int> majorIds)
        {
            var accountExist = await _unitOfWork.AccountRepository.FindById(accountId);
            if (accountExist == null)
            {
                // throw new HttpRequestException("Account không tồn tại");
                throw new HttpRequestException("Account is not exist, id: " + accountId);
            }
            foreach (var majorId in majorIds)
            {
                var majorExist = await _unitOfWork.FacilityMajorRepository.FindByIdAsync(majorId);
                if (majorExist == null)
                {
                    // throw new HttpRequestException("Major không tồn tại, id: " + majorId);
                    throw new HttpRequestException("Major is not exist, id: " + majorId);}
            }

            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var assignments = await _unitOfWork.AssigneeFacilityMajorAssignmentRepository.FindByAccountId(accountId);
                    await _unitOfWork.AssigneeFacilityMajorAssignmentRepository.DeleteByAccountId(accountId);
                    foreach (var majorId in majorIds)
                    {

                        var newAssignment = new AssigneeFacilityMajorAssignment
                        {
                            AccountId = accountId,
                            FacilityMajorId = majorId,
                            IsHead = accountExist.Role.Id == 2,
                            WorkDescription = null
                        };
                        await _assigneeFacilityMajorAssignmentRepository.CreateAsync(newAssignment);
                    }

                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error in AssignStaff");
                    Console.WriteLine("\n\n\n" + ex.Message.ToString() + "\n\n\n");
                    // throw new HttpRequestException("Phân công nhân viên thất bại: " + ex.Message);
                    throw new HttpRequestException("Failed to assign staff: " + ex.Message);
                }
            }
        }

        public async Task UpdateStaffAssignment(int accountId, int majorId, string workDescription)
        {
            var accountExist = await _unitOfWork.AccountRepository.FindById(accountId);
            if (accountExist == null)
            {
                // throw new HttpRequestException("Account không tồn tại");
                throw new HttpRequestException("Account is not exist, id: " + accountId);
            }
            var majorExist = await _unitOfWork.FacilityMajorRepository.FindByIdAsync(majorId);
            if (majorExist == null)
            {
                // throw new HttpRequestException("Major không tồn tại");
                throw new HttpRequestException("Major is not exist, id: " + majorId);
            }
            var assignment = await _unitOfWork.AssigneeFacilityMajorAssignmentRepository.FindByFacilityMajorIdAndAccountId(majorId, accountId);
            if (assignment == null)
            {
                // throw new HttpRequestException("Assignment không tồn tại");
                throw new HttpRequestException("Assignment is not exist, majorId: " + majorId + ", accountId: " + accountId);
            }
            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    assignment.WorkDescription = workDescription;
                    await _unitOfWork.AssigneeFacilityMajorAssignmentRepository.UpdateAsync(assignment);
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error in UpdateStaffAssignment");
                    Console.WriteLine("\n\n\n" + ex.Message.ToString() + "\n\n\n");
                    // throw new HttpRequestException("Cập nhật assignment thất bại: " + ex.Message);
                    throw new HttpRequestException("Failed to update assignment: " + ex.Message);
                }
            }

        }

        public async Task DeleteStaffAssignment(int accountId, int majorId)
        {
            var accountExist = await _unitOfWork.AccountRepository.FindById(accountId);
            if (accountExist == null)
            {
                // throw new HttpRequestException("Account không tồn tại");
                throw new HttpRequestException("Account is not exist, id: " + accountId);
            }
            var majorExist = await _unitOfWork.FacilityMajorRepository.FindByIdAsync(majorId);
            if (majorExist == null)
            {
                // throw new HttpRequestException("Major không tồn tại");
                throw new HttpRequestException("Major is not exist, id: " + majorId);
            }
            var assignment = await _unitOfWork.AssigneeFacilityMajorAssignmentRepository.FindByFacilityMajorIdAndAccountId(majorId, accountId);
            if (assignment == null)
            {
                // throw new HttpRequestException("Assignment không tồn tại");
                throw new HttpRequestException("Assignment is not exist, majorId: " + majorId + ", accountId: " + accountId);
            }
            try
            {
                if (accountExist.RoleId == 3)
                {
                    var serviceRequests = await _unitOfWork.ServiceRequestRepository.FindByAssignedAssigneeId(accountId);
                    foreach (var serviceRequest in serviceRequests)
                    {
                        if (serviceRequest.RequestStatusId != 7 && serviceRequest.RequestStatusId != 8 && serviceRequest.RequestStatusId != 9)
                        {
                            serviceRequest.RequestStatusId = 4;
                            serviceRequest.AssignedAssigneeId = null;
                            await _serviceRequestRepository.UpdateAsync(serviceRequest.Id, serviceRequest);
                        }
                    }
                }
                else if (accountExist.RoleId == 2)
                {
                    var majors = await _unitOfWork.AssigneeFacilityMajorAssignmentRepository.FindByAccountId(accountId);
                    foreach (var major in majors)
                    {
                        var majorAssignments = (await _unitOfWork.AssigneeFacilityMajorAssignmentRepository.FindByFacilityMajorId(major.FacilityMajorId)).Where(x => x.IsHead == true && x.Account.IsDeactivated == false && x.AccountId != accountId);
                        if (majorAssignments.Count() < 1)
                        {
                            // throw new HttpRequestException("phải có ít nhất 1 head phụ trách cho major id " + major.FacilityMajorId);
                            throw new HttpRequestException("There must be at least 1 head in charge of major id " + major.FacilityMajorId);
                        }
                    }

                }           
                await _unitOfWork.AssigneeFacilityMajorAssignmentRepository.DeleteAsync(assignment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteStaffAssignment");
                Console.WriteLine("\n\n\n" + ex.Message.ToString() + "\n\n\n");
                // throw new HttpRequestException("Xóa assignment thất bại: " + ex.Message);
                throw new HttpRequestException("Failed to delete assignment: " + ex.Message);
            }
        }

    }
}
