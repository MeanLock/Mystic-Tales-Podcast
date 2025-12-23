import { loginRequiredAxiosInstance } from "@/core/api/appApiAxios/config/instances";
import { callAxiosRestApi } from "@/core/api/appApiAxios/index";
import type {
  ListenSessionBookingTracks,
  ListenSessionEpisodes,
  ListenSessionProcedure,
} from "@/core/types/audio";

interface EpisodeLatestListenSessionEpisode {
  ListenSession: ListenSessionEpisodes | null;
  ListenSessionProcedure: ListenSessionProcedure | null;
}

interface BookingLatestListenSessionEpisode {
  ListenSession: ListenSessionBookingTracks | null;
  ListenSessionProcedure: ListenSessionProcedure | null;
}

export const getEpisodeLatestSession =
  async (): Promise<EpisodeLatestListenSessionEpisode> => {
    const response = await callAxiosRestApi({
      instance: loginRequiredAxiosInstance,
      method: "get",
      url: `/api/podcast-service/api/episodes/listen-sessions/latest`,
      config: {
        headers: {
          "X-DeviceInfo-Token": localStorage.getItem("device_info_token") || "",
        },
      },
    });
    return response.data;
  };

export const getBookingLatestSession =
  async (): Promise<BookingLatestListenSessionEpisode> => {
    const response = await callAxiosRestApi({
      instance: loginRequiredAxiosInstance,
      method: "get",
      url: `/api/booking-management-service/api/bookings/listen-sessions/latest`,
      config: {
        headers: {
          "X-DeviceInfo-Token": localStorage.getItem("device_info_token") || "",
        },
      },
    });
    return response.data;
};
