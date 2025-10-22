import { useEffect } from "react";

import { setupAudioMode } from "../lib/audio-init";
import { playerEngine } from "@/src/services/audio/playerEngine";
import { store } from "@/src/store/store";
import { updatePosition } from "@/src/features/mediaPlayer/playerSlice";



const SetUp = () => {
  playerEngine.onStatus = (s) => {
  // push progress về Redux mỗi ~250ms
  store.dispatch(updatePosition({ position: Math.floor((s.positionMillis ?? 0) / 1000) }));
};
  return <></>;
};

export default SetUp;
