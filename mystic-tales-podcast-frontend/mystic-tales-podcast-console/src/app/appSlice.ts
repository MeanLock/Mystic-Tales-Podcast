/* eslint-disable @typescript-eslint/no-explicit-any */
import type { PayloadAction } from "@reduxjs/toolkit";
import { createSlice } from "@reduxjs/toolkit";

interface IQuestionContent {
    Id: string;
    MainImageUrl: string;
    QuestionTypeId: number;
    Content: string;
    Description: string;
    ConfigJson: unknown;
    Options: [];
}

interface SurveyResponse {
    isEnd?: boolean;
    isNext?: boolean;
    parentId?: string;
    IsValid: boolean;
    ValueJson: {
        QuestionContent: IQuestionContent;
        QuestionResponse: unknown;
    };
}

interface SurveyData {
    taken_subject?: string;
    InvalidReason: string;
    SurveyResponses: SurveyResponse[];
}

interface AppState {
    surveyData: SurveyData | null;
    infoSurvey: any;
    isValid: boolean;
}

const initialState: AppState = {
    surveyData: null,
    infoSurvey: null,
    isValid: false,
};

// Helper function để so sánh câu trả lời
const compareAnswers = (answer1: any, answer2: any, questionType: string) => {
    if (!answer1 || !answer2) return true; // Nếu một trong hai chưa trả lời thì không so sánh

    switch (questionType) {
        case "SingleChoice":
            //console.log('Comparing SingleChoice:', answer1.SingleChoice, answer2.SingleChoice);
            return answer1.SingleChoice === answer2.SingleChoice;
        case "MultipleChoice":
            const arr1 = Array.isArray(answer1.MultipleChoice)
                ? answer1.MultipleChoice.sort()
                : [];
            const arr2 = Array.isArray(answer2.MultipleChoice)
                ? answer2.MultipleChoice.sort()
                : [];
            //console.log('Comparing MultipleChoice:', arr1, arr2);
            return JSON.stringify(arr1) === JSON.stringify(arr2);
        case "Input":
            //console.log('Comparing Input:', answer1.Input?.Value, answer2.Input?.Value);
            return answer1.Input?.Value === answer2.Input?.Value;
        // case "Range":
        //     return (
        //         answer1.Range?.Min === answer2.Range?.Min &&
        //         answer1.Range?.Max === answer2.Range?.Max
        //     );
        case "Ranking":
            //console.log('Comparing Ranking:',  JSON.stringify(answer1.Ranking), JSON.stringify(answer2.Ranking));
            return (
                JSON.stringify(answer1.Ranking) ===
                JSON.stringify(answer2.Ranking)
            );
        default:
            return true;
    }
};

// Helper function để validate câu trả lời lặp lại
const validateDuplicateAnswers = (
    state: any,
    currentQuestionId: string,
    questionType: string
) => {
    const responses = state.surveyData?.SurveyResponses || [];

    const current = responses.find(
        (response: any) =>
            response.ValueJson.QuestionContent.Id === currentQuestionId
    );
    if (!current || !current.parentId) return;

    const parent = responses.find(
        (response: any) =>
            response.ValueJson.QuestionContent.Id === current.parentId
    );
    if (!parent) return;

    const answer1 = current.ValueJson.QuestionResponse;
    const answer2 = parent.ValueJson.QuestionResponse;
    //console.log('Validating answers:', answer1, answer2, questionType);
    //console.log('SO SÁNH KQ:', compareAnswers(answer1, answer2, questionType));
    if (!compareAnswers(answer1, answer2, questionType)) {
        const hasReason = !!state.surveyData.InvalidReason;
        const mess = `${hasReason ? ". " : ""}Câu ${current.parentId} có đáp án không khớp với nhau`;
        if (
            state.surveyData &&
            !state.surveyData.InvalidReason.includes(mess)
        ) {
            state.surveyData.InvalidReason =
                state.surveyData.InvalidReason + mess;
        }
        
        // THIẾU: Set IsValid = false cho cả 2 câu hỏi
        state.surveyData.SurveyResponses = state.surveyData.SurveyResponses.map((response : any) => {
            if (response.ValueJson.QuestionContent.Id === currentQuestionId ||
                response.ValueJson.QuestionContent.Id === current.parentId) {
                return { ...response, IsValid: false };
            }
            return response;
        });
    } else {
        // Nếu khớp, có thể xóa error message và set IsValid = true
        const mess = `Câu ${current.parentId} có đáp án không khớp với nhau`;
        if (state.surveyData && state.surveyData.InvalidReason.includes(mess)) {
            state.surveyData.InvalidReason = state.surveyData.InvalidReason
                .replace(new RegExp(`\\.?\\s*${mess.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')}`, 'g'), '')
                .trim();
        }
        
        // Set IsValid = true nếu không có lỗi khác
        state.surveyData.SurveyResponses = state.surveyData.SurveyResponses.map((response: any) => {
            if (response.ValueJson.QuestionContent.Id === currentQuestionId ||
                response.ValueJson.QuestionContent.Id === current.parentId) {
                return { ...response, IsValid: true };
            }
            return response;
        });
    }
};

