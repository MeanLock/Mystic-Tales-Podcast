
import { AxiosInstance } from "axios";
import { callAxiosRestApi } from "../../api/rest-api/main/api-call";

const BASE_URL = "subscription-service/api/podcast-subscriptions";
const DASHBOARD_URL = "subscription-service/api/report/podcast-subscriptions";

export const getSubscriptionDetail = async (instance: AxiosInstance, subscriptionId: Number ) => {
    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${BASE_URL}/${subscriptionId}`,
    }, "");

    return response;
}
export const getShowSubscriptionTransaction = async (instance: AxiosInstance, showId: string ) => {
    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${DASHBOARD_URL}/shows/${showId}/dashboard`,
    }, "");

    return response;
}
export const getChannelSubscriptionTransaction = async (instance: AxiosInstance, channelId: string ) => {
    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${DASHBOARD_URL}/channels/${channelId}/dashboard`,
    }, "");

    return response;
}
export const getSummarySubscription = async (instance: AxiosInstance, reportPeriod: string) => {
    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${DASHBOARD_URL}/statistics/summary/me?report_period=${reportPeriod}`,
    });

    return response;
}

export const addChannelSubscription = async (instance: AxiosInstance, channelId: string, 
    payload: {
        PodcastSubscriptionCreateInfo:{
            Name: string;
            Description: string;
            PodcastSubscriptionCycleTypePriceCreateInfoList: {
                SubscriptionCycleTypeId: number;
                Price: number;
            }[];
            PodcastSubscriptionBenefitMappingCreateInfoList: Number[];
        }
    } ) => {

    const response = await callAxiosRestApi({
        instance: instance,
        method: "post",
        url: `${BASE_URL}/channels/${channelId}`,
        data: payload,
    }, "");

    return response;
}
export const addShowSubscription = async (instance: AxiosInstance, showId: string, 
    payload: {
        PodcastSubscriptionCreateInfo:{
            Name: string;
            Description: string;
            PodcastSubscriptionCycleTypePriceCreateInfoList: {
                SubscriptionCycleTypeId: number;
                Price: number;
            }[];
            PodcastSubscriptionBenefitMappingCreateInfoList: Number[];
        }
    } ) => {

    const response = await callAxiosRestApi({
        instance: instance,
        method: "post",
        url: `${BASE_URL}/shows/${showId}`,
        data: payload,
    }, "");

    return response;
}
export const updateSubscription = async (instance: AxiosInstance, subscriptionId: string, 
    payload: {
        PodcastSubscriptionUpdateInfo:{
            Name: string;
            Description: string;
            PodcastSubscriptionCycleTypePriceUpdateInfoList: {
                SubscriptionCycleTypeId: number;
                Price: number;
            }[];
            PodcastSubscriptionBenefitMappingUpdateInfoList: Number[];
        }
    } ) => {

    const response = await callAxiosRestApi({
        instance: instance,
        method: "put",
        url: `${BASE_URL}/${subscriptionId}`,
        data: payload,
    }, "");

    return response;
}
export const activeSubscription = async (instance: AxiosInstance, subscriptionId: string, isActive: boolean) => {

    const response = await callAxiosRestApi({
        instance: instance,
        method: "put",
        url: `${BASE_URL}/${subscriptionId}/active/${isActive}`,
    }, "");

    return response;
}
export const deleteSubscription = async (instance: AxiosInstance, subscriptionId: string) => {

    const response = await callAxiosRestApi({
        instance: instance,
        method: "delete",
        url: `${BASE_URL}/${subscriptionId}`,
    }, "");

    return response;
}