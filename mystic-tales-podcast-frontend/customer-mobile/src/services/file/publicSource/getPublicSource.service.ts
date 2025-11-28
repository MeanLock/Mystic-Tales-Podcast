// services/helpers/getPublicSourceHelper.ts
import { baseQuery, withAuthMode } from "@/src/services/baseApi";
import type { FetchBaseQueryError } from "@reduxjs/toolkit/query";

export const fetchPublicFileUrl = async (
  fileKey: string,
  api: any,
  extraOptions: any
): Promise<string> => {
  console.log("[fetchPublicFileUrl] called with fileKey:", fileKey);
  try {
    const encoded = encodeURIComponent(fileKey).replace(/%2F/g, "/");
    const result = await baseQuery(
      withAuthMode(
        {
          url: `/api/user-service/api/misc/public-source/get-file-url/${fileKey}`,
          method: "GET",
        },
        { authMode: "public" }
      ),
      api,
      extraOptions
    );

    if ("error" in result) {
      console.warn("[fetchPublicFileUrl] error:", result.error);
      // Return a local placeholder so callers always get a usable image URL
      return "https://i.pinimg.com/736x/62/07/15/620715d7b709a2f7f137227885c66793.jpg";
    }

    const payload = result.data as any;
    console.log("Payload: ", payload.FileUrl);
    if (typeof payload === "object" && payload?.FileUrl) {
      if (payload.FileUrl === "" || !payload.FileUrl) {
        return "https://i.pinimg.com/736x/62/07/15/620715d7b709a2f7f137227885c66793.jpg";
      } else {
        console.log("Payload Url: ", payload.FileUrl);
        return payload.FileUrl;
      }
    }
    if (typeof payload === "string") return payload;
    return "https://i.pinimg.com/736x/62/07/15/620715d7b709a2f7f137227885c66793.jpg";
  } catch (err) {
    console.warn("[fetchPublicFileUrl] Exception:", err);
    return "https://i.pinimg.com/736x/62/07/15/620715d7b709a2f7f137227885c66793.jpg";
  }
};
