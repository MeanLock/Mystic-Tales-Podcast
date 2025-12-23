// src/core/api/appApi/index.ts
import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react";
import type { BaseQueryFn } from "@reduxjs/toolkit/query";
import type { AuthMode, ApiErrorModel, PollConfig } from "@/core/types";
import { prepareAuthHeaders } from "./modes";
import { pollSagaResult } from "./polling";

/** Thay theo backend thực tế của bạn */
export const BASE_URL = import.meta.env.VITE_PUBLIC_API_URL;

/** raw baseQuery (fetchBaseQuery) */
const rawBaseQuery = fetchBaseQuery({
  baseUrl: BASE_URL,
  // Include credentials (cookies) like axios withCredentials: true — some endpoints rely on cookies.
  credentials: "include",
  // Timeout 5 phút (300000 milliseconds)
  timeout: 300000,
  prepareHeaders: (headers) => {
    headers.set("ngrok-skip-browser-warning", "69420");
    return headers;
  },
});

/** Wrapper 3-mode:
 *  - Nhận args bất kỳ ({ url, method, body, params, authMode }) → gắn header theo mode
 *  - Cho phép dùng trong mọi endpoint được inject
 */
const modeAwareBaseQuery: BaseQueryFn<
  {
    url: string;
    method?: string;
    body?: any;
    params?: any;
    authMode?: AuthMode;
    responseHandler?:
      | "json"
      | "text"
      | ((response: Response) => Promise<unknown>);
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

  // Clone headers tạm để gắn token theo mode
  const headers = new Headers();
  const auth = prepareAuthHeaders(headers, authMode);
  if (!auth.ok) {
    return {
      error: { kind: "HTTP_ERROR", message: auth.errMsg ?? "Unauthorized" },
    };
  }

  // Gắn thêm header từ endpoint nếu có
  // IMPORTANT: Headers.set() normalizes header names, so we need to convert to plain object
  const headersRecord: Record<string, string> = {};

  // Copy auth headers
  headers.forEach((value, key) => {
    headersRecord[key] = value;
  });

  // Add endpoint headers with exact casing
  if (endpointHeaders) {
    Object.entries(endpointHeaders).forEach(([k, v]) => {
      if (v != null) headersRecord[k] = v;
    });
  }

  // Truyền headers sang fetchBaseQuery qua "headers" và cho phép override responseHandler
  const baseQueryArgs: any = {
    url,
    method,
    body,
    params,
    headers: headersRecord,
  };
  if (responseHandler) baseQueryArgs.responseHandler = responseHandler;

  const res: any = await rawBaseQuery(baseQueryArgs, api, extraOptions);

  // Chuẩn hoá lỗi (tối giản)
  if (res?.error) {
    const kind: ApiErrorModel["kind"] =
      typeof res.error?.status === "number" ? "HTTP_ERROR" : "NETWORK_ERROR";
    return { error: { kind, message: "Request failed", details: res.error } };
  }
  return { data: res.data };
};

/** HỘP CHUNG appApi:
 *  - Dùng modeAwareBaseQuery
 *  - Cho phép inject endpoints ở bất cứ nơi đâu trong codebase
 *  - Cung cấp sẵn 1 endpoint mẫu "saga.getResultOnce" (KHÔNG polling) + 1 endpoint mẫu "saga.kickoffThenWait" (CÓ polling)
 *    → bạn có thể xoá nếu muốn, nhưng giữ lại để tham chiếu cấu trúc queryFn.
 */
export const appApi = createApi({
  reducerPath: "appApi",
  baseQuery: modeAwareBaseQuery,
  tagTypes: ["Account", "LatestSession"],
  endpoints: (build) => ({
    /** MẪU 1: Hỏi kết quả Saga 1 lần (không poll) — dùng khi bạn tự poll bên ngoài */
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

    /** MẪU 2: Kickoff rồi WAIT (polling) — minh hoạ rõ ràng Saga polling trong 1 endpoint
     *  - Bạn gọi endpoint này với { kickoff: {url/method/body...}, poll: {interval/maxAttempts...} }
     *  - Endpoint sẽ: gọi kickoff -> lấy SagaInstanceId -> poll tới SUCCESS/FAILED -> trả payload cuối
     *  - Không đụng axios, không middleware
     */
    kickoffThenWait: build.mutation<
      any,
      {
        kickoff: {
          url: string;
          method?: string;
          body?: any;
          params?: any;
          authMode?: AuthMode;
        };
        poll?: PollConfig;
      }
    >({
      async queryFn(arg, api, extraOptions, baseQuery) {
        const { kickoff, poll } = arg;

        // 1) gọi kickoff - baseQuery chỉ nhận 1 argument
        const kickoffRes: any = await baseQuery(kickoff);
        if (kickoffRes.error)
          return { error: kickoffRes.error as ApiErrorModel };

        const sagaId =
          kickoffRes.data?.SagaInstanceId ??
          kickoffRes.data?.sagaInstanceId ??
          kickoffRes.data?.id;
        if (!sagaId) {
          // Không phải Saga — trả thẳng data kickoff
          return { data: kickoffRes.data };
        }

        try {
          // 2) Poll đến khi xong
          const finalPayload = await pollSagaResult<any>({
            sagaId,
            baseQuery,
            api,
            extraOptions,
            config: poll,
          });
          return { data: finalPayload };
        } catch (e: any) {
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

/** Hooks mặc định (bạn có thể xoá nếu không cần) */
export const { useGetSagaResultOnceQuery, useKickoffThenWaitMutation } = appApi;
