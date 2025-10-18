INSERT INTO Account (email, password, roleId, fullName, dob, gender, address, phone, isFilterSurveyRequired)
VALUES
('user1@example.com', '$2a$10$gXw69WT9ARJHK3UcZsTQueOM9vL8/fPGVFj1/j7T1GrhliMRAgH9a', 1, N'Nguyễn Văn A', '1990-01-01', 'male', N'Hà Nội', '0900000001', 0),
('user2@example.com', '$2a$10$gXw69WT9ARJHK3UcZsTQueOM9vL8/fPGVFj1/j7T1GrhliMRAgH9a', 2, N'Trần Thị B', '1992-02-02', 'female', N'Hồ Chí Minh', '0900000002', 0),
('user3@example.com', '$2a$10$gXw69WT9ARJHK3UcZsTQueOM9vL8/fPGVFj1/j7T1GrhliMRAgH9a', 4, N'Lê Văn C', '1993-03-03', 'male', N'Đà Nẵng', '0900000003', 1),
('user4@example.com', '$2a$10$gXw69WT9ARJHK3UcZsTQueOM9vL8/fPGVFj1/j7T1GrhliMRAgH9a', 1, N'Phạm Thị D', '1994-04-04', 'female', N'Hải Phòng', '0900000004', 0),
('user5@example.com', '$2a$10$gXw69WT9ARJHK3UcZsTQueOM9vL8/fPGVFj1/j7T1GrhliMRAgH9a', 2, N'Hoàng Văn E', '1995-05-05', 'male', N'Cần Thơ', '0900000005', 0),
('user6@example.com', '$2a$10$gXw69WT9ARJHK3UcZsTQueOM9vL8/fPGVFj1/j7T1GrhliMRAgH9a', 4, N'Vũ Thị F', '1996-06-06', 'female', N'Quảng Ninh', '0900000006', 1),
('user7@example.com', '$2a$10$gXw69WT9ARJHK3UcZsTQueOM9vL8/fPGVFj1/j7T1GrhliMRAgH9a', 1, N'Đặng Văn G', '1997-07-07', 'male', N'Bắc Ninh', '0900000007', 0),
('user8@example.com', '$2a$10$gXw69WT9ARJHK3UcZsTQueOM9vL8/fPGVFj1/j7T1GrhliMRAgH9a', 2, N'Bùi Thị H', '1998-08-08', 'female', N'Hưng Yên', '0900000008', 0),
('user9@example.com', '$2a$10$gXw69WT9ARJHK3UcZsTQueOM9vL8/fPGVFj1/j7T1GrhliMRAgH9a', 4, N'Ngô Văn I', '1999-09-09', 'male', N'Nam Định', '0900000009', 0),
('user10@example.com', '$2a$10$gXw69WT9ARJHK3UcZsTQueOM9vL8/fPGVFj1/j7T1GrhliMRAgH9a', 1, N'Tạ Thị K', '2000-10-10', 'female', N'Thái Bình', '0900000010', 0);

INSERT INTO AccountProfile (accountId, countryRegion, maritalStatus, averageIncome, educationLevel, jobField, provinceCode, districtCode, wardCode)
VALUES (3, N'Việt Nam', N'Single', N'10-20 triệu', N'Đại học', N'IT', 1, 1, 1);
