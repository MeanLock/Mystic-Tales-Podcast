// src/lib/audio-init.ts
import { Audio, InterruptionModeAndroid, InterruptionModeIOS } from "expo-av";

export async function setupAudioMode() {
  await Audio.setAudioModeAsync({
    playsInSilentModeIOS: true,
    staysActiveInBackground: true,

    // dùng enum mới:
    interruptionModeIOS: InterruptionModeIOS.DoNotMix,
    interruptionModeAndroid: InterruptionModeAndroid.DoNotMix,

    shouldDuckAndroid: true,
    playThroughEarpieceAndroid: false,
  });
}
