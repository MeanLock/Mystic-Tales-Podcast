import { useEffect } from "react";

import { setupAudioMode } from "../lib/audio-init";

const SetUp = () => {
  useEffect(() => {
    setupAudioMode().catch(console.warn);
  }, []);

  return <></>;
};

export default SetUp;
