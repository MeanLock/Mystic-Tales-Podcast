-- INSERT SAMPLE SURVEY, QUESTION, OPTION, TAKEN RESULT, RESPONSE

-- 1. Insert Survey
INSERT INTO Survey (
    requesterId, title, description, surveyTypeId, surveyTopicId, surveySpecificTopicId, startDate, endDate, kpi, securityModeId, theoryPrice, extraPrice, takerBaseRewardPrice, profitPrice, allocBaseAmount, allocTimeAmount, allocLevelAmount, maxXp, isAvailable, configJsonString, publishedAt, deletedAt, createdAt, updatedAt
) VALUES (
    2, -- requesterId (giả sử là accountId 200)
    N'Survey mẫu tổng hợp',
    N'Survey kiểm thử với 7 loại câu hỏi',
    1, -- surveyTypeId
    NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL,
    1, -- isAvailable
    N'{"backgroundGradient1Color":"#ffffff","backgoundGradient2Color":"#eeeeee","titleColor":"#000000","contentColor":"#333333","buttonBackgroundColor":"#007bff","buttonContentColor":"#ffffff","password":null}',
    GETDATE(), NULL, GETDATE(), GETDATE()
);

DECLARE @SurveyId INT = SCOPE_IDENTITY();

-- 2. Insert SurveyQuestion & SurveyOption cho từng câu hỏi
-- Q1: Single Choice
INSERT INTO SurveyQuestion (surveyId, questionTypeId, content, description, timeLimit, isVoiced, [order], configJsonString, deletedAt)
VALUES (@SurveyId, 1, N'Bạn thích màu nào nhất?', N'Chọn một màu bạn thích nhất.', 30, 0, 1, N'{}', NULL);
DECLARE @Q1Id INT = SCOPE_IDENTITY();
INSERT INTO SurveyOption (surveyQuestionId, content, [order]) VALUES
(@Q1Id, N'Đỏ', 1),
(@Q1Id, N'Xanh', 2),
(@Q1Id, N'Vàng', 3);

-- Q2: Multiple Choice
INSERT INTO SurveyQuestion (surveyId, questionTypeId, content, description, timeLimit, isVoiced, [order], configJsonString, deletedAt)
VALUES (@SurveyId, 2, N'Bạn thích những loại trái cây nào?', N'Chọn nhiều loại nếu muốn.', 30, 0, 2, N'{"minChoiceCount":1,"maxChoiceCount":3}', NULL);
DECLARE @Q2Id INT = SCOPE_IDENTITY();
INSERT INTO SurveyOption (surveyQuestionId, content, [order]) VALUES
(@Q2Id, N'Táo', 1),
(@Q2Id, N'Cam', 2),
(@Q2Id, N'Chuối', 3);

-- Q3: Single Slider
INSERT INTO SurveyQuestion (surveyId, questionTypeId, content, description, timeLimit, isVoiced, [order], configJsonString, deletedAt)
VALUES (@SurveyId, 3, N'Bạn đánh giá mức độ hài lòng từ 1 đến 10.', N'Kéo thanh trượt để chọn mức độ.', 30, 0, 3, N'{"min":1,"max":10,"step":1,"unit":"điểm"}', NULL);
DECLARE @Q3Id INT = SCOPE_IDENTITY();

-- Q4: Range Slider
INSERT INTO SurveyQuestion (surveyId, questionTypeId, content, description, timeLimit, isVoiced, [order], configJsonString, deletedAt)
VALUES (@SurveyId, 4, N'Chọn khoảng tuổi phù hợp.', N'Kéo để chọn khoảng tuổi.', 30, 0, 4, N'{"min":18,"max":60,"step":1,"unit":"tuổi"}', NULL);
DECLARE @Q4Id INT = SCOPE_IDENTITY();

-- Q5: Single input by types
INSERT INTO SurveyQuestion (surveyId, questionTypeId, content, description, timeLimit, isVoiced, [order], configJsonString, deletedAt)
VALUES (@SurveyId, 5, N'Nhập số điện thoại của bạn.', N'Vui lòng nhập số điện thoại.', 30, 0, 5, N'{"fieldInputTypeId":1}', NULL);
DECLARE @Q5Id INT = SCOPE_IDENTITY();

