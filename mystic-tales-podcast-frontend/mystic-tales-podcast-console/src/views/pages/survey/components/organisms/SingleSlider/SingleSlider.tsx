import "./styles.scss";
import React, { useCallback, useEffect, useMemo, useState } from "react";
import Slider from "@mui/material/Slider";
import TextField from "@mui/material/TextField";
import Box from "@mui/material/Box";
import Typography from "@mui/material/Typography";
import type {
    OptionType,
    QuestionType,
    SurveyType,
} from "@/core/types/tools";

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
            | Record<string, string | number>
    ) => void;
};

const SingleSlider = ({ question, handleUpdateQuestion, formData }: Props) => {
    const [value, setValue] = useState(
        Number(question?.ConfigJson?.Max) || 10
    );
    const min = useMemo(
        () => Number(question?.ConfigJson?.Min) || 0,
        [question]
    );
    const max = useMemo(
        () => Number(question?.ConfigJson?.Max) || 0,
        [question]
    );
    const step = useMemo(
        () => Number(question?.ConfigJson?.Step) || 0,
        [question]
    );
    const unit = useMemo(
        () => question?.ConfigJson?.Unit || "",
        [question]
    );
    const handleSliderChange = (_event: Event, newValue: number) => {
        setValue(newValue);
    };

    const handleMinChange = useCallback(
        (event: React.ChangeEvent<HTMLInputElement>) => {
            const value = Number(event.target.value);
            if (value >= max) {
                return;
            }
            handleUpdateQuestion("ConfigJson", {
                ...question.ConfigJson,
                Min: value,
            });
        },
        [handleUpdateQuestion, max, question.ConfigJson]
    );

    const handleMaxChange = useCallback(
        (event: React.ChangeEvent<HTMLInputElement>) => {
            const value = Number(event.target.value);
            if (value <= min) {
                return;
            }
            handleUpdateQuestion("ConfigJson", {
                ...question.ConfigJson,
                Max: value,
            });
        },
        [handleUpdateQuestion, min, question.ConfigJson]
    );

    const handleStepChange = useCallback(
        (event: React.ChangeEvent<HTMLInputElement>) => {
            const value = Number(event.target.value);
            if (value >= max - min) {
                return;
            }
            handleUpdateQuestion("ConfigJson", {
                ...question.ConfigJson,
                Step: event.target.value,
            });
        },
        [handleUpdateQuestion, max, min, question.ConfigJson]
    );

    const handleUnitChange = (event: React.ChangeEvent<HTMLInputElement>) => {
        handleUpdateQuestion("ConfigJson", {
            ...question.ConfigJson,
            Unit: event.target.value,
        });
    };

    useEffect(() => {
        if (Object.keys(question.ConfigJson).length === 0) {
            handleUpdateQuestion("ConfigJson", {
                Min: 0,
                Max: 10,
                Step: 1,
                Unit: "",
            });
        }
    }, [handleUpdateQuestion, question.ConfigJson]);

    return (
        <Box className="single-slider bg-gray-500 p-6 rounded-lg flex flex-col gap-6">
            <Box className="flex flex-col items-center w-full">
                <Box className="flex justify-between w-full px-1 mb-2">
                    <Typography variant="body2" color="white">
                        {min}
                    </Typography>
                    <Typography
                        variant="body2"
                        color="white"
                        sx={{ flexGrow: 1, textAlign: "center" }}
                    >
                        {step}
                    </Typography>
                    <Typography variant="body2" color="white">
                        {max}
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
                            backgroundColor: formData?.ConfigJson
                                ?.ButtonBackgroundColor
                                ? formData?.ConfigJson
                                      ?.ButtonBackgroundColor
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
                        value={min}
                        onChange={handleMinChange}
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
                        value={step}
                        onChange={handleStepChange}
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
                        value={max}
                        onChange={handleMaxChange}
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
                        value={unit}
                        onChange={handleUnitChange}
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
    );
};

export default SingleSlider;
