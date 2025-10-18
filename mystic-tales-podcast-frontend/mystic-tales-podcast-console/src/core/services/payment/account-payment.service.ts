
import { AxiosInstance } from "axios";
import { callAxiosRestApi } from "../../api/rest-api/main/api-call";

const BASE_URL = "/Payment/account";

export const getBalanceDepositHistory = async (instance: AxiosInstance) => {

    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${BASE_URL}/balance-deposits/history`,
    });

    return response;
}
