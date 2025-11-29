import { useEffect } from "react";

import { setupAudioMode } from "../lib/audio-init";
import { playerEngine } from "@/src/services/audio/playerEngine";
import { store } from "@/src/store/store";

const SetUp = () => {
  playerEngine.onStatus = (s) => {};
  return <></>;
};

export default SetUp;
