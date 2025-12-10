import { AxiosInstance } from "axios";
import { callAxiosRestApi } from "../../api/rest-api/main/api-call";

const BASE_URL = "podcast-service/api/channels";

export const getChannelList = async (instance: AxiosInstance) => {

    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${BASE_URL}`,
    });

    return response;
}

export const getChannelDetail = async (instance: AxiosInstance, channelId: string) => {
    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${BASE_URL}/${channelId}`,
    }, "");

    return response;
}