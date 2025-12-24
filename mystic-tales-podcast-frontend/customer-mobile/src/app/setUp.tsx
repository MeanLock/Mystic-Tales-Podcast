import { Audio, InterruptionModeAndroid, InterruptionModeIOS } from "expo-av";
import { useEffect, useRef } from "react";
import { usePlayer } from "../core/services/player/usePlayer";
import { useSelector } from "react-redux";
import { RootState } from "../store/store";

const SetUp = () => {
  const { loadFromLatestListenSessionAndPlay } = usePlayer();
  const user = useSelector((state: RootState) => state.auth.user);
  const hasLoadedSession = useRef(false);

  useEffect(() => {
    Audio.setAudioModeAsync({
      allowsRecordingIOS: false,
      staysActiveInBackground: true,
      playsInSilentModeIOS: true,
      // những dòng quan trọng nè
      shouldDuckAndroid: true,
      interruptionModeAndroid: InterruptionModeAndroid.DoNotMix,
      interruptionModeIOS: InterruptionModeIOS.DoNotMix,
      playThroughEarpieceAndroid: false,
    });
  }, []);

  useEffect(() => {
    if (user && !hasLoadedSession.current) {
      if (__DEV__) {
        console.log("[SetUp] Loading latest session for user:", user.Email);
      }
      loadFromLatestListenSessionAndPlay();
      hasLoadedSession.current = true;
    }
  }, [user, loadFromLatestListenSessionAndPlay]);

  return <></>;
};

export default SetUp;
