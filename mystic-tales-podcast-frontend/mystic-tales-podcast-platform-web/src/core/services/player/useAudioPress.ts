import { useEffect, useState } from "react";
import { getAudioEngine } from "./playerBridge";

export function useAudioProgress(pollMs = 250) {
  const [time, setTime] = useState(0);
  const [dur, setDur] = useState(0);

  useEffect(() => {
    const engine = getAudioEngine();

    const onTick = () => {
      setTime(engine.getCurrentTime());
      const d = engine.getDuration();
      if (Number.isFinite(d) && d > 0) setDur(d);
    };

    const id = setInterval(onTick, pollMs);
    onTick(); // tick đầu
    return () => clearInterval(id);
  }, [pollMs]);

  return { currentTime: time, duration: dur };
}
