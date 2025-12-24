
import { AxiosInstance } from "axios";
import { callAxiosRestApi } from "../../api/rest-api/main/api-call";

const BASE_URL = "podcast-service/api/episodes";


export const getEpisodeDetail = async (instance: AxiosInstance, episodeId: string) => {
    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${BASE_URL}/me/${episodeId}`,
    }, "");

    return response;
}
export const getLicenses = async (instance: AxiosInstance, episodeId: string) => {
    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${BASE_URL}/${episodeId}/licenses`,
    }, "");

    return response;
}
export const getLicenseFile= async (instance: AxiosInstance, fileKey: string) => {

    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${BASE_URL}/license-document/get-file-url/${fileKey}`,
    });

    return response;
}
export const getLicenseTypes = async (instance: AxiosInstance) => {
    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${BASE_URL}/podcast-episode-license-types`,
    }, "");

    return response;
}
export const uploadLicense = async (instance: AxiosInstance, episodeId: string,
    payload: {
        LicenseDocumentFiles: File[],
    }) => {

    const formData = new FormData();

    if (payload.LicenseDocumentFiles && payload.LicenseDocumentFiles.length > 0) {
        payload.LicenseDocumentFiles.forEach(file => {
            formData.append("LicenseDocumentFiles", file);
        });
    }
    const response = await callAxiosRestApi({
        instance: instance,
        method: "post",
        url: `${BASE_URL}/${episodeId}/licenses`,
        data: formData
    });

    return response;
};
export const deleteLicense = async (instance: AxiosInstance, episodeId: string, licenseId: string) => {

    const response = await callAxiosRestApi({
        instance: instance,
        method: "delete",
        url: `${BASE_URL}/${episodeId}/licenses/${licenseId}`,
    });

    return response;
};
export const createEpisode = async (instance: AxiosInstance,
    payload: {
        EpisodeCreateInfo: {
            Name: string,
            ExplicitContent: boolean,
            SeasonNumber: number,
            EpisodeOrder: number,
            HashtagIds: number[],
            PodcastShowId: string,
            Description: string,
            PodcastEpisodeSubscriptionTypeId: number,
        }
        MainImageFile?: File
    } | any) => {

    const formData = new FormData();
    formData.append("EpisodeCreateInfo", JSON.stringify(payload.EpisodeCreateInfo));
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
export const updateEpisode = async (instance: AxiosInstance, episodeId: string,
    payload: {
        EpisodeUpdateInfo: {
            Name: string,
            ExplicitContent: boolean,
            SeasonNumber: number,
            EpisodeOrder: number,
            HashtagIds: number[],
            Description: string,
            PodcastEpisodeSubscriptionTypeId: number,
        }
        MainImageFile?: File
    } | any) => {

    const formData = new FormData();
    formData.append("EpisodeUpdateInfo", JSON.stringify(payload.EpisodeUpdateInfo));
    if (payload.MainImageFile) {
        formData.append("MainImageFile", payload.MainImageFile);
    }

    const response = await callAxiosRestApi({
        instance: instance,
        method: "put",
        url: `${BASE_URL}/${episodeId}`,
        data: formData
    });

    return response;
};
export const deleteEpisode = async (instance: AxiosInstance, episodeId: string) => {

    const response = await callAxiosRestApi({
        instance: instance,
        method: "delete",
        url: `${BASE_URL}/${episodeId}`,
    });

    return response;
};
export const requestPublish = async (instance: AxiosInstance, episodeId: string) => {

    const response = await callAxiosRestApi({
        instance: instance,
        method: "post",
        url: `${BASE_URL}/${episodeId}/publish-review/create-request`,
    });

    return response;
};
export const publishEpisode = async (instance: AxiosInstance, episodeId: string, isPublish: boolean,
    payload?: {
        EpisodePublishInfo: {
            ReleaseDate: string;
        }
    } | null
) => {
    const response = await callAxiosRestApi({
        instance: instance,
        method: "put",
        url: `${BASE_URL}/${episodeId}/publish/${isPublish}`,
        data: payload ? payload : null,
    });

    return response;
};
export const discardPublishEpisode = async (instance: AxiosInstance, episodeId: string) => {
    const response = await callAxiosRestApi({
        instance: instance,
        method: "post",
        url: `${BASE_URL}/${episodeId}/publish-review/discard-request`,
    });

    return response;
};