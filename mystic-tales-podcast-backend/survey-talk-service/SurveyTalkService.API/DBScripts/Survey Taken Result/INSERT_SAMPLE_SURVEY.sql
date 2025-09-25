-- Insert 7 questions (1 per type) for surveyId = 6

INSERT INTO SurveyQuestion (surveyId, [order], questionTypeId, content, configJsonString)
VALUES
-- 1. Single Choice
( 6, 1, 1, N'Câu hỏi chọn 1 đáp án', N'{
  "jumpLogics": [
    {
      "conditions": [
        {
          "questionOrder": 1,
          "conjunction": null,
          "operator": "Chọn",
          "optionOrder": 1
        }
      ],
      "targetQuestionOrder": 2
    }
  ]
}'),
-- 2. Multiple Choice
( 6, 2, 2, N'Câu hỏi chọn nhiều đáp án', N'{
  "minChoiceCount": 1,
  "maxChoiceCount": 3,
  "jumpLogics": [
    {
      "conditions": [
        {
          "questionOrder": 2,
          "conjunction": "Và",
          "operator": "Chọn",
          "optionOrder": 2
        }
      ],
      "targetQuestionOrder": 3
    }
  ]
}'),
-- 3. Single Slider
( 6, 3, 3, N'Câu hỏi trượt 1 giá trị', N'{
  "min": 0,
  "max": 10,
  "step": 1,
  "unit": "điểm"
}'),
-- 4. Range Slider
(6, 4, 4, N'Câu hỏi trượt chọn khoảng', N'{
  "min": 1,
  "max": 100,
  "step": 5,
  "unit": "kg"
}'),
-- 5. Single input by types (dùng SurveyFieldInputType)
( 6, 5, 5, N'Câu hỏi nhập text', N'{
  "fieldInputTypeId": 1
}'),
-- 6. Rating
( 6, 6, 6, N'Câu hỏi đánh giá', N'{
  "ratingLength": 5,
  "ratingIcon": "star",
  "jumpLogics": [
    {
      "conditions": [
        {
          "questionOrder": 6,
          "conjunction": null,
          "operator": "≥",
          "compareValue": 4
        }
      ],
      "targetQuestionOrder": 7
    }
  ]
}'),
-- 7. Ranking
( 6, 7, 7, N'Câu hỏi xếp hạng', N'{}');

-- Insert options for question type 1 (id: 601) - Single Choice
INSERT INTO SurveyOption (surveyQuestionId, [order], content)
VALUES
(15, 1, N'Đáp án 1'),
(15, 2, N'Đáp án 2'),
(15, 3, N'Đáp án 3');

-- Insert options for question type 2 (id: 602) - Multiple Choice
INSERT INTO SurveyOption ( surveyQuestionId, [order], content)
VALUES
( 16, 1, N'Đáp án 1'),
(16, 2, N'Đáp án 2'),
(16, 3, N'Đáp án 3');

-- Insert options for question type 6 (id: 606) - Rating
INSERT INTO SurveyOption ( surveyQuestionId, [order], content)
VALUES
(20, 1, N'Rất không hài lòng'),
( 20, 2, N'Bình thường'),
( 20, 3, N'Rất hài lòng');

Select * from SurveyQuestion

