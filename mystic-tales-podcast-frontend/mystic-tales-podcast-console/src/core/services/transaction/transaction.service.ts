import { AxiosInstance } from "axios";
import { callAxiosRestApi } from "../../api/rest-api/main/api-call";

const BASE_URL = "transaction-service/api/account-balance-transactions";
const DASHBOARD_URL = "transaction-service/api/report/account-balance-transactions";

export const getTransactionList = async (instance: AxiosInstance) => {

    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${BASE_URL}/balance-withdrawal-request`,
    });

    return response;
}

export const getReceiptUrl = async (instance: AxiosInstance, fileKey: string) => {

    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${BASE_URL}/transfer-receipt-image/get-file-url/${fileKey}`,
    });

    return response;
}
export const confirmTransaction = async (
    instance: AxiosInstance,
    transactionId: string,
    isReject: boolean,
    payload: {
        TransferReceiptImageFile: File;
        AccountBalanceWithdrawalRequestInfo: {
            RejectedReason: string
        }
    }
) => {
    const formData = new FormData();
    formData.append("AccountBalanceWithdrawalRequestInfo", JSON.stringify(payload.AccountBalanceWithdrawalRequestInfo));
    if (!isReject && payload.TransferReceiptImageFile) {
        formData.append("TransferReceiptImageFile", payload.TransferReceiptImageFile);
    }

    const response = await callAxiosRestApi({
        instance: instance,
        method: "put",
        url: `${BASE_URL}/balance-withdrawal/${transactionId}/confirm/${isReject}`,
        data: formData,
    });

    return response;
}

export const getSummaryBalance = async (instance: AxiosInstance, reportPeriod: string) => {
    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${DASHBOARD_URL}/statistics/summary/system?report_period=${reportPeriod}`,
    });

    return response;
}