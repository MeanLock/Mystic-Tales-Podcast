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
  // const token = getAccessToken();
  const token = `eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjEwMTIiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiVsWpIFRo4buLIEYiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJ2dXRoaWZAZW1haWwuY29tIiwiaWQiOiIxMDEyIiwicm9sZV9pZCI6IjEiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJDdXN0b21lciIsImJhbGFuY2UiOiIwLjAwIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9zZXJpYWxudW1iZXIiOiJhZDI0NGMwMy04ZWEyLTQ5NDUtOTZhNy1hZmRjZDIxZDE3ZTciLCJleHAiOjc3NjEzODUzOTUsImlzcyI6ImxvY2FsaG9zdCIsImF1ZCI6ImxvY2FsaG9zdCJ9.hKMmv2Ax0EhF1pwAw5LLLdv3YKXb-LaoVMIBu-hYUBhoJjoLHt0LdCa-yfxeHlF-0Kxuc6WG8VWci_oO9eEU8ySOAca9EsFF4LARqf1xXqwA295ync6TBrWMpM1Wjf5UCw_GxGq9iSfESYn9aMfxlEdvj33CRI39xD-xhaqgg_JpCk1BV1Iz0pYjEW_Jibo9Qm6VfxVadciZxfGa89h6YVEOOGTlaGWwCKYk0HuK5ygXKpcGGGjtswH-dhcwoBUl1XHK_g9czryS-tiHlTTPV6lPd1m7IWq4VhbIrtV_Qaz2OuGTT3NjOg8ARlgzNo5qWfNC2cIe-KBAaIENHTReiQ`;
  if (mode === "required" && !token) {
    return { ok: false, errMsg: "Missing access token" };
  }

  if (mode === "required" || (mode === "hybrid" && token)) {
    if (token) headers.set("Authorization", `Bearer ${token}`);
  }
  // mode public: không gắn, nhưng nếu bạn muốn luôn gắn khi có token, đổi logic ở trên.
  return { ok: true };
}
