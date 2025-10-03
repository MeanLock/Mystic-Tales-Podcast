import { useCallback, useMemo } from "react";
import "./styles.scss";
import {
  Typography,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Box,
} from "@mui/material";
import type { OptionType, QuestionType } from "@/core/types/tools";
import { useGetSurveyQuestionTypes } from "@/core/services/CreateSurveyTool/InputDatas/get-survey-question-types";
import Email from "../../molecules/email/Email";
import TextArea from "../../molecules/text-area/TextArea";
import ShortSentences from "../../molecules/short-sentences/ShortSentences";
import Date from "../../molecules/date/Date";
import DateMonth from "../../molecules/date-month/DateMonth";
import Year from "../../molecules/year/Year";
import Time from "../../molecules/time/Time";
import FullTime from "../../molecules/full-time/FullTime";
import NumberInput from "../../molecules/number/NumberInput";

type Props = {
  question: QuestionType;
  fieldInputTypes: any;
  handleUpdateQuestion: (
    key: keyof QuestionType,
    value:
      | string
      | number
      | boolean
      | OptionType[]
      | Record<string, string | number>
  ) => void;
};

const SingleInput = ({
  question,
  fieldInputTypes,
  handleUpdateQuestion,
}: Props) => {
  const { data } = useGetSurveyQuestionTypes({});
  const questionTypes = useMemo(() => data?.data || fieldInputTypes, [data]);

  const handleUpdateTypeQuestion = useCallback(
    (id: number) => {
      handleUpdateQuestion("ConfigJson", {
        ...question.ConfigJson,
        FieldInputTypeId: id,
      });
      if (question?.Options?.length) {
        handleUpdateQuestion("Options", []);
      }
    },
    [handleUpdateQuestion, question.ConfigJson, question?.Options?.length]
  );

  const viewRender = useMemo(() => {
    const fieldInputTypeIdValue = question?.ConfigJson?.FieldInputTypeId;
    const typeId = Number(fieldInputTypeIdValue) || 1;
    if (typeId === 1) {
      return (
        <ShortSentences
          isDisable={true}
          question={question}
          handleUpdateQuestion={handleUpdateQuestion}
        />
      );
    }
    switch (typeId) {
      case 1:
        return (
          <ShortSentences
            isDisable={true}
            question={question}
            handleUpdateQuestion={handleUpdateQuestion}
          />
        );
      case 2:
        return <TextArea />;
      case 3:
        return <Email />;
      case 4:
        return <NumberInput />;
      case 5:
        return <Date />;
      case 6:
        return <DateMonth />;
      case 7:
        return <Year />;
      case 8:
        return <Time />;
      case 9:
        return <FullTime />;
      default:
        return (
          <ShortSentences
            isDisable={true}
            question={question}
            handleUpdateQuestion={handleUpdateQuestion}
          />
        );
    }
  }, [handleUpdateQuestion, question]);

  return (
    <Box className="single-input flex flex-col gap-4 p-4 bg-gray-100 rounded-lg">
      <Typography variant="body1" className="text-gray-700">
        Chọn kiểu câu trả lời
      </Typography>

      <FormControl fullWidth size="small">
        <InputLabel>Kiểu câu trả lời</InputLabel>
        <Select
          value={
            question?.ConfigJson?.FieldInputTypeId || questionTypes[0].id || 0
          }
          label="Kiểu câu trả lời"
          onChange={(e) => handleUpdateTypeQuestion(e.target.value)}
        >
          {questionTypes.map((item: { Id: number; Name: string }) => {
            return (
              <MenuItem key={item.Id} value={item.Id}>
                {item.Name}
              </MenuItem>
            );
          })}
        </Select>
      </FormControl>
      {viewRender}
    </Box>
  );
};

export default SingleInput;
