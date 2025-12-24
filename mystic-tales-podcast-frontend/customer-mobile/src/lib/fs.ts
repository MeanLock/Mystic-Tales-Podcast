// src/lib/fs.ts
import { Paths, getInfoAsync, makeDirectoryAsync } from "expo-file-system";

const DOC_DIR = Paths.document ?? Paths.cache;

if (!DOC_DIR) {
  throw new Error("No app-scoped directory available");
}

export const AUDIO_DIR = `${DOC_DIR}audio/`;

export async function ensureAudioDir() {
  const info = await getInfoAsync(AUDIO_DIR);
  if (!info.exists) {
    await makeDirectoryAsync(AUDIO_DIR, { intermediates: true });
  }
}

export function filePathFromId(id: string, ext = "mp3") {
  return `${AUDIO_DIR}${id}.${ext}`;
}
