import { appApi } from "@/core/api/appApi";

export const authApi = appApi.injectEndpoints({
  endpoints: (build) => ({
    login: build.mutation<
      { AccessToken: string; Message: string },
      { ManualLoginInfo: { Email: string; Password: string } }
    >({
      async queryFn({ ManualLoginInfo }, api) {
        const result = await api
          .dispatch(
            appApi.endpoints.kickoffThenWait.initiate({
              kickoff: {
                url: "/api/user-service/api/auth/login-manual",
                method: "POST",
                body: { ManualLoginInfo },
                authMode: "public",
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

// Hooks
export const { useLoginMutation } = authApi;
