import { useCallback, useEffect, useState } from "react";
import type {
  OptionType,
  QuestionType,
  SurveyType,
} from "@/core/types/tools";
import "./styles.scss";
import { Box, Slider, TextField, Typography } from "@mui/material";

type SliderDataType = {
  Min: number;
  Max: number;
  Step: number;
  Unit: string;
};

export type RangeSliderConfigJsonType = SliderDataType;

type Props = {
  question: QuestionType;
  formData: SurveyType;
  handleUpdateQuestion: (
    key: keyof QuestionType,
    value:
      | string
      | number
      | boolean
      | OptionType[]
      | RangeSliderConfigJsonType
      | Record<string, unknown>
  ) => void;
};

const defaultData: SliderDataType = {
  Min: 1,
  Max: 10,
  Step: 1,
  Unit: "",
};

const RangeSlider = ({ question, handleUpdateQuestion, formData }: Props) => {
  const [value, setValue] = useState<number[]>([
    defaultData.Min,
    defaultData.Max,
  ]);

  const handleSliderChange = (_event: Event, newValue: number | number[]) => {
    setValue(newValue as number[]);
  };

  const handleRangeConfigChange = useCallback(
    (key: keyof SliderDataType, newValue: SliderDataType[typeof key]) => {
      if (typeof handleUpdateQuestion !== "function") return;

      const currentData = question?.ConfigJson || defaultData;
      handleUpdateQuestion("ConfigJson", {
        ...currentData,
        [key]: newValue,
      });
    },
    [handleUpdateQuestion, question?.ConfigJson]
  );

  useEffect(() => {
    const config = question?.ConfigJson;
    const isEmptyConfigObject =
      config && typeof config === "object" && Object.keys(config).length === 0;
    const isDataMissing = !config;

    if (isEmptyConfigObject || isDataMissing) {
      if (typeof handleUpdateQuestion === "function") {
        handleUpdateQuestion("ConfigJson", { ...defaultData });
        setValue([defaultData.Min, defaultData.Max]);
      }
    } else {
      setValue([Number(config.Min), Number(config.Max)]);
    }
  }, [question?.ConfigJson, handleUpdateQuestion]);

  const config = question?.ConfigJson as SliderDataType | undefined;
  const currentData =
    config &&
    typeof config === "object" &&
    "Min" in config &&
    "Max" in config &&
    "Step" in config &&
    "Unit" in config
      ? config
      : defaultData;

  return (
    <div className="range-slider">
      <Box className="range-slider-item flex flex-col gap-4">
        <Box className="flex flex-col items-center w-full">
          <Box className="flex justify-between w-full px-1 mb-2">
            <Typography variant="body2" color="white">
              {currentData.Min}
            </Typography>
            <Typography
              variant="body2"
              color="white"
              sx={{
                flexGrow: 1,
                textAlign: "center",
              }}
            >
              {currentData.Step}
            </Typography>
            <Typography variant="body2" color="white">
              {currentData.Max}
            </Typography>
          </Box>
          <Slider
            value={value}
            onChange={handleSliderChange}
            aria-labelledby="range-slider"
            valueLabelDisplay="auto"
            min={currentData.Min}
            max={currentData.Max}
            step={currentData.Step}
            sx={{
              "& .MuiSlider-thumb": {
                backgroundColor: formData?.ConfigJson?.ButtonBackgroundColor
                  ? formData?.ConfigJson?.ButtonBackgroundColor
                  : "#000",
                borderRadius: 0,
                width: 26,
                height: 26,
                "&:focus, &:hover, &.Mui-active": {
                  boxShadow: "inherit",
                },
              },
            }}
          />
        </Box>

        <Box className="flex justify-between gap-4">
          <Box>
            <Typography color="white" mb={2}>
              Min
            </Typography>
            <TextField
              type="number"
              value={currentData.Min}
              onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
                handleRangeConfigChange("Min", Number(e.target.value))
              }
              InputLabelProps={{ shrink: true }}
              sx={{
                flexGrow: 1,
                backgroundColor: "white",
                borderRadius: 1,
              }}
            />
          </Box>
          <Box>
            <Typography color="white" mb={2}>
              Step
            </Typography>
            <TextField
              type="number"
              value={currentData.Step}
              onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
                handleRangeConfigChange("Step", Number(e.target.value))
              }
              InputLabelProps={{ shrink: true }}
              sx={{
                flexGrow: 1,
                backgroundColor: "white",
                borderRadius: 1,
              }}
            />
          </Box>
          <Box>
            <Typography color="white" mb={2}>
              Max
            </Typography>
            <TextField
              type="number"
              value={currentData.Max}
              onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
                handleRangeConfigChange("Max", Number(e.target.value))
              }
              InputLabelProps={{ shrink: true }}
              sx={{
                flexGrow: 1,
                backgroundColor: "white",
                borderRadius: 1,
              }}
            />
          </Box>
          <Box>
            <Typography color="white" mb={2}>
              Unit
            </Typography>
            <TextField
              value={currentData.Unit}
              onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
                handleRangeConfigChange("Unit", e.target.value)
              }
              InputLabelProps={{ shrink: true }}
              placeholder="Đơn vị"
              sx={{
                flexGrow: 1,
                backgroundColor: "white",
                borderRadius: 1,
              }}
            />
          </Box>
        </Box>
      </Box>
    </div>
  );
};

export default RangeSlider;
