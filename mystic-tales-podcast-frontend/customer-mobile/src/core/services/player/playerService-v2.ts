import { loginRequiredAxiosInstance } from "../../api/appApiAxios/config/instances";
import { callAxiosRestApi } from "../../api/appApiAxios/index";
import {
  ListenSessionEpisodes,
  ListenSessionBookingTracks,
  ListenSessionProcedure,
} from "../../types/audio.type";
import * as SecureStore from "expo-secure-store";

type CurrentPodcastSubscriptionRegistrationBenefit = {
  Id: number;
  Name: string;
};

interface ListenToEpisodeResponse {
  isError: boolean;
  messageId: string;
  data: {
    ListenSession: ListenSessionEpisodes | null;
    ListenSessionProcedure: ListenSessionProcedure;
  } | null;
  missingBenefits?: string[];
}

function getMissingConditions(message: string): string[] {
  const key = "missing conditions:";

  const index = message.indexOf(key);
  if (index === -1) return [];

  return message
    .slice(index + key.length)
    .split(",")
    .map((item) => item.trim())
    .filter(Boolean);
}

export const listenToEpisodeV2 = async ({
  PodcastEpisodeId,
  SourceType,
  CurrentPodcastSubscriptionRegistrationBenefitList,
  continue_listen_session_id,
}: {
  PodcastEpisodeId: string;
  SourceType: "SpecifyShowEpisodes" | "SavedEpisodes";
  CurrentPodcastSubscriptionRegistrationBenefitList: CurrentPodcastSubscriptionRegistrationBenefit[];
  continue_listen_session_id?: string;
}): Promise<ListenToEpisodeResponse> => {
  let listenToEpisodeResponse: ListenToEpisodeResponse = {
    isError: true,
    messageId: "UnknownError",
    data: null,
  };

  try {
    const deviceToken = await SecureStore.getItemAsync("device_info_token");
    const response = await callAxiosRestApi({
      instance: loginRequiredAxiosInstance,
      method: "post",
      url: `/api/podcast-service/api/episodes/${PodcastEpisodeId}/listen${
        continue_listen_session_id
          ? `?continue_listen_session_id=${continue_listen_session_id}`
          : ""
      }`,
      data: {
        SourceType,
        CurrentPodcastSubscriptionRegistrationBenefitList,
      },
      config: {
        headers: {
          "X-DeviceInfo-Token": deviceToken,
        },
      },
    });
    if (response.success) {
      listenToEpisodeResponse = {
        isError: false,
        messageId: "Success",
        data: response.data,
      };
    } else {
      const content = response.message.content;
      const message =
        typeof content === "object" && content !== null
          ? content.message
          : content;

      if (message) {
        // Parse message string to get reason
        let errorReason = "";
        const messageText =
          typeof message === "string" ? message : message.message || "";

        // Extract reason from message string: "reason: xxxx"
        const reasonMatch = messageText.match(/reason:\s*([^,}]+)/i);
        if (reasonMatch && reasonMatch[1]) {
          errorReason = reasonMatch[1].trim();
        }

        console.log(">>>>>>>>>>>>messageText:", messageText);
        console.log(">>>>>>>>>>>>error reason:", errorReason);

        // Lỗi có message cụ thể từ server
        if (
          errorReason.includes("is not in Published status") ||
          messageText.includes("is not in Published status")
        ) {
          // Trường hợp episode đã bị gỡ
          listenToEpisodeResponse = {
            isError: true,
            messageId: "listen-failed-2",
            data: null,
          };
        } else if (
          errorReason.includes("no subscription registration") ||
          messageText.includes("no subscription registration")
        ) {
          // Trường hợp chưa đăng ký gói trả phí
          listenToEpisodeResponse = {
            isError: true,
            messageId: "listen-failed-3/4",
            data: null,
          };
        } else if (
          errorReason.includes("missing conditions") ||
          messageText.includes("missing conditions")
        ) {
          // Trường hợp thiếu điều kiện để nghe (gói mà người dùng đăng ký chưa đủ quyền)
          const missingBenefits = getMissingConditions(messageText);
          listenToEpisodeResponse = {
            isError: true,
            messageId: "listen-failed-5",
            missingBenefits: missingBenefits,
            data: null,
          };
        } else {
          // Lỗi khác có message nhưng không match pattern nào
          listenToEpisodeResponse = {
            isError: true,
            messageId: "listen-failed-1",
            data: null,
          };
        }
      } else {
        // Lỗi mà không có message cụ thể từ server
        listenToEpisodeResponse = {
          isError: true,
          messageId: "listen-failed-1",
          data: null,
        };
      }
    }
  } catch (error) {
    // Lỗi ngoại lệ
    console.error("listenToEpisode error:", error);
    listenToEpisodeResponse = {
      isError: true,
      messageId: "listen-failed-1",
      data: null,
    };
  }
  return listenToEpisodeResponse;
};

