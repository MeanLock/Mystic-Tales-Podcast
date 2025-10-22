
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using ModerationService.DataAccess.Entities;
using ModerationService.BusinessLogic.DTOs.FilterTag;
using ModerationService.BusinessLogic.DTOs.Survey;
using System.Diagnostics;
using Newtonsoft.Json;
using ModerationService.Infrastructure.Services.OpenAI._4oMini;
using ModerationService.Infrastructure.Configurations.OpenAI.interfaces;
using ModerationService.BusinessLogic.Helpers.FileHelpers;

namespace ModerationService.BusinessLogic.Services.OpenAIServices
{
    public class SurveyOpenAIService
    {
        // LOGGER
        private readonly ILogger<SurveyOpenAIService> _logger;

        // CONFIG
        private readonly IOpenAIConfig _openAIConfig;

        // SERVICES
        private readonly FileIOHelper _fileIOHelper;
        private readonly OpenAI4oMiniService _openAI4oMiniService;


        public SurveyOpenAIService(
            IOpenAIConfig openAIConfig,
            ILogger<SurveyOpenAIService> logger,
            FileIOHelper fileIOHelper,
            OpenAI4oMiniService openAI4oMiniService
            )
        {
            _fileIOHelper = fileIOHelper;
            _openAIConfig = openAIConfig;

            _openAI4oMiniService = openAI4oMiniService;

            _logger = logger;
        }


