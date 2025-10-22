using Microsoft.Extensions.Logging;
using SystemConfigurationService.Common.AppConfigurations.App.interfaces;
using SystemConfigurationService.Common.AppConfigurations.FilePath.interfaces;
using SystemConfigurationService.DataAccess.Data;
using SystemConfigurationService.DataAccess.UOW;
using SystemConfigurationService.BusinessLogic.DTOs.Survey.Session.V2;
using SystemConfigurationService.DataAccess.Entities;
using SystemConfigurationService.BusinessLogic.DTOs.Survey.Filters;
using Newtonsoft.Json.Linq;
using SystemConfigurationService.BusinessLogic.Exceptions;
using Newtonsoft.Json;
using SystemConfigurationService.DataAccess.Repositories.interfaces;
using SystemConfigurationService.BusinessLogic.Enums;
using SystemConfigurationService.BusinessLogic.DTOs.Survey.JsonConfigs;
using SystemConfigurationService.BusinessLogic.Helpers.AuthHelpers;
using SystemConfigurationService.BusinessLogic.Helpers.FileHelpers;
using SystemConfigurationService.BusinessLogic.Helpers.DateHelpers;

namespace SystemConfigurationService.BusinessLogic.Services.DbServices.SurveyServices
{
    public class SurveySessionService
    {
        // LOGGER
        private readonly ILogger<SurveySessionService> _logger;

        // CONFIG
        public readonly IAppConfig _appConfig;
        private readonly IFilePathConfig _filePathConfig;

        // DB CONTEXT
        private readonly AppDbContext _appDbContext;

        // Helper
        private readonly BcryptHelper _bcryptHelper;
        private readonly JwtHelper _jwtHelper;
        private readonly FileIOHelper _fileIOHelper;
        private readonly DateHelper _dateHelper;


        // UNIT OF WORK
        private readonly IUnitOfWork _unitOfWork;

        // REPOSITORIES
        private readonly IGenericRepository<Survey> _surveyGenericRepository;
        private readonly IGenericRepository<SurveyQuestion> _surveyQuestionGenericRepository;
        private readonly IGenericRepository<SurveyOption> _surveyOptionGenericRepository;


        public SurveySessionService(
            ILogger<SurveySessionService> logger,
            AppDbContext appDbContext,
            BcryptHelper bcryptHelper,
            JwtHelper jwtHelper,
            DateHelper dateHelper,
            IUnitOfWork unitOfWork,

            IGenericRepository<SurveyQuestion> surveyQuestionGenericRepository,
            IGenericRepository<SurveyOption> surveyOptionGenericRepository,
            IGenericRepository<Survey> surveyGenericRepository,

            FileIOHelper fileIOHelper,
            IFilePathConfig filePathConfig,
            IAppConfig appConfig
            )
        {
            _logger = logger;
            _appDbContext = appDbContext;
            _bcryptHelper = bcryptHelper;
            _jwtHelper = jwtHelper;
            _dateHelper = dateHelper;
            _unitOfWork = unitOfWork;

            _surveyQuestionGenericRepository = surveyQuestionGenericRepository;
            _surveyOptionGenericRepository = surveyOptionGenericRepository;
            _surveyGenericRepository = surveyGenericRepository;

            _fileIOHelper = fileIOHelper;
            _filePathConfig = filePathConfig;

            _appConfig = appConfig;
        }
        public int RandomNumber10To15()
        {
            var rnd = new Random();
            return rnd.Next(10, 16); // 16 là exclusive, nên sẽ random từ 10 đến 15
        }

