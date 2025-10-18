import { AxiosInstance } from "axios";
import { callAxiosRestApi } from "../../../api/rest-api/main/api-call";

const BASE_URL = "Survey/core";

export const getFilterSurveys = async (instance: AxiosInstance) => {

    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${BASE_URL}/filter/surveys`,
    });

    return response;
}
export const getCommunitySurveys = async (instance: AxiosInstance, deadlineQuery?: number) => {
    const url = deadlineQuery
        ? `${BASE_URL}/community/surveys?Deadline=${deadlineQuery}`
        : `${BASE_URL}/community/surveys`;

    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: url,
    });

    return response;
}
export const getCommunitySurveyDetail = async (instance: AxiosInstance, surveyId: number) => {

    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${BASE_URL}/community/surveys/${surveyId}`,
    });

    return response;
};
export const getFilterSurveyDetail = async (instance: AxiosInstance, surveyId: number) => {

    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${BASE_URL}/filter/surveys/${surveyId}`,
    });

    return response;
};

export const getQuestionTypes = async (instance: AxiosInstance) => {

    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${BASE_URL}/survey-question-types`,
    });

    return response;
}

export const getFieldInputTypes = async (instance: AxiosInstance) => {

    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${BASE_URL}/survey-field-input-types`,
    });

    return response;
}
export const updateMaxXpCommunitySurvey = async (instance: AxiosInstance, surveyId: number, maxXp: number) => {
    const request_body = {
        MaxXp: maxXp,
    };
    const response = await callAxiosRestApi({
        instance: instance,
        method: "put",
        url: `${BASE_URL}/community/surveys/${surveyId}`,
        data: request_body,
    });

    return response;
};

export const deleteSurvey = async (instance: AxiosInstance, surveyId: number) => {
    const response = await callAxiosRestApi({
        instance: instance,
        method: "delete",
        url: `${BASE_URL}/surveys/${surveyId}`,
    });

    return response;
};