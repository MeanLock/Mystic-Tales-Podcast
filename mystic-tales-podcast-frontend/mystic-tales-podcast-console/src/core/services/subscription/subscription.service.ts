
import { AxiosInstance } from "axios";
import { callAxiosRestApi } from "../../api/rest-api/main/api-call";

const DASHBOARD_URL = "subscription-service/api/report/podcast-subscriptions";
const BASE_URL = "subscription-service/api/podcast-subscriptions";

export const getSummarySubscription = async (instance: AxiosInstance, reportPeriod: string) => {
    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${DASHBOARD_URL}/statistics/summary/system?report_period=${reportPeriod}`,
    });

    return response;
}
export const getTotalSubscription = async (instance: AxiosInstance, reportPeriod: string) => {
    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${DASHBOARD_URL}/statistics/summary/system/total?report_period=${reportPeriod}`,
    });

    return response;
}
export const getSubscriptionHoldingList = async (instance: AxiosInstance ) => {
    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${BASE_URL}/holding`,
    }, "");

    return response;
}