// src/core/api/appApi/polling.ts
import type { BaseQueryFn } from "@reduxjs/toolkit/query";
import type { SagaEnvelope, PollConfig, ApiErrorModel } from "@/core/types";

/** Tiện ích sleep + jitter nhẹ cho backoff */
const sleep = (ms: number) => new Promise((r) => setTimeout(r, ms));
const jitter = (ms: number) => Math.round(ms * (0.75 + Math.random() * 0.5));
const backoffMs = (base: number, attempt: number) =>
  Math.min(base * Math.max(1, attempt), base * 4);

/** Parse ResultData (string -> object) an toàn */
function tryParse<T = unknown>(raw?: string): T | undefined {
  if (!raw) return undefined as any;
  try {
    return JSON.parse(raw.replace(/\r?\n/g, "").trim()) as T;
  } catch {
    return undefined as any;
  }
}

/** Poll tới khi Saga SUCCESS/FAILED hoặc timeout attempts.
 *  - baseQuery: chính là fetchBaseQuery đã cấu hình trong appApi (được RTKQ truyền vào).
 *  - api/extraOptions: tham số nội bộ RTKQ (để có abort signal, v.v.)
 */
export async function pollSagaResult<T = unknown>(opts: {
  sagaId: string;
  baseQuery: BaseQueryFn<any, unknown, unknown>;
  api: any;
  extraOptions: any;
  config?: PollConfig;
}): Promise<T> {
  const { sagaId, baseQuery, api, extraOptions, config = {} } = opts;

  const interval = config.intervalMs ?? 1500;
  const maxAttempts = config.maxAttempts ?? 30;
  const backoff = config.backoff ?? true;
  const resultPath =
    config.resultPath ??
    ((id) =>
      `/api/saga-orchestrator-service/api/orchestration/result-data/${id}`);

  for (let attempt = 1; attempt <= maxAttempts; attempt++) {
    if (api?.signal?.aborted) {
      const err: ApiErrorModel = {
        kind: "CANCELLED",
        message: "Polling cancelled",
      };
      throw Object.assign(new Error(err.message), err);
    }

    const res: any = await baseQuery(
      { url: resultPath(sagaId), method: "GET" },
      api,
      extraOptions
    );

    if (res?.error) {
      // Lỗi HTTP/Network trong lúc poll → trả về luôn cho gọn (tuỳ ý bạn muốn retry hay fail ngay)
      const kind: ApiErrorModel["kind"] =
        typeof res.error?.status === "number" ? "HTTP_ERROR" : "NETWORK_ERROR";
        throw Object.assign(new Error("Saga result error"), {
        kind,
        details: res.error,
      });
    }

    const env = res.data as SagaEnvelope;
    console.log("Saga Result: ", env.ResultData);
    if (env.FlowStatus === "SUCCESS") {
      const parsed = tryParse<T>(env.ResultData);
      console.log("Parsed Saga Result:", parsed);
      return parsed as T; // Assert non-null for successful saga
    }
    if (env.FlowStatus === "FAILED") {
      const err: ApiErrorModel = {
        kind: "SAGA_FAILED",
        message: env.ErrorMessage ?? "Saga failed",
      };
      throw Object.assign(new Error(err.message), err);
    }

    const base = interval;
    const wait = backoff ? jitter(backoffMs(base, attempt)) : base;
    await sleep(wait);
  }

  const err: ApiErrorModel = {
    kind: "TIMEOUT",
    message: "Saga polling timed out",
  };
  throw Object.assign(new Error(err.message), err);
}