-------------------------------------------------------------------------------------------------------------------
-- Insert sample survey taken result for surveyId = 9
INSERT INTO SurveyQuestion (surveyId, [order], questionTypeId, content, configJsonString)
VALUES
-- 1. Single Choice
(9, 1, 1, N'Câu hỏi chọn 1 đáp án', N'{
  "DisplayLogics": [],
  "JumpLogics": [
    {
      "Conditions": [
        {
          "QuestionOrder": 1,
          "Conjunction": null,
          "Operator": "Chọn",
          "OptionOrder": 1
        }
      ],
      "TargetQuestionOrder": 2
    }
  ]
}'),
-- 2. Multiple Choice
(9, 2, 2, N'Câu hỏi chọn nhiều đáp án', N'{
  "MinChoiceCount": 1,
  "MaxChoiceCount": 3,
  "DisplayLogics": [],
  "JumpLogics": [
    {
      "Conditions": [
        {
          "QuestionOrder": 2,
          "Conjunction": "Và",
          "Operator": "Chọn",
          "OptionOrder": 2
        }
      ],
      "TargetQuestionOrder": 3
    }
  ]
}'),
-- 3. Single Slider
(9, 3, 3, N'Câu hỏi trượt 1 giá trị', N'{
  "Min": 0,
  "Max": 10,
  "Step": 1,
  "Unit": "điểm",
  "DisplayLogics": [],
  "JumpLogics": []
}'),
-- 4. Range Slider
(9, 4, 4, N'Câu hỏi trượt chọn khoảng', N'{
  "Min": 1,
  "Max": 100,
  "Step": 5,
  "Unit": "kg",
  "DisplayLogics": [],
  "JumpLogics": []
}'),
-- 5. Single input by types (dùng SurveyFieldInputType)
(9, 5, 5, N'Câu hỏi nhập text', N'{
  "FieldInputTypeId": 1,
  "DisplayLogics": [],
  "JumpLogics": []
}'),
-- 6. Rating
(9, 6, 6, N'Câu hỏi đánh giá', N'{
  "RatingLength": 5,
  "RatingIcon": "star",
  "DisplayLogics": [],
  "JumpLogics": [
    {
      "Conditions": [
        {
          "QuestionOrder": 6,
          "Conjunction": null,
          "Operator": ">=",
          "CompareValue": 4
        }
      ],
      "TargetQuestionOrder": 7
    }
  ]
}'),
-- 7. Ranking
(9, 7, 7, N'Câu hỏi xếp hạng', N'{
  "DisplayLogics": [],
  "JumpLogics": []
}');

-- Insert options for question type 1 (order 1) - Single Choice
INSERT INTO SurveyOption (surveyQuestionId, [order], content)
VALUES
(21, 1, N'Đáp án 1'),
(21, 2, N'Đáp án 2'),
(21, 3, N'Đáp án 3');

-- Insert options for question type 2 (order 2) - Multiple Choice
INSERT INTO SurveyOption (surveyQuestionId, [order], content)
VALUES
(22, 1, N'Đáp án 1'),
(22, 2, N'Đáp án 2'),
(22, 3, N'Đáp án 3');

-- Insert options for question type 6 (order 6) - Rating
INSERT INTO SurveyOption (surveyQuestionId, [order], content)
VALUES
(26, 1, N'Rất không hài lòng'),
(26, 2, N'Bình thường'),
(26, 3, N'Rất hài lòng');

-- Insert sample reward tracking for surveyId = 9
INSERT INTO SurveyRewardTracking (surveyId, rewardPrice, rewardXp, createdAt)
VALUES
(9, 10000, 100, GETDATE()),
(9, 5000, 50, GETDATE()),
(9, 15000, 150, GETDATE()),
(9, 7000, 70, GETDATE());

-- Insert sample SurveyTakenResult cho surveyId = 9
INSERT INTO SurveyTakenResult (surveyId, takerId, isValid, completedAt)
VALUES
(9, 1001, 1, GETDATE());

