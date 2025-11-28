import Rating from "@mui/material/Rating";
import Slider from '@mui/material/Slider';
import TextField from "@mui/material/TextField";
import StarBorderIcon from '@mui/icons-material/StarBorder';
import FavoriteIcon from '@mui/icons-material/Favorite';
import { useContext, useEffect, useState } from "react";
import { FieldInputType, Question, QuestionType } from "../../../../../core/types";
import { getFieldInputTypes, getQuestionTypes } from "../../../../../core/services/survey/survey-core/survey-core.service";
import { managerAxiosInstance } from "../../../../../core/api/rest-api/config/instances/v2/manager-axios-instance";
import { CommunitySurveyDetailViewContext } from "..";

const renderQuestionPreview = (question: Question, index: number, getQuestionTypeName: (typeId: number) => string, getInputTypeName: (typeId?: number) => string) => {
    const config = question.ConfigJsonString ? JSON.parse(question.ConfigJsonString) : {};
    const hasOptions = question.Options && question.Options.length > 0
    const isSingleSliderType = question.QuestionTypeId === 3
    const isRangeSliderType = question.QuestionTypeId === 4
    const isInputType = question.QuestionTypeId === 5
    const isRatingType = question.QuestionTypeId === 6
    return (
        <div key={question.Id} className="mb-4">
            <div className="bg-secondary p-3 text-white fw-bold border border-secondary" >
                Question {index + 1}
            </div>

            <div className="bg-white p-4 border border-top-0">
                <div className="mb-3">
                    <label className="form-label fw-bold">Question</label>
                    <input
                        value={question.Content}
                        readOnly
                        className="form-control bg-light"
                    />
                </div>

                <div className="mb-3">
                    <label className="form-label">Description</label>
                    <textarea
                        value={question.Description || ""}
                        readOnly
                        className="form-control bg-light"
                        style={{ minHeight: '60px' }}
                    />
                </div>

                <div className="d-flex gap-2 mb-3">
                    <span className="badge bg-primary">
                        {getQuestionTypeName(question.QuestionTypeId)}
                    </span>
                    {config.fieldInputTypeId && (
                        <span className="badge bg-success">
                            {getInputTypeName(config.fieldInputTypeId)}
                        </span>
                    )}

                </div>

                {hasOptions && (
                    <div className="mb-3">
                        <label className="form-label">Choices</label>
                        <div className="d-flex flex-column gap-2">
                            {question.Options?.sort((a, b) => a.Order - b.Order).map((option) => (
                                <input
                                    key={option.Id}
                                    value={option.Content}
                                    readOnly
                                    className="form-control bg-light"
                                />
                            ))}
                        </div>
                    </div>
                )}

                {(isSingleSliderType || isRangeSliderType || isRatingType || isInputType) && (
                    <div className="mb-3">
                        {isSingleSliderType && (
                            <div>
                                <Slider
                                    min={config.min}
                                    max={config.max}
                                    step={config.step}
                                    defaultValue={config.min || 0}
                                    valueLabelDisplay="auto"
                                />
                                <div className="d-flex align-items-center gap-3 text-secondary mt-2">
                                    {config.min !== null && (
                                        <div className="border rounded px-3 py-1">
                                            <small>Min:</small> {config.min}
                                        </div>
                                    )}
                                    {config.max !== null && (
                                        <div className="border rounded px-3 py-1">
                                            <small>Max:</small> {config.max}
                                        </div>
                                    )}
                                    {config.step && (
                                        <div className="border rounded px-3 py-1">
                                            <small>Step:</small> {config.step}
                                        </div>
                                    )}
                                    {config.unit && (
                                        <div className="border rounded px-3 py-1">
                                            <small>Unit:</small> {config.unit}
                                        </div>
                                    )}
                                </div>
                            </div>
                        )}
                        {isRangeSliderType && (
                            <div>
                                <Slider
                                    min={config.min}
                                    max={config.max}
                                    step={config.step}
                                    value={[
                                        config.min || 0,
                                        config.max || 100
                                    ]}
                                    valueLabelDisplay="auto"
                                />
                                <div className="d-flex align-items-center gap-3 text-secondary mt-2">
                                    {config.min !== null && (
                                        <div className="border rounded px-3 py-1">
                                            <small>Min:</small> {config.min}
                                        </div>
                                    )}
                                    {config.max !== null && (
                                        <div className="border rounded px-3 py-1">
                                            <small>Max:</small> {config.max}
                                        </div>
                                    )}
                                    {config.step && (
                                        <div className="border rounded px-3 py-1">
                                            <small>Step:</small> {config.step}
                                        </div>
                                    )}
                                    {config.unit && (
                                        <div className="border rounded px-3 py-1">
                                            <small>Unit:</small> {config.unit}
                                        </div>
                                    )}
                                </div>
                            </div>
                        )}
                        {isRatingType && (
                            <div>
                                <Rating
                                    name="simple-controlled"
                                    max={config.ratingLength || 5}
                                    value={5}
                                    icon={<FavoriteIcon />}
                                    readOnly
                                />
                            </div>
                        )}
                        {isInputType && (
                            <div className="text-area w-full">
                                <TextField
                                    disabled
                                    fullWidth
                                    multiline
                                    rows={3}
                                    placeholder="Vui lòng nhập tại đây"
                                    variant="outlined"
                                    size="small"
                                />
                            </div>
                        )}
                    </div>
                )}
            </div>
        </div>
    )
}
const PreviewQuestionsTab = () => {
    const [questionTypes, setQuestionTypes] = useState<QuestionType[]>([])
    const [fieldInputTypes, setFieldInputTypes] = useState<FieldInputType[]>([])
    const context = useContext(CommunitySurveyDetailViewContext)
    const questions = context?.surveyData?.Questions.sort((a, b) => a.Order - b.Order)

    const getQuestionTypeName = (typeId?: number): string => {
        const type = questionTypes.find((type: QuestionType) => type.Id === typeId);
        return type ? String(type.Name) : "Unknown";
    };

    const getInputTypeName = (typeId?: number): string => {
        const type = fieldInputTypes.find((type: FieldInputType) => type.Id === typeId);
        return type ? String(type.Name) : "Unknown";
    };

    useEffect(() => {
        const fetchData = async () => {
            const [questionTypesResponse, fieldInputTypesResponse] = await Promise.all([
                getQuestionTypes(managerAxiosInstance),
                getFieldInputTypes(managerAxiosInstance),
            ]);

            if (questionTypesResponse.success && questionTypesResponse.data) {
                setQuestionTypes(questionTypesResponse.data);
            } else {
                console.error('Lỗi khi fetch question types:', questionTypesResponse.message);
            }

            if (fieldInputTypesResponse.success && fieldInputTypesResponse.data) {
                setFieldInputTypes(fieldInputTypesResponse.data);
            } else {
                console.error('Lỗi khi fetch field input types:', fieldInputTypesResponse.message);
            }
        };

        fetchData();
    }, []);

   return (
    <div className=" mx-0 p-6">
        {questions && questions.length > 0 ? (
            questions.map((question: Question, index: number) =>
                renderQuestionPreview(question, index, getQuestionTypeName, getInputTypeName)
            )
        ) : (
            <div className="text-center text-danger">Không có câu hỏi nào để hiển thị.</div>
        )}
    </div>
)
}
export default PreviewQuestionsTab