-- Q6: Rating
INSERT INTO SurveyQuestion (surveyId, questionTypeId, content, description, timeLimit, isVoiced, [order], configJsonString, deletedAt)
VALUES (@SurveyId, 6, N'Đánh giá chất lượng dịch vụ.', N'Chọn số sao phù hợp.', 30, 0, 6, N'{"ratingLength":5,"ratingIcon":"star"}', NULL);
DECLARE @Q6Id INT = SCOPE_IDENTITY();

-- Q7: Ranking
INSERT INTO SurveyQuestion (surveyId, questionTypeId, content, description, timeLimit, isVoiced, [order], configJsonString, deletedAt)
VALUES (@SurveyId, 7, N'Xếp hạng các tiêu chí sau theo mức độ quan trọng.', N'Kéo thả để sắp xếp.', 30, 0, 7, N'{}', NULL);
DECLARE @Q7Id INT = SCOPE_IDENTITY();
INSERT INTO SurveyOption (surveyQuestionId, content, [order]) VALUES
(@Q7Id, N'Giá cả', 1),
(@Q7Id, N'Chất lượng', 2),
(@Q7Id, N'Dịch vụ', 3);

-- 3. Insert SurveyTakenResult
INSERT INTO SurveyTakenResult (
    surveyId, takerId, isValid, invalidReason, moneyEarned, xpEarned, completedAt
) VALUES (
    @SurveyId, 3, 1, NULL, 10.0, 5, '2025-06-15T10:00:00Z'
);
DECLARE @TakenResultId INT = SCOPE_IDENTITY();

-- 4. Insert SurveyResponse cho từng câu hỏi
INSERT INTO SurveyResponse (surveyTakenResultId, surveyQuestionId, isValid, valueJsonString) VALUES
-- Q1: Single Choice
(@TakenResultId, @Q1Id, 1, N'{"questionContent":{"id":'+CAST(@Q1Id AS NVARCHAR)+',"questionTypeId":1,"content":"Bạn thích màu nào nhất?","description":"Chọn một màu bạn thích nhất.","configJson":{},"options":[{"id":1,"content":"Đỏ"},{"id":2,"content":"Xanh"},{"id":3,"content":"Vàng"}]},"questionResponse":{"input":null,"range":null,"ranking":null,"singleChoice":2,"multipleChoice":null,"speechText":null}}'),
-- Q2: Multiple Choice
(@TakenResultId, @Q2Id, 1, N'{"questionContent":{"id":'+CAST(@Q2Id AS NVARCHAR)+',"questionTypeId":2,"content":"Bạn thích những loại trái cây nào?","description":"Chọn nhiều loại nếu muốn.","configJson":{"minChoiceCount":1,"maxChoiceCount":3},"options":[{"id":4,"content":"Táo"},{"id":5,"content":"Cam"},{"id":6,"content":"Chuối"}]},"questionResponse":{"input":null,"range":null,"ranking":null,"singleChoice":null,"multipleChoice":[4,6],"speechText":null}}'),
-- Q3: Single Slider
(@TakenResultId, @Q3Id, 1, N'{"questionContent":{"id":'+CAST(@Q3Id AS NVARCHAR)+',"questionTypeId":3,"content":"Bạn đánh giá mức độ hài lòng từ 1 đến 10.","description":"Kéo thanh trượt để chọn mức độ.","configJson":{"min":1,"max":10,"step":1,"unit":"điểm"},"options":[]},"questionResponse":{"input":{"value":8,"valueType":"number"},"range":null,"ranking":null,"singleChoice":null,"multipleChoice":null,"speechText":null}}'),
-- Q4: Range Slider
(@TakenResultId, @Q4Id, 1, N'{"questionContent":{"id":'+CAST(@Q4Id AS NVARCHAR)+',"questionTypeId":4,"content":"Chọn khoảng tuổi phù hợp.","description":"Kéo để chọn khoảng tuổi.","configJson":{"min":18,"max":60,"step":1,"unit":"tuổi"},"options":[]},"questionResponse":{"input":null,"range":{"min":25,"max":35},"ranking":null,"singleChoice":null,"multipleChoice":null,"speechText":null}}'),
-- Q5: Single input by types
(@TakenResultId, @Q5Id, 1, N'{"questionContent":{"id":'+CAST(@Q5Id AS NVARCHAR)+',"questionTypeId":5,"content":"Nhập số điện thoại của bạn.","description":"Vui lòng nhập số điện thoại.","configJson":{"fieldInputTypeId":1},"options":[]},"questionResponse":{"input":{"value":"0912345678","valueType":"string"},"range":null,"ranking":null,"singleChoice":null,"multipleChoice":null,"speechText":null}}'),
-- Q6: Rating
(@TakenResultId, @Q6Id, 1, N'{"questionContent":{"id":'+CAST(@Q6Id AS NVARCHAR)+',"questionTypeId":6,"content":"Đánh giá chất lượng dịch vụ.","description":"Chọn số sao phù hợp.","configJson":{"ratingLength":5,"ratingIcon":"star"},"options":[]},"questionResponse":{"input":{"value":4,"valueType":"number"},"range":null,"ranking":null,"singleChoice":null,"multipleChoice":null,"speechText":null}}'),
-- Q7: Ranking
(@TakenResultId, @Q7Id, 1, N'{"questionContent":{"id":'+CAST(@Q7Id AS NVARCHAR)+',"questionTypeId":7,"content":"Xếp hạng các tiêu chí sau theo mức độ quan trọng.","description":"Kéo thả để sắp xếp.","configJson":{},"options":[{"id":7,"content":"Giá cả"},{"id":8,"content":"Chất lượng"},{"id":9,"content":"Dịch vụ"}]},"questionResponse":{"input":null,"range":null,"ranking":[{"surveyOptionId":8,"rankIndex":1},{"surveyOptionId":7,"rankIndex":2},{"surveyOptionId":9,"rankIndex":3}],"singleChoice":null,"multipleChoice":null,"speechText":null}}');


