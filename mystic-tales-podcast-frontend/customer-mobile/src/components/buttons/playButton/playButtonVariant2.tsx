import { pauseAudio } from "@/src/features/mediaPlayer/playerSlice";
import { RootState } from "@/src/store/store";
import { MaterialIcons } from "@expo/vector-icons";
import { Pressable, StyleSheet, Text } from "react-native";
import { useDispatch, useSelector } from "react-redux";
import { EqualizerVariant1 } from "../../equalizer/Variant1";
import { usePlayer } from "@/src/core/services/player/usePlayer";

interface PlayButtonProps {
  episodeId: string;
  audioLength: number;
  onPlayPress: () => void;
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
  episodeId,
  audioLength,
  onPlayPress,
}: PlayButtonProps) => {
  // REDUX Play Control
  const player = useSelector((state: RootState) => state.player);

  // HOOKS
  const dispatch = useDispatch();
  const { checkIsCurrentPlay } = usePlayer();
  if (checkIsCurrentPlay(episodeId)) {
    return (
      <Pressable
        style={style.playButton}
        onPress={() => dispatch(pauseAudio())}
        className="w-8/12"
      >
        <EqualizerVariant1 color="#AEE339" />
        <Text className="text-[#AEE339] text-xs font-bold">
          {formatAudioLength(audioLength)}
        </Text>
      </Pressable>
    );
  } else {
    return (
      <Pressable
        style={style.playButton}
        className="w-8/12"
        onPress={onPlayPress}
      >
        <MaterialIcons name="play-arrow" size={17} color="#AEE339" />
        <Text
          numberOfLines={1}
          className="text-[10px] font-bold text-[#AEE339]"
        >
          {formatAudioLength(audioLength)}
        </Text>
      </Pressable>
    );
  }
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
    gap: 5,
  },
});
