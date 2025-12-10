
import { AxiosInstance } from "axios";
import { callAxiosRestApi } from "../../api/rest-api/main/api-call";

const BASE_URL = "transaction-service/api/account-balance-transactions";

export const withdrawalSubscription = async (instance: AxiosInstance, amount: Number ) => {

    const payload = {
        Amount: amount
    }
    const response = await callAxiosRestApi({
        instance: instance,
        method: "post",
        url: `${BASE_URL}/balance-withdrawal`,
        data: payload,
    }, "");

    return response;
}
export const getHistoryWithdrawal = async (instance: AxiosInstance ) => {

    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${BASE_URL}/balance-withdrawal-request`,
    }, "");

    return response;
}