DECLARE @TakenResultId INT = SCOPE_IDENTITY();

-- Xoá tất cả các SurveyResponse của SurveyTakenResult này
DELETE FROM SurveyResponse WHERE surveyTakenResultId = 1;

SELECT TOP (1000) [id]
      ,[surveyTakenResultId]
      ,[surveyQuestionId]
      ,[isValid]
      ,[valueJsonString]
  FROM [SurveyTalkDB_test].[dbo].[SurveyResponse]

-- Thêm lại tất cả các SurveyResponse
DECLARE @Q1Json NVARCHAR(MAX) = N'{"questionContent":{"id":1,"questionTypeId":1,"content":"Bạn thích màu nào nhất?","description":"Chọn một màu bạn thích nhất.","configJson":{},"options":[{"id":1,"content":"Đỏ"},{"id":2,"content":"Xanh"},{"id":3,"content":"Vàng"}]},"questionResponse":{"input":null,"range":null,"ranking":null,"singleChoice":2,"multipleChoice":null,"speechText":null}}';
DECLARE @Q2Json NVARCHAR(MAX) = N'{"questionContent":{"id":2,"questionTypeId":2,"content":"Bạn thích những loại trái cây nào?","description":"Chọn nhiều loại nếu muốn.","configJson":{"minChoiceCount":1,"maxChoiceCount":3},"options":[{"id":4,"content":"Táo"},{"id":5,"content":"Cam"},{"id":6,"content":"Chuối"}]},"questionResponse":{"input":null,"range":null,"ranking":null,"singleChoice":null,"multipleChoice":[4,6],"speechText":null}}';
DECLARE @Q3Json NVARCHAR(MAX) = N'{"questionContent":{"id":3,"questionTypeId":3,"content":"Bạn đánh giá mức độ hài lòng từ 1 đến 10.","description":"Kéo thanh trượt để chọn mức độ.","configJson":{"min":1,"max":10,"step":1,"unit":"điểm"},"options":[]},"questionResponse":{"input":{"value":8,"valueType":"number"},"range":null,"ranking":null,"singleChoice":null,"multipleChoice":null,"speechText":null}}';
DECLARE @Q4Json NVARCHAR(MAX) = N'{"questionContent":{"id":4,"questionTypeId":4,"content":"Chọn khoảng tuổi phù hợp.","description":"Kéo để chọn khoảng tuổi.","configJson":{"min":18,"max":60,"step":1,"unit":"tuổi"},"options":[]},"questionResponse":{"input":null,"range":{"min":25,"max":35},"ranking":null,"singleChoice":null,"multipleChoice":null,"speechText":null}}';
DECLARE @Q5Json NVARCHAR(MAX) = N'{"questionContent":{"id":5,"questionTypeId":5,"content":"Nhập số điện thoại của bạn.","description":"Vui lòng nhập số điện thoại.","configJson":{"fieldInputTypeId":1},"options":[]},"questionResponse":{"input":{"value":"0912345678","valueType":"string"},"range":null,"ranking":null,"singleChoice":null,"multipleChoice":null,"speechText":null}}';
DECLARE @Q6Json NVARCHAR(MAX) = N'{"questionContent":{"id":6,"questionTypeId":6,"content":"Đánh giá chất lượng dịch vụ.","description":"Chọn số sao phù hợp.","configJson":{"ratingLength":5,"ratingIcon":"star"},"options":[]},"questionResponse":{"input":{"value":4,"valueType":"number"},"range":null,"ranking":null,"singleChoice":null,"multipleChoice":null,"speechText":null}}';
DECLARE @Q7Json NVARCHAR(MAX) = N'{"questionContent":{"id":7,"questionTypeId":7,"content":"Xếp hạng các tiêu chí sau theo mức độ quan trọng.","description":"Kéo thả để sắp xếp.","configJson":{},"options":[{"id":7,"content":"Giá cả"},{"id":8,"content":"Chất lượng"},{"id":9,"content":"Dịch vụ"}]},"questionResponse":{"input":null,"range":null,"ranking":[{"surveyOptionId":8,"rankIndex":1},{"surveyOptionId":7,"rankIndex":2},{"surveyOptionId":9,"rankIndex":3}],"singleChoice":null,"multipleChoice":null,"speechText":null}}';