-- Giả sử SurveyTakenResultId là 3
-- Insert SurveyResponse cho các câu hỏi id 21-27 (tương ứng order 1-7)
INSERT INTO SurveyResponse (surveyTakenResultId, isValid, surveyQuestionId, valueJsonString)
VALUES
(3, 1, 21, N'{
  "QuestionContent": {
    "Id": 21,
    "QuestionTypeId": 1,
    "Content": "Câu hỏi chọn 1 đáp án",
    "Description": null,
    "ConfigJson": {},
    "Options": [
      { "Id": 31, "Content": "Đáp án 1" },
      { "Id": 32, "Content": "Đáp án 2" },
      { "Id": 33, "Content": "Đáp án 3" }
    ]
  },
  "QuestionResponse": {
    "Input": null,
    "Range": null,
    "Ranking": null,
    "SingleChoice": 31,
    "MultipleChoice": null,
    "SpeechText": null
  }
}'),
(3, 1, 22, N'{
  "QuestionContent": {
    "Id": 22,
    "QuestionTypeId": 2,
    "Content": "Câu hỏi chọn nhiều đáp án",
    "Description": null,
    "ConfigJson": { "MinChoiceCount": 1, "MaxChoiceCount": 3 },
    "Options": [
      { "Id": 34, "Content": "Đáp án 1" },
      { "Id": 35, "Content": "Đáp án 2" },
      { "Id": 36, "Content": "Đáp án 3" }
    ]
  },
  "QuestionResponse": {
    "Input": null,
    "Range": null,
    "Ranking": null,
    "SingleChoice": null,
    "MultipleChoice": [34, 36],
    "SpeechText": null
  }
}'),
(3, 1, 23, N'{
  "QuestionContent": {
    "Id": 23,
    "QuestionTypeId": 3,
    "Content": "Câu hỏi trượt 1 giá trị",
    "Description": null,
    "ConfigJson": { "Min": 0, "Max": 10, "Step": 1, "Unit": "điểm" }
  },
  "QuestionResponse": {
    "Input": { "Value": 7, "ValueType": "number" },
    "Range": null,
    "Ranking": null,
    "SingleChoice": null,
    "MultipleChoice": null,
    "SpeechText": null
  }
}'),
(3, 1, 24, N'{
  "QuestionContent": {
    "Id": 24,
    "QuestionTypeId": 4,
    "Content": "Câu hỏi trượt chọn khoảng",
    "Description": null,
    "ConfigJson": { "Min": 1, "Max": 100, "Step": 5, "Unit": "kg" }
  },
  "QuestionResponse": {
    "Input": null,
    "Range": { "Min": 10, "Max": 30 },
    "Ranking": null,
    "SingleChoice": null,
    "MultipleChoice": null,
    "SpeechText": null
  }
}'),
(3, 1, 25, N'{
  "QuestionContent": {
    "Id": 25,
    "QuestionTypeId": 5,
    "Content": "Câu hỏi nhập text",
    "Description": null,
    "ConfigJson": { "FieldInputTypeId": 1 }
  },
  "QuestionResponse": {
    "Input": { "Value": "Nội dung trả lời", "ValueType": "string" },
    "Range": null,
    "Ranking": null,
    "SingleChoice": null,
    "MultipleChoice": null,
    "SpeechText": null
  }
}'),
(3, 1, 26, N'{
  "QuestionContent": {
    "Id": 26,
    "QuestionTypeId": 6,
    "Content": "Câu hỏi đánh giá",
    "Description": null,
    "ConfigJson": { "RatingLength": 5, "RatingIcon": "star" }
  },
  "QuestionResponse": {
    "Input": { "Value": 4, "ValueType": "number" },
    "Range": null,
    "Ranking": null,
    "SingleChoice": null,
    "MultipleChoice": null,
    "SpeechText": null
  }
}'),
(3, 1, 27, N'{
  "QuestionContent": {
    "Id": 27,
    "QuestionTypeId": 7,
    "Content": "Câu hỏi xếp hạng",
    "Description": null,
    "ConfigJson": {},
    "Options": null
  },
  "QuestionResponse": {
    "Input": null,
    "Range": null,
    "Ranking": [
      { "SurveyOptionId": 37, "RankIndex": 1 },
      { "SurveyOptionId": 38, "RankIndex": 2 }
    ],
    "SingleChoice": null,
    "MultipleChoice": null,
    "SpeechText": null
  }
}');

INSERT INTO SurveyStatusTracking(surveyId, surveyStatusId )
VALUES
(10, 2);