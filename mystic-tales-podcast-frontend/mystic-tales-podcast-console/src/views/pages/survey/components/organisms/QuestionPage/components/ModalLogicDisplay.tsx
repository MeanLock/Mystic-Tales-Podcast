/* eslint-disable @typescript-eslint/no-explicit-any */
import { useState } from "react";
import type { OptionType, QuestionType } from "@/core/types/tools";
import type { RangeSliderConfigJsonType } from "../../RangeSlider/RangeSlider";

type DisplayLogicType = {
    Conditions: {
        QuestionOrder: number;
        Conjunction: string | null;
        Operator: string;
        OptionOrder?: number;
        CompareValue?: number;
    }[];
    TargetQuestionOrder: number | "end";
};

type ConfigJsonType = {
    DisplayLogics?: DisplayLogicType[];
    BackgroundGradient1Color?: string;
    BackgroundGradient2Color?: string;
    TitleColor?: string;
    ContentColor?: string;
    ButtonBackgroundColor?: string;
    ButtonContentColor?: string;
    Password?: string;
    Brightness?: number;
    IsResizableIframeEnabled?: boolean;
};

type OperatorType = {
    value: string;
    label: string;
};

export default function LogicComponentDisplay({
    handleUpdateQuestion,
    question,
    questions,
}: {
    question: QuestionType;
    questions: QuestionType[];
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
}) {
    const [isModalOpen, setIsModalOpen] = useState(false);

    const handleOpenModal = () => {
        setIsModalOpen(true);
    };

    const handleCloseModal = () => {
        setIsModalOpen(false);
    };

    return (
        <>
            <div style={styles.modalContainer}>
                <div style={styles.header}>
                    <h3 style={styles.title}>
                        Display Logic - Điều kiện hiện câu hỏi này
                    </h3>
                    <button
                        style={styles.closeButton}
                        onClick={handleCloseModal}
                    >
                        ×
                    </button>
                </div>
                <button style={styles.addButton} onClick={handleOpenModal}>
                    Thêm logic
                </button>
            </div>

            {isModalOpen && (
                <ModalLogic
                    onClose={handleCloseModal}
                    handleUpdateQuestion={handleUpdateQuestion}
                    questionsData={questions}
                    question={question}
                />
            )}
        </>
    );
}

