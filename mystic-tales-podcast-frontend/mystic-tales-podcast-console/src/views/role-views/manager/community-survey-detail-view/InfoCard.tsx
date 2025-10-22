import type React from "react"
import { useContext } from "react";
import { CommunitySurveyDetailViewContext } from ".";
import { formatDate } from "../../../../core/utils/date.util";

interface InfoCardProps { }

export const InfoCard: React.FC<InfoCardProps> = () => {
    const context = useContext(CommunitySurveyDetailViewContext);
    const survey = context?.surveyData;
    if (!survey) return null;


    return (
        <div className="survey-info-card">
            <h5 className="fw-bold mb-4"> {survey.Title}</h5>

            <div className="info-row">
                <span className="label">Requester ID</span>
                <span className="value">{survey.SurveyPrivateData?.RequesterId}</span>
            </div>

            <div className="info-row">
                <span className="label">Question Count</span>
                <span className="value">{survey.QuestionCount}</span>
            </div>

            {/* <div className="info-row">
                <span className="label">Publish Date</span>
                <span className="value">{formatDate(survey.PublishedAt)}</span>
            </div> */}

            <div className="info-row">
                <span className="label">Theory Price</span>
                <span className="value">{survey.SurveyPrivateData?.TheoryPrice}đ</span>
            </div>

            <div className="info-row">
                <span className="label">Base Price</span>
                <span className="value">{survey.TakerBaseRewardPrice}đ</span>
            </div>

            <div className="info-row">
                <span className="label">Extra Price</span>
                <span className="value">{survey.SurveyPrivateData?.ExtraPrice}đ</span>
            </div>
        </div>
    )
}
