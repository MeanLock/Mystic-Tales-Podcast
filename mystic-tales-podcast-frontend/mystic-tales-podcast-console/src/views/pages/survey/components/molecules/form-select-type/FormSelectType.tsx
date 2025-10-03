import "./styles.scss";
import { Typography } from "@mui/material";
import CheckCircleOutlineIcon from "@mui/icons-material/CheckCircleOutline";
import TextFieldsIcon from "@mui/icons-material/TextFields";
import StarIcon from "@mui/icons-material/Star";
import TuneIcon from "@mui/icons-material/Tune";
import BarChartIcon from "@mui/icons-material/BarChart";
import { useMemo, type ReactNode } from "react";
import { SurveyQuestionType } from "../../../constants/questions";
import type { QuestionType } from "@/core/types/tools";

type Props = {
  handleUpdateQuestion: (
    key: keyof QuestionType,
    value: string | number | boolean | Record<string, string | number>
  ) => void;
  SurveyQuestionTypes: any;
};
const FormSelectType = ({
  handleUpdateQuestion,
  SurveyQuestionTypes,
}: Props) => {
  const handleInsertIcon = (type: string) => {
    switch (type) {
      case "Single Choice":
        return <CheckCircleOutlineIcon />;
      case "Multiple Choice":
        return <CheckCircleOutlineIcon />;
      case "Single Slider":
        return <TuneIcon />;
      case "Range Slider":
        return <TuneIcon />;
      case "Single input by types":
        return <TextFieldsIcon />;
      case "Rating":
        return <StarIcon />;
      case "Ranking":
        return <BarChartIcon />;
      default:
        return undefined;
    }
  };
  //   const { data } = useGetSurveyQuestionType({});
  const questionTypes = useMemo(
    () =>
      (SurveyQuestionTypes?.data || SurveyQuestionType)?.map(
        (item: SurveyQuestionType) => ({
          id: item?.id,
          label: item?.name || "",
          icon: handleInsertIcon(item?.name || ""),
        })
      ),
    [SurveyQuestionTypes?.data]
  );

  const handleSelectType = (id: number) => {
    handleUpdateQuestion("QuestionTypeId", id);
    if (id === 5) {
      handleUpdateQuestion("ConfigJson", {
        FieldInputTypeId: 1,
      });
    }
  };

  return (
    <div className="w-full flex justify-center">
      <div className="form-select-type-grid grid grid-cols-2 gap-6">
        {questionTypes.map(
          (type: { id: number; label: string; icon: ReactNode }) => (
            <div
              key={type.id}
              className="form-select-type-card flex flex-col items-center justify-center p-4 cursor-pointer"
              onClick={() => handleSelectType(type.id)}
            >
              <div className="form-select-type-icon mb-2">{type.icon}</div>
              <Typography
                variant="body1"
                className="form-select-type-label text-center"
              >
                {type.label}
              </Typography>
            </div>
          )
        )}
      </div>
    </div>
  );
};

export default FormSelectType;
