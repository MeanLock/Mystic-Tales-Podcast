// src/core/api/appApi/index.ts
import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react";
import type { BaseQueryFn } from "@reduxjs/toolkit/query";
import { prepareAuthHeaders, type AuthMode } from "./modes";
import { pollSagaResult, type PollConfig, type ApiErrorModel } from "./polling";

/** Thay bằng backend của mobile (hoặc EXPO env) */
export const BASE_URL =
  process.env.EXPO_PUBLIC_API_URL ??
  "https://fast-scorpion-strictly.ngrok-free.app"; // giống baseApi cũ mobile

/** raw fetchBaseQuery – không gắn Authorization ở đây */
const rawBaseQuery = fetchBaseQuery({
  baseUrl: BASE_URL,
  prepareHeaders: (headers) => {
    headers.set("ngrok-skip-browser-warning", "69420");
    return headers;
  },
});

/** BaseQuery hiểu 3 mode:
 *  - args: { url, method, body, params, authMode, headers, responseHandler }
 *  - tự gắn token theo authMode
 */
const modeAwareBaseQuery: BaseQueryFn<
  {
    url: string;
    method?: string;
    body?: any;
    params?: any;
    authMode?: AuthMode;
    responseHandler?: "json" | "text";
    headers?: Record<string, string>;
  },
  unknown,
  ApiErrorModel
> = async (args, api, extraOptions) => {
  const {
    url,
    method = "GET",
    body,
    params,
    authMode = "public",
    responseHandler,
    headers: endpointHeaders,
  } = args;

  // 1) Gắn auth header theo mode
  const headers = new Headers();
  const auth = prepareAuthHeaders(headers, authMode);
  if (!auth.ok) {
    return {
      error: {
        kind: "HTTP_ERROR",
        message: auth.errMsg ?? "Unauthorized",
      },
    };
  }

  // 2) Merge thêm header custom của endpoint
  const headersRecord: Record<string, string> = {};
  headers.forEach((value, key) => {
    headersRecord[key] = value;
  });
  if (endpointHeaders) {
    Object.entries(endpointHeaders).forEach(([k, v]) => {
      if (v != null) headersRecord[k] = v;
    });
  }

  // 3) Gọi fetchBaseQuery
  const baseQueryArgs: any = {
    url,
    method,
    body,
    params,
    headers: headersRecord,
  };
  if (responseHandler) baseQueryArgs.responseHandler = responseHandler;

  // DEBUG: Log API call details
  console.log("[API REQUEST]", {
    url: `${BASE_URL}${url}`,
    method,
    body,
    params,
    authMode,
    headers: headersRecord,
  });

  const res: any = await rawBaseQuery(baseQueryArgs, api, extraOptions);

  if (res?.error) {
    const kind: ApiErrorModel["kind"] =
      typeof res.error?.status === "number" ? "HTTP_ERROR" : "NETWORK_ERROR";
    console.log("[API ERROR]", {
      url: `${BASE_URL}${url}`,
      method,
      error: res.error,
      kind,
    });
    return {
      error: {
        kind,
        message: "Request failed",
        details: res.error,
      },
    };
  }

  console.log("[API SUCCESS]", {
    url: `${BASE_URL}${url}`,
    method,
    data: res.data,
  });

  return { data: res.data };
};

/** appApi dùng chung toàn app (giống web) */
export const appApi = createApi({
  reducerPath: "appApi",
  baseQuery: modeAwareBaseQuery,
  tagTypes: ["Account"],
  endpoints: (build) => ({
    /** Hỏi kết quả Saga 1 lần (không poll) – nếu muốn tự poll ngoài */
    getSagaResultOnce: build.query<
      any,
      { sagaId: string; authMode?: AuthMode }
    >({
      query: ({ sagaId, authMode = "required" }) => ({
        url: `/api/saga-orchestrator-service/api/orchestration/result-data/${sagaId}`,
        method: "GET",
        authMode,
      }),
    }),

    /** Kickoff rồi WAIT (polling) – dùng chung cho mọi Saga-based flow */
    kickoffThenWait: build.mutation<
      any,
      {
        kickoff: {
          url: string;
          method?: string;
          body?: any;
          params?: any;
          authMode?: AuthMode;
          headers?: Record<string, string>;
        };
        poll?: PollConfig;
      }
    >({
      async queryFn(arg, api, extraOptions, baseQuery) {
        const { kickoff, poll } = arg;

        console.log("[KICKOFF START]", {
          url: kickoff.url,
          method: kickoff.method || "POST",
          body: kickoff.body,
          params: kickoff.params,
          authMode: kickoff.authMode,
        });

        // 1) Gọi kickoff (login, cancel booking, v.v…)
        const kickoffRes: any = await baseQuery(kickoff);
        if (kickoffRes.error) {
          return { error: kickoffRes.error as ApiErrorModel };
        }

        const sagaId =
          kickoffRes.data?.SagaInstanceId ??
          kickoffRes.data?.sagaInstanceId ??
          kickoffRes.data?.id;

        console.log("[KICKOFF RESPONSE]", {
          sagaId,
          data: kickoffRes.data,
        });

        // Nếu backend không dùng Saga -> trả luôn data kickoff (non-saga endpoint)
        if (!sagaId) {
          console.log("[NON-SAGA ENDPOINT] Returning kickoff data directly");
          return { data: kickoffRes.data };
        }

        console.log("[SAGA POLLING START]", { sagaId, pollConfig: poll });

        try {
          // 2) Poll saga tới khi xong
          const finalPayload = await pollSagaResult<any>({
            sagaId,
            baseQuery,
            api,
            extraOptions,
            config: poll,
          });

          console.log("[SAGA POLLING SUCCESS]", {
            sagaId,
            finalPayload,
          });

          // finalPayload chính là object kết quả của Saga (vd: { AccessToken, RefreshToken } hoặc { Message } )
          return { data: finalPayload };
        } catch (e: any) {
          console.log("[SAGA POLLING ERROR]", {
            sagaId,
            error: e,
            kind: e?.kind,
            message: e?.message,
          });
          const err: ApiErrorModel = {
            kind: e?.kind ?? "SAGA_FAILED",
            message: e?.message ?? "Saga error",
            details: e,
          };
          return { error: err };
        }
      },
    }),
  }),
});

export const { useGetSagaResultOnceQuery, useKickoffThenWaitMutation } = appApi;
