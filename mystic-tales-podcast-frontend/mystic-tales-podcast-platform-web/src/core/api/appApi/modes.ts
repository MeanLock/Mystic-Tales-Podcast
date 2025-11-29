// src/core/api/appApi/modes.ts
import type { AuthMode } from "@/core/types";
import { getAccessToken } from "./token";

/** Quy tắc 3 mode:
 *  - public: không yêu cầu token (mặc định không gắn header)
 *  - required: bắt buộc có token; nếu không có → trả lỗi sớm
 *  - hybrid: có token thì gắn, không có vẫn gọi được
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
  // mode public: không gắn, nhưng nếu bạn muốn luôn gắn khi có token, đổi logic ở trên.
  return { ok: true };
}
