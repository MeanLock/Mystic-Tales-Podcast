
import { AxiosInstance } from "axios";
import { callAxiosRestApi } from "../../api/rest-api/main/api-call";
import { AudioTuningRequest } from "@/core/types";

const BASE_URL = "podcast-service/api";

export const uploadAudio = async (instance: AxiosInstance, episodeId: string,
    payload: {
        AudioFile: File,
    }) => {

    const formData = new FormData();

    if (payload.AudioFile) {
        formData.append("AudioFile", payload.AudioFile);
    }

    const response = await callAxiosRestApi({
        instance: instance,
        method: "put",
        url: `${BASE_URL}/episodes/${episodeId}/audio`,
        data: formData
    });

    return response;
};


export const audioTuning = async (instance: AxiosInstance, episodeId: string, payload: AudioTuningRequest) => {
  let data: any;

  const { AudioFile, GeneralTuningProfileRequestInfo } = payload;

  if (AudioFile instanceof File) {
    const form = new FormData();
    form.append('GeneralTuningProfileRequestInfo', JSON.stringify(GeneralTuningProfileRequestInfo));
    form.append('AudioFile', AudioFile);
    data = form;
   } 

  const response = await callAxiosRestApi({
    instance,
    method: 'post',
    url: `${BASE_URL}/episodes/${episodeId}/audio-tuning/general`,
    data,
    config: {
        responseType: 'blob'
    }
  }, 'Audio Tuning');

  return response;
};

export const getBackgroundSounds = async (instance: AxiosInstance) => {

    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${BASE_URL}/background-sound-tracks`,
    });

    return response;
};

export const getBackgroundSoundFile = async (instance: AxiosInstance, fileKey: string) => {

    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${BASE_URL}/background-sound-tracks/get-file-url/${fileKey}`,
    });

    return response;
};

export const getAudioFile = async (instance: AxiosInstance, fileKey: string) => {

    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${BASE_URL}/episodes/audio/get-file-url/${fileKey}`,
    });

    return response;
};




