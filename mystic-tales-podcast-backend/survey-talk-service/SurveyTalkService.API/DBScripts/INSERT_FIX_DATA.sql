INSERT INTO Role (id, name) VALUES
(1, N'Admin'),
(2, N'Manager'),
(3, N'Survey Supervisor'),
(4, N'Customer');

-- Seed SurveyStatus table
INSERT INTO SurveyStatus (id, name) VALUES 
(1, N'Editing'),
(2, N'Published'),
(3, N'Completed'),
(4, N'Deleted'),
(5, N'Deactivated');

-- Seed SurveyTopic table
INSERT INTO SurveyTopic (name) VALUES 
(N'Ẩm thực'),
(N'Giáo dục'),
(N'Sức khỏe & Thể dục'),
(N'Công nghệ'),
(N'Tài chính cá nhân'),
(N'Mua sắm & Thương mại'),
(N'Du lịch & Giải trí'),
(N'Xã hội & Hành vi'),
(N'Môi trường'),
(N'Chính trị & Pháp luật');


INSERT INTO SurveySpecificTopic (name, surveyTopicId) VALUES
(N'Món ăn yêu thích', 1),
(N'Thói quen ăn uống', 1),
(N'Chế độ ăn kiêng', 1),
(N'Chất lượng giáo dục', 2),
(N'Học trực tuyến', 2),
(N'Hành vi học tập', 2),
(N'Thói quen tập luyện', 3),
(N'Sức khỏe tinh thần', 3),
(N'Chế độ sinh hoạt', 3),
(N'Thiết bị thông minh', 4),
(N'Ứng dụng di động', 4),
(N'Trí tuệ nhân tạo', 4),
(N'Chi tiêu cá nhân', 5),
(N'Đầu tư nhỏ lẻ', 5),
(N'Quản lý nợ', 5),
(N'Trải nghiệm mua sắm online', 6),
(N'Thói quen tiêu dùng', 6),
(N'So sánh giá cả', 6),
(N'Kế hoạch du lịch', 7),
(N'Phương tiện di chuyển', 7),
(N'Địa điểm yêu thích', 7),
(N'Mạng xã hội', 8),
(N'Thói quen sử dụng điện thoại', 8),
(N'Tâm lý người tiêu dùng', 8),
(N'Bảo vệ môi trường', 9),
(N'Tái chế và rác thải', 9),
(N'Biến đổi khí hậu', 9),
(N'Nhận thức chính trị', 10),
(N'Tham gia bầu cử', 10),
(N'Luật pháp và quyền công dân', 10);


INSERT INTO SurveyType (id, name) VALUES 
(1, N'Filter Survey'),
(2, N'Community Survey'),
(3, N'Market Research Survey');


INSERT INTO SurveySecurityMode (id, name, description) VALUES
(1, N'Basic', N'6 loại câu hỏi để bạn tha hồ tùy chỉnh khảo sát. Random captcha giữa những câu hỏi. Random Time-limit cho câu hỏi'),
(2, N'Advance', N'6 loại câu hỏi để bạn tha hồ tùy chỉnh khảo sát. Random captcha giữa những câu hỏi. Random Re-question câu hỏi bất kỳ cho bài khảo sát. Chủ động điều chỉnh Time-limit cho từng câu hỏi. Cơ chế Jump Logic giúp khảo sát được liền mạch và chắt lọc thông tin hơn.'),
(3, N'Pro', N'6 loại câu hỏi để bạn tha hồ tùy chỉnh khảo sát. Random captcha giữa những câu hỏi. Random Re-question câu hỏi bất kỳ cho bài khảo sát. Chủ động điều chỉnh Time-limit cho từng câu hỏi. Cơ chế Jump Logic giúp khảo sát được liền mạch và chắt lọc thông tin hơn. Tính năng set voice-answer cho câu hỏi');


