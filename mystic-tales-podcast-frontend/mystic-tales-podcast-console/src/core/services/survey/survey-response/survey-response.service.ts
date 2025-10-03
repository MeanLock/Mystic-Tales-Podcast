import { AxiosInstance } from "axios";
import { callAxiosRestApi } from "../../../api/rest-api/main/api-call";

const BASE_URL = "Survey/response";

export const getFilterSurveySummary = async (instance: AxiosInstance, SurveyId: Number) => {  
    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${BASE_URL}/filter/surveys/${SurveyId}`, 
    });

    return response;
};