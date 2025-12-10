import { AxiosInstance } from "axios";
import { callAxiosRestApi } from "../../api/rest-api/main/api-call";

const BASE_URL = "podcast-service/api/background-sound-tracks";

export const getAudioBackgroundSound = async (instance: AxiosInstance, fileKey: string) => {

    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${BASE_URL}/get-file-url/${fileKey}`,
    });

    return response;
}

export const getBackgroundSoundTracks = async (instance: AxiosInstance) => {

    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${BASE_URL}`,
    });

    return response;
}


export const addBackgroundSoundTracks = async (instance: AxiosInstance,
    payload: {
        BackgroundSoundTrackCreateInfo: {
            Name: string,
            Description: string
        }
        MainImageFile?: File
        AudioFile?: File
    } | any) => {
    const formData = new FormData();
    formData.append("BackgroundSoundTrackCreateInfo", JSON.stringify(payload.BackgroundSoundTrackCreateInfo));
    if (payload.MainImageFile) {
        formData.append("MainImageFile", payload.MainImageFile);
    }
    if (payload.AudioFile) {
        formData.append("AudioFile", payload.AudioFile);
    }

    const response = await callAxiosRestApi({
        instance: instance,
        method: "post",
        url: `${BASE_URL}`,
        data: formData
    });

    return response;
};

export const updateBackgroundSoundTracks = async (instance: AxiosInstance,id: string,
    payload: {
        BackgroundSoundTrackUpdateInfo: {
            Name: string,
            Description: string
        }
        MainImageFile?: File
        AudioFile?: File
    } | any) => {
    const formData = new FormData();
    formData.append("BackgroundSoundTrackUpdateInfo", JSON.stringify(payload.BackgroundSoundTrackUpdateInfo));
    if (payload.MainImageFile) {
        formData.append("MainImageFile", payload.MainImageFile);
    }
    if (payload.AudioFile) {
        formData.append("AudioFile", payload.AudioFile);
    }

    const response = await callAxiosRestApi({
        instance: instance,
        method: "put",
        url: `${BASE_URL}/${id}`,
        data: formData
    });

    return response;
};
export const deleteBackgroundSoundTracks = async (instance: AxiosInstance,id: string) => {

    const response = await callAxiosRestApi({
        instance: instance,
        method: "delete",
        url: `${BASE_URL}/${id}`,
    });

    return response;
};
