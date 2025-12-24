const NODE_ENV = (import.meta as any).env.MODE || "development";
const REST_API_BASE_URL = (import.meta as any).env.VITE_BACKEND_REST_API_URL;
const GRAPHQL_API_BASE_URL = (import.meta as any).env.VITE_BACKEND_GRAPHQL_API_URL;
const GRAPHQL_API_WS_URL = (import.meta as any).env.VITE_BACKEND_GRAPHQL_API_URL_WS;

export const ENV_CONFIG = {
  NODE_ENV,
  REST_API_BASE_URL,
  GRAPHQL_API_BASE_URL,
  GRAPHQL_API_WS_URL,
  // Có thể mở rộng thêm các biến môi trường khác nếu cần
};