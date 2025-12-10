import { pauseAudio } from "@/src/features/mediaPlayer/playerSlice";
import { RootState } from "@/src/store/store";
import { MaterialIcons } from "@expo/vector-icons";
import { Pressable, StyleSheet, Text } from "react-native";
import { Dimensions } from "react-native";
import { useDispatch, useSelector } from "react-redux";
import { EqualizerVariant1 } from "../../equalizer/Variant1";

const formatAudioLength = (seconds: number): string => {
  if (!seconds || seconds <= 0) {
    return "0 sec";
  }
  const hours = Math.floor(seconds / 3600);
  const minutes = Math.floor((seconds % 3600) / 60);
  const secs = seconds % 60;
  let result = "";
  if (hours > 0) {
    return `${hours}h`;
  }
  if (minutes > 0) {
    return `${minutes}m`;
  }
  if (secs > 0) {
    return `${secs}s`;
  }
  return result;
};

const PlayButtonVariant1 = ({
  episodeId,
  audioLength,
  onPlayPress,
}: {
  episodeId: string;
  audioLength: number;
  onPlayPress: () => void;
}) => {
  // REDUX Play Control
  const player = useSelector((state: RootState) => state.player);

  // HOOKS
  const dispatch = useDispatch();
  if (
    player.playMode.playStatus === "play" &&
    player.currentAudio?.Id === episodeId
  ) {
    return (
      <Pressable
        style={styles.playButton}
        onPress={() => dispatch(pauseAudio())}
      >
        <EqualizerVariant1 color="#AEE339" />
        <Text className="text-[#AEE339] text-xs font-bold">
          {formatAudioLength(audioLength)}
        </Text>
      </Pressable>
    );
  } else {
    return (
      <Pressable style={styles.playButton} onPress={onPlayPress}>
        {/* <EqualizerVariant1 color="#AEE339" /> */}
        <MaterialIcons name="play-arrow" size={17} color="#AEE339" />
        <Text className="text-[#AEE339] text-xs font-bold">
          {formatAudioLength(audioLength)}
        </Text>
      </Pressable>
    );
  }
};
export default PlayButtonVariant1;

const styles = StyleSheet.create({
  episodeCardContainer: {
    width: "100%",
    flexDirection: "row",
    alignItems: "center",
    paddingVertical: 8,
    height: Dimensions.get("window").width * 0.2,
    gap: 10,
  },
  informationContainer: {
    flex: 1,
    height: Dimensions.get("window").width * 0.2,
    justifyContent: "space-between",
    paddingRight: 10,
  },
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
