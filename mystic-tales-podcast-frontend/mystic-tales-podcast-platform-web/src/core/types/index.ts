// src/core/api/appApi/types.ts
export type AuthMode = "public" | "required" | "hybrid";

/** Gắn vào mỗi query call (qua extraOptions hoặc args) để chỉ định mode */
export interface ModeOptions {
  authMode?: AuthMode; // mặc định: "public"
}

/** Dạng dữ liệu BE trả khi hỏi kết quả Saga */
export type FlowStatus = "PENDING" | "RUNNING" | "SUCCESS" | "FAILED";

export interface SagaEnvelope {
  SagaId: string;
  FlowStatus: FlowStatus;
  ResultData?: string; // JSON string
  ErrorMessage?: string;
}

/** Cấu hình polling cho Saga */
export interface PollConfig {
  intervalMs?: number; // default 1500
  timeoutPerRequestMs?: number; // default 12000 (chỉ tác dụng nếu BE tôn trọng)
  maxAttempts?: number; // default 30 (~45s)
  backoff?: boolean; // default true (backoff nhẹ + jitter)
  /** Đường dẫn lấy kết quả từ sagaId (tuỳ backend) */
  resultPath?: (sagaId: string) => string;
}

/** Lỗi chuẩn hoá tối giản cho FE */
export type ApiErrorKind =
  | "HTTP_ERROR"
  | "NETWORK_ERROR"
  | "SAGA_FAILED"
  | "TIMEOUT"
  | "CANCELLED";
export interface ApiErrorModel {
  kind: ApiErrorKind;
  message: string;
  code?: number | string;
  details?: any;
}