        public async Task<SurveyEditingSessionDTO> GenerateSurveyEditingSessionDTO(Survey survey, int? version)
        {
            try
            {
                if (survey == null)
                {
                    throw new Exception("survey is null");
                }
                Console.WriteLine("GenerateSurveyEditingSessionDTO for survey id: " + survey.SurveyStatusTrackings.OrderByDescending(sst => sst.CreatedAt).FirstOrDefault()?.SurveyId + " and version: " + version);
                SurveyEditingSessionDTO surveyEditingSessionDTO = new SurveyEditingSessionDTO
                {
                    Id = survey.Id,
                    RequesterId = survey.RequesterId,
                    Title = survey.Title,
                    Description = survey.Description,
                    SurveyTypeId = survey.SurveyTypeId,
                    SurveyTopicId = survey.SurveyTopicId,
                    SurveySpecificTopicId = survey.SurveySpecificTopicId,
                    MainImageBase64 = await _fileIOHelper.ConvertFileToBase64Async(FilePathHelper.CombinePaths(_filePathConfig.SURVEY_ORIGINAL_IMAGE_PATH, survey.Id.ToString())),
                    BackgroundImageBase64 = await _fileIOHelper.ConvertFileToBase64Async(FilePathHelper.CombinePaths(_filePathConfig.SURVEY_ORIGINAL_IMAGE_PATH, survey.Id.ToString())),
                    SurveyStatusId = (await _unitOfWork.SurveyRepository.GetLatestSurveyStatusTrackingBySurveyIdAsync(survey.Id)).SurveyStatusId,
                    Version = survey.SurveyTypeId == 3 ? version : null,
                    MarketSurveyVersionStatusId = survey.SurveyMarketVersionStatusTrackings.OrderByDescending(smt => smt.CreatedAt).FirstOrDefault()?.SurveyStatusId,
                    SecurityModeId = survey.SecurityModeId,
                    ConfigJson = JObject.Parse(survey.ConfigJsonString).ToObject<SurveyEditingSessionSurveyConfigJsonDTO>(),
                    Questions = (await Task.WhenAll(survey.SurveyQuestions.Select(async sq =>
                    {
                        try
                        {
                            return new SurveyEditingSessionQuestionDTO
                            {
                                Id = sq.Id,
                                QuestionTypeId = sq.QuestionTypeId,
                                MainImageBase64 = await _fileIOHelper.ConvertFileToBase64Async(FilePathHelper.CombinePaths(_filePathConfig.SURVEY_ORIGINAL_IMAGE_PATH, survey.Id.ToString(), "question_" + sq.Id)),
                                Version = sq.Version,
                                IsReanswerRequired = sq.IsReanswerRequired,
                                ReferenceSurveyQuestionId = sq.ReferenceSurveyQuestionId,
                                Content = sq.Content,
                                Description = sq.Description,
                                TimeLimit = survey.SecurityModeId == 1 ? null : sq.TimeLimit,
                                IsVoiced = sq.IsVoiced,
                                Order = sq.Order,
                                ConfigJson = JObject.Parse(sq.ConfigJsonString).ToObject<SurveyEditingSessionQuestionConfigJsonDTO>(),
                                Options = (await Task.WhenAll(sq.SurveyOptions.Select(async so => new SurveyEditingSessionOptionDTO
                                {
                                    Id = so.Id,
                                    Content = so.Content,
                                    Order = so.Order,
                                            MainImageBase64 = await _fileIOHelper.ConvertFileToBase64Async(FilePathHelper.CombinePaths(_filePathConfig.SURVEY_ORIGINAL_IMAGE_PATH, survey.Id.ToString(), "question_" + sq.Id, "option_" + so.Id)),                                }))).ToList()
                            };
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("\n" + ex.Message + "\n");
                            Console.WriteLine("\n" + ex.StackTrace + "\n");
                            throw new HttpRequestException("Lỗi tạo phiên edititng survey, lỗi: " + ex.Message);
                        }
                    }
                    ))).ToList()

                };

                return surveyEditingSessionDTO;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.Message + "\n");
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                throw new HttpRequestException("Lỗi tạo phiên edititng survey");
            }
        }

        public async Task UpdateSurveyByEditingSession(SurveyEditingSessionDTO surveyEditingSessionDTO)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // 1. Lấy survey từ DB
                    SurveyFilterObject surveyFilterObject = new SurveyFilterObject { };
                    var survey = await _unitOfWork.SurveyRepository.FindByIdAndFilterObjectAsync(surveyEditingSessionDTO.Id, surveyFilterObject);
                    if (survey == null)
                        throw new Exception($"Survey {surveyEditingSessionDTO.Id} không tồn tại");

                    // 2. Cập nhật các field scalar [field cập nhật]
                    survey.Title = surveyEditingSessionDTO.Title ?? survey.Title;
                    survey.Description = surveyEditingSessionDTO.Description ?? survey.Description;
                    survey.SurveyTopicId = surveyEditingSessionDTO.SurveyTopicId;
                    survey.SurveySpecificTopicId = surveyEditingSessionDTO.SurveySpecificTopicId;
                    survey.SecurityModeId = surveyEditingSessionDTO.SecurityModeId;
                    await _surveyGenericRepository.UpdateAsync(survey.Id, survey);

                    // 3. Cập nhật ảnh chính và background
                    if (surveyEditingSessionDTO.MainImageBase64 != null)
                    {
                        await _fileIOHelper.UploadBase64FileAsync(surveyEditingSessionDTO.MainImageBase64, $"{_filePathConfig.SURVEY_ORIGINAL_IMAGE_PATH}\\{survey.Id}", "main");
                    }
                    else
                    {
                        await _fileIOHelper.DeleteFileAsync(FilePathHelper.CombinePaths($"{_filePathConfig.SURVEY_ORIGINAL_IMAGE_PATH}\\{survey.Id}", "main"));
                    }
                    if (surveyEditingSessionDTO.BackgroundImageBase64 != null)
                    {
                        await _fileIOHelper.UploadBase64FileAsync(surveyEditingSessionDTO.BackgroundImageBase64, $"{_filePathConfig.SURVEY_ORIGINAL_IMAGE_PATH}\\{survey.Id}", "background");
                    }
                    else
                    {
                        await _fileIOHelper.DeleteFileAsync(FilePathHelper.CombinePaths($"{_filePathConfig.SURVEY_ORIGINAL_IMAGE_PATH}\\{survey.Id}", "background"));
                    }

