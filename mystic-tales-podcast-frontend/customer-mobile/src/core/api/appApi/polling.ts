// src/core/api/appApi/polling.ts
import type { BaseQueryFn } from "@reduxjs/toolkit/query";

export type PollConfig = {
  intervalMs?: number;
  maxAttempts?: number;
  backoff?: boolean;
  resultPath?: (id: string) => string;
};

export type ApiErrorModel = {
  kind:
    | "HTTP_ERROR"
    | "NETWORK_ERROR"
    | "SAGA_FAILED"
    | "TIMEOUT"
    | "CANCELLED";
  message: string;
  details?: unknown;
};

export type SagaEnvelope = {
  FlowStatus: "PENDING" | "SUCCESS" | "FAILED" | string;
  ResultData?: string | null;
  ErrorMessage?: string | null;
};

/** sleep + jitter cho backoff */
const sleep = (ms: number) => new Promise((r) => setTimeout(r, ms));
const jitter = (ms: number) => Math.round(ms * (0.75 + Math.random() * 0.5));
const backoffMs = (base: number, attempt: number) =>
  Math.min(base * Math.max(1, attempt), base * 4);

/** Parse JSON string an toàn */
function tryParse<T = unknown>(raw?: string | null): T | undefined {
  if (!raw) return undefined as any;
  try {
    return JSON.parse(raw.replace(/\r?\n/g, "").trim()) as T;
  } catch {
    return undefined as any;
  }
}

/** Poll tới khi Saga SUCCESS/FAILED hoặc timeout attempts */
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
    ((id: string) =>
      `/api/saga-orchestrator-service/api/orchestration/result-data/${id}`);

  console.log("[POLL CONFIG]", {
    sagaId,
    interval,
    maxAttempts,
    backoff,
    resultPath: resultPath(sagaId),
  });

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
      console.log("[POLL ERROR]", {
        sagaId,
        attempt,
        error: res.error,
      });
      const kind: ApiErrorModel["kind"] =
        typeof res.error?.status === "number" ? "HTTP_ERROR" : "NETWORK_ERROR";
      throw Object.assign(new Error("Saga result error"), {
        kind,
        details: res.error,
      } as ApiErrorModel);
    }

    const env = res.data as SagaEnvelope;
    console.log("[POLL ATTEMPT]", {
      sagaId,
      attempt,
      maxAttempts,
      flowStatus: env.FlowStatus,
      hasResultData: !!env.ResultData,
      errorMessage: env.ErrorMessage,
    });

    if (env.FlowStatus === "SUCCESS") {
      const parsed = tryParse<T>(env.ResultData ?? undefined);
      console.log("[POLL SUCCESS]", {
        sagaId,
        attempt,
        resultData: env.ResultData,
        parsed,
      });
      return parsed as T;
    }
    if (env.FlowStatus === "FAILED") {
      console.log("[POLL FAILED]", {
        sagaId,
        attempt,
        errorMessage: env.ErrorMessage,
      });
      const err: ApiErrorModel = {
        kind: "SAGA_FAILED",
        message: env.ErrorMessage ?? "Saga failed",
      };
      throw Object.assign(new Error(err.message), err);
    }

    const base = interval;
    const wait = backoff ? jitter(backoffMs(base, attempt)) : base;
    console.log("[POLL WAITING]", {
      sagaId,
      attempt,
      waitMs: wait,
    });
    await sleep(wait);
  }

  console.log("[POLL TIMEOUT]", {
    sagaId,
    maxAttempts,
    message: "Saga polling timed out",
  });
  const err: ApiErrorModel = {
    kind: "TIMEOUT",
    message: "Saga polling timed out",
  };
  throw Object.assign(new Error(err.message), err);
}