        public async Task<List<SummarizedFilterTagDTO>> GetSummarizedDefaultFilterTagAsync(
            List<FilterTag> filterTags,
            JArray surveyResponses,
            Account account,
            AccountProfile accountProfile,
            List<SurveyTopicFavorite> surveyTopicFavorites)
        {
            try
            {
                // Chuẩn bị function schema cho OpenAI function calling (giữ nguyên như GetFilterTagSummariesAsync)
                var functionSchema = new JObject
                {
                    ["name"] = "summarize_filter_tags",
                    ["description"] = "Tóm tắt ngắn gọn tiếng Việt cho từng filter tag dựa trên response, profile, favorite topic.",
                    ["parameters"] = new JObject
                    {
                        ["type"] = "object",
                        ["properties"] = new JObject
                        {
                            ["result"] = new JObject
                            {
                                ["type"] = "array",
                                ["items"] = new JObject
                                {
                                    ["type"] = "object",
                                    ["properties"] = new JObject
                                    {
                                        ["FilterTagId"] = new JObject { ["type"] = "integer" },
                                        ["Summary"] = new JObject { ["type"] = "string", ["description"] = "Tóm tắt ngắn gọn tiếng Việt hoặc null nếu không liên quan." }
                                    },
                                    ["required"] = new JArray { "FilterTagId", "Summary" }
                                }
                            }
                        },
                        ["required"] = new JArray { "result" }
                    }
                };

                // Modify data
                accountProfile.Account = null;
                surveyTopicFavorites = surveyTopicFavorites.Select(f => new SurveyTopicFavorite
                {
                    SurveyTopicId = f.SurveyTopicId,
                    FavoriteScore = f.FavoriteScore,
                    SurveyTopic = new SurveyTopic
                    {
                        Name = f.SurveyTopic.Name,
                    },
                }).ToList();
                filterTags = filterTags.Select(tag => new FilterTag
                {
                    Id = tag.Id,
                    FilterTagTypeId = tag.FilterTagTypeId,
                    Name = tag.Name,
                    FilterTagType = new FilterTagType
                    {
                        Id = tag.FilterTagType.Id,
                        Name = tag.FilterTagType.Name
                    }
                }).ToList();

                // Format data
                var filterTagsJson = JArray.FromObject(filterTags).ToString();
                var surveyResponsesJson = surveyResponses.ToString();
                var accountProfileJson = JObject.FromObject(accountProfile).ToString();
                var surveyTopicFavoritesJson = JArray.FromObject(surveyTopicFavorites).ToString();
                var accountInfoJson = JObject.FromObject(new { account.Dob, account.Gender }).ToString();



                // Chuẩn bị prompt
                var prompt = $@"Bạn là AI chuyên phân tích survey. Dưới đây là danh sách filter tag, danh sách response của 1 người dùng cho filter survey, thông tin profile, các chủ đề yêu thích và thông tin tài khoản (gồm ngày sinh và giới tính). Hãy chú ý sử dụng thông tin ngày sinh (dob) và giới tính (gender) của account để tóm tắt chính xác, phù hợp ngữ cảnh cá nhân hóa cho từng filter tag. Hãy trả về 1 mảng JSON, mỗi phần tử gồm FilterTagId và Summary tiếng Việt thật ngắn gọn, súc tích, đủ ý (không cần diễn giải chi tiết, không lặp lại nội dung câu hỏi), tóm tắt dựa trên response, profile, favorite topic và thông tin account. Nếu ở tag nào đó không được đề cập trong các yếu tố đầu vào (hoặc không chất lọc được thông tin) thì Summary phải để null, bạn buộc phải tuân theo rule này (không được viết các câu kiểu như 'không có thông tin', 'không có dữ liệu', 'không có', 'không rõ', v.v.). Không tự chế thêm tag, chỉ dùng đúng filterTagId đã cho. Kết quả trả về đúng format function calling OpenAI yêu cầu.\n\nfilterTags: {filterTagsJson}\nsurveyResponses: {surveyResponsesJson}\naccountProfile: {accountProfileJson}\nsurveyTopicFavorites: {surveyTopicFavoritesJson}\naccount: {accountInfoJson}";

                // Chuẩn bị request body cho OpenAI
                var body = new JObject
                {
                    ["model"] = _openAIConfig.BaseModel,
                    ["messages"] = new JArray
        {
            new JObject { ["role"] = "system", ["content"] = "Bạn là AI chuyên phân tích survey và trả về JSON cho BE xử lý." },
            new JObject { ["role"] = "user", ["content"] = prompt }
        },
                    ["functions"] = new JArray { functionSchema },
                    ["function_call"] = new JObject { ["name"] = "summarize_filter_tags" }
                };

                var argumentsJson = await _openAI4oMiniService.CallOpenAIChatCompletionFunctionAsync(body);
                List<SummarizedFilterTagDTO> resultList = null;
                if (!string.IsNullOrEmpty(argumentsJson.ToString()))
                {
                    var resultObj = JObject.Parse(argumentsJson.ToString());
                    if (resultObj["result"] != null)
                    {
                        resultList = JsonConvert.DeserializeObject<List<SummarizedFilterTagDTO>>(resultObj["result"].ToString());
                    }
                }
                // Đảm bảo trả về đủ các tag, kể cả summary null
                var resultDict = resultList?.ToDictionary(x => x.FilterTagId) ?? new Dictionary<int, SummarizedFilterTagDTO>();
                var fullResult = filterTags.Select(tag =>
                    resultDict.TryGetValue(tag.Id, out var summary)
                        ? summary
                        : new SummarizedFilterTagDTO { FilterTagId = tag.Id, Summary = null }
                ).ToList();
                return fullResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                throw new Exception($"lỗi trong hàm GetSummarizedDefaultFilterTagAsync {ex.Message}, dòng {new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()}");
            }

        }