export const listenToBookingTrackV2 = async ({
  BookingId,
  BookingPodcastTrackId,
}: {
  BookingId: number;
  BookingPodcastTrackId: string;
}): Promise<{
  ListenSession: ListenSessionBookingTracks | null;
  ListenSessionProcedure: ListenSessionProcedure;
}> => {
  let listenToBookingTrackResponse: {
    ListenSession: ListenSessionBookingTracks | null;
    ListenSessionProcedure: ListenSessionProcedure;
  } = {
    ListenSession: null,
    ListenSessionProcedure: {} as ListenSessionProcedure,
  };
  try {
    const deviceToken =
      (await SecureStore.getItemAsync("device_info_token")) || "";
    const response = await callAxiosRestApi({
      instance: loginRequiredAxiosInstance,
      method: "post",
      url: `/api/booking-management-service/api/bookings/${BookingId}/booking-podcast-tracks/${BookingPodcastTrackId}/listen`,
      data: {
        SourceType: "BookingProducingTracks",
      },
      config: {
        headers: {
          "X-DeviceInfo-Token": deviceToken,
        },
      },
    });
    if (response.success) {
      listenToBookingTrackResponse = {
        ListenSession: response.data.ListenSession,
        ListenSessionProcedure: response.data.ListenSessionProcedure,
      };
    } else {
      console.log(
        ">>>>>>>>>>>>response.message:",
        (response.message.content?.message as unknown as { errors: any }).errors
      );
    }
  } catch (error) {
    console.log(">>>>>>>>>>>>error:", error);
    if (__DEV__) {
      console.error("listenToBookingTrackV2 error:", error);
    }
  }
  return listenToBookingTrackResponse;
};

export const getLatestEpisodeListenSessionV2 = async (): Promise<{
  ListenSession: ListenSessionEpisodes | null;
  ListenSessionProcedure: ListenSessionProcedure | null;
}> => {
  let latestEpisodeListenSessionResponse = {
    ListenSession: null,
    ListenSessionProcedure: null,
  };
  try {
    const deviceToken =
      (await SecureStore.getItemAsync("device_info_token")) || "";
    const response = await callAxiosRestApi({
      instance: loginRequiredAxiosInstance,
      method: "get",
      url: `/api/podcast-service/api/episodes/listen-sessions/latest`,
      config: {
        headers: {
          "X-DeviceInfo-Token": deviceToken,
        },
      },
    });
    if (response.success) {
      latestEpisodeListenSessionResponse = {
        ListenSession: response.data.ListenSession,
        ListenSessionProcedure: response.data.ListenSessionProcedure,
      };
    } else {
      if (__DEV__) {
        console.error(
          "getLatestEpisodeListenSessionV2 failed:",
          response.message
        );
      }
    }
  } catch (error) {
    if (__DEV__) {
      console.error("getLatestEpisodeListenSession error:", error);
    }
  }
  return latestEpisodeListenSessionResponse;
};

export const getLatestBookingListenSessionV2 = async (): Promise<{
  ListenSession: ListenSessionBookingTracks | null;
  ListenSessionProcedure: ListenSessionProcedure | null;
}> => {
  let latestBookingListenSessionResponse = {
    ListenSession: null,
    ListenSessionProcedure: null,
  };
  try {
    const deviceToken =
      (await SecureStore.getItemAsync("device_info_token")) || "";
    const response = await callAxiosRestApi({
      instance: loginRequiredAxiosInstance,
      method: "get",
      url: `/api/booking-management-service/api/bookings/listen-sessions/latest`,
      config: {
        headers: {
          "X-DeviceInfo-Token": deviceToken,
        },
      },
    });
    if (response.success) {
      console.log(">>>>>>>>>>>>>>>response.data:", response.data);
      latestBookingListenSessionResponse = {
        ListenSession: response.data.ListenSession,
        ListenSessionProcedure: response.data.ListenSessionProcedure,
      };
    } else {
      if (__DEV__) {
        console.error(
          "getLatestBookingListenSessionV2 failed:",
          response.message
        );
      }
    }
  } catch (error) {
    if (__DEV__) {
      console.error("getLatestBookingListenSession error:", error);
    }
  }
  return latestBookingListenSessionResponse;
};

