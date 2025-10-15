import { createSlice, PayloadAction } from "@reduxjs/toolkit";

/** ===== Types ===== */
export type CurrentAudioType = {
  Id: string;
  Name: string;
  LatestPosition: number; // giây
  AudioLength: number; // giây
  MainFileKey: string;
  ImageUrl: string;
  PodcasterName: string;
  Show: { Id: string; Name: string };
} | null;

export type QueuedAudioType = {
  Index: number; // 0-based; luôn được chuẩn hoá
  Id: string;
  Name: string;
  AudioLength: number;
  MainFileKey: string;
  ImageUrl: string;
  PodcasterName: string;
  Show: { Id: string; Name: string };
};

export type PlayerMode = {
  playStatus: "playing" | "pause" | "stop";
  nextMode: "normal" | "show" | "favorites" | "downloads";
};

export type PlayerType = {
  playerMode: PlayerMode;
  currentAudio: CurrentAudioType;
  queueAudios: QueuedAudioType[];
};

/** ===== Helpers ===== */
const clamp = (n: number, min: number, max: number) =>
  Math.max(min, Math.min(max, n));

/** Chuẩn hoá & sắp xếp theo Index tăng dần, sau đó reindex 0..n-1 */
function normalizeQueue(queue: QueuedAudioType[]): QueuedAudioType[] {
  return queue
    .slice()
    .sort((a, b) => a.Index - b.Index)
    .map((q, i) => ({ ...q, Index: i }));
}

/** Tạo CurrentAudio từ QueuedAudio (về vị trí 0) */
function toCurrent(q: QueuedAudioType): NonNullable<CurrentAudioType> {
  return {
    Id: q.Id,
    Name: q.Name,
    LatestPosition: 0,
    AudioLength: q.AudioLength,
    MainFileKey: q.MainFileKey,
    ImageUrl: q.ImageUrl,
    PodcasterName: q.PodcasterName,
    Show: q.Show,
  };
}

/** ===== Initial ===== */
const initialState: PlayerType = {
  playerMode: { playStatus: "stop", nextMode: "normal" },
  currentAudio: null,
  queueAudios: [],
};

/** ===== Slice ===== */
const playerSlice = createSlice({
  name: "player",
  initialState,
  reducers: {

    // ACTION 1 //
    /** Play: có thể truyền audio mới; nếu không, tiếp tục play current */
    play(
      state,
      action: PayloadAction<
        { audio?: NonNullable<CurrentAudioType> } | undefined
      >
    ) {
      const audio = action?.payload?.audio;
      if (audio) {
        state.currentAudio = {
          ...audio,
          LatestPosition: clamp(
            audio.LatestPosition ?? 0,
            0,
            audio.AudioLength
          ),
        };
      } else if (!state.currentAudio) {
        // Không có current thì nếu queue có bài -> auto lấy bài đầu hàng đợi
        if (state.queueAudios.length > 0) {
          const first = normalizeQueue(state.queueAudios)[0];
          state.currentAudio = toCurrent(first);
          state.queueAudios = normalizeQueue(
            state.queueAudios.filter((q) => q.Id !== first.Id)
          );
        } else {
          // không có gì để play
          state.playerMode.playStatus = "stop";
          return;
        }
      }
      state.playerMode.playStatus = "playing";
    },

    // ACTION 2 //
    /** Pause (dừng tạm) */
    pause(state) {
      if (state.playerMode.playStatus === "playing") {
        state.playerMode.playStatus = "pause";
      }
    },

    // ACTION 3 //
    /** Cập nhật vị trí nghe của current (giây) */
    updatePosition(state, action: PayloadAction<{ position: number }>) {
      if (!state.currentAudio) return;
      state.currentAudio.LatestPosition = clamp(
        action.payload.position,
        0,
        state.currentAudio.AudioLength
      );
    },

    // ACTION 4 //
    /** Sang bài tiếp theo: lấy item có Index nhỏ nhất trong queue */
    nextTrack(state) {
      if (state.queueAudios.length === 0) {
        // Không còn bài nào -> đặt stop nếu current cũng không có
        if (!state.currentAudio) state.playerMode.playStatus = "stop";
        return;
      }
      const normalized = normalizeQueue(state.queueAudios);
      const next = normalized[0];
      state.currentAudio = toCurrent(next);
      state.queueAudios = normalizeQueue(
        normalized.filter((q) => q.Id !== next.Id)
      );
      state.playerMode.playStatus = "playing";
    },

    // ACTION 5 //
    /** Đổi nextMode (normal/show/favorites/downloads) */
    setNextMode(state, action: PayloadAction<PlayerMode["nextMode"]>) {
      state.playerMode.nextMode = action.payload;
    },

    // ACTION 6 //
    /** Thêm 1 audio vào queue (mặc định đẩy cuối hàng) */
    enqueue(
      state,
      action: PayloadAction<Omit<QueuedAudioType, "Index"> & { index?: number }>
    ) {
      const { index, ...audio } = action.payload;
      const q = normalizeQueue(state.queueAudios);
      const insertAt = index != null ? clamp(index, 0, q.length) : q.length;
      const newItem: QueuedAudioType = { ...audio, Index: insertAt };

      const before = q.slice(0, insertAt);
      const after = q
        .slice(insertAt)
        .map((item) => ({ ...item, Index: item.Index + 1 }));

      state.queueAudios = normalizeQueue([...before, newItem, ...after]);
    },

    // ACTION 7 //
    /** Xoá 1 audio khỏi queue theo Id (hoặc Index) */
    removeFromQueue(
      state,
      action: PayloadAction<{ id?: string; index?: number }>
    ) {
      const { id, index } = action.payload;
      let q = state.queueAudios;
      if (id) q = q.filter((x) => x.Id !== id);
      else if (index != null) q = q.filter((x) => x.Index !== index);
      state.queueAudios = normalizeQueue(q);
    },

    // ACTION 8 //
    /** Di chuyển vị trí trong queue: fromIndex -> toIndex */
    moveInQueue(
      state,
      action: PayloadAction<{ fromIndex: number; toIndex: number }>
    ) {
      const { fromIndex, toIndex } = action.payload;
      const q = normalizeQueue(state.queueAudios);
      if (
        fromIndex < 0 ||
        fromIndex >= q.length ||
        toIndex < 0 ||
        toIndex >= q.length
      )
        return;

      const item = q[fromIndex];
      const without = q.filter((_, i) => i !== fromIndex);
      const insertAt = clamp(toIndex, 0, without.length);

      const before = without.slice(0, insertAt);
      const after = without.slice(insertAt);

      state.queueAudios = normalizeQueue([...before, { ...item }, ...after]);
    },

    // ACTION 9 //
    /** Stop toàn bộ (xoá current + queue, set stop) */
    stopAll(state) {
      state.currentAudio = null;
      state.queueAudios = [];
      state.playerMode.playStatus = "stop";
    },

    // ACTION 10 //
    /** (tuỳ chọn) Hydrate toàn bộ state từ persisted/server */
    hydrate(state, action: PayloadAction<PlayerType>) {
      state.playerMode = action.payload.playerMode;
      state.currentAudio = action.payload.currentAudio;
      state.queueAudios = normalizeQueue(action.payload.queueAudios);
    },
  },
});

export const {
  play,
  pause,
  updatePosition,
  nextTrack,
  setNextMode,
  enqueue,
  removeFromQueue,
  moveInQueue,
  stopAll,
  hydrate,
} = playerSlice.actions;

export default playerSlice.reducer;
