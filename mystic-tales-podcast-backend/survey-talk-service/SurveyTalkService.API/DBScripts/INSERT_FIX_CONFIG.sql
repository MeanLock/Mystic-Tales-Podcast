INSERT INTO SystemConfigProfile (name, description, isActive)
VALUES (N'System Default', N'Cấu hình hệ thống mặc định', 1);

-- SYSTEMS
INSERT INTO AccountGeneralConfig (configProfileId, xpLevelThreshold, filterSurveyCycle, dailyActiveCountPeriod, safetyFilterRate)
VALUES (1, 100, 90, 30, 0.1);

-- LEVEL
INSERT INTO AccountLevelSettingConfig (configProfileId, level, dailyReductionXp, progressionSurveyCount, bonusRate) VALUES 
(1, 1, 1, 0, 0.1),
(1, 2, 2, 1, 0.2),
(1, 3, 3, 2, 0.3),
(1, 4, 4, 3, 0.4),
(1, 5, 5, 4, 0.5),
(1, 6, 6, 5, 0.6),
(1, 7, 7, 6, 0.7),
(1, 8, 8, 7, 0.8),
(1, 9, 9, 8, 0.9),
(1, 10, 10, 9, 1.0);

-- SURVEYS
INSERT INTO SurveyGeneralConfig (configProfileId, pricePerQuestion, xpPerQuestion, publishProfitRate, basePriceAllocationRate, timePriceAllocationRate, levelPriceAllocationRate)
VALUES (1, 500, 1, 0.2, 0.7, 0.2, 0.1);

INSERT INTO SurveyTimeRateConfig (configProfileId, minDurationRate, maxDurationRate, rate) VALUES 
(1, 1, 1, 1.5),
(1, 1.1, 1.5, 1.2),
(1, 1.6, 2, 1.0),
(1, 2.1, 99999999, 0.9);

INSERT INTO SurveySecurityModeConfig (configProfileId, surveySecurityModeId, rate) VALUES 
(1, 1, 1.0),
(1, 2, 1.5),
(1, 3, 2.2);

-- SURVEYS MARKET
INSERT INTO SurveyMarketConfig (configProfileId, dataTransactionProfitRate) VALUES (1, 0.2);