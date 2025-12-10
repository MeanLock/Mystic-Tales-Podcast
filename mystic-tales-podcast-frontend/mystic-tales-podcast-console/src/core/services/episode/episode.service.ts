import { AxiosInstance } from "axios";
import { callAxiosRestApi } from "../../api/rest-api/main/api-call";

const BASE_URL = "podcast-service/api/episodes";

export const getAudioEpisode = async (instance: AxiosInstance, fileKey: string) => {

    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${BASE_URL}/audio/get-file-url/${fileKey}`,
    });

    return response;
}

export const getEpisodeDMCAList = async (instance: AxiosInstance) => {

    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${BASE_URL}/dmca-assignable`,
    });

    return response;
}


export const getEpisodeDetail = async (instance: AxiosInstance, id: string) => {

    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${BASE_URL}/${id}`,
    });

    return response;
}

