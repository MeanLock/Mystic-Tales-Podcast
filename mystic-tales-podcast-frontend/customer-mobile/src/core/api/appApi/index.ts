// src/core/api/appApi/index.ts
import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react";
import type { BaseQueryFn } from "@reduxjs/toolkit/query";
import { prepareAuthHeaders, type AuthMode } from "./modes";
import { pollSagaResult, type PollConfig, type ApiErrorModel } from "./polling";

/** Thay bằng backend của mobile (hoặc EXPO env) */
export const BASE_URL =
  process.env.EXPO_PUBLIC_API_URL ??
  "https://fast-scorpion-strictly.ngrok-free.app"; // giống baseApi cũ mobile

// Helper: format log đẹp, có khung và xuống dòng
const safeStringify = (value: any) => {
  try {
    return JSON.stringify(value, null, 2);
  } catch {
    return String(value);
  }
};

const debugLog = (title: string, data: Record<string, any>) => {
  if (!__DEV__) return; // chỉ log khi dev, tránh spam production

  const header =
    "========================" + title + " ============================";
  const lines: string[] = [header];

  Object.entries(data).forEach(([key, value]) => {
    if (value === undefined) return;

    if (typeof value === "object" && value !== null) {
      lines.push(`${key}:`);
      lines.push(safeStringify(value));
    } else {
      lines.push(`${key}: ${String(value)}`);
    }
  });

  lines.push(
    "========================================================================"
  );
  console.log(lines.join("\n"));
};

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
  const fullUrl = `${url}`;

  // DEBUG: Log API call details
  debugLog("[API REQUEST]", {
    url: fullUrl,
    method,
    authMode,
    hasHeaders: Object.keys(headersRecord || {}).length > 0,
    body,
    params,
    headers: headersRecord ? "Có Token" : "Không Token",
  });

  const res: any = await rawBaseQuery(baseQueryArgs, api, extraOptions);

  if (res?.error) {
    const kind: ApiErrorModel["kind"] =
      typeof res.error?.status === "number" ? "HTTP_ERROR" : "NETWORK_ERROR";

    debugLog("[API ERROR]", {
      url: fullUrl,
      method,
      kind,
      error: res.error,
    });

    return {
      error: {
        kind,
        message: "Request failed",
        details: res.error,
      },
    };
  }

  debugLog("[API SUCCESS]", {
    url: fullUrl,
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

        debugLog("[KICKOFF START]", {
          url: `${BASE_URL}${kickoff.url}`,
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

        debugLog("[KICKOFF RESPONSE]", {
          sagaId,
          data: kickoffRes.data,
        });

        // Nếu backend không dùng Saga -> trả luôn data kickoff (non-saga endpoint)
        if (!sagaId) {
          debugLog("[NON-SAGA ENDPOINT]", {
            message: "Returning kickoff data directly",
            data: kickoffRes.data,
          });

          return { data: kickoffRes.data };
        }

        debugLog("[SAGA POLLING START]", {
          sagaId,
          pollConfig: poll,
        });

        try {
          // 2) Poll saga tới khi xong
          const finalPayload = await pollSagaResult<any>({
            sagaId,
            baseQuery,
            api,
            extraOptions,
            config: poll,
          });

          debugLog("[SAGA POLLING SUCCESS]", {
            sagaId,
            finalPayload,
          });

          // finalPayload chính là object kết quả của Saga (vd: { AccessToken, RefreshToken } hoặc { Message } )
          return { data: finalPayload };
        } catch (e: any) {
          debugLog("[SAGA POLLING ERROR]", {
            sagaId,
            kind: e?.kind,
            message: e?.message,
            error: e,
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
