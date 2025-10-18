import { Text } from "@/src/components/ui/Text";
import { View } from "@/src/components/ui/View";
import { pause, play, seekBy } from "@/src/features/mediaPlayer/playerSlice";
import { RootState } from "@/src/store/store";
import { MaterialIcons } from "@expo/vector-icons";
import { Pressable, StyleSheet } from "react-native";
import { Image } from "react-native";
import { useDispatch, useSelector } from "react-redux";

const PlayerButtonUI = () => {
  const playerState = useSelector((state: RootState) => state.player);
  const dispatch = useDispatch();

  const handlePausePress = () => {
    dispatch(pause());
  };

  const handlePlayPress = () => {
    dispatch(play());
  };

  return (
    <View className="px-1" style={styles.container}>
      <View style={styles.imageContainer}>
        <Image
          style={styles.image}
          source={{ uri: playerState.currentAudio?.ImageUrl }}
        />
      </View>

      <View className="h-full flex justify-between items-start p-2 w-3/4">
        <Text numberOfLines={1} className="text-white w-3/4">
          {playerState.currentAudio?.Name}
        </Text>
        <Text numberOfLines={1} className="text-sm text-gray-400 w-3/4">
          {playerState.currentAudio?.PodcasterName}
        </Text>
      </View>

      <View style={styles.actionsContainer}>
        {playerState.playerMode.playStatus === "playing" ? (
          <Pressable onPress={handlePausePress}>
            <MaterialIcons name="pause" color="#fff" size={30} />
          </Pressable>
        ) : (
          <Pressable onPress={handlePlayPress}>
            <MaterialIcons name="play-arrow" color={"#fff"} size={30} />
          </Pressable>
        )}
        <Pressable>
          <MaterialIcons
            onPress={() => dispatch(seekBy({ delta: -10 }))}
            name="forward-10"
            color="#fff"
            size={30}
          />
        </Pressable>
      </View>
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
    height: 60,
    width: 60,
    alignItems: "center",
    justifyContent: "center",
  },
  image: {
    height: 50,
    width: 50,
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
