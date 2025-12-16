// src/store/downloadsSlice.ts
import { createSlice, createAsyncThunk, PayloadAction } from "@reduxjs/toolkit";
import {
  downloadAudio,
  removeAudio,
  getLocalUriIfExists,
  DlProg,
} from "@/src/lib/download";

type DownloadItem = {
  episodeId: string;
  localUri: string;
  size?: number;
  createdAt: number;
};
type DownloadsState = {
  items: Record<string, DownloadItem>;
  progress: Record<string, number>;
};

const initialState: DownloadsState = { items: {}, progress: {} };

export const startDownload = createAsyncThunk<
  { episodeId: string; localUri: string },
  { episodeId: string; url: string; ext?: string }
>("downloads/start", async ({ episodeId, url, ext = "mp3" }, { dispatch }) => {
  const res = await downloadAudio(url, episodeId, ext, (p: DlProg) => {
    dispatch(setProgress({ episodeId, progress: p.progress }));
  });
  return { episodeId, localUri: res.uri };
});

export const removeDownload = createAsyncThunk<void, { episodeId: string }>(
  "downloads/remove",
  async ({ episodeId }) => {
    await removeAudio(episodeId);
  }
);

const slice = createSlice({
  name: "downloads",
  initialState,
  reducers: {
    setProgress: (
      st,
      a: PayloadAction<{ episodeId: string; progress: number }>
    ) => {
      st.progress[a.payload.episodeId] = a.payload.progress;
    },
    hydrateLocal: (st, a: PayloadAction<DownloadItem[]>) => {
      for (const it of a.payload) st.items[it.episodeId] = it;
    },
  },
  extraReducers: (b) => {
    b.addCase(startDownload.fulfilled, (st, a) => {
      const { episodeId, localUri } = a.payload;
      st.items[episodeId] = { episodeId, localUri, createdAt: Date.now() };
      st.progress[episodeId] = 1;
    });
    b.addCase(removeDownload.fulfilled, (st, a: any) => {
      const id = a.meta.arg.episodeId;
      delete st.items[id];
      delete st.progress[id];
    });
  },
});
export const { setProgress, hydrateLocal } = slice.actions;
export default slice.reducer;
