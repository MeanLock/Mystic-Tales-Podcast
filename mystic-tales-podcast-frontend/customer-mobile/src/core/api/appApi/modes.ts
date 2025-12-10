// src/core/api/appApi/modes.ts
import { getAccessToken } from "./token";

export type AuthMode = "public" | "required" | "hybrid";

/**
 * Quy tắc 3 mode:
 *  - public: không cần token
 *  - required: bắt buộc có token, nếu không có -> fail sớm
 *  - hybrid: có token thì gắn, không có vẫn call bình thường
 */
export function prepareAuthHeaders(
  headers: Headers,
  mode: AuthMode = "public"
): { ok: boolean; errMsg?: string } {
  const token = getAccessToken();

  if (mode === "required" && !token) {
    return { ok: false, errMsg: "Missing access token" };
  }

  if (mode === "required" || (mode === "hybrid" && token)) {
    if (token) headers.set("Authorization", `Bearer ${token}`);
  }

  return { ok: true };
}
