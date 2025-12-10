import { AxiosInstance } from "axios";
import { callAxiosRestApi } from "../../api/rest-api/main/api-call";

const BASE_URL = "moderation-service/api";

export const getEpisodeReports = async (instance: AxiosInstance) => {

    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${BASE_URL}/episode-reports`,
    });

    return response;
}
export const getEpisodeReviewSession = async (instance: AxiosInstance) => {

    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${BASE_URL}/episode-reports/episode-report-review-sessions`,
    });

    return response;
}

export const getEpisodeReviewSessionDetail = async (instance: AxiosInstance, id: string) => {
    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${BASE_URL}/episode-reports/episode-report-review-sessions/${id}`,
    });

    return response;
}

export const resolveEpisodeReviewSession = async (instance: AxiosInstance, id: string, isResolved: boolean) => {

    const response = await callAxiosRestApi({
        instance: instance,
        method: "post",
        url: `${BASE_URL}/episode-reports/episode-report-review-sessions/${id}/resolve/${isResolved}`,
    });

    return response;
}