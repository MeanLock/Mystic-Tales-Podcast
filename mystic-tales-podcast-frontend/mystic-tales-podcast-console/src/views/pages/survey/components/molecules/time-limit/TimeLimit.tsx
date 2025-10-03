import { Box, Input, Switch, Typography } from "@mui/material";
import type { OptionType, QuestionType, SurveyType } from "@/core/types/tools";
import type { RangeSliderConfigJsonType } from "../../organisms/RangeSlider/RangeSlider";
import { useCallback, useEffect, useMemo, useState } from "react";
import { toast } from "react-toastify";
import Swal from "sweetalert2";

type Props = {
  question: QuestionType;
  isAdvance: boolean;
  isPro: boolean;
  securityModes: any;
  isBasic: boolean;
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
};
//Thịnh, trang này copy lại đi, sửa logic cho hợp lí r
const TimeLimit = ({
  securityModes, // Thịnh, thêm dòng này
  question,
  handleUpdateQuestion,
  isAdvance,
  isPro,
  isBasic,
  setFormData,
}: Props) => {
  const [isOpen, setIsOpne] = useState(false);
  const value = useMemo(() => Number(question?.TimeLimit) || 0, [question]);

  const action = useCallback(() => {
    if (isOpen) {
      handleUpdateQuestion("TimeLimit", 0);
      // setFormData((prev) => ({ ...prev, SecurityModeId: 1 }));
    }
    if (!isOpen && !isAdvance && !isPro) {
      setFormData((prev: any) => ({ ...prev, SecurityModeId: 2 }));
      toast("Đã cập nhật Chế độ bảo mật thành Advance");
    }
    setIsOpne(!isOpen);
  }, [handleUpdateQuestion, isAdvance, isOpen, setFormData]);

  const handleChangeSwitch = () => {
    if (isAdvance && !isOpen) {
      action();
      return;
    }
    // Thịnh, tạo hàm để lấy securityModes description và name
    const generateSecurityModesHTML = () => {
      if (!securityModes || !Array.isArray(securityModes)) {
        return '<p style="font-size: 14px; color: #999;">Không có dữ liệu chế độ bảo mật</p>';
      }

      return securityModes
        .map((mode, index) => {
          const isAdvanceMode =
            mode.Id === 2 || mode.Name?.toLowerCase().includes("Advance");
          const backgroundColor = isAdvanceMode ? "#4caf50" : "transparent";
          const textColor = isAdvanceMode ? "#ffffff" : "#333";
          const marginBottom = index < securityModes.length - 1 ? "15px" : "0";
          const padding = "15px";
          const borderRadius = "8px";
          const boxShadow = isAdvanceMode
            ? "0 4px 6px rgba(0, 0, 0, 0.1)"
            : "none";

          const descriptionLines = (mode.Description || "Không có mô tả")
            .split(".")
            .filter((line: any) => line.trim() !== "")
            .map(
              (line: any) =>
                `<p style="font-size: 14px; margin: 5px 0;">${line.trim()}.</p>`
            );

          return `
            <div style="
                margin-bottom: ${marginBottom}; 
                background-color: ${backgroundColor}; 
                color: ${textColor}; 
                padding: ${padding}; 
                border-radius: ${borderRadius}; 
                box-shadow: ${boxShadow};
                font-family: Arial, sans-serif;
            ">
                <p style="font-size: 16px; font-weight: bold; margin: 0 0 10px 0;">${
                  mode.Name || "Không có tên"
                }</p>
                ${descriptionLines.join("")} 
            </div>
        `;
        })
        .join("");
    };
    Swal.fire({
      title: isOpen
        ? "Bạn muốn tắt Giới hạn thời gian câu hỏi?"
        : "Bạn muốn bật Giới hạn thời gian câu hỏi ?",
      showCancelButton: true,
      confirmButtonText: "Yes",
      html: `
            <div style="text-align: left; padding: 15px; font-family: Arial, sans-serif;">
                ${generateSecurityModesHTML()}
            </div>
        `,
    }).then((result) => {
      if (result.isConfirmed) {
        action();
      }
    });
  };

  const handleChangeValue = (value: number) => {
    if(value < 0){
      
      handleUpdateQuestion("TimeLimit", 0);
      toast.error("Thời gian không thể nhỏ hơn 0 giây");
    }
    handleUpdateQuestion("TimeLimit", value);
  };

  useEffect(() => {
    if (Boolean(question?.TimeLimit) && !isOpen) {
      setIsOpne(true);
    }
    if (!question?.TimeLimit && isOpen) {
      setIsOpne(false);
    }
    //console.log("question?.Order: ",isOpen, isBasic);
    if (isBasic) {
      setIsOpne(false);
      handleUpdateQuestion("TimeLimit", 0);
    }
  }, [question?.Order]);
  return (
    <div className="w-full mb-2">
      <div>
        <Box
          sx={{
            display: "flex",
            alignItems: "center",
            justifyContent: "space-between",
            padding: "4px 10px",
            borderRadius: "8px",
            minHeight: "20px",
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
            Giới hạn thời gian câu hỏi này
          </Typography>

          <Switch checked={isOpen} onChange={handleChangeSwitch} />
        </Box>
        {isOpen ? (
          <>
            <Input
              className="w-full px-2 border-[1px] border-[#ccc] border-solid"
              placeholder="Thời gian (giây)"
              value={value}
              type="number"
              inputProps={{ min: 0 }}
              onChange={(e) => handleChangeValue(Number(e.target.value))}
            />
            <p>{value} giây</p>
          </>
        ) : null}
      </div>
    </div>
  );
};

export default TimeLimit;
