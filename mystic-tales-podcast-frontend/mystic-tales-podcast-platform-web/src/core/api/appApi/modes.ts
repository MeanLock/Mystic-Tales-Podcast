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
  const token = `eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjEiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiRFhCIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvZW1haWxhZGRyZXNzIjoiZHhiYWNoMjAwNEBnbWFpbC5jb20iLCJpZCI6IjEiLCJyb2xlX2lkIjoiMSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkN1c3RvbWVyIiwiYmFsYW5jZSI6IjAuMDAiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3NlcmlhbG51bWJlciI6IjhjZGJlMWU2LTEwNDMtNDFmYS05ZGZlLWExYzBlYzY5NzQyMSIsImV4cCI6Nzc2MjY4NzMyMywiaXNzIjoibG9jYWxob3N0IiwiYXVkIjoibG9jYWxob3N0In0.Wt4b5heKvHBgc33ekDruYfvrule-ViKGMbe7u43wDlGZjaJ52Q8EpA9PbrEksHs8t6Osxa5qWAYi8OpeTnMvIEE0cp9uN9flYkoE8FFCx_Id5SULGczFiC99MclYjLEz_bLUPV40NpWTrS89oo3gZ3XrOal23jBVFidKClz0E-tvAIciWEat82MDt36zDYd1NIeogHDzu7TBFRtI2yocgsMIYrSNpFT3TC0L-nuiVRZ0TgL1D4MtQs79deJ2nYEN4bIHocdSEuApc6z1UhftLxPBCi7fxi9LD2_r8Wpli2v1hxv3VnRrUUN--Yh7jOIaXvfgFHAZVru5XeiD4JxN9w`;
  if (mode === "required" && !token) {
    return { ok: false, errMsg: "Missing access token" };
  }

  if (mode === "required" || (mode === "hybrid" && token)) {
    if (token) headers.set("Authorization", `Bearer ${token}`);
  }
  // mode public: không gắn, nhưng nếu bạn muốn luôn gắn khi có token, đổi logic ở trên.
  return { ok: true };
}
