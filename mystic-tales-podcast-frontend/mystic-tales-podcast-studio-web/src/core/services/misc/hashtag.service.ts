import { AxiosInstance } from "axios";
import { callAxiosRestApi } from "../../api/rest-api/main/api-call";

const BASE_URL = "podcast-service/api/hash-tags";

export const getHashtags = async (instance: AxiosInstance, keyword: string) => {

    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${BASE_URL}?keyword=${keyword}`,
    });

    return response;
}
export const createHashtag = async (instance: AxiosInstance, payload: {HashtagName: string}) => {

    const response = await callAxiosRestApi({
        instance: instance,
        method: "post",
        url: `${BASE_URL}`,
        data: payload
    });

    return response;
}
