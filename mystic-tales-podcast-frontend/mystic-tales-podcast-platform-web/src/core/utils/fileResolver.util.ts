import { store } from "@/redux/store";
import _get from "lodash/get";
import _set from "lodash/set";
import fileApi from "../services/file/file.service";

export type FileResolveConfig = {
  path: string;
  type:
    | "AccountPublic"
    | "TemplateCommitment"
    | "PodcasterCommitment"
    | "BookingPublic"
    | "PodcastPublic";
  output: string;
};

/** Map type => endpoint trigger (dùng endpoint, không phải hook) */
const fileTypeTriggerMap = {
  AccountPublic: fileApi.endpoints.getAccountPublicSource.initiate,
  TemplateCommitment:
    fileApi.endpoints.getTemplatePodcastBuddyCommitmentFile.initiate,
  PodcasterCommitment: fileApi.endpoints.getPodcastBuddyCommitmentFile.initiate,
  BookingPublic: fileApi.endpoints.getBookingPublicSource.initiate,
  PodcastPublic: fileApi.endpoints.getPodcastPublicSource.initiate,
};

/** Resolve một FileKey theo type */
async function resolveFileKey(type: FileResolveConfig["type"], key: string) {
  const trigger = fileTypeTriggerMap[type];
  if (!trigger) return null;

  // Endpoints expect { FileKey: string } or { fileEnum: string } object, not plain string
  const arg =
    type === "TemplateCommitment" ? { fileEnum: key } : { FileKey: key };

  // Debug: log API call details
  console.log(`[FileResolver] 🔍 Calling API:`, {
    type,
    fileKey: key,
    endpoint:
      type === "AccountPublic"
        ? "getAccountPublicSource"
        : type === "TemplateCommitment"
        ? "getTemplatePodcastBuddyCommitmentFile"
        : type === "PodcasterCommitment"
        ? "getPodcastBuddyCommitmentFile"
        : type === "BookingPublic"
        ? "getBookingPublicSource"
        : type === "PodcastPublic"
        ? "getPodcastPublicSource"
        : "unknown",
    args: arg,
  });

  const res: any = await store.dispatch(trigger(arg as any) as any);

  if ("data" in res && res.data) {
    console.log(`[FileResolver] ✅ Resolved:`, {
      fileKey: key,
      fileUrl: res.data.FileUrl,
    });
    return res.data.FileUrl;
  }

  console.log(`[FileResolver] ❌ Failed to resolve:`, {
    fileKey: key,
    response: res,
  });
  return null;
}

/** Resolve toàn bộ object / array */
export async function resolveFiles<T extends object>(
  data: T | T[],
  configs: FileResolveConfig[]
): Promise<{ resolvedData: T | T[]; loading: boolean }> {
  const items = Array.isArray(data) ? data : [data];

  const resolvedItems = await Promise.all(
    items.map(async (item) => {
      const clone = { ...item };
      for (const conf of configs) {
        const fileKey = _get(item, conf.path);
        if (fileKey) {
          const fileUrl = await resolveFileKey(conf.type, fileKey);
          if (fileUrl) _set(clone, conf.output, fileUrl);
        }
      }
      return clone;
    })
  );

  return {
    resolvedData: Array.isArray(data) ? resolvedItems : resolvedItems[0],
    loading: false,
  };
}