INSERT INTO SurveyQuestionType (id, name, price, deactivatedAt) VALUES
(1, N'Single Choice', 1000, NULL),
(2, N'Multiple Choice', 1000, NULL),
(3, N'Single Slider', 1000, NULL),
(4, N'Range Slider', 1000, NULL),
(5, N'Single input by types', 1000, NULL),
(6, N'Rating', 1000, NULL),
(7, N'Ranking', 1000, NULL);


INSERT INTO SurveyFieldInputType (id, name) VALUES
(1, N'Kiểu câu ngắn không xuống dòng'),
(2, N'Kiểu văn bản dài có xuống dòng'),
(3, N'Kiếu email'),
(4, N'Kiếu số'),
(5, N'Kiểu ngày tháng năm (DD/MM/YYYY)'),
(6, N'Kiểu ngày tháng (DD/MM)'),
(7, N'Kiểu năm (4 số)'),
(8, N'Kiểu giờ phút (HH:MM)'),
(9, N'Kiểu ngày tháng năm giờ phút (DD/MM/YYYY HH:MM)');


---- PaymentType seed data
--INSERT INTO PaymentType (id, name, operationType) VALUES
--(1, N'Community survey publishment', N'-'),
--(2, N'Community survey taken earn', N'+'),
--(3, N'Data purchase', N'-'),
--(4, N'Data selling earn', N'+'),
--(5, N'Account balance deposits', N'+'),
--(6, N'Account balance withdrawal', N'-');

---- PaymentStatus seed data
--INSERT INTO PaymentStatus (id, name) VALUES
--(1, N'Processing'),
--(2, N'Success'),
--(3, N'Cancelled'),
--(4, N'Error');

-- TransactionType seed data
INSERT INTO TransactionType (id, name, operationType) VALUES
(1, N'Community survey publishment', N'-'),
(2, N'Community survey taken earn', N'+'),
(3, N'Data purchase', N'-'),
(4, N'Data selling earn', N'+'),
(5, N'Account balance deposits', N'+'),
(6, N'Account balance withdrawal', N'-');

-- TransactionStatus seed data
INSERT INTO TransactionStatus(id, name) VALUES
(1, N'Processing'),
(2, N'Success'),
(3, N'Cancelled'),
(4, N'Error');


INSERT INTO FilterTagType (id, name) VALUES 
(1, N'Default'),
(2, N'Additional');