export const navigateEpisodeInProcedureV2 = async ({
  ListenSessionNavigateType,
  ListenSessionId,
  ListenSessionProcedureId,
  CurrentPodcastSubscriptionRegistrationBenefitList,
}: {
  ListenSessionNavigateType: "Next" | "Previous";
  ListenSessionId: string;
  ListenSessionProcedureId: string;
  CurrentPodcastSubscriptionRegistrationBenefitList:
    | CurrentPodcastSubscriptionRegistrationBenefit[]
    | null;
}): Promise<{
  ListenSession: ListenSessionEpisodes | null;
  ListenSessionProcedure: ListenSessionProcedure | null;
}> => {
  let navigateEpisodeResponse: {
    ListenSession: ListenSessionEpisodes | null;
    ListenSessionProcedure: ListenSessionProcedure | null;
  } = {
    ListenSession: null,
    ListenSessionProcedure: null,
  };
  try {
    const deviceToken =
      (await SecureStore.getItemAsync("device_info_token")) || "";
    const response = await callAxiosRestApi({
      instance: loginRequiredAxiosInstance,
      method: "post",
      url: `/api/podcast-service/api/episodes/listen-sessions/navigate?listen_session_navigate_type=${ListenSessionNavigateType}`,
      data: {
        CurrentListenSession: {
          ListenSessionId,
          ListenSessionProcedureId,
        },
        CurrentPodcastSubscriptionRegistrationBenefitList,
      },
      config: {
        headers: {
          "X-DeviceInfo-Token": deviceToken,
        },
      },
    });
    if (response.success) {
      navigateEpisodeResponse = {
        ListenSession: response.data.ListenSession,
        ListenSessionProcedure: response.data.ListenSessionProcedure,
      };
    } else {
      if (__DEV__) {
        console.error("navigateEpisodeInProcedureV2 failed:", response.message);
      }
    }
  } catch (error) {
    if (__DEV__) {
      console.error("navigateEpisodeInProcedureV2 error:", error);
    }
  }
  return navigateEpisodeResponse;
};

export const navigateBookingTrackInProcedureV2 = async ({
  ListenSessionNavigateType,
  ListenSessionId,
  ListenSessionProcedureId,
}: {
  ListenSessionNavigateType: "Next" | "Previous";
  ListenSessionId: string;
  ListenSessionProcedureId: string;
}): Promise<{
  ListenSession: ListenSessionBookingTracks | null;
  ListenSessionProcedure: ListenSessionProcedure | null;
}> => {
  let navigateBookingTrackResponse: {
    ListenSession: ListenSessionBookingTracks | null;
    ListenSessionProcedure: ListenSessionProcedure | null;
  } = {
    ListenSession: null,
    ListenSessionProcedure: null,
  };
  try {
    const deviceToken =
      (await SecureStore.getItemAsync("device_info_token")) || "";
    const response = await callAxiosRestApi({
      instance: loginRequiredAxiosInstance,
      method: "post",
      url: `/api/booking-management-service/api/bookings/listen-sessions/navigate?listen_session_navigate_type=${ListenSessionNavigateType}`,
      data: {
        CurrentListenSession: {
          ListenSessionId,
          ListenSessionProcedureId,
        },
      },
      config: {
        headers: {
          "X-DeviceInfo-Token": deviceToken,
        },
      },
    });
    if (response.success) {
      navigateBookingTrackResponse = {
        ListenSession: response.data.ListenSession,
        ListenSessionProcedure: response.data.ListenSessionProcedure,
      };
    } else {
      if (__DEV__) {
        console.error(
          "navigateBookingTrackInProcedureV2 failed:",
          response.message
        );
      }
    }
  } catch (error) {
    if (__DEV__) {
      console.error("navigateBookingTrackInProcedureV2 error:", error);
    }
  }
  return navigateBookingTrackResponse;
};
