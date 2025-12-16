import AutoResolvingImage from "@/src/components/autoResolveImage/AutoResolvingImage";
import { Text } from "@/src/components/ui/Text";
import { View } from "@/src/components/ui/View";
import { usePlayer } from "@/src/core/services/player/usePlayer";
import { RootState } from "@/src/store/store";
import { MaterialIcons } from "@expo/vector-icons";
import { Pressable, StyleSheet } from "react-native";
import { useDispatch, useSelector } from "react-redux";

const formatTime = (seconds: number): string => {
  const mins = Math.floor(seconds / 60);
  const secs = Math.floor(seconds % 60);
  return `${mins}:${secs < 10 ? "0" : ""}${secs}`;
};

const PlayerButtonUI = () => {
  const { state: UiState, play, pause, seekTo } = usePlayer();

  const handlePlayPausePress = async (e: any) => {
    e.stopPropagation();
    if (UiState.isPlaying) {
      await pause();
    } else {
      await play();
    }
  };

  const handleSeekPress = (e: any) => {
    e.stopPropagation();
    seekTo(UiState.currentTime + 10);
  };

  return (
    <View className="p-1" style={styles.container}>
      <View style={styles.imageContainer}>
        <AutoResolvingImage
          FileKey={UiState.currentAudio?.image || ""}
          type="PodcastPublicSource"
          style={styles.image}
        />
      </View>

      <View className="h-full flex justify-between items-start p-2 w-[60%]">
        <Text numberOfLines={1} className="text-white w-full font-semibold">
          {UiState.currentAudio?.name === undefined
            ? "Loading Audio Name..."
            : UiState.currentAudio?.name}
        </Text>
        <Text numberOfLines={1} className="text-sm text-gray-400 w-2/3">
          {UiState.currentAudio?.podcasterName === undefined
            ? "Loading Podcaster Name..."
            : UiState.currentAudio?.podcasterName}
        </Text>
      </View>

      {/* <View className="h-full flex flex-row items-center justify-center w-1/6  gap-1">
        <Text className="text-sm text-[#D9D9D9]">
          {formatTime(UiState.currentTime)}
        </Text>
        <Text className="text-sm text-[#D9D9D9]">
          / {formatTime(UiState.duration)}
        </Text>
      </View> */}

      {UiState.currentAudio && (
        <View style={styles.actionsContainer}>
          {UiState.isPlaying ? (
            <Pressable onPress={handlePlayPausePress}>
              <MaterialIcons name="pause" color="#fff" size={30} />
            </Pressable>
          ) : (
            <Pressable onPress={handlePlayPausePress}>
              <MaterialIcons name="play-arrow" color={"#fff"} size={30} />
            </Pressable>
          )}
          <Pressable onPress={handleSeekPress}>
            <MaterialIcons name="forward-10" color="#fff" size={30} />
          </Pressable>
        </View>
      )}
    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    flexDirection: "row",
    height: 60,
    alignItems: "center",
  },
  imageContainer: {
    height: 45,
    width: 45,
    alignItems: "center",
    justifyContent: "center",
  },
  image: {
    height: 40,
    width: 40,
    borderRadius: 8,
    resizeMode: "cover",
  },
  actionsContainer: {
    flex: 1,
    flexDirection: "row",
    alignItems: "center",
    justifyContent: "flex-end",
    gap: 15,
    marginRight: 10,
  },
});

export default PlayerButtonUI;