        /// <summary>
        /// Tóm tắt các additional filter tag dựa trên surveyResponses của một lần làm survey, trả về {FilterTagId, Summary} cho mỗi tag.
        /// </summary>
        /// <param name="filterTags">Danh sách additional filter tag</param>
        /// <param name="surveyResponses">Các câu trả lời survey của lần làm này</param>
        /// <returns>Danh sách summary cho từng additional filter tag</returns>
        public async Task<List<SummarizedFilterTagDTO>> GetSummarizedAddtionalFilterTagBySurveyTakenResponsesAsync(
            List<FilterTag> filterTags,
            JArray surveyResponses
            )
        {
            try
            {
                var functionSchema = new JObject
                {
                    ["name"] = "summarize_additional_filter_tags",
                    ["description"] = "Tóm tắt ngắn gọn tiếng Việt cho từng additional filter tag dựa trên surveyResponses, phản ánh tần suất và xu hướng hoạt động của người dùng trên nền tảng.",
                    ["parameters"] = new JObject
                    {
                        ["type"] = "object",
                        ["properties"] = new JObject
                        {
                            ["result"] = new JObject
                            {
                                ["type"] = "array",
                                ["items"] = new JObject
                                {
                                    ["type"] = "object",
                                    ["properties"] = new JObject
                                    {
                                        ["FilterTagId"] = new JObject { ["type"] = "integer" },
                                        ["Summary"] = new JObject { ["type"] = "string", ["description"] = "Tóm tắt ngắn gọn tiếng Việt hoặc null nếu không liên quan." }
                                    },
                                    ["required"] = new JArray { "FilterTagId", "Summary" }
                                }
                            }
                        },
                        ["required"] = new JArray { "result" }
                    }
                };

                // Modify data
                filterTags = filterTags.Select(tag => new FilterTag
                {
                    Id = tag.Id,
                    FilterTagTypeId = tag.FilterTagTypeId,
                    Name = tag.Name,
                    FilterTagType = new FilterTagType
                    {
                        Id = tag.FilterTagType.Id,
                        Name = tag.FilterTagType.Name
                    }
                }).ToList();

                // Format data
                var filterTagsJson = JArray.FromObject(filterTags).ToString();
                var surveyResponsesJson = surveyResponses.ToString();
                // Console.WriteLine("surveyResponsesJson: "+ surveyResponsesJson);

                var prompt = $@"Bạn là AI chuyên phân tích survey. Dưới đây là danh sách additional filter tag và danh sách response của một lần làm survey của người dùng. Hãy trả về 1 mảng JSON, mỗi phần tử gồm FilterTagId và Summary tiếng Việt thật ngắn gọn, súc tích, đủ ý, phản ánh tần suất và xu hướng hoạt động của người dùng trên nền tảng dựa trên các response này. Nếu ở tag nào đó không được đề cập trong các yếu tố đầu vào (hoặc không chất lọc được thông tin) thì Summary phải để null, bạn buộc phải tuân theo rule này (không được viết các câu kiểu như 'không có thông tin', 'không có dữ liệu', 'không có', 'không rõ', v.v.). Không tự chế thêm tag, chỉ dùng đúng FilterTagId đã cho. Kết quả trả về đúng format function calling OpenAI yêu cầu.\n\nfilterTags: {filterTagsJson}\nsurveyResponses: {surveyResponsesJson}";

                var body = new JObject
                {
                    ["model"] = _openAIConfig.BaseModel,
                    ["messages"] = new JArray
                {
                    new JObject { ["role"] = "system", ["content"] = "Bạn là AI chuyên phân tích survey và trả về JSON cho BE xử lý." },
                    new JObject { ["role"] = "user", ["content"] = prompt }
                },
                    ["functions"] = new JArray { functionSchema },
                    ["function_call"] = new JObject { ["name"] = "summarize_additional_filter_tags" }
                };

                var argumentsJson = await _openAI4oMiniService.CallOpenAIChatCompletionFunctionAsync(body);
                List<SummarizedFilterTagDTO> resultList = null;
                if (!string.IsNullOrEmpty(argumentsJson.ToString()))
                {
                    var resultObj = JObject.Parse(argumentsJson.ToString());
                    if (resultObj["result"] != null)
                    {
                        resultList = JsonConvert.DeserializeObject<List<SummarizedFilterTagDTO>>(resultObj["result"].ToString());
                    }
                }
                // Đảm bảo trả về đủ các tag, kể cả summary null
                var resultDict = resultList?.ToDictionary(x => x.FilterTagId) ?? new Dictionary<int, SummarizedFilterTagDTO>();
                var fullResult = filterTags.Select(tag =>
                    resultDict.TryGetValue(tag.Id, out var summary)
                        ? summary
                        : new SummarizedFilterTagDTO { FilterTagId = tag.Id, Summary = null }
                ).ToList();
                return fullResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                throw new Exception($"lỗi trong hàm GetSummarizedAddtionalFilterTagBySurveyTakenResponsesAsync {ex.Message}, dòng {new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()}");
            }

        }

