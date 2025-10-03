import type { SurveyType } from "@/core/types/tools";
import "./styles.scss";
import Box from "@mui/material/Box";
import Typography from "@mui/material/Typography";
import WarningIcon from "@mui/icons-material/Warning";
import { useEffect, useState } from "react";

type Props = {
  formData: SurveyType;
  defaultBackgroundThemes: any; // Thịnh thêm dòng này
};

const getErrors = (formData: SurveyType): string[] => {
  const errors: string[] = [];
  if (formData?.Questions) {
    formData.Questions.forEach((question: any) => {
      if (!question.Content) {
        errors.push(`Câu hỏi ${question.Order} chưa điền tiêu đề`);
      }

      if (!question.QuestionTypeId) {
        errors.push(`Câu hỏi ${question.Order} chưa chọn loại câu hỏi`);
      }

      if (
        question.QuestionTypeId === 1 ||
        question.QuestionTypeId === 2 ||
        question.QuestionTypeId === 7
      ) {
        if (question.Options && question.Options.length > 0) {
          question.Options.forEach((option: any, optionIndex: any) => {
            if (!option.Content) {
              errors.push(
                `Câu hỏi ${question.Order}, Tùy chọn ${
                  optionIndex + 1
                } chưa điền nội dung`
              );
            }
          });
        } else {
          errors.push(
            `Câu hỏi ${question.Order} (${
              question.QuestionTypeId === 1
                ? "Trắc nghiệm 1 lựa chọn"
                : question.QuestionTypeId === 2
                ? "Trắc nghiệm nhiều lựa chọn"
                : "Xếp hạng"
            }) cần có ít nhất một tùy chọn`
          );
        }
      }
    });
  }
  return errors;
};

const CompletePage = ({ formData, defaultBackgroundThemes }: Props) => {
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  const [listBackground, setListBackground] = useState<any[]>([]);

  const errors = getErrors(formData);

  useEffect(() => {
    setListBackground(
      //JSON.parse(localStorage.getItem("listBackground") || "[]")
      defaultBackgroundThemes || [] // Thịnh thêm dòng này
    );
  }, []);

  return (
    <div
      className="min-h-[100%] question-main flex-1 flex flex-col overflow-y-auto relative items-center justify-center"
      style={{
        ...(formData?.ConfigJson?.Background === "image" && {
          backgroundImage: `url(${
            formData?.ConfigJson?.IsUseBackgroundImageBase64 &&
            formData.BackgroundImageBase64
              ? formData.BackgroundImageBase64
              : formData?.ConfigJson?.DefaultBackgroundImageId
              ? listBackground.find(
                  (item) =>
                    item.Id === // Thịnh
                    formData?.ConfigJson?.DefaultBackgroundImageId
                )?.MainImageUrl
              : ""
          })`,
          backgroundSize: "cover",
          backgroundPosition: "center",
          backgroundRepeat: "no-repeat",
          filter: `Brightness(${formData.ConfigJson.Brightness / 100})`,
          backgroundColor: "transparent",
        }),
        ...(formData.ConfigJson?.Background === "color_gradient" && {
          // Thịnh, thiếu config
          background: `linear-gradient(to right, ${formData.ConfigJson.BackgroundGradient1Color}, ${formData.ConfigJson.BackgroundGradient2Color})`,
          filter: `Brightness(${formData.ConfigJson.Brightness / 100})`,
        }),
        ...(formData.Background?.startsWith("#") && {
          backgroundColor: formData.Background,
          filter: `Brightness(${formData.ConfigJson.Brightness / 100})`,
        }),
      }}
    >
      <Box className="flex flex-col items-center space-y-4 w-[100%]">
        {errors.length > 0 ? (
          <Box className="flex flex-col space-y-2 p-4">
            {errors.map((error, index) => (
              <ErrorItem key={index} title={error} />
            ))}
          </Box>
        ) : (
          <Box className="p-4">
            <Typography variant="h4">Survey Completed!</Typography>
            {formData.Title && (
              <Typography variant="body1">
                Survey Title: {formData.Title}
              </Typography>
            )}
          </Box>
        )}
      </Box>
    </div>
  );
};

export default CompletePage;

const ErrorItem = ({ title }: { title: string }) => {
  return (
    <Box className="bg-red-500 text-white p-3 flex items-center space-x-2 rounded w-[100%]">
      <WarningIcon fontSize="small" />
      <Typography variant="body2">{title}</Typography>
    </Box>
  );
};
