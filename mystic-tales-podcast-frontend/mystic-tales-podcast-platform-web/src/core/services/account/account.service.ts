import { appApi } from "@/core/api/appApi";

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
        return { data: result.data as any };
      },
    }),
  }),
});

export const { usePodcasterApplyMutation } = accountApi;