        /// <summary>
        /// Gộp và tóm tắt các additional filter tag từ nhiều group (20 survey gần nhất + group hiện tại), trả về {FilterTagId, Summary} cho mỗi tag.
        /// </summary>
        /// <param name="filterTags">Danh sách filter tag (id, name)</param>
        /// <param name="groupedTagFilters">Danh sách các group additional filter tag + summary trong 20 survey gần nhất</param>
        /// <param name="currentTakerTagFilters">Group additional filter tag + summary hiện tại của người dùng</param>
        /// <returns>Danh sách summary cuối cùng cho từng filter tag</returns>
        public async Task<List<SummarizedFilterTagDTO>> GetMergedAdditionalFilterTagSummariesAsync(
            List<FilterTag> filterTags,
            List<List<SurveyTakenResultTagFilter>> groupedTagFilters,
            List<TakerTagFilter> currentTakerTagFilters)
        {
            try
            {
                // Chuẩn bị function schema cho OpenAI function calling
                var functionSchema = new JObject
                {
                    ["name"] = "merge_additional_filter_tag_summaries",
                    ["description"] = "Gộp và tóm tắt chung nhất tiếng Việt cho từng additional filter tag dựa trên nhiều group summary (từ các survey gần nhất và group hiện tại).",
                    ["parameters"] = new JObject
                    {
                        ["type"] = "object",
                        ["properties"] = new JObject
                        {
                            ["result"] = new JObject
                            {
                                ["type"] = "array",
                                ["items"] = new JObject
                                {
                                    ["type"] = "object",
                                    ["properties"] = new JObject
                                    {
                                        ["FilterTagId"] = new JObject { ["type"] = "integer" },
                                        ["Summary"] = new JObject { ["type"] = "string", ["description"] = "Tóm tắt chung nhất tiếng Việt hoặc null nếu không liên quan." }
                                    },
                                    ["required"] = new JArray { "FilterTagId", "Summary" }
                                }
                            }
                        },
                        ["required"] = new JArray { "result" }
                    }
                };
                // Modify data
                filterTags = filterTags.Select(tag => new FilterTag
                {
                    Id = tag.Id,
                    FilterTagTypeId = tag.FilterTagTypeId,
                    Name = tag.Name,
                    FilterTagType = new FilterTagType
                    {
                        Id = tag.FilterTagType.Id,
                        Name = tag.FilterTagType.Name
                    }
                }).ToList();

                // Format data
                var filterTagsJson = JArray.FromObject(filterTags).ToString();
                var groupedSummaries = new JArray();
                foreach (var group in groupedTagFilters)
                {
                    var groupArr = new JArray();
                    foreach (var tagFilter in group)
                    {
                        groupArr.Add(new JObject
                        {
                            ["FilterTagId"] = tagFilter.AdditionalFilterTagId,
                            ["Summary"] = tagFilter.Summary
                        });
                    }
                    groupedSummaries.Add(groupArr);
                }
                var currentGroupArr = new JArray();
                foreach (var tagFilter in currentTakerTagFilters)
                {
                    currentGroupArr.Add(new JObject
                    {
                        ["FilterTagId"] = tagFilter.FilterTagId,
                        ["Summary"] = tagFilter.Summary
                    });
                }
                groupedSummaries.Add(currentGroupArr);
                var groupedSummariesJson = groupedSummaries.ToString();

                var prompt = $@"Bạn là AI chuyên phân tích survey. Dưới đây là danh sách filter tag (id, name) và các nhóm summary (mỗi nhóm là 1 lần làm survey hoặc group hiện tại, mỗi phần tử gồm FilterTagId, Summary). Hãy gộp các summary cùng FilterTagId thành 1 Summary chung nhất, ngắn gọn, súc tích, đủ ý, phản ánh xu hướng hoạt động của người dùng. Nếu ở tag nào đó không được đề cập trong các yếu tố đầu vào (hoặc không chất lọc được thông tin) thì Summary phải để null, bạn buộc phải tuân theo rule này (không được viết các câu kiểu như 'không có thông tin', 'không có dữ liệu', 'không có', 'không rõ', v.v.). Không tự chế thêm tag, chỉ dùng đúng FilterTagId đã cho. Kết quả trả về đúng format function calling OpenAI yêu cầu.\n\nfilterTags: {filterTagsJson}\ngroupedSummaries: {groupedSummariesJson}";

                var body = new JObject
                {
                    ["model"] = _openAIConfig.BaseModel,
                    ["messages"] = new JArray
                {
                    new JObject { ["role"] = "system", ["content"] = "Bạn là AI chuyên phân tích survey và trả về JSON cho BE xử lý." },
                    new JObject { ["role"] = "user", ["content"] = prompt }
                },
                    ["functions"] = new JArray { functionSchema },
                    ["function_call"] = new JObject { ["name"] = "merge_additional_filter_tag_summaries" }
                };

                var argumentsJson = await _openAI4oMiniService.CallOpenAIChatCompletionFunctionAsync(body);
                List<SummarizedFilterTagDTO> resultList = null;
                if (!string.IsNullOrEmpty(argumentsJson.ToString()))
                {
                    var resultObj = JObject.Parse(argumentsJson.ToString());
                    if (resultObj["result"] != null)
                    {
                        resultList = JsonConvert.DeserializeObject<List<SummarizedFilterTagDTO>>(resultObj["result"].ToString());
                    }
                }
                // Đảm bảo trả về đủ các tag, kể cả summary null
                var resultDict = resultList?.ToDictionary(x => x.FilterTagId) ?? new Dictionary<int, SummarizedFilterTagDTO>();
                var fullResult = filterTags.Select(tag =>
                    resultDict.TryGetValue(tag.Id, out var summary)
                        ? summary
                        : new SummarizedFilterTagDTO { FilterTagId = tag.Id, Summary = null }
                ).ToList();
                return fullResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                throw new Exception($"lỗi trong hàm GetMergedAdditionalFilterTagSummariesAsync {ex.Message}, dòng {new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()}");
            }


        }