INSERT INTO SurveyResponse (surveyTakenResultId, surveyQuestionId, isValid, valueJsonString) VALUES
(1, 1, 1, @Q1Json),
(1, 2, 1, @Q2Json),
(1, 3, 1, @Q3Json),
(1, 4, 1, @Q4Json),
(1, 5, 1, @Q5Json),
(1, 6, 1, @Q6Json),
(1, 7, 1, @Q7Json);
-- Kết thúc script mẫu
--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
-- Insert 1 survey mới với mọi câu hỏi đều liên quan mọi filter tag
INSERT INTO Survey (
    requesterId, title, description, surveyTypeId, surveyTopicId, surveySpecificTopicId, startDate, endDate, kpi, securityModeId, theoryPrice, extraPrice, takerBaseRewardPrice, profitPrice, allocBaseAmount, allocTimeAmount, allocLevelAmount, maxXp, isAvailable, configJsonString, publishedAt, deletedAt, createdAt, updatedAt
) VALUES (
    2, -- requesterId
    N'Survey liên quan tất cả filter tag',
    N'Survey kiểm thử với mọi câu hỏi liên quan mọi filter tag',
    1, -- surveyTypeId
    NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL,
    1, -- isAvailable
    N'{"backgroundGradient1Color":"#ffffff","backgoundGradient2Color":"#eeeeee","titleColor":"#000000","contentColor":"#333333","buttonBackgroundColor":"#007bff","buttonContentColor":"#ffffff","password":null}',
    GETDATE(), NULL, GETDATE(), GETDATE()
);

DECLARE @SurveyAllTagId INT = SCOPE_IDENTITY();

-- Insert 7 câu hỏi, mỗi câu hỏi đều liên quan đến mọi filter tag (giả định mô tả liên quan)
INSERT INTO SurveyQuestion (surveyId, questionTypeId, content, description, timeLimit, isVoiced, [order], configJsonString, deletedAt)
VALUES (@SurveyAllTagId, 1, N'Giới tính của bạn là gì?', N'Câu hỏi liên quan filter tag Giới tính', 30, 0, 1, N'{}', NULL);
DECLARE @Q1AllTagId INT = SCOPE_IDENTITY();
INSERT INTO SurveyOption (surveyQuestionId, content, [order]) VALUES
(@Q1AllTagId, N'Nam', 1),
(@Q1AllTagId, N'Nữ', 2),
(@Q1AllTagId, N'Khác', 3);

