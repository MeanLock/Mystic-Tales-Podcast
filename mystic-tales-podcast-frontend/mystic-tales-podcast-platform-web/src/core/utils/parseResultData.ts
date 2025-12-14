// src/core/utils/parseResultData.ts
export function parseResultData<T = unknown>(raw?: string): T | undefined {
  if (!raw) return undefined;
  try {
    // Một số BE trả kèm \r\n hoặc JSON lồng; xử lý nhẹ cho an toàn.
    const cleaned = raw.replace(/\r?\n/g, "").trim();
    return JSON.parse(cleaned) as T;
  } catch {
    return undefined as any;
  }
}
