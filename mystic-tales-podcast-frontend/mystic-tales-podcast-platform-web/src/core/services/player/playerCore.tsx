import { useEffect, useRef } from "react";
import { useDispatch, useSelector } from "react-redux";
import type { RootState } from "@/redux/store";
import { getAudioEngine } from "./audioEngine";
import {
  pauseAudio,
  playAudio,
  nextAudio,
} from "@/redux/slices/mediaPlayerSlice/mediaPlayerSlice";

export default function PlayerCore() {
  const dispatch = useDispatch();
  const { currentAudio, playMode } = useSelector((s: RootState) => s.player);
  const engineRef = useRef(getAudioEngine());

  // Attach listeners (time/duration/end)
  useEffect(() => {
    const engine = engineRef.current;
    engine.attachListeners({
      ended: () => {
        // khi bài hát kết thúc, gọi next
        dispatch(nextAudio());
      },
      // Bỏ canplay listener để không tự động play
    });
    return () => engine.detachListeners();
  }, [dispatch]);

  // Khi đổi bài (currentAudio)
  // Chỉ load, không auto-play (để useEffect playStatus xử lý)
  useEffect(() => {
    if (!currentAudio) return;
    const engine = engineRef.current;
    engine.load(currentAudio.FileUrl);
  }, [currentAudio]);

  // Play/Pause từ Redux
  useEffect(() => {
    const engine = engineRef.current;
    if (playMode.playStatus === "play") {
      engine.play().catch(() => void 0);
    } else if (playMode.playStatus === "pause") {
      engine.pause();
    } else if (playMode.playStatus === "stop") {
      engine.pause();
      // có thể clear src nếu muốn
    }
  }, [playMode.playStatus]);

  // Volume (Redux 0..100 -> audio 0..1)
  useEffect(() => {
    const engine = engineRef.current;
    engine.setVolume((playMode.volume ?? 100) / 100);
  }, [playMode.volume]);

  return null; // không render UI
}
