// src/services/fileApi.ts
import { baseApi } from "../baseApi";

export type FileUrlResponse = {
  url: string;
  contentType?: string; // server nên trả
  expiresAt?: number; // epoch ms, nếu là signed URL (khuyến nghị)
};

export const fileApi = baseApi.injectEndpoints({
  endpoints: (b) => ({
    getFileUrl: b.query<FileUrlResponse, string>({
      // fileKey -> url
      query: (fileKey) => ({
        url: `/api/user-service/api/misc/file-source/get-file-url/${encodeURIComponent(
          fileKey
        )}`,
        method: "GET",
      }),
      // TTL ngắn vì signed URL thường hết hạn nhanh: 1–5 phút
      keepUnusedDataFor: 120,
      // Cache key chỉ phụ thuộc vào fileKey
      serializeQueryArgs: ({ endpointName, queryArgs }) =>
        `${endpointName}:${queryArgs}`,
      // Nếu response bọc data, chuẩn hoá tại đây:
      transformResponse: (r: any): FileUrlResponse => {
        // ví dụ r = { url, contentType, expiresAt } hoặc r.data
        const data = r?.data ?? r;
        return {
          url: String(data?.FileUrl ?? data?.Url ?? data?.URL),
          contentType: data?.contentType ?? data?.ContentType ?? "unknown",
          expiresAt: data?.expiresAt ?? data?.ExpiresAt ?? "unknown",
        };
      },
      providesTags: (_res, _err, fileKey) => [
        { type: "FileUrl" as const, id: fileKey },
      ],
    }),
  }),
  overrideExisting: false,
});

export const { useGetFileUrlQuery, useLazyGetFileUrlQuery } = fileApi;
