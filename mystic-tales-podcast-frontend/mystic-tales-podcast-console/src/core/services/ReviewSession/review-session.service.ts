import { AxiosInstance } from "axios";
import { callAxiosRestApi } from "../../api/rest-api/main/api-call";

const BASE_URL = "podcast-service/api/review-sessions/episode-publish";

export const getEpisodePublishList = async (instance: AxiosInstance) => {

    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${BASE_URL}`,
    });

    return response;
}

export const getEpisodePublishDetail = async (instance: AxiosInstance, id: Number) => {

    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${BASE_URL}/${id}`,
    });

    return response;
}

export const createEditRequire = async (instance: AxiosInstance, id: Number,
    payload: {
        EpisodePublishReviewSessionUpdateInfo: {
            Note: string,
            PodcastIllegalContentTypeIds: number[],
        }
    } | any) => {

    const response = await callAxiosRestApi({
        instance: instance,
        method: "put",
        url: `${BASE_URL}/${id}/edit-require`,
        data: payload
    });

    return response;
};
export const acceptEpisodePublish = async (instance: AxiosInstance, id: Number, isAccepted : boolean,
    payload: {
        EpisodePublishReviewSessionUpdateInfo: {
            Note: string,
            PodcastIllegalContentTypeIds: number[],
        }
    } | any) => {

    const response = await callAxiosRestApi({
        instance: instance,
        method: "put",
        url: `${BASE_URL}/${id}/accept/${isAccepted}`,
        data: payload
    });

    return response;
};
