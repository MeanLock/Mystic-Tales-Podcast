import { AxiosInstance } from "axios";
import { callAxiosRestApi } from "../../../api/rest-api/main/api-call";

const BASE_URL = "Report/user";

export const getSummaryCountAccountRegistration = async (instance: AxiosInstance, reportPeriod: string) => {
    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${BASE_URL}/account-registration/summary-count?report_period=${reportPeriod}`,
    });

    return response;
}
export const getPlatformFeedback = async (instance: AxiosInstance, reportPeriod?: string) => {
    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${BASE_URL}/platform-feedback?report_period=${reportPeriod}`,
    });

    return response;
}