INSERT INTO FilterTag (id, name, tagColor, filterTagTypeId) VALUES 
(1, N'Giới tính', '#FF9AA2', 1),
(2, N'Độ tuổi', '#FFB347', 1),
(3, N'Khu vực sống', '#A1C6EA', 1),
(4, N'Nghề nghiệp', '#9DE0AD', 1),
(5, N'Trình độ học vấn', '#CABBE9', 1),
(6, N'Tình trạng hôn nhân', '#FFC09F', 1),
(7, N'Thu nhập', '#E2F0CB', 1),
(8, N'Sở thích chính', '#F6A6B2', 1),
(9, N'Số người trong gia đình', '#B5EAD7', 1),
(10, N'Loại thiết bị đang dùng', '#D5AAFF', 1),
(11, N'Loại nhà ở (thuê / sở hữu)', '#8AC6D1', 1),
(12, N'Tôn giáo', '#F4B393', 1),
(13, N'Mức độ sử dụng Internet', '#B6E2D3', 1),
(14, N'Quan tâm tiêu dùng thông minh', '#F7C8E0', 2),
(15, N'Quan tâm mua sắm online', '#D3C0F9', 2),
(16, N'Quan tâm tiêu dùng xanh', '#CDEAC0', 2),
(17, N'Quan tâm thương hiệu cao cấp', '#FFDAC1', 2),
(18, N'Quan tâm điện thoại di động', '#A0CED9', 2),
(19, N'Quan tâm sản phẩm công nghệ mới', '#FBE7C6', 2),
(20, N'Quan tâm game & giải trí số', '#FFB7B2', 2),
(21, N'Quan tâm về an ninh mạng', '#D0F4DE', 2),
(22, N'Quan tâm tiết kiệm & ngân hàng', '#CBAACB', 2),
(23, N'Quan tâm đầu tư (chứng khoán / crypto)', '#FFABAB', 2),
(24, N'Quan tâm tín dụng & vay tiêu dùng', '#FFC3A0', 2),
(25, N'Quan tâm thể dục thể thao', '#F3FFE2', 2),
(26, N'Quan tâm chế độ ăn uống lành mạnh', '#D9F8C4', 2),
(27, N'Quan tâm sức khỏe tinh thần', '#A1E3D8', 2),
(28, N'Quan tâm chăm sóc sức khỏe chủ động', '#FFB6B9', 2),
(29, N'Quan tâm học online', '#FFDEB4', 2),
(30, N'Quan tâm học kỹ năng mềm', '#B1E5F2', 2),
(31, N'Quan tâm ngoại ngữ', '#FFD6E0', 2),
(32, N'Quan tâm định hướng nghề nghiệp', '#C4FAF8', 2),
(33, N'Quan tâm chăm sóc con cái', '#D3F8E2', 2),
(34, N'Quan tâm nuôi thú cưng', '#DCD6F7', 2),
(35, N'Quan tâm quan hệ xã hội', '#F6DFEB', 2),
(36, N'Quan tâm vấn đề xã hội', '#E4BAD4', 2),
(37, N'Quan tâm phim ảnh', '#FFF5BA', 2),
(38, N'Quan tâm âm nhạc', '#D5AAFF', 2),
(39, N'Quan tâm văn hóa đọc', '#ACE7EF', 2),
(40, N'Quan tâm du lịch & khám phá', '#F7DAD9', 2);

INSERT INTO SurveyDefaultBackgroundTheme (id, configJsonString) VALUES
(1, N'{"TitleColor":"#2f2f2f","ContentColor":"#444444","ButtonBackgroundColor":"#f75c83","ButtonContentColor":"#fec347"}'),
(2, N'{"TitleColor":"#2f2f2f","ContentColor":"#444444","ButtonBackgroundColor":"#fcbc72","ButtonContentColor":"#7a4b09"}'),
(3, N'{"TitleColor":"#2f2f2f","ContentColor":"#444444","ButtonBackgroundColor":"#bc73bc","ButtonContentColor":"#fec347"}'),
(4, N'{"TitleColor":"#2f2f2f","ContentColor":"#444444","ButtonBackgroundColor":"#4ea295","ButtonContentColor":"#fec347"}'),
(5, N'{"TitleColor":"#2f2f2f","ContentColor":"#444444","ButtonBackgroundColor":"#bc6235","ButtonContentColor":"#fec347"}'),
(6, N'{"TitleColor":"#2f2f2f","ContentColor":"#444444","ButtonBackgroundColor":"#f6567a","ButtonContentColor":"#fec347"}'),
(7, N'{"TitleColor":"#2f2f2f","ContentColor":"#444444","ButtonBackgroundColor":"#1ba9c5","ButtonContentColor":"#fec347"}'),
(8, N'{"TitleColor":"#2f2f2f","ContentColor":"#444444","ButtonBackgroundColor":"#027186","ButtonContentColor":"#ffffff"}'),
(9, N'{"TitleColor":"#2f2f2f","ContentColor":"#444444","ButtonBackgroundColor":"#6eaf99","ButtonContentColor":"#ffffff"}'),
(10, N'{"TitleColor":"#2f2f2f","ContentColor":"#444444","ButtonBackgroundColor":"#f52828","ButtonContentColor":"#fec347"}'),
(11, N'{"TitleColor":"#ffffff","ContentColor":"#dddddd","ButtonBackgroundColor":"#fec347","ButtonContentColor":"#040040"}');

