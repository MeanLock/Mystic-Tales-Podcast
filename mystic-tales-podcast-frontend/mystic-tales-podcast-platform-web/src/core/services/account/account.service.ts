// @ts-nocheck

import { appApi } from "@/core/api/appApi";
import type { AccountMeFromApi } from "@/core/types/account";
import { setUser } from "@/redux/slices/authSlice/authSlice";

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
          const accountInformations = rawData.Account;

          // Update Redux state
          api.dispatch(setUser(accountInformations));

          // Return data for the query
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
  }),
});

export const {
  usePodcasterApplyMutation,
  useUpdateAccountMeQuery,
  useGetAccountInformationsQuery,
  useUpdateAccountInformationsMutation,
} = accountApi;
