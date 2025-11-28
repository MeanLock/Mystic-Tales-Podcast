import { AxiosInstance } from "axios";
import { callAxiosRestApi } from "../../../api/rest-api/main/api-call";

const BASE_URL = "Report/transaction";

export const getSummaryProfitTransactionReport = async (instance: AxiosInstance, reportPeriod: string) => {
    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${BASE_URL}/community/summary-profit?report_period=${reportPeriod}`,
    });

    return response;
}

export const getPeriodicProfitTransactionReport = async (instance: AxiosInstance, reportPeriod: string) => {
    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${BASE_URL}/community/periodic-profit?report_period=${reportPeriod}`,
    });

    return response;
}

export const getPeriodicAmountAccountBalance = async (instance: AxiosInstance, reportPeriod: string) => {
    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${BASE_URL}/account-balance/periodic-amount?report_period=${reportPeriod}`,
    });

    return response;
}

export const getSummaryCountAccountBalance = async (instance: AxiosInstance, reportPeriod: string) => {
    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${BASE_URL}/account-balance/summary-count?report_period=${reportPeriod}`,
    });

    return response;
}