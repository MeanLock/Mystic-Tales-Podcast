// @ts-nocheck
import { appApi } from "@/core/api/appApi";
import type { AccountMeFromApi } from "@/core/types/account";

export const accountApi = appApi.injectEndpoints({
  endpoints: (build) => ({
    podcasterApply: build.mutation<
      { Message: string },
      { applyPodcasterFormData: any }
    >({
      async queryFn({ applyPodcasterFormData }, api) {
        const result = await api
          .dispatch(
            appApi.endpoints.kickoffThenWait.initiate({
              kickoff: {
                url: "/api/user-service/api/accounts/podcaster/apply",
                method: "POST",
                body: applyPodcasterFormData,
                authMode: "required",
              },
              poll: {
                intervalMs: 1000,
                maxAttempts: 30,
              },
            })
          )
          .unwrap();
        return { data: result as any };
      },
    }),
    updateAccountMe: build.query<{ Account: AccountMeFromApi }, void>({
      async queryFn(_arg, api, _extraOptions, baseQuery) {
        const result = await baseQuery({
          url: "/api/user-service/api/accounts/me",
          method: "GET",
          authMode: "required",
        });

        if (result.error) {
          return { error: result.error as any };
        }

        if (result.data) {
          const rawData = result.data as { Account: AccountMeFromApi };
          return { data: rawData };
        }

        return { error: { kind: "NETWORK_ERROR", message: "No data" } as any };
      },
      providesTags: ["Account"],
    }),

    getAccountInformations: build.query<{ Account: AccountMeFromApi }, void>({
      async queryFn(_arg, api, _extraOptions, baseQuery) {
        const result = await baseQuery({
          url: "/api/user-service/api/accounts/me",
          method: "GET",
          authMode: "required",
        });
        if (result.error) {
          return { error: result.error as any };
        }
        if (result.data) {
          const rawData = result.data as { Account: AccountMeFromApi };
          return { data: rawData };
        }
        return { error: { kind: "NETWORK_ERROR", message: "No data" } as any };
      },
      providesTags: ["Account"],
    }),

    updateAccountInformations: build.mutation<
      { Message: string },
      { uploadAccountInformationsFormData: any; accountId: number }
    >({
      async queryFn({ uploadAccountInformationsFormData, accountId }, api) {
        const result = await api
          .dispatch(
            appApi.endpoints.kickoffThenWait.initiate({
              kickoff: {
                url: `/api/user-service/api/accounts/${accountId}`,
                method: "PUT",
                body: uploadAccountInformationsFormData,
                authMode: "required",
              },
              poll: {
                intervalMs: 1000,
                maxAttempts: 30,
              },
            })
          )
          .unwrap();
        return { data: result as any };
      },
      invalidatesTags: ["Account"],
    }),

    checkUserPodcastListenSlot: build.query<number, void>({
      query: () => ({
        url: "/api/user-service/api/accounts/me",
        method: "GET",
        authMode: "required",
      }),
      transformResponse: (response: { Account: AccountMeFromApi }) => {
        return response.Account.PodcastListenSlot;
      },
    }),
  }),
});

export const {
  usePodcasterApplyMutation,
  useUpdateAccountMeQuery,
  useGetAccountInformationsQuery,
  useUpdateAccountInformationsMutation,
  useLazyCheckUserPodcastListenSlotQuery,
  useLazyUpdateAccountMeQuery
} = accountApi;
