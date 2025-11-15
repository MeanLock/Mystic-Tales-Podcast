import { store } from "@/redux/store";
import _get from "lodash/get";
import _set from "lodash/set";
import _unset from "lodash/unset";
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

/** Fallback URL cho từng type (nếu không có file key hoặc resolve lỗi) */
function getFallbackUrl(type: FileResolveConfig["type"]): string | null {
  if (type === "AccountPublic") return "/images/unknown/user.png";
  if (type === "PodcastPublic") return "/images/unknown/content.png";
  return null;
}

/** Resolve một FileKey theo type
 *  - Nếu resolve được: trả về fileUrl
 *  - Nếu KHÔNG resolve được: trả fallback (nếu có), hoặc null
 */
async function resolveFileKey(
  type: FileResolveConfig["type"],
  key: string | undefined | null
): Promise<string | null> {
  const trigger = fileTypeTriggerMap[type];

  // Nếu không có trigger hoặc key trống: trả fallback (nếu có)
  if (!trigger || !key) {
    const fallback = getFallbackUrl(type);
    if (fallback) {
      console.log(
        `[FileResolver] ⚠️ No fileKey for type=${type}, use fallback`,
        {
          fallback,
        }
      );
    }
    return fallback;
  }

  const arg =
    type === "TemplateCommitment" ? { fileEnum: key } : { FileKey: key };

  console.log(`[FileResolver] 🔍 Calling API:`, {
    type,
    fileKey: key,
    arg,
  });

  const res: any = await store.dispatch(trigger(arg as any) as any);

  if ("data" in res && res.data && res.data.FileUrl) {
    console.log(`[FileResolver] ✅ Resolved:`, {
      fileKey: key,
      fileUrl: res.data.FileUrl,
    });
    return res.data.FileUrl;
  }

  console.log(`[FileResolver] ❌ Failed to resolve:`, {
    fileKey: key,
    res,
  });

  // Resolve fail → thử fallback
  const fallback = getFallbackUrl(type);
  if (fallback) {
    console.log(`[FileResolver] ↩️ Use fallback for type=${type}`, {
      fallback,
    });
  }
  return fallback;
}

/** Helper: handle 1 cặp (path, output) cụ thể */
async function handleOnePath(
  clone: any,
  conf: FileResolveConfig,
  itemPath: string,
  outputPath: string
): Promise<void> {
  const fileKey = _get(clone, itemPath) as string | undefined;

  const fileUrl = await resolveFileKey(conf.type, fileKey);

  // Không có url & không có fallback -> thôi, không set gì cả,
  // nhưng nếu có fileKey thì vẫn xoá cho sạch
  if (!fileUrl) {
    if (fileKey !== undefined) {
      _unset(clone, itemPath);
    }
    return;
  }

  // Set ImageUrl (hoặc tương đương)
  _set(clone, outputPath, fileUrl);

  // Xoá luôn trường *FileKey sau khi resolve thành công hoặc fallback
  if (fileKey !== undefined) {
    _unset(clone, itemPath);
  }
}

/** Hỗ trợ path có [] cho array, ví dụ:
 *  - "ShowList[].MainImageFileKey"
 *  - "ShowList[].Podcaster.MainImageFileKey"
 */
async function applyConfigToData<T>(
  clone: T,
  conf: FileResolveConfig
): Promise<void> {
  // Case có wildcard []
  if (conf.path.includes("[].")) {
    const [arrayPath, restPath] = conf.path.split("[]."); // "ShowList", "MainImageFileKey"
    const arr: any[] = _get(clone as any, arrayPath);

    if (!Array.isArray(arr)) {
      console.warn(
        `[FileResolver] ⚠️ Expected array at "${arrayPath}" but got`,
        arr
      );
      return;
    }

    // Loop từng phần tử: ShowList[0], ShowList[1], ...
    for (let index = 0; index < arr.length; index++) {
      const itemPath =
        restPath && restPath.length > 0
          ? `${arrayPath}[${index}].${restPath}`
          : `${arrayPath}[${index}]`;

      const outputPath = conf.output.replace("[]", `[${index}]`);

      await handleOnePath(clone, conf, itemPath, outputPath);
    }
  } else {
    // Path phẳng, không wildcard
    await handleOnePath(clone, conf, conf.path, conf.output);
  }
}

/** Resolve toàn bộ object / section, trả về bản clone mới */
export async function resolveFiles<T>(
  data: T,
  configs: FileResolveConfig[]
): Promise<{ resolvedData: T; loading: boolean }> {
  // Deep clone để không mutate data từ RTK Query
  const clone: T =
    typeof structuredClone === "function"
      ? structuredClone(data)
      : (JSON.parse(JSON.stringify(data)) as T);

  for (const conf of configs) {
    await applyConfigToData(clone, conf);
  }

  return {
    resolvedData: clone,
    loading: false,
  };
}