INSERT INTO SurveyQuestion (surveyId, questionTypeId, content, description, timeLimit, isVoiced, [order], configJsonString, deletedAt)
VALUES (@SurveyAllTagId, 2, N'Bạn thuộc độ tuổi nào?', N'Câu hỏi liên quan filter tag Độ tuổi', 30, 0, 2, N'{"minChoiceCount":1,"maxChoiceCount":3}', NULL);
DECLARE @Q2AllTagId INT = SCOPE_IDENTITY();
INSERT INTO SurveyOption (surveyQuestionId, content, [order]) VALUES
(@Q2AllTagId, N'18-25', 1),
(@Q2AllTagId, N'26-35', 2),
(@Q2AllTagId, N'36-50', 3);

INSERT INTO SurveyQuestion (surveyId, questionTypeId, content, description, timeLimit, isVoiced, [order], configJsonString, deletedAt)
VALUES (@SurveyAllTagId, 3, N'Bạn sống ở khu vực nào?', N'Câu hỏi liên quan filter tag Khu vực sống', 30, 0, 3, N'{"min":1,"max":10,"step":1,"unit":"điểm"}', NULL);
DECLARE @Q3AllTagId INT = SCOPE_IDENTITY();

INSERT INTO SurveyQuestion (surveyId, questionTypeId, content, description, timeLimit, isVoiced, [order], configJsonString, deletedAt)
VALUES (@SurveyAllTagId, 4, N'Nghề nghiệp hiện tại của bạn?', N'Câu hỏi liên quan filter tag Nghề nghiệp', 30, 0, 4, N'{"min":18,"max":60,"step":1,"unit":"tuổi"}', NULL);
DECLARE @Q4AllTagId INT = SCOPE_IDENTITY();

INSERT INTO SurveyQuestion (surveyId, questionTypeId, content, description, timeLimit, isVoiced, [order], configJsonString, deletedAt)
VALUES (@SurveyAllTagId, 5, N'Trình độ học vấn của bạn?', N'Câu hỏi liên quan filter tag Trình độ học vấn', 30, 0, 5, N'{"fieldInputTypeId":1}', NULL);
DECLARE @Q5AllTagId INT = SCOPE_IDENTITY();

INSERT INTO SurveyQuestion (surveyId, questionTypeId, content, description, timeLimit, isVoiced, [order], configJsonString, deletedAt)
VALUES (@SurveyAllTagId, 6, N'Tình trạng hôn nhân của bạn?', N'Câu hỏi liên quan filter tag Tình trạng hôn nhân', 30, 0, 6, N'{"ratingLength":5,"ratingIcon":"star"}', NULL);
DECLARE @Q6AllTagId INT = SCOPE_IDENTITY();

INSERT INTO SurveyQuestion (surveyId, questionTypeId, content, description, timeLimit, isVoiced, [order], configJsonString, deletedAt)
VALUES (@SurveyAllTagId, 7, N'Thu nhập của bạn thuộc mức nào?', N'Câu hỏi liên quan filter tag Thu nhập', 30, 0, 7, N'{}', NULL);
DECLARE @Q7AllTagId INT = SCOPE_IDENTITY();
INSERT INTO SurveyOption (surveyQuestionId, content, [order]) VALUES
(@Q7AllTagId, N'< 5 triệu', 1),
(@Q7AllTagId, N'5-15 triệu', 2),
(@Q7AllTagId, N'> 15 triệu', 3);

-- Insert SurveyTakenResult cho takerId = 3
INSERT INTO SurveyTakenResult (
    surveyId, takerId, isValid, invalidReason, moneyEarned, xpEarned, completedAt
) VALUES (
    @SurveyAllTagId, 3, 1, NULL, 10.0, 5, GETDATE()
);

SELECT *
  FROM [SurveyTalkDB_test].[dbo].SurveyTakenResult


DECLARE @TakenResultAllTagId INT = SCOPE_IDENTITY();