                    // 4. Cập nhật ConfigJsonString
                    if (surveyEditingSessionDTO.ConfigJson != null)
                    {
                        survey.ConfigJsonString = JsonConvert.SerializeObject(surveyEditingSessionDTO.ConfigJson);
                    }

                    // 5. Xử lý các question
                    // var dtoQuestionIds = surveyEditingSessionDTO.Questions?.Where(q => q.Id != null).Select(q => q.Id).ToHashSet() ?? new HashSet<Guid?>(); // [CHỈNH LẠI] sau sẽ có id mới
                    var dtoQuestionIds = surveyEditingSessionDTO.Questions?.Select(q => q.Id).ToHashSet() ?? new HashSet<Guid>();
                    var dbQuestions = survey.SurveyQuestions.ToList();
                    // Xóa question không còn trong DTO
                    foreach (var dbQ in dbQuestions)
                    {
                        if (!dtoQuestionIds.Contains(dbQ.Id))
                        {
                            // Xóa file ảnh question
                            await _fileIOHelper.DeleteFolderAsync($"{_filePathConfig.SURVEY_ORIGINAL_IMAGE_PATH}\\{survey.Id}\\question_{dbQ.Id}");
                            if (surveyEditingSessionDTO.SurveyStatusId == 2 && surveyEditingSessionDTO.SurveyTypeId == 2)
                            {
                                await _unitOfWork.SurveyQuestionRepository.DeleteByIdAsync(dbQ.Id, _dateHelper.GetNowByAppTimeZone());
                            }
                            else
                            {
                                await _unitOfWork.SurveyQuestionRepository.DeleteByIdAsync(dbQ.Id);
                            }
                        }
                    }
                    // Cập nhật hoặc thêm mới question
                    foreach (var dtoQ in surveyEditingSessionDTO.Questions ?? new List<SurveyEditingSessionQuestionDTO>())
                    {
                        var dbQ = dbQuestions.FirstOrDefault(q => q.Id == dtoQ.Id);
                        // Console.WriteLine("Processing question with ID: " + dtoQ.Id);
                        // Console.WriteLine("Question content: " + dtoQ.Content);
                        // Console.WriteLine("is existing question: " + (dbQ == null ? "No" : "Yes"));
                        if (dbQ == null) // [CHỈNH LẠI] sau sẽ có id mới 
                        {
                            // Thêm mới question
                            var newQ = new SurveyQuestion
                            {
                                Id = dtoQ.Id,
                                SurveyId = survey.Id,
                                QuestionTypeId = dtoQ.QuestionTypeId,
                                IsReanswerRequired = dtoQ.IsReanswerRequired,
                                ReferenceSurveyQuestionId = dtoQ.ReferenceSurveyQuestionId,
                                Content = dtoQ.Content,
                                Description = dtoQ.Description,
                                TimeLimit = dtoQ.TimeLimit == null ? RandomNumber10To15() : dtoQ.TimeLimit.Value,
                                IsVoiced = dtoQ.IsVoiced,
                                Order = (byte)dtoQ.Order,
                                Version = dtoQ.Version.HasValue ? (byte?)dtoQ.Version.Value : null,
                                ConfigJsonString = dtoQ.ConfigJson != null ? Newtonsoft.Json.JsonConvert.SerializeObject(dtoQ.ConfigJson) : null,
                                // SurveyOptions = new List<SurveyOption>()
                            };
                            await _surveyQuestionGenericRepository.CreateAsync(newQ);
                            // Lưu ảnh question nếu có
                            if (dtoQ.MainImageBase64 != null)
                            {
                                await _fileIOHelper.UploadBase64FileAsync(dtoQ.MainImageBase64, $"{_filePathConfig.SURVEY_ORIGINAL_IMAGE_PATH}\\{survey.Id}\\question_{newQ.Id}", "main");
                            }
                            // Thêm mới option cho question mới
                            foreach (var dtoO in dtoQ.Options ?? new List<SurveyEditingSessionOptionDTO>())
                            {
                                // if (dtoO.Id == null) // [CHỈNH LẠI] sau sẽ có id mới
                                // {
                                var newO = new SurveyOption
                                {
                                    Id = dtoO.Id,
                                    SurveyQuestionId = newQ.Id,
                                    Content = dtoO.Content,
                                    Order = (byte)dtoO.Order
                                };
                                await _surveyOptionGenericRepository.CreateAsync(newO);
                                // Lưu ảnh option nếu có
                                if (dtoO.MainImageBase64 != null)
                                {
                                    await _fileIOHelper.UploadBase64FileAsync(dtoO.MainImageBase64, $"{_filePathConfig.SURVEY_ORIGINAL_IMAGE_PATH}\\{survey.Id}\\question_{newQ.Id}\\option_{newO.Id}", "main");
                                }
                                //}
                            }
                            continue;
                        }
                        dbQ.QuestionTypeId = dtoQ.QuestionTypeId;
                        dbQ.IsReanswerRequired = dtoQ.IsReanswerRequired;
                        dbQ.ReferenceSurveyQuestionId = dtoQ.ReferenceSurveyQuestionId;
                        dbQ.Content = dtoQ.Content;
                        dbQ.Description = dtoQ.Description;
                        dbQ.TimeLimit = dtoQ.TimeLimit == null ? RandomNumber10To15() : dtoQ.TimeLimit.Value;
                        dbQ.IsVoiced = dtoQ.IsVoiced;
                        dbQ.Order = (byte)dtoQ.Order;
                        if (dtoQ.ConfigJson != null)
                            dbQ.ConfigJsonString = JsonConvert.SerializeObject(dtoQ.ConfigJson);
                        // Xử lý ảnh question
                        if (dtoQ.MainImageBase64 != null)
                        {
                            await _fileIOHelper.UploadBase64FileAsync(dtoQ.MainImageBase64, $"{_filePathConfig.SURVEY_ORIGINAL_IMAGE_PATH}\\{survey.Id}\\question_{dbQ.Id}", "main");
                        }
                        else
                        {
                            await _fileIOHelper.DeleteFileAsync(FilePathHelper.CombinePaths($"{_filePathConfig.SURVEY_ORIGINAL_IMAGE_PATH}\\{survey.Id}\\question_{dbQ.Id}", "main"));
                        }
                        await _surveyQuestionGenericRepository.UpdateAsync(dbQ.Id, dbQ);

                        // Xử lý option
                        // var dtoOptionIds = dtoQ.Options?.Where(o => o.Id != null).Select(o => o.Id).ToHashSet() ?? new HashSet<Guid?>(); // [CHỈNH LẠI] sau sẽ có id mới
                        var dtoOptionIds = dtoQ.Options?.Select(o => o.Id).ToHashSet() ?? new HashSet<Guid>();
                        var dbOptions = dbQ.SurveyOptions.ToList();
                        // Xóa option không còn trong DTO
                        foreach (var dbO in dbOptions)
                        {
                            if (!dtoOptionIds.Contains(dbO.Id))
                            {
                                await _fileIOHelper.DeleteFolderAsync($"{_filePathConfig.SURVEY_ORIGINAL_IMAGE_PATH}\\{survey.Id}\\question_{dbQ.Id}\\option_{dbO.Id}");
                                _appDbContext.SurveyOptions.Remove(dbO);
                            }
                        }
                        // Cập nhật hoặc thêm mới option
                        foreach (var dtoO in dtoQ.Options ?? new List<SurveyEditingSessionOptionDTO>())
                        {
                            var dbO = dbOptions.FirstOrDefault(o => o.Id == dtoO.Id);
                            if (dbO == null)
                            {
                                // Thêm mới option nếu cần
                                var newO = new SurveyOption
                                {
                                    SurveyQuestionId = dbQ.Id,
                                    Content = dtoO.Content,
                                    Order = (byte)dtoO.Order
                                };
                                await _surveyOptionGenericRepository.CreateAsync(newO);
                                // Lưu ảnh option nếu có
                                if (dtoO.MainImageBase64 != null)
                                {
                                    await _fileIOHelper.UploadBase64FileAsync(dtoO.MainImageBase64, $"{_filePathConfig.SURVEY_ORIGINAL_IMAGE_PATH}\\{survey.Id}\\question_{dbQ.Id}\\option_{newO.Id}", "main");
                                }
                                continue;
                            }
                            dbO.Content = dtoO.Content;
                            dbO.Order = (byte)dtoO.Order;
                            if (dtoO.MainImageBase64 != null)
                            {
                                await _fileIOHelper.UploadBase64FileAsync(dtoO.MainImageBase64, $"{_filePathConfig.SURVEY_ORIGINAL_IMAGE_PATH}\\{survey.Id}\\question_{dbQ.Id}\\option_{dbO.Id}", "main");
                            }
                            else
                            {
                                await _fileIOHelper.DeleteFileAsync($"{_filePathConfig.SURVEY_ORIGINAL_IMAGE_PATH}\\{survey.Id}\\question_{dbQ.Id}\\option_{dbO.Id}\\main");
                            }
                            await _surveyOptionGenericRepository.UpdateAsync(dbO.Id, dbO);
                        }
                    }
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine("\n\n\n" + ex.StackTrace + "\n\n\n");
                    Console.WriteLine("\n" + ex.Message + "\n");
                    throw new Exception("Lỗi cập nhật phiên chỉnh sửa survey, lỗi: " + ex.Message);
                }

            }

        }


        public async Task<SurveyTakingSessionDTO> GenerateSurveyTakingSessionDTO(Survey survey, int? version)
        {
            if (survey == null)
                throw new Exception("survey is null");
            SurveyConfigJsonDTO surveyConfigJson = (JObject.Parse(survey.ConfigJsonString)).ToObject<SurveyConfigJsonDTO>();
            string backgroundImageUrl = "";
            if (surveyConfigJson.IsUseBackgroundImageBase64 == true)
            {
                backgroundImageUrl = await _fileIOHelper.GeneratePresignedUrlAsync(FilePathHelper.CombinePaths(_filePathConfig.SURVEY_ORIGINAL_IMAGE_PATH, survey.Id.ToString(), "background"));
            }
            else
            {
                backgroundImageUrl = await _fileIOHelper.GeneratePresignedUrlAsync(FilePathHelper.CombinePaths(_filePathConfig.SURVEY_DEFAULT_BACKGROUND_IMAGE_PATH, surveyConfigJson.DefaultBackgroundImageId.ToString(), "main"));
            }

            var dto = new SurveyTakingSessionDTO
            {
                Id = survey.Id,
                RequesterId = survey.RequesterId,
                Title = survey.Title,
                Description = survey.Description,
                SurveyTypeId = survey.SurveyTypeId,
                SurveyTopicId = survey.SurveyTopicId,
                SurveySpecificTopicId = survey.SurveySpecificTopicId,
                MainImageUrl = await _fileIOHelper.GeneratePresignedUrlAsync(FilePathHelper.CombinePaths(_filePathConfig.SURVEY_ORIGINAL_IMAGE_PATH, survey.Id.ToString(), "main")),
                BackgroundImageUrl = backgroundImageUrl,
                SurveyStatusId = survey.SurveyStatusTrackings.OrderByDescending(sst => sst.CreatedAt).FirstOrDefault()?.SurveyStatusId,
                Version = survey.SurveyTypeId == 3 ? version : null,
                MarketSurveyVersionStatusId = survey.SurveyMarketVersionStatusTrackings.OrderByDescending(smt => smt.CreatedAt).FirstOrDefault()?.SurveyStatusId,
                SecurityModeId = survey.SecurityModeId,
                ConfigJson = JsonConvert.DeserializeObject<SurveyTakingSessionConfigJsonDTO>(survey.ConfigJsonString),
                Questions = (await Task.WhenAll(survey.SurveyQuestions.Select(async sq => new SurveyTakingSessionQuestionDTO
                {
                    Id = sq.Id,
                    QuestionTypeId = sq.QuestionTypeId != null ? sq.QuestionTypeId.Value : 1,
                    Version = sq.Version,
                    MainImageUrl = await _fileIOHelper.GeneratePresignedUrlAsync(FilePathHelper.CombinePaths(_filePathConfig.SURVEY_ORIGINAL_IMAGE_PATH, $"{survey.Id}/question_{sq.Id}", "main")),
                    Content = sq.Content,
                    Description = sq.Description,
                    TimeLimit = sq.TimeLimit,
                    IsVoiced = sq.IsVoiced,
                    Order = sq.Order,
                    ConfigJson = JsonConvert.DeserializeObject<SurveyTakingSessionQuestionConfigJsonDTO>(sq.ConfigJsonString),
                    Options = (await Task.WhenAll(sq.SurveyOptions.Select(async so => new SurveyTakingSessionOptionDTO
                    {
                        Id = so.Id,
                        Content = so.Content,
                        Order = so.Order,
                        MainImageUrl = await _fileIOHelper.GeneratePresignedUrlAsync(FilePathHelper.CombinePaths(_filePathConfig.SURVEY_ORIGINAL_IMAGE_PATH, $"{survey.Id}/question_{sq.Id}/option_{so.Id}", "main"))
                    })))
                    .OrderBy(so => so.Order)
                    .ToList()
                }))).OrderBy(sq => sq.Order) // Sắp xếp theo thứ tự
                .ToList()
            };
            return dto;




        }

        /////////////////////////////////////////////////////////////

        public async Task<SurveyEditingSessionDTO> GetSurveyEditingSession(int surveyId, int userId, int? Version = null)
        {
            try
            {

                SurveyFilterObject surveyFilterObject = new SurveyFilterObject { };

                // Survey survey = await _unitOfWork.SurveyRepository.FindByIdAndFilterObjectAsync(surveyId, surveyFilterObject);
                Survey survey = await _unitOfWork.SurveyRepository.FindByIdAsync(surveyId);
                if (survey == null)
                {
                    throw new Exception("survey không tồn tại");
                }
                else if (survey.SurveyTypeId == 3 && Version == null)
                {
                    throw new Exception("phiên bản survey không được để trống");
                }
                else if (survey.SurveyTypeId == 3 && Version != null && survey.SurveyMarkets.Where(sm => sm.Version == Version).ToList().Count == 0)
                {
                    throw new Exception("không tìm thấy survey theo điều kiện và có id " + surveyId.ToString() + " và version " + Version.Value.ToString());
                }


                Account account = await _unitOfWork.AccountRepository.FindByIdAsync(userId);
                if ((account.RoleId == 2 || account.RoleId == 3) && survey.SurveyTypeId == 1)
                {
                    surveyFilterObject = new SurveyFilterObject
                    {
                        IsDeletedContain = false,
                        // IsInvalidTakenResultContain = false,
                        SurveyTypeId = 1,
                        SurveyStatusIds = new List<int> { 1 }
                    };
                    survey = await _unitOfWork.SurveyRepository.FindByIdAndFilterObjectAsync(surveyId, surveyFilterObject);
                    if (survey == null)
                    {
                        throw new Exception("không được phép chỉnh sửa survey ở thời điểm hiện tại");
                    }
                }
                else if (account.RoleId == 4 && survey.SurveyTypeId == 2)
                {
                    surveyFilterObject = new SurveyFilterObject
                    {
                        RequesterId = userId,
                        IsDeletedContain = false,
                        // IsInvalidTakenResultContain = false,
                        SurveyTypeId = 2,
                        SurveyStatusIds = new List<int> { 1, 2 }
                    };
                    survey = await _unitOfWork.SurveyRepository.FindByIdAndFilterObjectAsync(surveyId, surveyFilterObject);
                    if (survey == null)
                    {
                        throw new Exception("không được phép chỉnh sửa survey ở thời điểm hiện tại");
                    }
                }
                else if ((account.RoleId == 2 || account.RoleId == 3) && survey.SurveyTypeId == 3)
                {
                    surveyFilterObject = new SurveyFilterObject
                    {
                        IsDeletedContain = false,
                        // IsInvalidTakenResultContain = false,
                        SurveyTypeId = 3,
                        SurveyStatusIds = new List<int> { 2 },
                        Version = Version,
                        SurveyMarketVersionStatusIds = new List<int> { 1 }
                    };
                    survey = await _unitOfWork.SurveyRepository.FindByIdAndFilterObjectAsync(surveyId, surveyFilterObject);
                    if (survey == null)
                    {
                        Console.WriteLine("không tìm thấy survey theo điều kiện và có id " + surveyId.ToString() + " và version " + Version.Value.ToString());
                        throw new Exception("không được phép chỉnh sửa survey ở thời điểm hiện tại, survey id " + surveyId.ToString() + " và version " + Version.Value.ToString());
                    }
                }
                else
                {
                    throw new Exception("không xác định");
                }

                // Console.WriteLine("\n\n\n\n\n\n okokokokokokokokokokokokokokokok \n\n\n\n\n\n");

                return await GenerateSurveyEditingSessionDTO(survey, Version);
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                throw new HttpRequestException("Lỗi tạo phiên edititng survey, lỗi: " + ex.Message);
            }

        }


        public async Task<SurveySessionUpdateTriggerResponseDTO> UpdateSurveyEditingSessionAutoTrigger(int surveyId, SurveyEditingSessionDTO surveyEditingSessionDTO, int userId)
        {

            if (surveyEditingSessionDTO == null)
            {
                throw new ForbiddenException("không tìm thất phiên chỉnh sửa tự động");
            }
            else if (surveyEditingSessionDTO.Id != surveyId)
            {
                Console.WriteLine("id phiên chỉnh sửa không khớp, survey id: " + surveyId.ToString() + " và phiên chỉnh sửa id: " + surveyEditingSessionDTO.Id.ToString());
                throw new ForbiddenException("id phiên chỉnh sửa không khớp");
            }

            SurveyFilterObject surveyFilterObject = new SurveyFilterObject { };

            Survey survey = await _unitOfWork.SurveyRepository.FindByIdAndFilterObjectAsync(surveyEditingSessionDTO.Id, surveyFilterObject);
            if (survey == null)
            {
                // return new SurveySessionUpdateTriggerResponseDTO("survey không tồn tại", false);
                throw new ForbiddenException("survey không tồn tại");
            }
            var currentSurveyStatus = await _unitOfWork.SurveyRepository.GetLatestSurveyStatusTrackingBySurveyIdAsync(survey.Id);
            if (currentSurveyStatus == null)
            {
                // return new SurveySessionUpdateTriggerResponseDTO("không tìm thấy trạng thái survey", false);
                throw new ForbiddenException("không tìm thấy trạng thái survey");
            }


            Account account = await _unitOfWork.AccountRepository.FindByIdAsync(userId);

            // kiểm tra survey type
            if (surveyEditingSessionDTO.SurveyTypeId != survey.SurveyTypeId)
            {
                throw new ForbiddenException("loại survey không khớp");
            }


            // kiêm tra quyền
            if (surveyEditingSessionDTO.SurveyTypeId == 1 && account.RoleId != 2 && account.RoleId != 3)
            {
                throw new ForbiddenException("không có quyền chỉnh sửa survey này");
            }
            else if (surveyEditingSessionDTO.SurveyTypeId == 2 && account.RoleId != 4)
            {
                throw new ForbiddenException("không có quyền chỉnh sửa survey này");
            }
            else if (surveyEditingSessionDTO.SurveyTypeId == 3 && account.RoleId != 2 && account.RoleId != 3)
            {
                throw new ForbiddenException("không có quyền chỉnh sửa survey này");
            }




            if (surveyEditingSessionDTO.SurveyTypeId == 1 && survey.SurveyTypeId == 1)
            {
                // kiểm tra survey status
                if (currentSurveyStatus?.SurveyStatusId != 1)
                {
                    throw new ForbiddenException("không được phép chỉnh sửa survey ở thời điểm hiện tại");
                }

                try
                {
                    await UpdateSurveyByEditingSession(surveyEditingSessionDTO);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("\n" + ex.Message + "\n");
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                    return new SurveySessionUpdateTriggerResponseDTO
                    {
                        Message = "Cập nhật phiên chỉnh sửa thất bại, lỗi: " + ex.Message,
                        IsSuccess = false
                    }; ;
                }

            }
            else if (surveyEditingSessionDTO.SurveyTypeId == 2 && survey.SurveyTypeId == 2)
            {
                // kiểm tra requester id
                if (survey.RequesterId != userId || surveyEditingSessionDTO.RequesterId != userId || surveyEditingSessionDTO.RequesterId != survey.RequesterId)
                {
                    throw new ForbiddenException("truy cập không hợp lệ");
                }

                // kiểm tra survey status (có trường hợp editing và đã publish)
                if (currentSurveyStatus.SurveyStatusId != 1 && currentSurveyStatus.SurveyStatusId != 2)
                {
                    throw new ForbiddenException("không được phép chỉnh sửa survey ở thời điểm hiện tại");
                }

                // if (currentSurveyStatus.SurveyStatusId == 2 && surveyEditingSessionDTO.Questions.Where(q => q.Id == null).ToList().Count > 0)  //[CHỈNH LẠI] sau này sẽ kiểm tra ID question có tồn tại hay không
                if (currentSurveyStatus.SurveyStatusId == 2 && surveyEditingSessionDTO.Questions.Any(q => !survey.SurveyQuestions.Any(sq => sq.Id == q.Id)))
                {
                    // return new SurveySessionUpdateTriggerResponseDTO
                    // {
                    //     Message = "không được phép thêm câu hỏi mới vào survey đã đăng",
                    //     IsSuccess = false
                    // };
                    throw new ForbiddenException("không được phép thêm câu hỏi mới vào survey đã đăng");
                }

                try
                {
                    await UpdateSurveyByEditingSession(surveyEditingSessionDTO);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("\n" + ex.Message + "\n");
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                    return new SurveySessionUpdateTriggerResponseDTO
                    {
                        Message = "Cập nhật phiên chỉnh sửa thất bại, lỗi: " + ex.Message,
                        IsSuccess = false
                    }; ;
                }

            }
            else if (surveyEditingSessionDTO.SurveyTypeId == 3 && survey.SurveyTypeId == 3)
            {
                // kiểm tra version có tồn tại

                if (surveyEditingSessionDTO.Version == null)
                {
                    throw new ForbiddenException("truy cập không hợp lệ");
                }
                var surveyMarket = survey.SurveyMarkets.Where(sm => sm.Version == surveyEditingSessionDTO.Version).FirstOrDefault();
                if (surveyMarket == null)
                {
                    throw new ForbiddenException("không tìm thấy phiên bản survey đang chỉnh sửa, survey id " + surveyEditingSessionDTO.Id.ToString() + " và version " + surveyEditingSessionDTO.Version.Value.ToString());
                }

                var versionStatus = survey.SurveyMarketVersionStatusTrackings
                    .Where(smt => smt.Version == surveyEditingSessionDTO.Version)
                    .OrderByDescending(smt => smt.CreatedAt)
                    .FirstOrDefault();
                if (versionStatus == null)
                {
                    throw new ForbiddenException("không tìm thấy trạng thái phiên bản survey, survey id " + surveyEditingSessionDTO.Id.ToString() + " và version " + surveyEditingSessionDTO.Version.Value.ToString());
                }

                // kiểm tra version status (có trường hợp editing)
                if (versionStatus.SurveyStatusId != 2)
                {
                    throw new ForbiddenException("không được phép chỉnh sửa survey ở thời điểm hiện tại, survey id " + surveyEditingSessionDTO.Id.ToString() + " và version " + surveyEditingSessionDTO.Version.Value.ToString());
                }

                try
                {
                    await UpdateSurveyByEditingSession(surveyEditingSessionDTO);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("\n" + ex.Message + "\n");
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                    return new SurveySessionUpdateTriggerResponseDTO
                    {
                        Message = "Cập nhật phiên chỉnh sửa thất bại, lỗi: " + ex.Message,
                        IsSuccess = false
                    }; ;
                }
            }
            else
            {
                throw new Exception("không xác định");
            }


            return new SurveySessionUpdateTriggerResponseDTO
            {
                Message = "Cập nhật phiên chỉnh sửa thành công",
                IsSuccess = true
            };


        }


        public async Task<SurveyTakingSessionDTO> GetSurveyTakingSession(int surveyId, int userId, SurveyTakingSubjectEnum TakingSubject, int? Version = null)
        {
            {
                try
                {

                    SurveyFilterObject surveyFilterObject = new SurveyFilterObject { };

                    // Survey survey = await _unitOfWork.SurveyRepository.FindByIdAndFilterObjectAsync(surveyId, surveyFilterObject);
                    Survey survey = await _unitOfWork.SurveyRepository.FindByIdAsync(surveyId);
                    if (survey == null)
                    {
                        throw new Exception("survey không tồn tại");
                    }
                    else if (survey.SurveyTypeId == 3 && Version == null)
                    {
                        throw new Exception("phiên bản survey không được để trống");
                    }
                    else if (survey.SurveyTypeId == 3 && Version != null && survey.SurveyMarkets.Where(sm => sm.Version == Version).ToList().Count == 0)
                    {
                        throw new Exception("không tìm thấy survey theo điều kiện và có id " + surveyId.ToString() + " và version " + Version.Value.ToString());
                    }


                    if (TakingSubject == SurveyTakingSubjectEnum.Preview)
                    {
                        surveyFilterObject = new SurveyFilterObject
                        {
                            IsDeletedContain = false,
                            // IsInvalidTakenResultContain = false,
                            SurveyTypeId = survey.SurveyTypeId,
                        };
                        survey = await _unitOfWork.SurveyRepository.FindByIdAndFilterObjectAsync(surveyId, surveyFilterObject);
                        if (survey == null)
                        {
                            throw new Exception("không tìm thấy survey theo điều kiện và có id " + surveyId.ToString());
                        }
                    }
                    else if (TakingSubject == SurveyTakingSubjectEnum.Verified || TakingSubject == SurveyTakingSubjectEnum.LevelUpdate)
                    {
                        Account account = await _unitOfWork.AccountRepository.FindByIdAsync(userId);
                        if (account.RoleId != 4)
                        {
                            throw new Exception("không tìm thấy tài khoản người dùng");
                        }

                        if (survey.SurveyTypeId == 1)
                        {
                            if (TakingSubject != SurveyTakingSubjectEnum.Verified)
                            {
                                throw new Exception("không thể làm survey loại này");
                            }
                            surveyFilterObject = new SurveyFilterObject
                            {
                                IsDeletedContain = false,
                                // IsInvalidTakenResultContain = false,
                                IsAvailable = true,
                                SurveyTypeId = survey.SurveyTypeId,
                                SurveyStatusIds = new List<int> { 2 }
                            };
                            survey = await _unitOfWork.SurveyRepository.FindByIdAndFilterObjectAsync(surveyId, surveyFilterObject);
                            if (survey == null)
                            {
                                throw new Exception("không thể tải phiên làm ở thời điểm hiện tại");
                            }
                        }
                        else if (survey.SurveyTypeId == 2)
                        {
                            surveyFilterObject = new SurveyFilterObject
                            {
                                IsDeletedContain = false,
                                // IsInvalidTakenResultContain = false,
                                IsAvailable = true,
                                SurveyTypeId = survey.SurveyTypeId,
                                SurveyStatusIds = new List<int> { 2 }
                            };
                            survey = await _unitOfWork.SurveyRepository.FindByIdAndFilterObjectAsync(surveyId, surveyFilterObject);
                            var surveyTakenResults = (await _unitOfWork.SurveyTakenResultRepository.FindBySurveyIdAsync(surveyId, false)).Where(str => str.CompletedAt != null && str.CompletedAt > survey.PublishedAt).ToList();

                            if (survey == null)
                            {
                                throw new Exception("không thể tải phiên làm ở thời điểm hiện tại");
                            }
                            if (survey.Kpi <= surveyTakenResults.Count())
                            {
                                throw new Exception("không thể tải phiên làm ở thời điểm hiện tại, đã đủ số lượng người làm");
                            }
                            if (survey.EndDate < DateOnly.FromDateTime(_dateHelper.GetNowByAppTimeZone()))
                            {
                                throw new Exception("không thể tải phiên làm ở thời điểm hiện tại, đã hết thời gian tiếp nhận kết quả");
                            }
                            if (survey.RequesterId == userId)
                            {
                                throw new Exception("không được phép làm survey của chính mình");
                            }
                        }
                        else if (survey.SurveyTypeId == 3)
                        {
                            if (TakingSubject != SurveyTakingSubjectEnum.Verified)
                            {
                                throw new Exception("không thể làm survey loại này");
                            }
                            surveyFilterObject = new SurveyFilterObject
                            {
                                IsDeletedContain = false,
                                // IsInvalidTakenResultContain = false,
                                IsAvailable = true,
                                SurveyTypeId = survey.SurveyTypeId,
                                SurveyStatusIds = new List<int> { 2 },
                                Version = Version,
                                SurveyMarketVersionStatusIds = new List<int> { 2 }
                            };
                            survey = await _unitOfWork.SurveyRepository.FindByIdAndFilterObjectAsync(surveyId, surveyFilterObject);
                            if (survey == null)
                            {
                                throw new Exception("không thể tải phiên làm ở thời điểm hiện tại, survey id " + surveyId.ToString() + " và version " + Version.Value.ToString());
                            }
                        }
                        else
                        {
                            throw new Exception("không xác định");
                        }


                    }




                    return await GenerateSurveyTakingSessionDTO(survey, Version);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                    throw new HttpRequestException("Tạo phiên làm khảo sát thất bại, lỗi: " + ex.Message);
                }
            }
        }

    }
}
