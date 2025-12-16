import { AxiosInstance } from "axios";
import { callAxiosRestApi } from "../../api/rest-api/main/api-call";

const BASE_URL = "moderation-service/api";

export const getShowReports = async (instance: AxiosInstance) => {

    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${BASE_URL}/show-reports`,
    });

    return response;
}
export const getShowReviewSession = async (instance: AxiosInstance) => {

    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${BASE_URL}/show-reports/show-report-review-sessions`,
    });

    return response;
}
export const getShowReviewSessionDetail = async (instance: AxiosInstance, id: string) => {
    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${BASE_URL}/show-reports/show-report-review-sessions/${id}`,
    });

    return response;
}

export const resolveShowReviewSession = async (instance: AxiosInstance, id: string, isResolved: boolean) => {

    const response = await callAxiosRestApi({
        instance: instance,
        method: "post",
        url: `${BASE_URL}/show-reports/show-report-review-sessions/${id}/resolve/${isResolved}`,
    });

    return response;
}