-- Insert SurveyResponse cho từng câu hỏi (giả lập response liên quan mọi filter tag)
DECLARE @Q1AllTagJson NVARCHAR(MAX) = N'{"questionContent":{"id":8,"questionTypeId":1,"content":"Giới tính của bạn là gì?","description":"Câu hỏi liên quan filter tag Giới tính","configJson":{},"options":[{"id":1,"content":"Nam"},{"id":2,"content":"Nữ"},{"id":3,"content":"Khác"}]},"questionResponse":{"input":null,"range":null,"ranking":null,"singleChoice":1,"multipleChoice":null,"speechText":null}}';
DECLARE @Q2AllTagJson NVARCHAR(MAX) = N'{"questionContent":{"id":9,"questionTypeId":2,"content":"Bạn thuộc độ tuổi nào?","description":"Câu hỏi liên quan filter tag Độ tuổi","configJson":{"minChoiceCount":1,"maxChoiceCount":3},"options":[{"id":4,"content":"18-25"},{"id":5,"content":"26-35"},{"id":6,"content":"36-50"}]},"questionResponse":{"input":null,"range":null,"ranking":null,"singleChoice":null,"multipleChoice":[4],"speechText":null}}';
DECLARE @Q3AllTagJson NVARCHAR(MAX) = N'{"questionContent":{"id":10,"questionTypeId":3,"content":"Bạn sống ở khu vực nào?","description":"Câu hỏi liên quan filter tag Khu vực sống","configJson":{"min":1,"max":10,"step":1,"unit":"điểm"},"options":[]},"questionResponse":{"input":{"value":5,"valueType":"number"},"range":null,"ranking":null,"singleChoice":null,"multipleChoice":null,"speechText":null}}';
DECLARE @Q4AllTagJson NVARCHAR(MAX) = N'{"questionContent":{"id":11,"questionTypeId":4,"content":"Nghề nghiệp hiện tại của bạn?","description":"Câu hỏi liên quan filter tag Nghề nghiệp","configJson":{"min":18,"max":60,"step":1,"unit":"tuổi"},"options":[]},"questionResponse":{"input":null,"range":{"min":25,"max":35},"ranking":null,"singleChoice":null,"multipleChoice":null,"speechText":null}}';
DECLARE @Q5AllTagJson NVARCHAR(MAX) = N'{"questionContent":{"id":12,"questionTypeId":5,"content":"Trình độ học vấn của bạn?","description":"Câu hỏi liên quan filter tag Trình độ học vấn","configJson":{"fieldInputTypeId":1},"options":[]},"questionResponse":{"input":{"value":"Đại học","valueType":"string"},"range":null,"ranking":null,"singleChoice":null,"multipleChoice":null,"speechText":null}}';
DECLARE @Q6AllTagJson NVARCHAR(MAX) = N'{"questionContent":{"id":13,"questionTypeId":6,"content":"Tình trạng hôn nhân của bạn?","description":"Câu hỏi liên quan filter tag Tình trạng hôn nhân","configJson":{"ratingLength":5,"ratingIcon":"star"},"options":[]},"questionResponse":{"input":{"value":2,"valueType":"number"},"range":null,"ranking":null,"singleChoice":null,"multipleChoice":null,"speechText":null}}';
DECLARE @Q7AllTagJson NVARCHAR(MAX) = N'{"questionContent":{"id":14,"questionTypeId":7,"content":"Thu nhập của bạn thuộc mức nào?","description":"Câu hỏi liên quan filter tag Thu nhập","configJson":{},"options":[{"id":7,"content":"< 5 triệu"},{"id":8,"content":"5-15 triệu"},{"id":9,"content":"> 15 triệu"}]},"questionResponse":{"input":null,"range":null,"ranking":[{"surveyOptionId":8,"rankIndex":1},{"surveyOptionId":7,"rankIndex":2},{"surveyOptionId":9,"rankIndex":3}],"singleChoice":null,"multipleChoice":null,"speechText":null}}';

INSERT INTO SurveyResponse (surveyTakenResultId, surveyQuestionId, isValid, valueJsonString) VALUES
(2, 8, 1, @Q1AllTagJson),
(2, 9, 1, @Q2AllTagJson),
(2, 10, 1, @Q3AllTagJson),
(2, 11, 1, @Q4AllTagJson),
(2, 12, 1, @Q5AllTagJson),
(2, 13, 1, @Q6AllTagJson),
(2, 14, 1, @Q7AllTagJson);
-- Kết thúc insert survey liên quan mọi filter tag

-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

