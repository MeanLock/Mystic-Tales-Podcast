
import { AxiosInstance } from "axios";
import { callAxiosRestApi } from "../../api/rest-api/main/api-call";

const BASE_URL = "podcast-service/api/shows";

export const getShowList = async (instance: AxiosInstance) => {
    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${BASE_URL}/me`,
    }, "");

    return response;
}
export const getShowDetail = async (instance: AxiosInstance, showId: string) => {
    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${BASE_URL}/me/${showId}`,
    }, "");

    return response;
}
export const getShowAssignableChannels = async (instance: AxiosInstance, showId: string) => {
    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${BASE_URL}/${showId}/show-assignable-channels`,
    }, "");

    return response;
}
export const assignChannel = async (instance: AxiosInstance, showId: string, payload: { PodcastChannelId: string }) => {
    const response = await callAxiosRestApi({
        instance: instance,
        method: "put",
        url: `${BASE_URL}/${showId}/assign-channel`,
        data: payload,
    }, "");

    return response;
}
export const createShow = async (instance: AxiosInstance,
    payload: {
        ShowCreateInfo: {
            Name: string,
            Copyright: string,
            PodcasterId: number,
            PodcastCategoryId: number,
            PodcastSubCategoryId: number,
            HashtagIds: number[],
            PodcastChannelId: string | null,
            Language: string,
            UploadFrequency: string,
            Description: string,
            PodcastShowSubscriptionTypeId: number,
        }
        MainImageFile?: File
    } | any) => {

    const formData = new FormData();
    formData.append("ShowCreateInfo", JSON.stringify(payload.ShowCreateInfo));
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
export const updateShow = async (instance: AxiosInstance,
    showId: string,
    payload: {
        ShowUpdateInfo: {
            Copyright: string,
            PodcastShowSubscriptionTypeId: number,
            Language: string,
            UploadFrequency: string,
            Name: string,
            Description: string,
            PodcastCategoryId: number,
            PodcastSubCategoryId: number,
            HashtagIds: number[],
        }
        MainImageFile?: File
    } | any) => {

    const formData = new FormData();
    formData.append("ShowUpdateInfo", JSON.stringify(payload.ShowUpdateInfo));
    if (payload.MainImageFile) {
        formData.append("MainImageFile", payload.MainImageFile);
    }

    const response = await callAxiosRestApi({
        instance: instance,
        method: "put",
        url: `${BASE_URL}/${showId}`,
        data: formData
    });

    return response;
};
export const publishShow = async (instance: AxiosInstance, showId: string, isPublish: boolean,
    payload?: {
        ShowPublishInfo: {
            ReleaseDate: string;
        }
    } | null
) => {
    const response = await callAxiosRestApi({
        instance: instance,
        method: "put",
        url: `${BASE_URL}/${showId}/publish/${isPublish}`,
        data: payload ? payload : null,
    });

    return response;
};
export const uploadTrailer = async (instance: AxiosInstance, showId: string, TrailerAudioFile: File) => {

    const form = new FormData();
    form.append('TrailerAudioFile', TrailerAudioFile);


    const response = await callAxiosRestApi({
        instance: instance,
        method: "put",
        url: `${BASE_URL}/${showId}/trailer-audio`,
        data: form,
    }, "");


    return response;
};
export const deleteShow = async (instance: AxiosInstance, showId: string) => {
    const response = await callAxiosRestApi({
        instance: instance,
        method: "delete",
        url: `${BASE_URL}/${showId}`,
    });

    return response;
};