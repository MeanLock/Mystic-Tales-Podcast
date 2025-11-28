import { ApolloClient } from "@apollo/client";
import { CHAT_MESSAGES_BY_1V1, CHAT_ROOMS_BY_ACCOUNT_ID } from "../../graphql/query/chat.query";
import { SEND_MESSAGE } from "../../graphql/mutation/chat.mutation";
import { AxiosInstance } from "axios";
import { callGraphQLMutation, callGraphQLQuery } from "../../api/graphql/main/api-call";
import { callAxiosRestApi } from "../../api/rest-api/main/api-call";

const BASE_URL = "/User/accounts";

export const getCustomerAccounts = async (instance: AxiosInstance) => {

    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${BASE_URL}/customer`,
    });

    return response;
}

export const getManagerAccounts = async (instance: AxiosInstance) => {

    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${BASE_URL}/staff`,
    });

    return response;
}

export const deactivateAccount = async (instance: AxiosInstance, accountId: number, isDeactivate: boolean) => {
    const response = await callAxiosRestApi({
        instance: instance,
        method: "put",
        url: `${BASE_URL}/${accountId}/deactivate/${isDeactivate}`,
    });

    return response;
};

export const updateAccount = async (instance: AxiosInstance, accountId: number, request_body: {
    FullName: string,
    Dob: string,
    Gender: string,
    Address: string,
    Phone: string,
} | any) => {

    const response = await callAxiosRestApi({
        instance: instance,
        method: "put",
        url: `${BASE_URL}/${accountId}`,
        data: request_body
    });

    return response;
};