-- ===================== INSERT FULL SAMPLE FOR SurveyTakenResultTagFilter (ALL ADDITIONAL FILTER TAGS) =====================
-- Danh sách id và tên tag lấy từ INSERT_FIX_DATA.sql, filterTagTypeId = 2 (id 14-40)
-- Group cho takenResultId = 1
INSERT INTO SurveyTakenResultTagFilter (surveyTakenResultId, additionalFilterTagId, summary) VALUES
(1, 14, N'Quan tâm tiêu dùng thông minh'),
(1, 15, N'Thường xuyên mua sắm online'),
(1, 16, N'Ưu tiên sản phẩm xanh'),
(1, 17, N'Thích thương hiệu nổi tiếng'),
(1, 18, N'Luôn cập nhật điện thoại mới'),
(1, 19, N'Yêu thích công nghệ mới'),
(1, 20, N'Thích chơi game giải trí'),
(1, 21, N'Quan tâm bảo mật thông tin'),
(1, 22, N'Thường gửi tiết kiệm'),
(1, 23, N'Đầu tư chứng khoán'),
(1, 24, N'Từng vay tiêu dùng'),
(1, 25, N'Thường xuyên tập thể thao'),
(1, 26, N'Ăn uống lành mạnh'),
(1, 27, N'Chú trọng sức khỏe tinh thần'),
(1, 28, N'Chủ động kiểm tra sức khỏe'),
(1, 29, N'Học online mỗi tuần'),
(1, 30, N'Thường học kỹ năng mềm'),
(1, 31, N'Học ngoại ngữ mới'),
(1, 32, N'Định hướng nghề nghiệp rõ ràng'),
(1, 33, N'Chăm sóc con cái'),
(1, 34, N'Nuôi thú cưng'),
(1, 35, N'Có nhiều mối quan hệ xã hội'),
(1, 36, N'Quan tâm các vấn đề xã hội'),
(1, 37, N'Thích xem phim'),
(1, 38, N'Nghe nhạc mỗi ngày'),
(1, 39, N'Thường xuyên đọc sách'),
(1, 40, N'Thích du lịch khám phá');

-- Group cho takenResultId = 2
INSERT INTO SurveyTakenResultTagFilter (surveyTakenResultId, additionalFilterTagId, summary) VALUES
(2, 14, N'Rất thích tiêu dùng thông minh'), -- gần giống
(2, 15, N'Mua hàng chủ yếu tại cửa hàng'), -- khác hoàn toàn
(2, 16, N'Chỉ chọn sản phẩm xanh'), -- gần giống
(2, 17, N'Không quan tâm thương hiệu'), -- khác hoàn toàn
(2, 18, N'Luôn đổi điện thoại khi có mẫu mới'), -- gần giống
(2, 19, N'Không thích công nghệ mới'), -- khác hoàn toàn
(2, 20, N'Chơi game giải trí mỗi tối'), -- gần giống
(2, 21, N'Không quan tâm an ninh mạng'), -- khác hoàn toàn
(2, 22, N'Đầu tư vào bất động sản'), -- khác hoàn toàn
(2, 23, N'Đầu tư crypto và chứng khoán'), -- gần giống
(2, 24, N'Không vay tiêu dùng'), -- khác hoàn toàn
(2, 25, N'Tập gym mỗi ngày'), -- gần giống
(2, 26, N'Ăn kiêng nghiêm ngặt'), -- gần giống
(2, 27, N'Quan tâm thiền và sức khỏe tinh thần'), -- gần giống
(2, 28, N'Không kiểm tra sức khỏe định kỳ'), -- khác hoàn toàn
(2, 29, N'Học online khi rảnh'), -- gần giống
(2, 30, N'Không học kỹ năng mềm'), -- khác hoàn toàn
(2, 31, N'Học tiếng Anh online'), -- gần giống
(2, 32, N'Chưa xác định nghề nghiệp'), -- khác hoàn toàn
(2, 33, N'Chăm sóc con nhỏ'), -- gần giống
(2, 34, N'Không nuôi thú cưng'), -- khác hoàn toàn
(2, 35, N'Ít giao tiếp xã hội'), -- khác hoàn toàn
(2, 36, N'Quan tâm môi trường'), -- gần giống
(2, 37, N'Xem phim mỗi tuần'), -- gần giống
(2, 38, N'Nghe nhạc khi làm việc'), -- gần giống
(2, 39, N'Không thích đọc sách'), -- khác hoàn toàn
(2, 40, N'Thích du lịch nước ngoài'); -- gần giống
-- ===================== END INSERT FULL SAMPLE FOR SurveyTakenResultTagFilter =====================
