import { Box, Slider, Typography } from "@mui/material";
import { useMemo } from "react";
import { useDispatch } from "react-redux";
import { handleChangeRangeSlide } from "@/app/appSlice";
import { useAppSelector } from "@/app/hooks";
import { HiddenCheck } from "../../molecules/hiddenCheck/HiddenCheck";

interface Props {
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    data: any;
}

const RangeSlideSlide = ({ data }: Props) => {
    const isValid = useAppSelector((state) => state.appSlice.isValid);
    const config = useAppSelector((state) => state.appSlice.infoSurvey);

    const value = useMemo(
        () => [
            data?.ValueJson?.QuestionResponse?.Range?.Min ||
                data?.ValueJson?.QuestionContent?.ConfigJson?.Min ||
                0,
            data?.ValueJson?.QuestionResponse?.Range?.Max ||
                data?.ValueJson?.QuestionContent?.ConfigJson?.Max ||
                10,
        ],
        [data]
    );
    const min = useMemo(
        () => data?.ValueJson?.QuestionContent?.ConfigJson?.Min,
        [data]
    );
    const max = useMemo(
        () => data?.ValueJson?.QuestionContent?.ConfigJson?.Max,
        [data]
    );
    const unit = useMemo(
        () => data?.ValueJson?.QuestionContent?.ConfigJson?.Unit || "",
        [data?.ValueJson?.QuestionContent?.ConfigJson?.Unit]
    );
    const step = useMemo(
        () => data?.ValueJson?.QuestionContent?.ConfigJson?.Step,
        [data]
    );
    const dispatch = useDispatch();
    const handleSliderChange = (_event: Event, newValue: number | number[]) => {
        if (Array.isArray(newValue)) {
            // if (isValid) return;
            dispatch(
                handleChangeRangeSlide({
                    idChoose: data?.ValueJson?.QuestionContent?.Id,
                    min: newValue[0],
                    max: newValue[1],
                })
            );
        }
    };
   return (
    <Box className="single-slider p-6 rounded-lg flex flex-col gap-6 relative">
        {isValid && (
            <div 
                className="absolute inset-0 bg-gray-200/50 z-10 rounded-lg"
                style={{ cursor: 'not-allowed' }}
            />
        )}
        <Box className="flex flex-col items-center w-full">
            <Box className="flex justify-between w-full px-1 mb-2">
                <Typography
                    variant="body2"
                    style={{
                        color: config?.ConfigJson?.ContentColor || "#000",
                    }}
                >
                    {min} {unit}
                </Typography>
                <Typography
                    variant="body2"
                    style={{
                        color: config?.ConfigJson?.ContentColor || "#000",
                    }}
                >
                    {max} {unit}
                </Typography>
            </Box>
            <Slider
                value={value}
                onChange={handleSliderChange}
                aria-labelledby="range-slider"
                valueLabelDisplay="auto"
                min={min}
                max={max}
                step={step}
                sx={{
                    // ...existing Slider sx props...
                }}
            />
        </Box>

        <HiddenCheck id={data?.ValueJson.QuestionContent.QuestionTypeId} />
    </Box>
);
};

export default RangeSlideSlide;
