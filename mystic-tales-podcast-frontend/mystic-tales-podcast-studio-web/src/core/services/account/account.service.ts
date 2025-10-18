import { ApolloClient } from "@apollo/client";
import { CHAT_MESSAGES_BY_1V1, CHAT_ROOMS_BY_ACCOUNT_ID } from "../../graphql/query/chat.query";
import { SEND_MESSAGE } from "../../graphql/mutation/chat.mutation";
import { AxiosInstance } from "axios";
import { callGraphQLMutation, callGraphQLQuery } from "../../api/graphql/main/api-call";
import { callAxiosRestApi } from "../../api/rest-api/main/api-call";


export const login = async (instance: AxiosInstance, data : {
    username: string,
    password: string,
} | any) => {

    const bodyData = {
        login: {
            Email : data.username,
            Password : data.password,
        }
    }

    const response = await callAxiosRestApi({
        instance: instance,
        method: "post",
        url: "/User/auth/login",
        data: bodyData,
    }, "Đăng nhập");

    console.log("Rest API Response:", response);
    return response;
}