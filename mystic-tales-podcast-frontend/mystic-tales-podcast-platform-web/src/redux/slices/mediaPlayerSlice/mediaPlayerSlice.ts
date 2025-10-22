import type {
  AddToQueuePayload,
  CurrentAudioUI,
  PlayMode,
  QueuedAudio,
  QueuedAudioWithNoIndex,
} from "@/core/types/audio";
import { createSlice, type PayloadAction } from "@reduxjs/toolkit";

interface MediaPlayerSlice {
  playMode: PlayMode;
  currentAudio: CurrentAudioUI;
  queueAudios: QueuedAudio[];
}

const initialState: MediaPlayerSlice = {
  playMode: {
    nextMode: "normal",
    playStatus: "stop",
  },
  currentAudio: null,
  queueAudios: [],
};

function reindexQueue(q: QueuedAudio[]) {
  for (let i = 0; i < q.length; i++) q[i].Index = i;
}

function clamp(n: number, min: number, max: number) {
  return Math.max(min, Math.min(n, max));
}

const mediaPlayerSlice = createSlice({
  name: "player",
  initialState: initialState,
  reducers: {
    // Action1: Phát Audio
    // - Nếu playload là null => chỉ chuyển trạng thái playStatus thành play
    // - Nếu có payload => chuyển audio kia ra, và đưa audio mới vào phát ngay
    playAudio(state, action: PayloadAction<CurrentAudioUI | null>) {
      if (action.payload) {
        state.currentAudio = action.payload;
        state.playMode.playStatus = "play";
      } else {
        state.playMode.playStatus = "play";
      }
    },

    // Action2: Tạm dừng Audio
    pauseAudio(state) {
      state.playMode.playStatus = "pause";
    },

    // Action3: Dừng media player
    // => dừng phát, xóa currentAudio và queueAudios
    stopAudio(state) {
      state.playMode.playStatus = "stop";
      state.currentAudio = null;
      state.queueAudios = [];
    },

    // Action4: Thêm audio vào queue
    // position: to-top | to-last
    addToQueue(state, action: PayloadAction<AddToQueuePayload>) {
      const { audio, position } = action.payload;
      if (position === "to-top") {
        state.queueAudios.unshift({ Index: 0, ...audio });
      } else {
        const nextIndex = state.queueAudios.length;
        state.queueAudios.push({ Index: nextIndex, ...audio });
      }
      state.queueAudios.forEach((item, i) => {
        item.Index = i;
      });
    },

    // Action5: Xóa audio khỏi queue
    removeFromQueue(state, action: PayloadAction<{ id: string }>) {
      state.queueAudios = state.queueAudios.filter(
        (x) => x.Id !== action.payload.id
      );
      reindexQueue(state.queueAudios);
    },

    // Action6: Cập nhật vị trí của các audio trong queue
    // from: vị trí cũ, to: vị trí mới
    // đưa from đến to, các audio khác dịch chuyển cho phù hợp
    moveInQueue(state, action: PayloadAction<{ from: number; to: number }>) {
      const len = state.queueAudios.length;
      if (len <= 1) return;

      let { from, to } = action.payload;
      from = clamp(from, 0, len - 1);
      to = clamp(to, 0, len - 1);

      if (from === to) return;

      const [item] = state.queueAudios.splice(from, 1);
      state.queueAudios.splice(to, 0, item);
      reindexQueue(state.queueAudios);
    },

    // Action7: Set Next Mode
    setNextMode(state, action: PayloadAction<PlayMode["nextMode"]>) {
      state.playMode.nextMode = action.payload;
    },

    // Action8: Next Audio
    // Check playMode.nextMode để quyết định next thế nào
    // Case1: nextMode === "normal": 
    //   - Nếu có audio trong queue -> phát audio đầu tiên trong queue (đưa nó lên làm current), xóa nó khỏi queue
    //   - Nếu không có audio trong queue -> dừng phát (stop luôn)

    // Case 2: nextMode === "show":
    //   - Nếu có audio trong queue -> phát audio đầu tiên trong queue (đưa nó lên làm current), xóa nó khỏi queue
    //   - Nếu không có audio trong queue -> gọi API getNextInShow(currentAudio.Id) để lấy audio tiếp theo trong show, nếu có thì phát, nếu không có thì dừng phát

    // Case 3: nextMode === "saved":
    //   - Nếu có audio trong queue -> phát audio đầu tiên trong queue (đưa nó lên làm current), xóa nó khỏi queue
    //   - Tương tự case "show", nhưng gọi API getNextInSaved(currentAudio.Id)
    
    // Case 4: nextMode === "bookings":
    //   - Nếu có audio trong queue -> phát audio đầu tiên trong queue (đưa nó lên làm current), xóa nó khỏi queue
    //   - Nếu không có audio trong queue -> dừng phát (stop luôn) (tạm thời chưa implement API getNextInBookings)

    nextAudio(state) {}
  },
});

export const {
  playAudio,
  pauseAudio,
  stopAudio,
  addToQueue,
  removeFromQueue,
  setNextMode,
} = mediaPlayerSlice.actions;
