using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Microsoft.EntityFrameworkCore;
using Architecture_1.BusinessLogic.Helpers;
using Architecture_1.Common.AppConfigurations.Jwt;
using Architecture_1.DataAccess.Data;
using Architecture_1.DataAccess.Entities;
using Architecture_1.DataAccess.Repositories.interfaces;
using Architecture_1.DataAccess.UOW;
using Architecture_1.Common.AppConfigurations.FileStorage.interfaces;
using Architecture_1.Infrastructure.Services.AWS.S3;
using Architecture_1.Common.AppConfigurations.App.interfaces;

namespace Architecture_1.BusinessLogic.Services.DbServices.FacilityServices
{
    public class FacilityItemService
    {
        // LOGGER
        private readonly ILogger<FacilityItemService> _logger;

        // CONFIG
        public readonly IAppConfig _appConfig;
        private readonly IFilePathConfig _filePathConfig;

        // DB CONTEXT
        private readonly AppDbContext _dbContext;

        // HELPERS
        private readonly FileHelpers _fileHelpers;

        // UNIT OF WORK
        private readonly IUnitOfWork _unitOfWork;

        // REPOSITORIES
        private readonly IGenericRepository<FacilityMajor> _facilityMajorRepository;
        private readonly IGenericRepository<Facility> _facilityRepository;
        private readonly IGenericRepository<FacilityItem> _facilityItemRepository;
        private readonly IGenericRepository<FacilityItemAssignment> _facilityItemAssignmentRepository;

        // AWS SERVICE
        private readonly AWSS3Service _awsS3Service;


        public FacilityItemService(
            ILogger<FacilityItemService> logger,
            AppDbContext dbContext,
            FileHelpers fileHelpers,
            IUnitOfWork unitOfWork,
            IGenericRepository<Facility> facilityRepository,
            IGenericRepository<FacilityItem> facilityItemRepository,
            IFilePathConfig filePathConfig,
            IGenericRepository<FacilityItemAssignment> facilityItemAssignmentRepository,
            IGenericRepository<FacilityMajor> facilityMajorRepository,
            AWSS3Service awsS3Service,
            IAppConfig appConfig
            )
        {
            _logger = logger;
            _dbContext = dbContext;
            _fileHelpers = fileHelpers;
            _unitOfWork = unitOfWork;

            _facilityRepository = facilityRepository;
            _facilityItemRepository = facilityItemRepository;
            _filePathConfig = filePathConfig;
            _facilityItemAssignmentRepository = facilityItemAssignmentRepository;
            _facilityMajorRepository = facilityMajorRepository;
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

        public async Task<JArray> GetFacilityItems()
        {
            var facilityItems = await _facilityItemRepository.FindAllAsync();

            var result = new List<object>();

            foreach (var facilityItem in facilityItems)
            {
                string folderPath = _filePathConfig.ITEM_IMAGE_PATH;
                int inUseCount = await _unitOfWork.FacilityItemAssignmentRepository.GetInUseItemCount(facilityItem.Id);

                result.Add(new
                {
                    Item = new
                    {
                        Id = facilityItem.Id,
                        Name = facilityItem.Name,
                        InUseCount = inUseCount,
                        Count = facilityItem.Count,
                        ImageUrl = await GenerateImageUrl(folderPath, facilityItem.Id.ToString(), "main"),
                        CreatedAt = facilityItem.CreatedAt,
                        UpdatedAt = facilityItem.UpdatedAt
                    }

                });

            }

            return JArray.FromObject(result);
        }

        public async Task CreateFacilityItem(dynamic facilityItem)
        {
            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var newFacilityItem = new FacilityItem
                    {
                        Name = facilityItem.Name,
                        Count = facilityItem.Count
                    };
                    await _facilityItemRepository.CreateAsync(newFacilityItem);

                    if (facilityItem.Image != null && facilityItem.Image != "")
                    {
                        string folderPath = _filePathConfig.ITEM_IMAGE_PATH + "\\" + newFacilityItem.Id;
                        string fileName = "main";
                        string base64Data = facilityItem.Image;

                        await SaveBase64File(base64Data, folderPath, fileName);
                    }
                    else
                    {
                        string folderPath = _filePathConfig.ITEM_IMAGE_PATH + "\\" + newFacilityItem.Id;
                        await CopyFile(_filePathConfig.ITEM_IMAGE_PATH, "unknown", folderPath, "main");
                    }

                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine("\n\n\n" + ex.Message.ToString() + "\n\n\n");
                    throw new HttpRequestException("Failed to create facility item: " + ex.Message);
                }
            }
        }

