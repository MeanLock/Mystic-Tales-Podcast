import { AxiosInstance } from "axios";
import { callAxiosRestApi } from "../../api/rest-api/main/api-call";


export const login = async (instance: AxiosInstance, data : {
    Email: string,
    Password: string,
} | any) => {

    const bodyData = {
        LoginInfo: {
            Email : data.email,
            Password : data.password,
        }
    }

    const response = await callAxiosRestApi({
        instance: instance,
        method: "post",
        url: "user-service/api/auth/login-manual",
        data: bodyData,
    }, "Sign in");

    return response;
}