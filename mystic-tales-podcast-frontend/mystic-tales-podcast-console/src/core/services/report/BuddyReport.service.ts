import { AxiosInstance } from "axios";
import { callAxiosRestApi } from "../../api/rest-api/main/api-call";

const BASE_URL = "moderation-service/api";

export const getBuddyReports = async (instance: AxiosInstance) => {

    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${BASE_URL}/buddy-reports`,
    });

    return response;
}
export const getBuddyReviewSession = async (instance: AxiosInstance) => {

    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${BASE_URL}/buddy-reports/buddy-report-review-sessions`,
    });

    return response;
}
export const getBuddyReviewSessionDetail = async (instance: AxiosInstance, id: string) => {
    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${BASE_URL}/buddy-reports/buddy-report-review-sessions/${id}`,
    });

    return response;
}
export const resolveBuddyReviewSession = async (instance: AxiosInstance, id: string, isResolved: boolean, resolvedViolationPoint: Number) => {
   const payload = {
     ResolvedViolationPoint: resolvedViolationPoint,
   }
    const response = await callAxiosRestApi({
        instance: instance,
        method: "post",
        url: `${BASE_URL}/buddy-reports/buddy-report-review-sessions/${id}/resolve/${isResolved}`,
        data: payload
    });

    return response;
}