function ModalLogic({
    onClose,
    questionsData,
    handleUpdateQuestion,
    question,
}: {
    question: QuestionType;
    onClose: () => void;
    questionsData: QuestionType[];
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
}) {
    const configJson = question?.ConfigJson as ConfigJsonType;
    const DisplayLogics = (configJson?.DisplayLogics ||
        []) as DisplayLogicType[];

    const handleAddLogic = () => {
        const newLogic: DisplayLogicType = {
            Conditions: [],
            TargetQuestionOrder: "end",
        };
        handleUpdateQuestion("ConfigJson", {
            ...configJson,
            DisplayLogics: [...DisplayLogics, newLogic],
        });
    };

    const handleDeleteLogic = (logicIndex: number) => {
        const newLogics = DisplayLogics.filter(
            (_, index) => index !== logicIndex
        );
        handleUpdateQuestion("ConfigJson", {
            ...configJson,
            DisplayLogics: newLogics,
        });
    };

    const handleAddCondition = (logicIndex: number) => {
        const logic = DisplayLogics[logicIndex];
        const newCondition = {
            QuestionOrder: 0,
            Conjunction: logic.Conditions.length > 0 ? "AND" : null,
            Operator: "",
            OptionOrder: 0,
            CompareValue: 0,
        };
        const newLogics = [...DisplayLogics];
        newLogics[logicIndex] = {
            ...logic,
            Conditions: [...logic.Conditions, newCondition],
        };
        handleUpdateQuestion("ConfigJson", {
            ...configJson,
            DisplayLogics: newLogics,
        });
    };

    const handleDeleteCondition = (
        logicIndex: number,
        conditionIndex: number
    ) => {
        const logic = DisplayLogics[logicIndex];
        const newConditions = logic.Conditions.filter(
            (_, index) => index !== conditionIndex
        );
        const newLogics = [...DisplayLogics];
        newLogics[logicIndex] = {
            ...logic,
            Conditions: newConditions,
        };
        handleUpdateQuestion("ConfigJson", {
            ...configJson,
            DisplayLogics: newLogics,
        });
    };

    const handleUpdateCondition = (
        logicIndex: number,
        conditionIndex: number,
        key: string,
        value: string | number
    ) => {
        const logic = DisplayLogics[logicIndex];
        const condition = logic.Conditions[conditionIndex];
        const newCondition = { ...condition, [key]: value };
        const newConditions = [...logic.Conditions];
        newConditions[conditionIndex] = newCondition;
        const newLogics = [...DisplayLogics];
        newLogics[logicIndex] = {
            ...logic,
            Conditions: newConditions,
        };
        handleUpdateQuestion("ConfigJson", {
            ...configJson,
            DisplayLogics: newLogics,
        });
    };

    const handleSaveLogic = () => {
        const isValid = DisplayLogics.every((logic) => {
            if (!logic.TargetQuestionOrder) {
                return false;
            }
            return logic.Conditions.every((condition) => {
                if (!condition.QuestionOrder || !condition.Operator) {
                    return false;
                }
                if (["Chọn", "Không Chọn"].includes(condition.Operator)) {
                    return condition.OptionOrder !== 0;
                }
                if (
                    ["=", "≠", ">", "≥", "<", "≤"].includes(condition.Operator)
                ) {
                    return condition.CompareValue !== 0;
                }
                return true;
            });
        });

        if (!isValid) {
            alert("Vui lòng điền đầy đủ thông tin cho tất cả điều kiện");
            return;
        }

        onClose();
    };

    const getOperatorsForQuestion = (
        QuestionTypeId: number
    ): OperatorType[] => {
        switch (QuestionTypeId) {
            case 1:
                return [
                    { value: "Chọn", label: "Chọn" },
                    { value: "Không Chọn", label: "Không Chọn" },
                ];
            case 2:
                return [
                    { value: "Chọn", label: "Chọn" },
                    { value: "Không Chọn", label: "Không Chọn" },
                ];
            case 6:
                return [
                    { value: "=", label: "=" },
                    { value: ">", label: ">" },
                    { value: "≥", label: "≥" },
                    { value: "<", label: "<" },
                    { value: "≤", label: "≤" },
                ];
            default:
                return [];
        }
    };

    return (
        <div style={styles.modalOverlay}>
            <div className="bg-white rounded-lg w-[90%] max-w-3xl max-h-[90vh] flex flex-col">
                <div className="p-6 border-b border-gray-200">
                    <h2 className="text-xl font-semibold text-gray-900 mb-2">
                        Display Logic - Điều kiện hiện câu hỏi
                    </h2>
                    <p className="text-sm text-gray-600">
                        Thêm điều kiện để hiển thị câu hỏi này dựa trên câu trả
                        lời của các câu hỏi khác
                    </p>
                </div>

                <div className="flex-1 overflow-y-auto p-4">
                    {DisplayLogics.map((logic, logicIndex) => (
                        <div
                            key={logicIndex}
                            className="mb-6 bg-white border border-gray-200 rounded-lg p-4"
                        >
                            <div className="flex justify-between items-center mb-4">
                                <h3 className="text-lg font-semibold text-gray-800">
                                    Logic {logicIndex + 1}
                                </h3>
                                <div className="flex gap-2">
                                    <button
                                        onClick={() =>
                                            handleAddCondition(logicIndex)
                                        }
                                        className="px-3 py-1.5 bg-blue-500 text-white rounded hover:bg-blue-600 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2"
                                    >
                                        + Thêm điều kiện
                                    </button>
                                    {DisplayLogics.length > 1 && (
                                        <button
                                            onClick={() =>
                                                handleDeleteLogic(logicIndex)
                                            }
                                            className="px-3 py-1.5 bg-red-500 text-white rounded hover:bg-red-600 focus:outline-none focus:ring-2 focus:ring-red-500 focus:ring-offset-2"
                                        >
                                            Xóa logic
                                        </button>
                                    )}
                                </div>
                            </div>

                            <div className="space-y-3">
                                {logic.Conditions.map(
                                    (condition: DisplayLogicType["Conditions"][number], conditionIndex: number) => (
                                        <div
                                            key={conditionIndex}
                                            className="flex items-center gap-2 p-3 bg-gray-50 rounded-lg hover:bg-gray-100"
                                        >
                                            {conditionIndex > 0 && (
                                                <select
                                                    value={
                                                        condition.Conjunction ||
                                                        "AND"
                                                    }
                                                    onChange={(e) =>
                                                        handleUpdateCondition(
                                                            logicIndex,
                                                            conditionIndex,
                                                            "Conjunction",
                                                            e.target.value
                                                        )
                                                    }
                                                    className="w-20 px-2 py-1.5 border border-gray-300 rounded hover:border-blue-500 focus:border-blue-500 focus:ring-1 focus:ring-blue-500"
                                                >
                                                    <option value="AND">
                                                        Và
                                                    </option>
                                                    <option value="OR">
                                                        Hoặc
                                                    </option>
                                                </select>
                                            )}

                                            <select
                                                value={
                                                    condition.QuestionOrder?.toString() ||
                                                    ""
                                                }
                                                onChange={(e) =>
                                                    handleUpdateCondition(
                                                        logicIndex,
                                                        conditionIndex,
                                                        "QuestionOrder",
                                                        parseInt(e.target.value)
                                                    )
                                                }
                                                className="flex-1 px-2 py-1.5 border border-gray-300 rounded hover:border-blue-500 focus:border-blue-500 focus:ring-1 focus:ring-blue-500"
                                            >
                                                <option value="">
                                                    Chọn câu hỏi
                                                </option>
                                                {questionsData
                                                    ?.filter(
                                                        (item) =>
                                                            item.QuestionTypeId ===
                                                                1 ||
                                                            item.QuestionTypeId ===
                                                                2 ||
                                                            item.QuestionTypeId ===
                                                                6
                                                    )
                                                    ?.map((q) => (
                                                        <option
                                                            key={q.Order}
                                                            value={q.Order.toString()}
                                                        >
                                                            {q.Order} .
                                                            {q.Content &&
                                                                ` ${q.Content.substring(
                                                                    0,
                                                                    50
                                                                )}${
                                                                    q.Content
                                                                        .length >
                                                                    50
                                                                        ? "..."
                                                                        : ""
                                                                }`}
                                                        </option>
                                                    ))}
                                            </select>

                                            <select
                                                value={condition.Operator || ""}
                                                onChange={(e) =>
                                                    handleUpdateCondition(
                                                        logicIndex,
                                                        conditionIndex,
                                                        "Operator",
                                                        e.target.value
                                                    )
                                                }
                                                className="w-40 px-2 py-1.5 border border-gray-300 rounded hover:border-blue-500 focus:border-blue-500 focus:ring-1 focus:ring-blue-500"
                                                disabled={
                                                    !condition.QuestionOrder
                                                }
                                            >
                                                <option value="">
                                                    Chọn điều kiện
                                                </option>
                                                {condition.QuestionOrder &&
                                                    getOperatorsForQuestion(
                                                        questionsData.find(
                                                            (q) =>
                                                                q.Order ===
                                                                condition.QuestionOrder
                                                        )?.QuestionTypeId || 0
                                                    ).map((op) => (
                                                        <option
                                                            key={op.value}
                                                            value={op.value}
                                                        >
                                                            {op.label}
                                                        </option>
                                                    ))}
                                            </select>

                                            {["Chọn", "Không Chọn"].includes(
                                                condition.Operator || ""
                                            ) && (
                                                <select
                                                    value={
                                                        condition.OptionOrder?.toString() ||
                                                        ""
                                                    }
                                                    onChange={(e) =>
                                                        handleUpdateCondition(
                                                            logicIndex,
                                                            conditionIndex,
                                                            "OptionOrder",
                                                            parseInt(
                                                                e.target.value
                                                            )
                                                        )
                                                    }
                                                    className="w-40 px-2 py-1.5 border border-gray-300 rounded hover:border-blue-500 focus:border-blue-500 focus:ring-1 focus:ring-blue-500"
                                                    disabled={
                                                        !condition.Operator
                                                    }
                                                >
                                                    <option value="">
                                                        Chọn đáp án
                                                    </option>
                                                    {condition.QuestionOrder &&
                                                        questionsData
                                                            .find(
                                                                (q) =>
                                                                    q.Order ===
                                                                    condition.QuestionOrder
                                                            )
                                                            ?.Options?.map(
                                                                (opt: any) => (
                                                                    <option
                                                                        key={
                                                                            opt.Order
                                                                        }
                                                                        value={opt.Order.toString()}
                                                                    >
                                                                        {
                                                                            opt.Content
                                                                        }
                                                                    </option>
                                                                )
                                                            )}
                                                </select>
                                            )}

                                            {[
                                                "=",
                                                // "≠",
                                                ">",
                                                "≥",
                                                "<",
                                                "≤",
                                            ].includes(
                                                condition.Operator || ""
                                            ) && (
                                                <input
                                                    type="number"
                                                    value={
                                                        condition.CompareValue ||
                                                        ""
                                                    }
                                                    onChange={(e) =>
                                                        handleUpdateCondition(
                                                            logicIndex,
                                                            conditionIndex,
                                                            "CompareValue",
                                                            parseInt(
                                                                e.target.value
                                                            )
                                                        )
                                                    }
                                                    className="w-32 px-2 py-1.5 border border-gray-300 rounded hover:border-blue-500 focus:border-blue-500 focus:ring-1 focus:ring-blue-500"
                                                    placeholder="Nhập giá trị"
                                                    disabled={
                                                        !condition.Operator
                                                    }
                                                />
                                            )}

                                            <button
                                                onClick={() =>
                                                    handleDeleteCondition(
                                                        logicIndex,
                                                        conditionIndex
                                                    )
                                                }
                                                className="p-1.5 text-red-500 hover:text-red-600 focus:outline-none focus:ring-2 focus:ring-red-500 focus:ring-offset-2 rounded"
                                            >
                                                <svg
                                                    xmlns="http://www.w3.org/2000/svg"
                                                    className="h-5 w-5"
                                                    viewBox="0 0 20 20"
                                                    fill="currentColor"
                                                >
                                                    <path
                                                        fillRule="evenodd"
                                                        d="M9 2a1 1 0 00-.894.553L7.382 4H4a1 1 0 000 2v10a2 2 0 002 2h8a2 2 0 002-2V6a1 1 0 100-2h-3.382l-.724-1.447A1 1 0 0011 2H9zM7 8a1 1 0 012 0v6a1 1 0 11-2 0V8zm5-1a1 1 0 00-1 1v6a1 1 0 102 0V8a1 1 0 00-1-1z"
                                                        clipRule="evenodd"
                                                    />
                                                </svg>
                                            </button>
                                        </div>
                                    )
                                )}
                            </div>

                            <div className="mt-4 p-3 bg-gray-50 rounded-lg">
                                <div className="flex items-center gap-3">
                                    <span className="font-semibold text-gray-700 whitespace-nowrap">
                                        THÌ
                                    </span>
                                    <button className="px-3 py-1.5 bg-green-500 text-white rounded hover:bg-green-600 focus:outline-none focus:ring-2 focus:ring-green-500 focus:ring-offset-2">
                                        Hiện
                                    </button>
                                    <select
                                        value={
                                            logic.TargetQuestionOrder === "end"
                                                ? "end"
                                                : logic.TargetQuestionOrder.toString()
                                        }
                                        onChange={(e) => {
                                            const updatedLogic: DisplayLogicType = {
                                                ...logic,
                                                TargetQuestionOrder:
                                                    e.target.value === "end"
                                                        ? "end"
                                                        : parseInt(
                                                              e.target.value
                                                          ),
                                            };
                                            const newLogics = [
                                                ...DisplayLogics,
                                            ];
                                            newLogics[logicIndex] =
                                                updatedLogic;
                                            handleUpdateQuestion(
                                                "ConfigJson",
                                                {
                                                    ...configJson,
                                                    DisplayLogics: newLogics,
                                                }
                                            );
                                        }}
                                        className="flex-1 px-2 py-1.5 border border-gray-300 rounded hover:border-blue-500 focus:border-blue-500 focus:ring-1 focus:ring-blue-500"
                                    >
                                        <option value="">
                                            Chọn câu hỏi đích
                                        </option>
                                        {questionsData.map((q) => (
                                            <option
                                                key={q.Order}
                                                value={q.Order.toString()}
                                            >
                                                Câu hỏi {q.Order}
                                                {q.Content &&
                                                    ` - ${q.Content.substring(
                                                        0,
                                                        50
                                                    )}${
                                                        q.Content.length > 50
                                                            ? "..."
                                                            : ""
                                                    }`}
                                            </option>
                                        ))}
                                        <option value="end">
                                            Kết thúc khảo sát
                                        </option>
                                    </select>
                                </div>
                            </div>
                        </div>
                    ))}

                    <button
                        onClick={handleAddLogic}
                        className="w-full mt-4 px-4 py-2 bg-green-500 text-white rounded-lg hover:bg-green-600 focus:outline-none focus:ring-2 focus:ring-green-500 focus:ring-offset-2"
                    >
                        + Thêm logic mới
                    </button>
                </div>

                <div className="p-4 border-t border-gray-200 flex justify-end gap-3">
                    <button
                        onClick={onClose}
                        className="px-4 py-2 text-gray-700 bg-gray-100 rounded hover:bg-gray-200 focus:outline-none focus:ring-2 focus:ring-gray-500 focus:ring-offset-2"
                    >
                        Hủy
                    </button>
                    <button
                        onClick={handleSaveLogic}
                        className="px-4 py-2 text-white bg-blue-500 rounded hover:bg-blue-600 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2"
                    >
                        Lưu
                    </button>
                </div>
            </div>
        </div>
    );
}

