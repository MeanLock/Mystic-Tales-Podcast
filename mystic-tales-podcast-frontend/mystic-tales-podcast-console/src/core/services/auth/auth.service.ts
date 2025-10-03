import { ApolloClient } from "@apollo/client";
import { CHAT_MESSAGES_BY_1V1, CHAT_ROOMS_BY_ACCOUNT_ID } from "../../graphql/query/chat.query";
import { SEND_MESSAGE } from "../../graphql/mutation/chat.mutation";
import { AxiosInstance } from "axios";
import { callGraphQLMutation, callGraphQLQuery } from "../../api/graphql/main/api-call";
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
        url: "/User/auth/login-manual",
        data: bodyData,
    }, "Đăng nhập");

    return response;
}