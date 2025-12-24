// src/features/anything/myEndpoints.ts
import { appApi } from "@/core/api/appApi";
import { pollSagaResult } from "@/core/api/appApi/polling";


export const myApi = appApi.injectEndpoints({
  endpoints: (build) => ({

    // Ví dụ: 1 endpoint public bình thường => Trả ra data luôn
    getPublicThing: build.query<any, void>({
      query: () => ({
        url: "/api/public/thing",
        method: "GET",
        authMode: "public",
      }),
    }),

    // Ví dụ: 2 kickoff -> saga -> payload
    // CÁCH 1: Dùng queryFn để gọi kickoffThenWait mutation từ appApi
    createSomething: build.mutation<any, { payload: any }>({
      async queryFn({ payload }, api) {
        // Gọi kickoffThenWait mutation
        const result = await api.dispatch(
          appApi.endpoints.kickoffThenWait.initiate({
            kickoff: {
              url: "/api/something/create",
              method: "POST",
              body: payload,
              authMode: "required",
            },
            poll: {
              intervalMs: 1000,
              maxAttempts: 30,
              // resultPath: (id) => `/custom/orchestration/${id}`, // nếu BE path khác
            },
          })
        );

        if ("error" in result) {
          return { error: result.error as any };
        }
        return { data: result.data as any };
      },
    }),

    // CÁCH 2: Dùng query với baseQuery trực tiếp (nếu bạn muốn custom logic)
    createSomethingV2: build.mutation<any, { payload: any }>({
      async queryFn({ payload }, api, extraOptions, baseQuery) {
        // 1. Kickoff
        const kickoffRes: any = await baseQuery({
          url: "/api/something/create",
          method: "POST",
          body: payload,
          authMode: "required",
        });
    
        if (kickoffRes.error) return { error: kickoffRes.error };
    
        const sagaId = kickoffRes.data?.SagaInstanceId;
        if (!sagaId) return { data: kickoffRes.data };
    
        // 2. Poll
        try {
          const finalPayload = await pollSagaResult({
            sagaId,
            baseQuery,
            api,
            extraOptions,
            config: { intervalMs: 1000, maxAttempts: 30 },
          });
          return { data: finalPayload };
        } catch (error) {
          return { error };
        }
      },
    }),
  }),
  overrideExisting: false,
});

// Hooks:
export const { useGetPublicThingQuery, useCreateSomethingMutation } = myApi;
