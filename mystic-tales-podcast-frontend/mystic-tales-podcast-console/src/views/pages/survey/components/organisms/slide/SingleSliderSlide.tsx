import { Box, Slider, Typography } from "@mui/material";
import { useMemo } from "react";
import { useDispatch } from "react-redux";
import { handleChangeSlider } from "@/app/appSlice";
import { useAppSelector } from "@/app/hooks";
import { HiddenCheck } from "../../molecules/hiddenCheck/HiddenCheck";

interface Props {
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    data: any;
}

const SingleSliderSlide = ({ data }: Props) => {
    const config = useAppSelector((state) => state.appSlice.infoSurvey);
    const isValid = useAppSelector((state) => state.appSlice.isValid);

    const value = useMemo(
        () =>
            data?.ValueJson?.QuestionResponse?.Input?.Value ??
            data?.ValueJson?.QuestionContent?.ConfigJson?.Min ??
            0,
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
        //if (isValid) return;
        if (typeof newValue === "number") {
            dispatch(
                handleChangeSlider({
                    idChoose: data?.ValueJson?.QuestionContent?.Id,
                    value: newValue,
                })
            );
        }
    };
    return (
         <Box className="single-slider p-6 rounded-lg flex flex-col gap-6 relative"> {/* Added relative */}
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
                        color="white"
                        style={{
                            color: config?.ConfigJson?.ContentColor || "#000",
                        }}
                    >
                        {min} {unit}
                    </Typography>
                    <Typography
                        variant="body2"
                        color="white"
                        sx={{ flexGrow: 1, textAlign: "center" }}
                    >
                        {/* {1} */}
                    </Typography>
                    <Typography
                        variant="body2"
                        color="white"
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
                    aria-labelledby="single-slider"
                    valueLabelDisplay="auto"
                    min={min}
                    max={max}
                    step={step}
                    sx={{
                        "& .MuiSlider-thumb": {
                            background:
                                config?.ConfigJson?.ButtonBackgroundColor?.startsWith(
                                    "linear-gradient"
                                ) ||
                                config?.ConfigJson?.ButtonBackgroundColor?.startsWith(
                                    "radial-gradient"
                                )
                                    ? config?.ConfigJson?.ButtonBackgroundColor
                                    : "",
                            backgroundColor: !(
                                config?.ConfigJson?.ButtonBackgroundColor?.startsWith(
                                    "linear-gradient"
                                ) ||
                                config?.ConfigJson?.ButtonBackgroundColor?.startsWith(
                                    "radial-gradient"
                                )
                            )
                                ? config?.ConfigJson?.ButtonBackgroundColor ||
                                  "#000"
                                : "",
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

            <HiddenCheck id={data?.ValueJson.QuestionContent.QuestionTypeId} />
        </Box>
    );
};

export default SingleSliderSlide;
