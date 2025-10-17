// src/features/mediaPlayer/playerMiddleware.ts
import { playerEngine } from "@/src/services/audio/playerEngine";
import {
  play,
  pause,
  nextTrack,
  stopAll,
  updatePosition,
  hydrate,
  enqueue,
  removeFromQueue,
  moveInQueueSwap,
} from "./playerSlice";
import type { RootState } from "@/src/store/store";
import type { Middleware } from "@reduxjs/toolkit";

export const playerMiddleware: Middleware<{}, RootState> =
  (store) => (next) => async (action) => {
    const result = next(action); // ⚠️ để reducer cập nhật state trước

    const state = store.getState().player;

    // Đồng bộ vị trí nghe (nếu muốn cập nhật từ engine → redux, thì gắn engine.onStatus ở chỗ init)
    switch (action.type) {
      case play.type: {
        if (state.currentAudio) {
          await playerEngine.playCurrent(state.currentAudio);
        }
        break;
      }
      case pause.type: {
        await playerEngine.pause();
        break;
      }
      case nextTrack.type: {
        // reducer đã đổi currentAudio → phát bài mới
        if (state.currentAudio) {
          await playerEngine.playCurrent(state.currentAudio);
        }
        break;
      }
      case stopAll.type: {
        await playerEngine.stop();
        break;
      }
      // Các thao tác queue không cần chạm engine trừ khi bạn tự động phát khi queue thay đổi.
      case hydrate.type:
      case enqueue.type:
      case removeFromQueue.type:
      case moveInQueueSwap.type:
        break;
    }

    return result;
  };
