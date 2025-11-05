// src/core/utils/backoff.ts
export const sleep = (ms: number) => new Promise((r) => setTimeout(r, ms));
export const jitter = (ms: number) =>
  Math.round(ms * (0.75 + Math.random() * 0.5));
export const backoffMs = (base: number, attempt: number) =>
  Math.min(base * Math.max(1, attempt), base * 4);
