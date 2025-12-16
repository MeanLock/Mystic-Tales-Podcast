import { Audio, InterruptionModeAndroid, InterruptionModeIOS } from "expo-av";
import { useEffect } from "react";
import { usePlayer } from "../core/services/player/usePlayer";
import { useSelector } from "react-redux";
import { RootState } from "../store/store";

const SetUp = () => {
  const { loadFromLatestListenSessionAndPlay } = usePlayer();
  const user = useSelector((state: RootState) => state.auth.user);
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
    if (user) {
      loadFromLatestListenSessionAndPlay();
    }
  }, [user]);
  return <></>;
};

export default SetUp;