        /// <summary>
        /// Tóm tắt các filter tag dựa trên thông tin segment người dùng mục tiêu (SurveyTakerSegment), SurveyTopic, SurveySpecificTopic.
        /// </summary>
        /// <param name="filterTags">Danh sách filter tag (default + additional)</param>
        /// <param name="surveyTakerSegment">Thông tin segment người dùng mục tiêu</param>
        /// <param name="surveyTopic">Chủ đề survey</param>
        /// <param name="surveySpecificTopic">Chủ đề cụ thể survey</param>
        /// <returns>Danh sách summary cho từng filter tag: {FilterTagId, Summary}</returns>
        public async Task<List<SummarizedFilterTagDTO>> GetSurveyTakerSegmentSummarizedFilterTagAsync(
            List<FilterTag> filterTags,
            SurveyTakerSegmentDTO surveyTakerSegment,
            SurveyTopic surveyTopic,
            SurveySpecificTopic surveySpecificTopic)
        {
            try
            {
                var functionSchema = new JObject
                {
                    ["name"] = "summarize_segment_filter_tags",
                    ["description"] = "Tóm tắt ngắn gọn tiếng Việt cho từng filter tag dựa trên thông tin segment người dùng mục tiêu, chủ đề survey và chủ đề cụ thể.",
                    ["parameters"] = new JObject
                    {
                        ["type"] = "object",
                        ["properties"] = new JObject
                        {
                            ["result"] = new JObject
                            {
                                ["type"] = "array",
                                ["items"] = new JObject
                                {
                                    ["type"] = "object",
                                    ["properties"] = new JObject
                                    {
                                        ["FilterTagId"] = new JObject { ["type"] = "integer" },
                                        ["Summary"] = new JObject { ["type"] = "string", ["description"] = "Tóm tắt ngắn gọn tiếng Việt hoặc null nếu không liên quan." }
                                    },
                                    ["required"] = new JArray { "FilterTagId", "Summary" }
                                }
                            }
                        },
                        ["required"] = new JArray { "result" }
                    }
                };
                // Modify data
                filterTags = filterTags.Select(tag => new FilterTag
                {
                    Id = tag.Id,
                    FilterTagTypeId = tag.FilterTagTypeId,
                    Name = tag.Name,
                    FilterTagType = new FilterTagType
                    {
                        Id = tag.FilterTagType.Id,
                        Name = tag.FilterTagType.Name
                    }
                }).ToList();

                // Format data
                var filterTagsJson = JArray.FromObject(filterTags).ToString();
                var surveyTakerSegmentJson = JObject.FromObject(surveyTakerSegment).ToString();
                // Console.WriteLine($"surveyTakerSegmentJson: {surveyTakerSegmentJson}");
                var topicObj = surveyTopic == null ? null : new JObject
                {
                    ["Id"] = surveyTopic.Id,
                    ["Name"] = surveyTopic.Name
                };
                var topicObjJson = topicObj?.ToString();
                var specificTopicObj = surveySpecificTopic == null ? null : new JObject
                {
                    ["Id"] = surveySpecificTopic.Id,
                    ["Name"] = surveySpecificTopic.Name,
                    ["SurveyTopicId"] = surveySpecificTopic.SurveyTopicId,
                    ["SurveyTopicName"] = surveySpecificTopic.SurveyTopic?.Name
                };
                var specificTopicObjJson = specificTopicObj?.ToString();
                // Nếu cần modify dữ liệu, modify ở đây trước khi đưa vào prompt

                var prompt = $@"Bạn là AI chuyên phân tích survey. Dưới đây là danh sách filter tag, thông tin segment người dùng mục tiêu (gồm các trường: CountryRegion, MaritalStatus, AverageIncome, EducationLevel, JobField, Prompt), chủ đề survey và chủ đề cụ thể. Hãy trả về 1 mảng JSON, mỗi phần tử gồm FilterTagId và Summary tiếng Việt thật ngắn gọn, súc tích, đủ ý, tóm tắt dựa trên thông tin segment, surveyTopic, surveySpecificTopic. Nếu ở tag nào đó không được đề cập trong các yếu tố đầu vào (hoặc không chất lọc được thông tin) thì Summary phải để null, bạn buộc phải tuân theo rule này (không được viết các câu kiểu như 'không có thông tin', 'không có dữ liệu', 'không có', 'không rõ', v.v.). Không tự chế thêm tag, chỉ dùng đúng FilterTagId đã cho. Kết quả trả về đúng format function calling OpenAI yêu cầu.\n\nfilterTags: {filterTagsJson}\nsurveyTakerSegment: {surveyTakerSegmentJson}\nsurveyTopic: {topicObjJson}\nsurveySpecificTopic: {specificTopicObjJson}";

                var body = new JObject
                {
                    ["model"] = _openAIConfig.BaseModel,
                    ["messages"] = new JArray
                {
                    new JObject { ["role"] = "system", ["content"] = "Bạn là AI chuyên phân tích survey và trả về JSON cho BE xử lý." },
                    new JObject { ["role"] = "user", ["content"] = prompt }
                },
                    ["functions"] = new JArray { functionSchema },
                    ["function_call"] = new JObject { ["name"] = "summarize_segment_filter_tags" }
                };

                var argumentsJson = await _openAI4oMiniService.CallOpenAIChatCompletionFunctionAsync(body);
                List<SummarizedFilterTagDTO> resultList = null;
                if (!string.IsNullOrEmpty(argumentsJson.ToString()))
                {
                    var resultObj = JObject.Parse(argumentsJson.ToString());
                    if (resultObj["result"] != null)
                    {
                        resultList = JsonConvert.DeserializeObject<List<SummarizedFilterTagDTO>>(resultObj["result"].ToString());
                    }
                }
                // Đảm bảo trả về đủ các tag, kể cả summary null
                var resultDict = resultList?.ToDictionary(x => x.FilterTagId) ?? new Dictionary<int, SummarizedFilterTagDTO>();
                var fullResult = filterTags.Select(tag =>
                    resultDict.TryGetValue(tag.Id, out var summary)
                        ? summary
                        : new SummarizedFilterTagDTO { FilterTagId = tag.Id, Summary = null }
                ).ToList();
                return fullResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                throw new Exception($"lỗi trong hàm GetSurveyTakerSegmentSummarizedFilterTagAsync {ex.Message}, dòng {new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()}");
                return filterTags.Select(tag => new SummarizedFilterTagDTO { FilterTagId = tag.Id, Summary = null }).ToList();
            }

        }

    }
}
