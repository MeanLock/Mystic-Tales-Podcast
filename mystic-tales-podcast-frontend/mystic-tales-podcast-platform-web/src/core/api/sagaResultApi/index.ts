// @ts-nocheck

// src/core/api/sagaResultApi/index.ts
import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react";
import type { SagaEnvelope } from "@/core/types";
// import { BASE_URL } from "@/core/api/base/http";
import { parseResultData } from "@/core/utils/parseResultData";

type GetSagaResultArgs = { sagaId: string };

type FlowStatus = "PENDING" | "RUNNING" | "SUCCESS" | "FAILED";

const BASE_URL = import.meta.env.VITE_PUBLIC_API_URL ?? "https://your.backend";

export interface SagaResultQuery<T = unknown> {
  flowStatus: FlowStatus;
  data?: T;
  errorMessage?: string;
}

export const sagaResultApi = createApi({
  reducerPath: "sagaResultApi",
  baseQuery: fetchBaseQuery({
    baseUrl: BASE_URL,
    prepareHeaders: (headers) => {
      headers.set("ngrok-skip-browser-warning", "true");
      headers.set("Alo", "123");
      return headers;
    },
  }),
  endpoints: (build) => ({
    getSagaResult: build.query<SagaResultQuery<any>, GetSagaResultArgs>({
      query: ({ sagaId }) => ({
        url: `/api/saga-orchestrator-service/api/orchestration/result-data/${sagaId}?ngrok-skip-browser-warning=true`,
        method: "GET",
      }),
      transformResponse: (res: SagaEnvelope) => {
        return {
          flowStatus: res.FlowStatus,
          data: res.ResultData,
          errorMessage: res.ErrorMessage,
        };
      },
    }),
  }),
});

export const { useGetSagaResultQuery } = sagaResultApi;