const styles = {
    modalContainer: {
        backgroundColor: "#fff",
        border: "1px solid #ddd",
        borderRadius: "8px",
        padding: "20px",
        maxWidth: "600px",
        margin: "20px auto",
        fontFamily: "Arial, sans-serif",
        boxShadow: "0 2px 8px rgba(0,0,0,0.1)",
    },
    header: {
        display: "flex",
        justifyContent: "space-between",
        alignItems: "center",
        marginBottom: "20px",
        borderBottom: "1px solid #eee",
        paddingBottom: "10px",
    },
    title: {
        margin: 0,
        fontSize: "18px",
        color: "#333",
        fontWeight: "bold",
    },
    closeButton: {
        background: "none",
        border: "none",
        fontSize: "24px",
        cursor: "pointer",
        color: "#999",
        padding: "0",
        width: "30px",
        height: "30px",
        borderRadius: "50%",
        display: "flex",
        alignItems: "center",
        justifyContent: "center",
    },
    addButton: {
        backgroundColor: "#1890ff",
        color: "#fff",
        border: "none",
        borderRadius: "6px",
        padding: "10px 20px",
        cursor: "pointer",
        fontSize: "14px",
        fontWeight: "500",
        width: "100%",
        transition: "background-color 0.3s",
    },
    modalOverlay: {
        position: "fixed" as const,
        top: 0,
        left: 0,
        right: 0,
        bottom: 0,
        backgroundColor: "rgba(0, 0, 0, 0.6)",
        display: "flex",
        alignItems: "center",
        justifyContent: "center",
        zIndex: 1000,
    },
};
