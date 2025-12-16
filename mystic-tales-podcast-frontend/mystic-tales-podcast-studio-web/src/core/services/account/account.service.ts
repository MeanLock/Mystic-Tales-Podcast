import { AxiosInstance } from "axios";
import { callAxiosRestApi } from "../../api/rest-api/main/api-call";

const BASE_URL = "user-service/api";

export const getAccountProfile = async (instance: AxiosInstance) => {
  const response = await callAxiosRestApi({
    instance: instance,
    method: "get",
    url: `${BASE_URL}/accounts/me`,
  }, "");

  return response;
}
export const getPodcasterProfile = async (instance: AxiosInstance, podcasterId: string) => {
  const response = await callAxiosRestApi({
    instance: instance,
    method: "get",
    url: `${BASE_URL}/accounts/podcaster/${podcasterId}`,
  }, "");

  return response;
}
export const updatePodcasterProfile = async (instance: AxiosInstance, accountId: string,
  payload: {
    PodcasterProfileUpdateInfo: {
      Name?: string;
      Description?: string;
      PricePerBookingWord?: number;
      IsBuddy?: boolean;
    }
    BuddyAudioFile?: File | null
  } | any
) => {
  const formData = new FormData();
  formData.append("PodcasterProfileUpdateInfo", JSON.stringify(payload.PodcasterProfileUpdateInfo));
  if (payload.BuddyAudioFile) {
    formData.append("BuddyAudioFile", payload.BuddyAudioFile);
  }

  const response = await callAxiosRestApi({
    instance: instance,
    method: "put",
    url: `${BASE_URL}/accounts/customer/podcaster/${accountId}`,
    data: formData
  }, "");

  return response;
}
