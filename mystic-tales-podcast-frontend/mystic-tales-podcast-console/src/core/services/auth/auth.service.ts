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
            Email: data.Email,
            Password: data.Password,
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

export const RegisterStaffAccount = async (instance: AxiosInstance,
    payload: {
        RegisterInfo: {
            Email: string,
            FullName: string,
            Password: string,
            Dob: string,
            Gender: string,
            Address: string,
            Phone: string,
        }
        MainImageFile?: File
    } | any) => {
    const formData = new FormData();
    formData.append("RegisterInfo", JSON.stringify(payload.RegisterInfo));
    if (payload.MainImageFile) {
        formData.append("MainImageFile", payload.MainImageFile);
    }

    const response = await callAxiosRestApi({
        instance: instance,
        method: "post",
        url: `${BASE_URL}/register/staff`,
        data: formData
    });

    return response;
};