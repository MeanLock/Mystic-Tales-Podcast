
import { AxiosInstance } from "axios";
import { callAxiosRestApi } from "../../api/rest-api/main/api-call";

const BASE_URL = "podcast-service/api/channels";

export const getChannelList = async (instance: AxiosInstance ) => {
    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${BASE_URL}/me`,
    }, "");

    return response;
}
export const getChannelDetail = async (instance: AxiosInstance, channelId: string ) => {
    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${BASE_URL}/me/${channelId}`,
    }, "");

    return response;
}
export const createChannel = async (instance: AxiosInstance,
    payload: {
        ChannelCreateInfo: {
            Name: string,
            Description: string,
            PodcasterId: number,
            PodcastCategoryId: number,
            PodcastSubCategoryId: number,
            HashtagIds: number[],
        }
        MainImageFile?: File
    } | any) => {

    const formData = new FormData();
    formData.append("ChannelCreateInfo", JSON.stringify(payload.ChannelCreateInfo));
    if (payload.MainImageFile) {
        formData.append("MainImageFile", payload.MainImageFile);
    }

    const response = await callAxiosRestApi({
        instance: instance,
        method: "post",
        url: `${BASE_URL}`,
        data: formData
    });

    return response;
};
export const updateChannel = async (instance: AxiosInstance,
    channelId: string,
    payload: {
        ChannelUpdateInfo: {
            Name: string,
            Description: string,
            PodcasterId: number,
            PodcastCategoryId: number,
            PodcastSubCategoryId: number,
            HashtagIds: number[],
        }
        MainImageFile?: File
    } | any) => {

    const formData = new FormData();
    formData.append("ChannelUpdateInfo", JSON.stringify(payload.ChannelUpdateInfo));
    if (payload.MainImageFile) {
        formData.append("MainImageFile", payload.MainImageFile);
    }

    const response = await callAxiosRestApi({
        instance: instance,
        method: "put",
        url: `${BASE_URL}/${channelId}`,
        data: formData
    });

    return response;
};
export const publishChannel = async (instance: AxiosInstance, channelId: string, isPublish: boolean) => {
    const response = await callAxiosRestApi({
        instance: instance,
        method: "put",
        url: `${BASE_URL}/${channelId}/publish/${isPublish}`,
    });

    return response;
};
export const deleteChannel = async (instance: AxiosInstance,
    channelId: string,
    payload: {
        ChannelDeletionOptions: {
          KeptShowIds: number[] 
        }
    } | any) => {
    const response = await callAxiosRestApi({
        instance: instance,
        method: "delete",
        url: `${BASE_URL}/${channelId}`,
        data: payload
    });

    return response;
};