export const appSlice = createSlice({
    name: "appSlice",
    initialState,
    reducers: {
        handleSetInfoSurvey(state, action: PayloadAction<SurveyData>) {
            state.infoSurvey = action.payload;
        },
        setSurveyData(state, action: PayloadAction<SurveyData>) {
            state.surveyData = action.payload;
        },
        // // clearSurveyData(state) {
        // //     state.surveyData = null;
        // // },
        handleAddQuestionResponse(
            state,
            action: PayloadAction<SurveyResponse>
        ) {
            if (state.surveyData) {
                state.surveyData.SurveyResponses = [
                    ...state.surveyData.SurveyResponses,
                    action.payload,
                ];
            }
        },
        // Cập nhật các handler functions
        handleUpdateSigleChoose(
            state,
            action: PayloadAction<{
                idChoose: string;
                questionId: string;
            }>
        ) {
            if (!state.isValid) {
                const clone = state.surveyData?.SurveyResponses.map((i) => {
                    if (
                        i.ValueJson.QuestionContent.Id ===
                        action.payload.questionId
                    ) {
                        return {
                            ...i,
                            isNext: true,
                            ValueJson: {
                                ...i.ValueJson,
                                QuestionResponse: {
                                    ...((typeof i.ValueJson.QuestionResponse ===
                                        "object" &&
                                        i.ValueJson.QuestionResponse) ||
                                        {}),
                                    SingleChoice: action.payload.idChoose,
                                },
                            },
                        };
                    }
                    return {
                        ...i,
                        // IsValid: true,
                    };
                });
                if (state.surveyData && clone) {
                    state.surveyData.SurveyResponses = clone;
                    // Validate câu trả lời lặp lại
                    validateDuplicateAnswers(
                        state,
                        action.payload.questionId,
                        "SingleChoice"
                    );
                }
            } else {
                if (!state.surveyData) return;
                state.surveyData.SurveyResponses =
                    state.surveyData.SurveyResponses.map((i) =>
                        i.ValueJson.QuestionContent.Id ===
                            action.payload.questionId
                            ? { ...i, IsValid: false }
                            : i
                    );
                const mess = `${state.surveyData.InvalidReason ? ". " : ""
                    }Câu ${action.payload.questionId
                    } Thời điểm ghi nhận không hợp lệ`;
                if (!state.surveyData.InvalidReason.includes(mess)) {
                    state.surveyData.InvalidReason =
                        state.surveyData.InvalidReason + mess;
                }
            }
        },

        handleUpdateMutilChoice(
            state,
            action: PayloadAction<{
                idChoose: string;
                questionId: string;
            }>
        ) {
            if (!state.isValid) {
                const clone = state.surveyData?.SurveyResponses.map((i) => {
                    if (
                        i.ValueJson.QuestionContent.Id ===
                        action.payload.questionId
                    ) {
                        const prevArr = Array.isArray(
                            (i.ValueJson.QuestionResponse as any)
                                ?.MultipleChoice
                        )
                            ? [
                                ...(i.ValueJson.QuestionResponse as any)
                                    .MultipleChoice,
                            ]
                            : [];
                        const idx = prevArr.findIndex(
                            (v) => v === action.payload.idChoose
                        );
                        let newArr;
                        if (idx > -1) {
                            newArr = prevArr.filter((v) => v !== action.payload.idChoose);
                        } else {
                            newArr = [...prevArr, action.payload.idChoose];
                        }
                        return {
                            // IsValid: true,
                            ...i,
                            isNext: true,
                            ValueJson: {
                                ...i.ValueJson,
                                QuestionResponse: {
                                    ...((typeof i.ValueJson.QuestionResponse ===
                                        "object" &&
                                        i.ValueJson.QuestionResponse) ||
                                        {}),
                                    MultipleChoice: newArr,
                                },
                            },
                        };
                    }
                    return {
                        ...i,
                        // IsValid: true,
                    };
                });
                if (state.surveyData && clone) {
                    state.surveyData.SurveyResponses = clone;
                    // Validate câu trả lời lặp lại
                    validateDuplicateAnswers(
                        state,
                        action.payload.questionId,
                        "MultipleChoice"
                    );
                }
            } else {
                if (!state.surveyData) return;
                state.surveyData.SurveyResponses =
                    state.surveyData.SurveyResponses.map((i) =>
                        i.ValueJson.QuestionContent.Id ===
                            action.payload.questionId
                            ? { ...i, IsValid: false }
                            : i
                    );
                const mess = `${state.surveyData.InvalidReason ? ". " : ""
                    }Câu ${action.payload.questionId
                    } Thời điểm ghi nhận không hợp lệ`;
                if (!state.surveyData.InvalidReason.includes(mess)) {
                    state.surveyData.InvalidReason =
                        state.surveyData.InvalidReason + mess;
                }
            }
        },

        handleChangeSlider(
            state,
            action: PayloadAction<{
                idChoose: string;
                value: number;
            }>
        ) {
            if (!state.isValid) {
                const clone = state.surveyData?.SurveyResponses.map((i) => {
                    if (
                        i.ValueJson.QuestionContent.Id ===
                        action.payload.idChoose
                    ) {
                        return {
                            ...i,
                            isNext: true,
                            ValueJson: {
                                ...i.ValueJson,
                                QuestionResponse: {
                                    ...((typeof i.ValueJson.QuestionResponse ===
                                        "object" &&
                                        i.ValueJson.QuestionResponse) ||
                                        {}),
                                    Input: {
                                        Value: action.payload.value,
                                        ValueType: "number",
                                    },
                                },
                            },
                        };
                    }
                    return {
                        ...i,
                        // IsValid: true,
                    };
                });
                if (state.surveyData && clone) {
                    state.surveyData.SurveyResponses = clone;
                    // Validate câu trả lời lặp lại
                    validateDuplicateAnswers(
                        state,
                        action.payload.idChoose,
                        "Input"
                    );
                }
            } else {
                if (!state.surveyData) return;
                state.surveyData.SurveyResponses =
                    state.surveyData.SurveyResponses.map((i) =>
                        i.ValueJson.QuestionContent.Id ===
                            action.payload.idChoose
                            ? { ...i, IsValid: false }
                            : i
                    );
                const mess = `${state.surveyData.InvalidReason ? ". " : ""
                    }Câu ${action.payload.idChoose
                    } Thời điểm ghi nhận không hợp lệ`;
                if (!state.surveyData.InvalidReason.includes(mess)) {
                    state.surveyData.InvalidReason =
                        state.surveyData.InvalidReason + mess;
                }
            }
        },

        handleChangeRangeSlide(
            state,
            action: PayloadAction<{
                idChoose: string;
                min: number;
                max: number;
            }>
        ) {
            if (!state.isValid) {
                const clone = state.surveyData?.SurveyResponses.map((i) => {
                    if (
                        i.ValueJson.QuestionContent.Id ===
                        action.payload.idChoose
                    ) {
                        return {
                            ...i,
                            isNext: true,
                            ValueJson: {
                                ...i.ValueJson,
                                QuestionResponse: {
                                    ...((typeof i.ValueJson.QuestionResponse ===
                                        "object" &&
                                        i.ValueJson.QuestionResponse) ||
                                        {}),
                                    Range: {
                                        Min: action.payload.min,
                                        Max: action.payload.max,
                                    },
                                },
                            },
                        };
                    }
                    return {
                        ...i,
                        // IsValid: true,
                    };
                });
                if (state.surveyData && clone) {
                    state.surveyData.SurveyResponses = clone;
                    // Validate câu trả lời lặp lại
                    validateDuplicateAnswers(
                        state,
                        action.payload.idChoose,
                        "Range"
                    );
                }
            } else {
                if (!state.surveyData) return;
                state.surveyData.SurveyResponses =
                    state.surveyData.SurveyResponses.map((i) =>
                        i.ValueJson.QuestionContent.Id ===
                            action.payload.idChoose
                            ? { ...i, IsValid: false }
                            : i
                    );
                const mess = `${state.surveyData.InvalidReason ? ". " : ""
                    }Câu ${action.payload.idChoose
                    } Thời điểm ghi nhận không hợp lệ`;
                if (!state.surveyData.InvalidReason.includes(mess)) {
                    state.surveyData.InvalidReason =
                        state.surveyData.InvalidReason + mess;
                }
            }
        },

        handleUpdateRating(
            state,
            action: PayloadAction<{
                idChoose:string;
                value: number;
            }>
        ) {
            if (!state.isValid) {
                const clone = state.surveyData?.SurveyResponses.map((i) => {
                    if (
                        i.ValueJson.QuestionContent.Id ===
                        action.payload.idChoose
                    ) {
                        return {
                            ...i,
                            isNext: true,
                            ValueJson: {
                                ...i.ValueJson,
                                QuestionResponse: {
                                    ...((typeof i.ValueJson.QuestionResponse ===
                                        "object" &&
                                        i.ValueJson.QuestionResponse) ||
                                        {}),
                                    Input: {
                                        Value: action.payload.value,
                                        ValueType: "number",
                                    },
                                },
                            },
                        };
                    }
                    return {
                        ...i,
                        // IsValid: true,
                    };
                });
                if (state.surveyData && clone) {
                    state.surveyData.SurveyResponses = clone;
                    // Validate câu trả lời lặp lại
                    validateDuplicateAnswers(
                        state,
                        action.payload.idChoose,
                        "Input"
                    );
                }
            } else {
                if (!state.surveyData) return;
                state.surveyData.SurveyResponses =
                    state.surveyData.SurveyResponses.map((i) =>
                        i.ValueJson.QuestionContent.Id ===
                            action.payload.idChoose
                            ? { ...i, IsValid: false }
                            : i
                    );
                const mess = `${state.surveyData.InvalidReason ? ". " : ""
                    }Câu ${action.payload.idChoose
                    } Thời điểm ghi nhận không hợp lệ`;
                if (!state.surveyData.InvalidReason.includes(mess)) {
                    state.surveyData.InvalidReason =
                        state.surveyData.InvalidReason + mess;
                }
            }
        },

        handleUpdateRaking(
            state,
            action: PayloadAction<{
                idChoose: string;
                ranking: { SurveyOptionId: string; RankIndex: number }[];
            }>
        ) {
            if (!state.isValid) {
                const clone = state.surveyData?.SurveyResponses.map((i) => {
                    if (
                        i.ValueJson.QuestionContent.Id ===
                        action.payload.idChoose
                    ) {
                        const isNext =
                            action.payload.ranking.length ===
                            i?.ValueJson?.QuestionContent?.Options?.length;
                        return {
                            ...i,
                            isNext: isNext,
                            ValueJson: {
                                ...i.ValueJson,
                                QuestionResponse: {
                                    ...((typeof i.ValueJson.QuestionResponse ===
                                        "object" &&
                                        i.ValueJson.QuestionResponse) ||
                                        {}),
                                    Ranking: action.payload.ranking,
                                },
                            },
                        };
                    }
                    return {
                        ...i,
                        // IsValid: true,
                    };
                });
                if (state.surveyData && clone) {
                    state.surveyData.SurveyResponses = clone;
                    // Validate câu trả lời lặp lại
                    validateDuplicateAnswers(
                        state,
                        action.payload.idChoose,
                        "Ranking"
                    );
                }
            } else {
                if (!state.surveyData) return;
                state.surveyData.SurveyResponses =
                    state.surveyData.SurveyResponses.map((i) =>
                        i.ValueJson.QuestionContent.Id ===
                            action.payload.idChoose
                            ? { ...i, IsValid: false }
                            : i
                    );
                const mess = `${state.surveyData.InvalidReason ? ". " : ""
                    }Câu ${action.payload.idChoose
                    } Thời điểm ghi nhận không hợp lệ`;
                if (!state.surveyData.InvalidReason.includes(mess)) {
                    state.surveyData.InvalidReason =
                        state.surveyData.InvalidReason + mess;
                }
            }
        },

        handleUpdateForm(
            state,
            action: PayloadAction<{
                idChoose: string;
                type: number;
                value: string | number;
            }>
        ) {
            if (!state.isValid) {
                const type = action.payload.type;
                const value = action.payload.value;
                const clone = state.surveyData?.SurveyResponses.map((i) => {
                    if (
                        i.ValueJson.QuestionContent.Id ===
                        action.payload.idChoose
                    ) {
                        return {
                            ...i,
                            isNext: true,
                            ValueJson: {
                                ...i.ValueJson,
                                QuestionResponse: {
                                    ...((typeof i.ValueJson.QuestionResponse ===
                                        "object" &&
                                        i.ValueJson.QuestionResponse) ||
                                        {}),
                                    Input: {
                                        Value: value,
                                        ValueType:
                                            type === 4 ? "number" : "string",
                                    },
                                },
                            },
                        };
                    }
                    return {
                        ...i,
                        // IsValid: true,
                    };
                });
                if (state.surveyData && clone) {
                    state.surveyData.SurveyResponses = clone;
                    // Validate câu trả lời lặp lại
                    validateDuplicateAnswers(
                        state,
                        action.payload.idChoose,
                        "Input"
                    );
                }
            } else {
                if (!state.surveyData) return;
                state.surveyData.SurveyResponses =
                    state.surveyData.SurveyResponses.map((i) =>
                        i.ValueJson.QuestionContent.Id ===
                            action.payload.idChoose
                            ? { ...i, IsValid: false }
                            : i
                    );
                const mess = `${state.surveyData.InvalidReason ? ". " : ""
                    }Câu ${action.payload.idChoose
                    } Thời điểm ghi nhận không hợp lệ`;
                if (!state.surveyData.InvalidReason.includes(mess)) {
                    state.surveyData.InvalidReason =
                        state.surveyData.InvalidReason + mess;
                }
            }
        },
        handleSetIsValid(state, action: PayloadAction<boolean>) {
            state.isValid = action.payload;
        },

        handleUpdateSpeechText(
            state,
            action: PayloadAction<{ text: string; questionId: string }>
        ) {
            if (!state.isValid) {
                const clone = state.surveyData?.SurveyResponses.map((i) => {
                    if (
                        i.ValueJson.QuestionContent.Id ===
                        action.payload.questionId
                    ) {
                        return {
                            ...i,
                            IsValid: true,
                            ValueJson: {
                                ...i.ValueJson,
                                QuestionResponse: {
                                    ...(typeof i.ValueJson.QuestionResponse ===
                                        "object" && i.ValueJson.QuestionResponse
                                        ? i.ValueJson.QuestionResponse
                                        : {}),
                                    SpeechText: action.payload.text,
                                },
                            },
                        };
                    }
                    return {
                        ...i,
                        IsValid: true,
                    };
                });
                if (state.surveyData && clone) {
                    state.surveyData.SurveyResponses = clone;
                }
            }
        },

        handleUpdateInvalidReason(
            state,
            action: PayloadAction<{ questionId: string }>
        ) {
            if (state.surveyData) {
                state.surveyData.InvalidReason =
                    state.surveyData.InvalidReason +
                    `${state.surveyData.InvalidReason ? ". " : ""}Câu ${action.payload.questionId
                    } phát hiện đầu vào ẩn`;
                state.surveyData.SurveyResponses =
                    state.surveyData.SurveyResponses.map((i) =>
                        i.ValueJson.QuestionContent.Id ===
                            action.payload.questionId
                            ? { ...i, IsValid: false }
                            : i
                    );
            }
        },
    },
});

export const {
    setSurveyData,
    // clearSurveyData,
    handleSetInfoSurvey,
    handleAddQuestionResponse,
    handleUpdateSigleChoose,
    handleUpdateMutilChoice,
    handleChangeRangeSlide,
    handleChangeSlider,
    handleUpdateRating,
    handleUpdateRaking,
    handleUpdateForm,
    handleSetIsValid,
    handleUpdateSpeechText,
    handleUpdateInvalidReason,
} = appSlice.actions;
export default appSlice.reducer;
