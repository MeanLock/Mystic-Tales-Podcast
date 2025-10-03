/* eslint-disable @typescript-eslint/no-explicit-any */
/* eslint-disable @typescript-eslint/no-unsafe-function-type */

import { Box, Switch, Typography } from "@mui/material";
import { useCallback, useMemo } from "react";
import type { OptionType, QuestionType } from "@/core/types/tools";
import type { RangeSliderConfigJsonType } from "../../RangeSlider/RangeSlider";

interface SwitchCustomizeProps {
    label: React.ReactNode;
    question: any;
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
    isMinMax?: boolean;
    type?: string;
}

export default function SwitchCustomize({
    isMinMax = false,
    label,
    question,
    handleUpdateQuestion,
    type = "",
}: SwitchCustomizeProps) {
    const handleChange = useCallback(
        (event: React.ChangeEvent<HTMLInputElement>) => {
            const value = event.target.checked;
            if (type === "IsChooseMuitiple") {
                handleUpdateQuestion("ConfigJson", {
                    ...question?.ConfigJson,
                    [type]: value,
                    MinChoiceCount: 1,
                    MaxChoiceCount: question?.Options?.length || 1,
                });
            } else {
                handleUpdateQuestion("ConfigJson", {
                    ...question?.ConfigJson,
                    [type]: value,
                });
            }
        },
        [
            handleUpdateQuestion,
            question?.ConfigJson,
            question?.Options?.length,
            type,
        ]
    );

    const checked = useMemo(() => {
        return question?.ConfigJson[type] || false;
    }, [question, type]);

    const min = useMemo(
        () => question?.ConfigJson?.MinChoiceCount || 1,
        [question?.ConfigJson?.MinChoiceCount]
    );
    const max = useMemo(
        () => question?.ConfigJson?.MaxChoiceCount || 1,
        [question?.ConfigJson?.MaxChoiceCount]
    );

    const handleChangeInputMin = useCallback(
        (event: React.ChangeEvent<HTMLInputElement>) => {
            const value = Number(event.target.value);
            if (
                value < 1 ||
                value > question?.Options?.length ||
                value >= question?.ConfigJson?.MaxChoiceCount
            ) {
                return;
            }
            handleUpdateQuestion("ConfigJson", {
                ...question?.ConfigJson,
                MinChoiceCount: value,
            });
        },
        [handleUpdateQuestion, question?.ConfigJson, question?.Options?.length]
    );

    const handleChangeInputMax = useCallback(
        (event: React.ChangeEvent<HTMLInputElement>) => {
            const value = Number(event.target.value);
            if (
                value < 1 ||
                value > question?.Options?.length ||
                value <= question?.ConfigJson?.MinChoiceCount
            ) {
                return;
            }
            handleUpdateQuestion("ConfigJson", {
                ...question?.ConfigJson,
                MaxChoiceCount: value,
            });
        },
        [handleUpdateQuestion, question?.ConfigJson, question?.Options?.length]
    );

    return (
        <div
            className="mb-2"
            style={{
                border: `${isMinMax ? "1px solid #ccc" : ""}`,
                borderRadius: 10,
            }}
        >
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
                    {label}
                </Typography>

                <Switch checked={checked} onChange={handleChange} />
            </Box>
            {isMinMax && checked ? (
                <Box padding={"10px"} paddingLeft={"40px"}>
                    <input
                        onChange={handleChangeInputMin}
                        placeholder="Chọn ít nhất câu trả lời"
                        style={{
                            height: 32,
                            fontSize: 12,
                            border: "1px solid #ccc",
                            borderRadius: 6,
                            display: "block",
                            padding: "10px 10px",
                            width: "100%",
                            background: "#f5f5f5",
                            outline: "none",
                        }}
                        type="number"
                        min={1}
                        max={question?.Options?.length || 1}
                        value={min}
                    />
                    <div
                        style={{
                            height: 4,
                            width: "100%",
                        }}
                    ></div>
                    <input
                        onChange={handleChangeInputMax}
                        type="number"
                        min={1}
                        max={question?.Options?.length || 1}
                        placeholder="Chọn nhiều nhất câu trả lời"
                        style={{
                            height: 32,
                            fontSize: 12,
                            border: "1px solid #ccc",
                            borderRadius: 6,
                            padding: "10px 10px",
                            display: "block",
                            width: "100%",
                            background: "#f5f5f5",
                            outline: "none",
                        }}
                        value={max}
                    />
                </Box>
            ) : (
                ""
            )}
        </div>
    );
}
