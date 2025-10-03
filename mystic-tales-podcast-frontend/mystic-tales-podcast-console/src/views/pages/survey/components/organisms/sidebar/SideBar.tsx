/* eslint-disable @typescript-eslint/no-explicit-any */
import { Box, IconButton, MenuItem, Select, Typography } from "@mui/material";
import { useMemo, useState } from "react";
import { SurveyQuestionType } from "../../../constants/questions";
import type { OptionType, QuestionType, SurveyType } from "@/core/types/tools";
import TimeLimit from "../../molecules/time-limit/TimeLimit";
import Voice from "../../molecules/voice/Voice";
import LogicComponent from "../QuestionPage/components/ModalLogic";
import LogicComponentDisplay from "../QuestionPage/components/ModalLogicDisplay";
import SwitchCustomize from "../QuestionPage/components/SwitchCustomize";
import type { RangeSliderConfigJsonType } from "../RangeSlider/RangeSlider";
import VisibilityIcon from "@mui/icons-material/Visibility";
import VisibilityOffIcon from "@mui/icons-material/VisibilityOff";
const Sidebar = ({
  question,
  securityModes, // Thịnh, thêm dòng này
  formData,
  setFormData,
  handleUpdateQuestion,
  listComponent,
}: {
  question: QuestionType;
  securityModes: any; // Thịnh, thêm dòng này
  formData: SurveyType;
  setFormData: React.Dispatch<React.SetStateAction<SurveyType>>;
  handleUpdateQuestion: (
    key: keyof QuestionType,
    value:
      | string
      | number
      | boolean
      | OptionType[]
      | Record<string, string | number>
      | RangeSliderConfigJsonType
      | Record<string, unknown>
  ) => void;
  listComponent: any;
}) => {
  const handleChangeType = (type: number) => {
    handleUpdateQuestion("QuestionTypeId", type);
    handleUpdateQuestion("ConfigJson", {});
    if (question?.Options?.length) {
      handleUpdateQuestion("Options", []);
    }
  };
  const [showImage, setShowImage] = useState(false);

  const isBasic = useMemo(() => formData?.SecurityModeId === 1, [formData]);
  const isAdvance = useMemo(() => formData?.SecurityModeId === 2, [formData]);
  const isPro = useMemo(() => formData?.SecurityModeId === 3, [formData]);

  return (
    <>
      <p>Chọn loại câu hỏi</p>
      <Select
        value={question?.QuestionTypeId || 0}
        onChange={(e) => handleChangeType(e.target.value)}
        label="Chọn loại câu hỏi"
        className="mb-4"
      >
        {SurveyQuestionType?.map((item: any) => {
          return (
            <MenuItem key={item.id} value={item.id}>
              {item.name}
            </MenuItem>
          );
        })}
      </Select>

      <Box // Thịnh, sửa switch thành mắt, copy lại nguyên file cho nhanh
        sx={{
          display: "flex",
          alignItems: "center",
          justifyContent: "space-between",
          padding: "4px 10px",
          minHeight: "20px",
          mb: 2,
        }}
      >
        <Typography
          variant="body1"
          sx={{
            fontWeight: 600,
            color: "#000",
            fontSize: "14px",
          }}
        >
          Hình ảnh/Video ở đầu câu hỏi
        </Typography>
        <IconButton
          size="small"
          onClick={() => {
            const newValue = !question?.ConfigJson?.ImageEndQuestion;
            setShowImage(newValue);
            const updatedConfigJson = {
              ...(question?.ConfigJson || {}),
              ImageEndQuestion: newValue,
            };
            handleUpdateQuestion("ConfigJson", updatedConfigJson);
          }}
          style={{ marginRight: "0.8rem" }}
        >
          {question?.ConfigJson?.ImageEndQuestion ? (
            <VisibilityOffIcon fontSize="medium" />
          ) : (
            <VisibilityIcon fontSize="medium" />
          )}
        </IconButton>
      </Box>

      <SwitchCustomize
        type="RequiredAnswer"
        question={question}
        handleUpdateQuestion={handleUpdateQuestion}
        label="Bắt buộc câu trả lời"
      />
      <SwitchCustomize
        type="IsUseLabel"
        question={question}
        handleUpdateQuestion={handleUpdateQuestion}
        label="Gắn nhãn ở đầu câu hỏi"
      />
      {/* <SwitchCustomize
                type="ImageEndQuestion"
                question={question}
                handleUpdateQuestion={handleUpdateQuestion}
                label="Hình ảnh/Video ở đầu câu hỏi"
            /> */}

      <Voice
        label="Sử dụng Voice"
        isPro={isPro}
        securityModes={securityModes} // Thịnh, thêm dòng này
        setFormData={setFormData}
        question={question}
        handleUpdateQuestion={handleUpdateQuestion}
      />
      {listComponent &&
        listComponent.map((Item: any) => {
          return Item.children;
        })}

      {formData?.SurveyTypeId === 3 || formData?.SecurityModeId === 1 ? null : (
        <>
          <LogicComponent
            questions={formData?.Questions || []}
            question={question}
            handleUpdateQuestion={handleUpdateQuestion}
          />
          <LogicComponentDisplay
            questions={formData?.Questions || []}
            question={question}
            handleUpdateQuestion={handleUpdateQuestion}
          />
        </>
      )}
      <SwitchCustomize
        type="NotBack"
        question={question}
        handleUpdateQuestion={handleUpdateQuestion}
        label="Không cho quay lại câu trước"
      />
      <SwitchCustomize
        type="ViewNumberQuestion"
        question={question}
        handleUpdateQuestion={handleUpdateQuestion}
        label="Hiện số thứ tự gốc của câu trả lời"
      />
      <TimeLimit
        isAdvance={isAdvance}
        isPro={isPro} // Thịnh dòng này và dòng isbasic
        isBasic={isBasic}
        securityModes={securityModes} // Thịnh, thêm dòng này
        setFormData={setFormData}
        question={question}
        handleUpdateQuestion={handleUpdateQuestion}
      />
    </>
  );
};

export default Sidebar;