        public async Task<JObject> GetFacilityItemDetail(int facilityItemId)
        {
            var facilityItem = await _facilityItemRepository.FindByIdAsync(facilityItemId);
            if (facilityItem == null)
            {
                throw new HttpRequestException("Facility item is not exist, id: " + facilityItemId);
            }

            string folderPath = _filePathConfig.ITEM_IMAGE_PATH;
            int inUseCount = await _unitOfWork.FacilityItemAssignmentRepository.GetInUseItemCount(facilityItem.Id);
            return JObject.FromObject(new
            {
                Item = new
                {
                    Id = facilityItem.Id,
                    Name = facilityItem.Name,
                    InUseCount = inUseCount,
                    Count = facilityItem.Count,
                    ImageUrl = await GenerateImageUrl(folderPath, facilityItem.Id.ToString(), "main"),
                    CreatedAt = facilityItem.CreatedAt,
                    UpdatedAt = facilityItem.UpdatedAt
                }
            });
        }

        public async Task UpdateFacilityItem(int facilityItemId, dynamic facilityItem)
        {
            var existingFacilityItem = await _facilityItemRepository.FindByIdAsync(facilityItemId);
            if (existingFacilityItem == null)
            {
                throw new HttpRequestException("Facility item is not exist, id: " + facilityItemId);
            }
            var inUseCount = await _unitOfWork.FacilityItemAssignmentRepository.GetInUseItemCount(facilityItemId);
            if (facilityItem.Count < inUseCount)
            {
                throw new HttpRequestException("The number of items in used is greater than the number of items updated, id: " + facilityItemId);
            }


            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    existingFacilityItem.Name = facilityItem.Name;
                    existingFacilityItem.Count = facilityItem.Count;


                    await _facilityItemRepository.UpdateAsync(facilityItemId, existingFacilityItem);

                    if (facilityItem.Image != null && facilityItem.Image != "")
                    {
                        string folderPath = _filePathConfig.ITEM_IMAGE_PATH + "\\" + existingFacilityItem.Id;
                        string fileName = "main";
                        string base64Data = facilityItem.Image;

                        await SaveBase64File(base64Data, folderPath, fileName);
                    }
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine("\n\n\n" + ex.Message.ToString() + "\n\n\n");
                    throw new HttpRequestException("Failed to update facility item: " + ex.Message);
                }
            }
        }

        public async Task DeleteFacilityItem(int facilityItemId)
        {
            var existingFacilityItem = await _facilityItemRepository.FindByIdAsync(facilityItemId);
            if (existingFacilityItem == null)
            {
                throw new HttpRequestException("Facility item is not exist, id: " + facilityItemId);
            }

            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var facilityItemAssignments = await _unitOfWork.FacilityItemAssignmentRepository.FindByFacilityItemId(facilityItemId);
                    Console.WriteLine("\n\n 1, " + facilityItemAssignments.Count());
                    if (facilityItemAssignments.Count() > 0)
                    {
                        await _unitOfWork.FacilityItemAssignmentRepository.DeleteRangeAsync(facilityItemAssignments);
                    }
                    await _facilityItemRepository.DeleteAsync(facilityItemId);
                    string folderPath = _filePathConfig.ITEM_IMAGE_PATH + "\\" + existingFacilityItem.Id;
                    string fileName = "main";
                    await DeleteFolder(folderPath);

                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine("\n\n\n" + ex.Message.ToString() + "\n\n\n");
                    // throw new HttpRequestException("Xóa vật tư thất bại: " + ex.Message);
                    throw new HttpRequestException("Failed to delete facility item: " + ex.Message);
                }
            }
        }

        public async Task AddFacilityItem(int facilityItemId, int count)
        {
            var existingFacilityItem = await _facilityItemRepository.FindByIdAsync(facilityItemId);
            if (existingFacilityItem == null)
            {
                // throw new HttpRequestException("Vật tư không tồn tại");
                throw new HttpRequestException("Facility item is not exist, id: " + facilityItemId);
            }

            try
            {
                existingFacilityItem.Count += count;

                await _facilityItemRepository.UpdateAsync(facilityItemId, existingFacilityItem);
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n\n\n" + ex.Message.ToString() + "\n\n\n");
                // throw new HttpRequestException("Thêm vật tư thất bại: " + ex.Message);
                throw new HttpRequestException("Failed to add facility item: " + ex.Message);
            }
        }

        public async Task SubtractFacilityItem(int facilityItemId, int count)
        {
            var existingFacilityItem = await _facilityItemRepository.FindByIdAsync(facilityItemId);

            if (existingFacilityItem == null)
            {
                // throw new HttpRequestException("Vật tư không tồn tại");
                throw new HttpRequestException("Facility item is not exist, id: " + facilityItemId);
            }
            existingFacilityItem.Count -= count;

            var inUseCount = await _unitOfWork.FacilityItemAssignmentRepository.GetInUseItemCount(facilityItemId);
            if (existingFacilityItem.Count < inUseCount)
            {
                // throw new HttpRequestException("Số lượng vật tư đang sử dụng nhiều hơn số lượng cập nhật");
                throw new HttpRequestException("The number of items in used is greater than the number of items updated, id: " + facilityItemId);
            }

            try
            {
                await _facilityItemRepository.UpdateAsync(facilityItemId, existingFacilityItem);
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n\n\n" + ex.Message.ToString() + "\n\n\n");
                // throw new HttpRequestException("Xóa vật tư thất bại: " + ex.Message);
                throw new HttpRequestException("Failed to subtract facility item: " + ex.Message);
            }
        }

        public async Task<JArray> GetFacilityItemAssignment(int facilityItemId)
        {
            var existingFacilityItem = await _facilityItemRepository.FindByIdAsync(facilityItemId);

            if (existingFacilityItem == null)
            {
                // throw new HttpRequestException("Vật tư không tồn tại");
                throw new HttpRequestException("Facility item is not exist, id: " + facilityItemId);
            }


            var facilityItemAssignments = await _facilityItemAssignmentRepository.FindAllAsync();
            var result = new List<object>();
            foreach (var facilityItemAssignment in facilityItemAssignments)
            {
                if (facilityItemAssignment.FacilityItemId == facilityItemId)
                {
                    var major = await _facilityMajorRepository.FindByIdAsync(facilityItemAssignment.FacilityMajorId);
                    var majorFolderPath = _filePathConfig.MAJOR_IMAGE_PATH;
                    result.Add(new
                    {
                        FacilityItemAssignment = facilityItemAssignment,
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
                            BackgroundImageUrl = await GenerateImageUrl(majorFolderPath, major.Id.ToString(), "background"),
                            ImageUrl = await GenerateImageUrl(majorFolderPath, major.Id.ToString(), "main")
                        }
                    });
                }
            }
            return JArray.FromObject(result);
        }


        public async Task CreateFacilityItemAssignment(int itemId, int Count, List<int> MajorIds)
        {
            var existingFacilityItem = await _facilityItemRepository.FindByIdAsync(itemId);
            if (existingFacilityItem == null)
            {
                // throw new HttpRequestException("Vật tư không tồn tại");
                throw new HttpRequestException("Facility item is not exist, id: " + itemId);
            }
            var inUseCount = await _unitOfWork.FacilityItemAssignmentRepository.GetInUseItemCount(itemId);



            if (existingFacilityItem.Count < inUseCount + Count * MajorIds.Count())
            {
                // throw new HttpRequestException("Số lượng vật tư đang sử dụng nhiều hơn số lượng cập nhật");
                throw new HttpRequestException("the number of items required to assign greater than inventory quantity, id: " + itemId);
            }

            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var facilityItemAssignments = await _unitOfWork.FacilityItemAssignmentRepository.FindByFacilityItemId(itemId);
                    foreach (var majorId in MajorIds)
                    {
                        var existingFacilityMajor = await _facilityMajorRepository.FindByIdAsync(majorId);
                        if (existingFacilityMajor == null)
                        {
                            // throw new HttpRequestException("Major không tồn tại");
                            throw new HttpRequestException("Facility major is not exist, id: " + majorId);
                        }
                        if (existingFacilityMajor.IsDeactivated == true)
                        {
                            // throw new HttpRequestException("Major đã bị khóa");
                            throw new HttpRequestException("Facility major is deactivated, id: " + majorId);
                        }

                        if (facilityItemAssignments.Any(fia => fia.FacilityMajorId == majorId))
                        {
                            var existingFacilityItemAssignment = facilityItemAssignments.First(fia => fia.FacilityMajorId == majorId);
                            existingFacilityItemAssignment.ItemCount += Count;
                            await _unitOfWork.FacilityItemAssignmentRepository.UpdateAsync(existingFacilityItemAssignment);



                        }
                        else
                        {
                            var newFacilityItemAssignment = new FacilityItemAssignment
                            {
                                FacilityItemId = itemId,
                                FacilityMajorId = majorId,
                                ItemCount = Count
                            };
                            await _facilityItemAssignmentRepository.CreateAsync(newFacilityItemAssignment);
                        }

                    }
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine("\n\n\n" + ex.Message.ToString() + "\n\n\n");
                    // throw new HttpRequestException("Thêm vật tư vào các facility major thất bại: " + ex.Message);
                    throw new HttpRequestException("Failed to add facility item to facility majors: " + ex.Message);
                }
            }

        }
    }
}
