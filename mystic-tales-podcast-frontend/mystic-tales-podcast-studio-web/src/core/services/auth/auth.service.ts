import { AxiosInstance } from "axios";
import { callAxiosRestApi } from "../../api/rest-api/main/api-call";

const BASE_URL = "user-service/api/auth";

export const login = async (instance: AxiosInstance, data: {
    Email: string,
    Password: string,
    DeviceInfo: {
        DeviceId: string,
        Platform: string,
        OSName: string
    }
} | any) => {

    const bodyData = {
        ManualLoginInfo: {
            Email: data.email,
            Password: data.password,
        },
         DeviceInfo: {
            DeviceId: data.DeviceInfo.DeviceId,
            Platform: data.DeviceInfo.Platform,
            OSName: data.DeviceInfo.OSName
        }
    }

    const response = await callAxiosRestApi({
        instance: instance,
        method: "post",
        url: `${BASE_URL}/login-manual`,
        data: bodyData,
    }, "Sign in");

    return response;
}

export const loginGoogle = async (instance: AxiosInstance, data: {
    AuthorizationCode: string,
    RedirectUri: string,
} | any) => {

    const bodyData = {
        GoogleAuth: {
            AuthorizationCode: data.AuthorizationCode,
            RedirectUri: data.RedirectUri,
        },
        DeviceInfo: {
            DeviceId: data.DeviceInfo.DeviceId,
            Platform: data.DeviceInfo.Platform,
            OSName: data.DeviceInfo.OSName
        }
    }

    const response = await callAxiosRestApi({
        instance: instance,
        method: "post",
        url: `${BASE_URL}/login-google`,
        data: bodyData,
    }, "Sign in");

    return response;
}