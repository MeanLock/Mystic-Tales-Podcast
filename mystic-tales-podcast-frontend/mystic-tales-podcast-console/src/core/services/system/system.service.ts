import { AxiosInstance } from "axios";
import { callAxiosRestApi } from "../../api/rest-api/main/api-call";

const BASE_URL = "system-configuration-service/api";

export const getConfigList = async (instance: AxiosInstance) => {

    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${BASE_URL}/system-configs`,
    });

    return response;
}
export const getActiveConfig = async (instance: AxiosInstance) => {

    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${BASE_URL}/system-configs/active`,
    });

    return response;
}
export const getConfigDetail = async (instance: AxiosInstance, id: Number) => {

    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${BASE_URL}/system-configs/${id}`,
    });

    return response;
}
export const deleteConfig = async (instance: AxiosInstance, id: Number) => {

    const response = await callAxiosRestApi({
        instance: instance,
        method: "delete",
        url: `${BASE_URL}/system-configs/${id}`,
    });

    return response;
}


export const createConfig = async (instance: AxiosInstance, 
    payload: {
        SystemConfigCreateInfo: any
    } | any) => {
   
    const response = await callAxiosRestApi({
        instance: instance,
        method: "post",
        url: `${BASE_URL}/system-configs`,
        data: payload
    });

    return response;
};

export const activeConfig = async (instance: AxiosInstance, id: Number, IsActive: boolean) => {
    const response = await callAxiosRestApi({
        instance: instance,
        method: "put",
        url: `${BASE_URL}/system-configs/${id}/active/${IsActive}`,
    });

    return response;
}


