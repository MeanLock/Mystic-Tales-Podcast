import { AxiosInstance } from "axios";
import { callAxiosRestApi } from "../../../api/rest-api/main/api-call";

const BASE_URL = "Report/survey";

export const getSummaryCountCommunitySurvey= async (instance: AxiosInstance, reportPeriod: string) => {
    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${BASE_URL}/community/summary-count?report_period=${reportPeriod}`,
    });
    return response;
}
