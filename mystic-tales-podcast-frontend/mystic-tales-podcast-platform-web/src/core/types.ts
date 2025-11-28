// src/core/types.ts

/** Auth mode for API calls */
export type AuthMode = "public" | "required" | "hybrid";

/** Saga envelope response */
export interface SagaEnvelope {
  FlowStatus: "SUCCESS" | "FAILED" | "PENDING" | "RUNNING";
  ResultData?: string;
  ErrorMessage?: string;
}

/** Poll configuration */
export interface PollConfig {
  intervalMs?: number;
  maxAttempts?: number;
  backoff?: boolean;
  resultPath?: (sagaId: string) => string;
}

/** API Error Model */
export interface ApiErrorModel {
  kind:
    | "HTTP_ERROR"
    | "NETWORK_ERROR"
    | "SAGA_FAILED"
    | "TIMEOUT"
    | "CANCELLED";
  message: string;
  details?: any;
}
