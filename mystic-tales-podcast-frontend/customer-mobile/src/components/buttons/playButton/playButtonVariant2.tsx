import { pauseAudio } from "@/src/features/mediaPlayer/playerSlice";
import { RootState } from "@/src/store/store";
import { MaterialIcons } from "@expo/vector-icons";
import {
  ActivityIndicator,
  Pressable,
  StyleSheet,
  Text,
  View,
} from "react-native";
import { useRef, useCallback } from "react";
import { useDispatch, useSelector } from "react-redux";
import { EqualizerVariant1 } from "../../equalizer/Variant1";
import { usePlayer } from "@/src/core/services/player/usePlayer";

interface PlayButtonProps {
  audioId: string;
  audioLength: number;
  onPlayPress: () => void;
  stopPropagation?: boolean;
}

const formatAudioLength = (seconds: number): string => {
  if (!seconds || seconds <= 0) {
    return "0 sec";
  }

  const hours = Math.floor(seconds / 3600);
  const minutes = Math.floor((seconds % 3600) / 60);
  const remainingSeconds = Math.floor(seconds % 60);

  // Format based on the length:
  // 1. If hours > 0: "X hours Y mins"
  // 2. If only minutes > 0: "X mins Y secs"
  // 3. If only seconds > 0: "X secs"

  if (hours > 0) {
    return `${hours} ${hours === 1 ? "hour" : "hours"} ${
      minutes > 0 ? `${minutes} ${minutes === 1 ? "min" : "mins"}` : ""
    }`.trim();
  } else if (minutes > 0) {
    return `${minutes} ${minutes === 1 ? "min" : "mins"} ${
      remainingSeconds > 0
        ? `${remainingSeconds} ${remainingSeconds === 1 ? "sec" : "secs"}`
        : ""
    }`.trim();
  } else {
    return `${remainingSeconds} ${remainingSeconds === 1 ? "sec" : "secs"}`;
  }
};

const PlayButtonVariant2 = ({
  audioId,
  audioLength,
  onPlayPress,
  stopPropagation = false,
}: PlayButtonProps) => {
  // HOOKS
  const { state: uiState } = usePlayer();
  const debounceTimerRef = useRef<number | null>(null);

  const handlePress = useCallback(
    (event: any) => {
      if (stopPropagation) {
        event.stopPropagation();
      }

      // Clear previous timer if exists
      if (debounceTimerRef.current) {
        clearTimeout(debounceTimerRef.current);
      }

      // Set new timer for debounce
      debounceTimerRef.current = setTimeout(() => {
        onPlayPress();
        debounceTimerRef.current = null;
      }, 1000);
    },
    [onPlayPress, stopPropagation]
  );

  return (
    <Pressable
      style={style.playButton}
      className="w-8/12"
      onPress={handlePress}
    >
      {uiState.isAudioLoading && uiState.loadingAudioId === audioId ? (
        // Đang load và chính nó đang load
        <ActivityIndicator size="small" color="#AEE339" />
      ) : uiState.isAudioLoading ? (
        // Đang load nhưng không phải nó
        <MaterialIcons name="play-disabled" size={17} color="#d9d9d9" />
      ) : uiState.currentAudio && uiState.currentAudio.id === audioId ? (
        // Đây là currentAudio - hiển thị progress bar
        <View className="flex flex-row items-center gap-2 h-[17px]">
          {uiState.isPlaying ? (
            <EqualizerVariant1 color="#aee339" key={audioId} />
          ) : (
            <MaterialIcons name="play-arrow" size={17} color="#AEE339" />
          )}
          <View className="w-8 h-[4px] rounded-full bg-slate-50 flex flex-row items-center justify-start overflow-hidden">
            <View
              style={{
                width: `${(uiState.currentTime / uiState.duration) * 100}%`,
              }}
              className="h-[4px] rounded-l-full bg-[#aee339]"
            ></View>
          </View>
        </View>
      ) : (
        // Không phải currentAudio
        <MaterialIcons name="play-arrow" size={17} color="#AEE339" />
      )}

      <Text
        numberOfLines={1}
        className={`text-[10px] font-bold ${
          uiState.isAudioLoading && uiState.loadingAudioId !== audioId
            ? "text-[#d9d9d9]"
            : "text-[#aee339]"
        }`}
      >
        {formatAudioLength(audioLength)}
      </Text>
    </Pressable>
  );
};

export default PlayButtonVariant2;

const style = StyleSheet.create({
  playButton: {
    borderRadius: 999,
    paddingVertical: 5,
    paddingHorizontal: 12,
    backgroundColor: "rgba(217, 217, 217, 0.2)",
    flexDirection: "row",
    alignItems: "center",
    justifyContent: "center",
    gap: 7,
  },
});
