import { Audio, InterruptionModeAndroid, InterruptionModeIOS } from "expo-av";
import { useEffect } from "react";

const SetUp = () => {
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
  return <></>;
};

export default SetUp;
