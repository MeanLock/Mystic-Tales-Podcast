import { AxiosInstance } from "axios";
import { callAxiosRestApi } from "../../api/rest-api/main/api-call";

const BASE_URL = "user-service/api";

export const getCustomerAccounts = async (instance: AxiosInstance) => {

    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${BASE_URL}/accounts/customers`,
    });

    return response;
}
export const getProfile = async (instance: AxiosInstance) => {

    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${BASE_URL}/accounts/me`,
    });

    return response;
}
export const getPodcasterAccounts = async (instance: AxiosInstance) => {

    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${BASE_URL}/accounts/admin/podcasters`,
    });

    return response;
}
export const getStaffAccounts = async (instance: AxiosInstance) => {

    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${BASE_URL}/accounts/staffs`,
    });

    return response;
}
   
export const deactivateAccount = async (instance: AxiosInstance, accountId: number, isDeactivate: boolean) => {
    const response = await callAxiosRestApi({
        instance: instance,
        method: "put",
        url: `${BASE_URL}/accounts/${accountId}/deactivate/${isDeactivate}`,
    });

    return response;
};
export const updateLevel = async (instance: AxiosInstance, accountId: number, ViolationLevel: number) => {
    const response = await callAxiosRestApi({
        instance: instance,
        method: "put",
        url: `${BASE_URL}/accounts/${accountId}/violation-level/${ViolationLevel }`,
    });

    return response;
};

export const updateAccount = async (instance: AxiosInstance, accountId: number,
    payload: {
        AccountUpdateInfo: {
            FullName: string,
            Dob: string,
            Gender: string,
            Address: string,
            Phone: string,
        }
        MainImageFile?: File
    } | any) => {
    const formData = new FormData();
    formData.append("AccountUpdateInfo", JSON.stringify(payload.AccountUpdateInfo));
    if (payload.MainImageFile) {
        formData.append("MainImageFile", payload.MainImageFile);
    }

    const response = await callAxiosRestApi({
        instance: instance,
        method: "put",
        url: `${BASE_URL}/accounts/${accountId}`,
        data: formData
    });

    return response;
};
export const verifyPodcaster = async (instance: AxiosInstance, accountId: number, isverify: boolean) => {

    const response = await callAxiosRestApi({
        instance: instance,
        method: "put",
        url: `${BASE_URL}/accounts/podcasters/${accountId}/verify/${isverify}`,
    });

    return response;
};
