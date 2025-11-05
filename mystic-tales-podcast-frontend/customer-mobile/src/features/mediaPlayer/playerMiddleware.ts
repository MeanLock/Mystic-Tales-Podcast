// src/features/mediaPlayer/playerMiddleware.ts
import { playerEngine } from "@/src/services/audio/playerEngine";
import {
  play,
  pause,
  nextTrack,
  stopAll,
  hydrate,
  enqueue,
  removeFromQueue,
  moveInQueueSwap,
  seekBy,
  seekTo,
  onEnded,
  seekPreview,
} from "./playerSlice";
import type { RootState } from "@/src/store/store";
import type { Middleware, UnknownAction } from "@reduxjs/toolkit";

// Type guard: kiểm tra có .type không
const isAction = (a: unknown): a is UnknownAction =>
  typeof a === "object" && a !== null && "type" in a;

export const playerMiddleware: Middleware<{}, RootState> =
  (store) => (next) => (action) => {
    const result = next(action); // để reducer cập nhật state trước

    // đảm bảo chỉ xử lý nếu là action hợp lệ
    if (!isAction(action)) return result;

    const state = store.getState().player;

    // chạy side-effects bất đồng bộ trong IIFE,
    // để middleware vẫn trả về kiểu 'unknown' đồng bộ.
    (async () => {
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
          if (state.currentAudio) {
            await playerEngine.playCurrent(state.currentAudio);
          }
          break;
        }
        case stopAll.type: {
          await playerEngine.stop();
          break;
        }
        case seekPreview.type: {
          // UI-only, không chạm engine
          break;
        }
        case seekBy.type:
        case seekTo.type: {
          if (state.currentAudio) {
            await playerEngine.seek(state.currentAudio.LatestPosition);
          }
          break;
        }
        case onEnded.type: {
          console.log(
            "[mw] onEnded -> play?",
            !!state.currentAudio,
            state.playerMode.playStatus
          );
          if (state.currentAudio && state.playerMode.playStatus === "playing") {
            await playerEngine.playCurrent(state.currentAudio);
          } else if (state.playerMode.playStatus === "stop") {
            await playerEngine.stop();
          }
          break;
        }
        case hydrate.type:
        case enqueue.type:
        case removeFromQueue.type:
        case moveInQueueSwap.type: {
          // nếu bạn có logic preload head queue thì gọi ở đây
          // await playerEngine.preload(head.Id, head.MainFileKey)
          break;
        }
      }
    })();

    return result; // <- đồng bộ, đúng kiểu 'unknown'
  };
