// src/lib/download.ts
import * as FileSystem from "expo-file-system";
import { ensureAudioDir, filePathFromId } from "./fs";

export type DlProg = {
  progress: number;
  bytesWritten: number;
  totalBytes: number;
};

export async function getLocalUriIfExists(id: string, ext = "mp3") {
  const uri = filePathFromId(id, ext);
  const info = await FileSystem.getInfoAsync(uri);
  return info.exists ? uri : null;
}

export async function downloadAudio(
  url: string,
  id: string,
  ext = "mp3",
  onProgress?: (p: DlProg) => void
) {
  await ensureAudioDir();
  const fileUri = filePathFromId(id, ext);
  const exists = await FileSystem.getInfoAsync(fileUri);
  if (exists.exists) return { uri: fileUri, fromCache: true };

  const dl = FileSystem.createDownloadResumable(url, fileUri, {}, (p) => {
    if (!onProgress) return;
    const { totalBytesWritten, totalBytesExpectedToWrite } = p;
    onProgress({
      bytesWritten: totalBytesWritten,
      totalBytes: totalBytesExpectedToWrite,
      progress: totalBytesExpectedToWrite
        ? totalBytesWritten / totalBytesExpectedToWrite
        : 0,
    });
  });
  const res = await dl.downloadAsync();
  if (!res?.uri) throw new Error("Download failed");
  return { uri: res.uri, fromCache: false };
}

export async function removeAudio(id: string, ext = "mp3") {
  const fileUri = filePathFromId(id, ext);
  const info = await FileSystem.getInfoAsync(fileUri);
  if (info.exists) await FileSystem.deleteAsync(fileUri, { idempotent: true });
}
