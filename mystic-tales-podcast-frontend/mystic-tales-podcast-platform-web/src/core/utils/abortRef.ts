// src/core/utils/abortRef.ts
export function createAbortRef() {
  const controller = new AbortController();
  return { controller, signal: controller.signal, abort: () => controller.abort() };
}
