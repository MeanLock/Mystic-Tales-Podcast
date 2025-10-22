// // src/redux/listeners/index.ts
// import { createListenerMiddleware, isAnyOf } from "@reduxjs/toolkit";
// import { type AppStartListening } from "@/redux/store";
// import { prefetchNextAudioUrl } from "@/core/services/audio/prefetch.service";

// // Ví dụ: lắng nghe player state để prefetch bài kế
// export const listenerMiddleware = createListenerMiddleware();

// export function startAppListeners(startListening: AppStartListening) {
//   // Ví dụ: khi state báo "near end" → prefetch URL audio kế
//   startListening({
//     predicate: (action, currentState, previousState) => {
//       const cur = (currentState as any).player;
//       const prev = (previousState as any).player;
//       return !!cur?.isNearlyEnd && cur?.isNearlyEnd !== prev?.isNearlyEnd;
//     },
//     effect: async (_action, listenerApi) => {
//       const s = listenerApi.getState() as any;
//       const nextFileKey = s.player?.nextTrack?.MainFileKey;
//       await prefetchNextAudioUrl(nextFileKey);
//     },
//   });

//   // Thêm các listeners khác: invalidate tags, refetch, v.v.
// }
