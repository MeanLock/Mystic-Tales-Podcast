import { ApolloClient } from "@apollo/client";
import { CHAT_MESSAGES_BY_1V1, CHAT_ROOMS_BY_ACCOUNT_ID } from "../../graphql/query/chat.query";
import { SEND_MESSAGE } from "../../graphql/mutation/chat.mutation";
import { AxiosInstance } from "axios";
import { callGraphQLMutation, callGraphQLQuery } from "../../api/graphql/main/api-call";
import { callAxiosRestApi } from "../../api/rest-api/main/api-call";


export const login = async (instance: AxiosInstance, data: {
    username: string,
    password: string,
} | any) => {

    const bodyData = {
        login: {
            Email: data.username,
            Password: data.password,
        }
    }

    const response = await callAxiosRestApi({
        instance: instance,
        method: "post",
        url: "/User/auth/login",
        data: bodyData,
    }, "Đăng nhập");

    console.log("Rest API Response:", response);
    return response;
}

export const AudioTuning = async (instance: AxiosInstance, payload: AudioTuningRequest) => {
  let data: any;

  const { AudioFile, GeneralTuningProfileRequestInfo } = payload;

  if (AudioFile instanceof File) {
    const form = new FormData();
    form.append('GeneralTuningProfileRequestInfo', JSON.stringify(GeneralTuningProfileRequestInfo));
    form.append('AudioFile', AudioFile);
    data = form;
   } 
  console.log("Audio Tuning Request Data:", data);

  const response = await callAxiosRestApi({
    instance,
    method: 'post',
    url: '/api/podcast-service/api/episodes/3b16bc12-715c-472b-a73e-f53b3ed686bf/audio-tuning/general',
    data,
    config: {
        responseType: 'blob'
    }
  }, 'Audio Tuning');

  return response;
};
export interface AudioTuningRequest {
  GeneralTuningProfileRequestInfo: {
    EqualizerProfile: {
      ExpandEqualizer: { Mood: string };
      BaseEqualizer: {
        HighMid: number;
        Mid: number;
        Air: number;
        LowMid: number;
        Treble: number;
        Low: number;
        Presence: number;
        SubBass: number;
        Bass: number;
      };
    };
    BackgroundMergeProfile: {
        BackgroundSoundTrackFileKey: string ;
        VolumeGainDb: number;
    }
    AITuningProfile: null
  };
  AudioFile?:  File;
}