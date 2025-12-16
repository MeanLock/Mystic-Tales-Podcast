import AutoResolvingImage from "@/src/components/autoResolveImage/AutoResolvingImage";
import PlayButtonVariant1 from "@/src/components/buttons/playButton/playButtonVariant1";
import { Episode } from "@/src/core/types/episode.type";
import { playAudio } from "@/src/features/mediaPlayer/playerSlice";
import { MaterialIcons } from "@expo/vector-icons";
import { Dimensions, Pressable, StyleSheet, Text, View } from "react-native";
import { useDispatch } from "react-redux";

const calculateReleaseDate = (releaseDate: string): string => {
  const date = new Date(releaseDate);
  const now = new Date();
  const diffMs = now.getTime() - date.getTime();
  const diffDays = Math.floor(diffMs / (1000 * 60 * 60 * 24));
  if (diffDays < 1) {
    return "Today";
  } else if (diffDays === 1) {
    return "Yesterday";
  } else {
    return `${diffDays} days ago`;
  }
};

const formatAudioLength = (seconds: number): string => {
  if (!seconds || seconds <= 0) {
    return "0 sec";
  }
  const hours = Math.floor(seconds / 3600);
  const minutes = Math.floor((seconds % 3600) / 60);
  const secs = seconds % 60;
  let result = "";
  if (hours > 0) {
    result += `${hours} hr `;
  }
  if (minutes > 0) {
    result += `${minutes} min `;
  }
  if (secs > 0) {
    result += `${secs} sec`;
  }
  return result;
};

const EpisodeCard = ({ episode }: { episode: Episode }) => {
  const dispatch = useDispatch();
  const handlePlayEpisodeFromSavedCard = () => {
    dispatch(
      playAudio({
        sourceType: "SavedEpisodes",
        audioId: episode.Id,
      })
    );
  };
  return (
    <View style={styles.episodeCardContainer}>
      <View className="aspect-square rounded-md relative overflow-hidden">
        <AutoResolvingImage
          FileKey={episode.MainImageFileKey}
          type="PodcastPublicSource"
          style={{
            height: Dimensions.get("window").width * 0.2,
            width: Dimensions.get("window").width * 0.2,
            resizeMode: "cover",
            aspectRatio: 1,
          }}
        />
      </View>
      <View style={styles.informationContainer}>
        <Text className="text-gray-400 text-sm mt-1">
          {calculateReleaseDate(episode.ReleaseDate)}
        </Text>
        <Text className="text-white text-lg" numberOfLines={2}>
          {episode.Name}
        </Text>
        <View className="flex-1 flex flex-row items-center mt-2">
          <PlayButtonVariant1
            episodeId={episode.Id}
            audioLength={episode.AudioLength}
            onPlayPress={handlePlayEpisodeFromSavedCard}
          />
        </View>
      </View>
    </View>
  );
};

export default EpisodeCard;

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
    backgroundColor: "rgba(174, 227, 57, 0.1)",
    flexDirection: "row",
    alignItems: "center",
    justifyContent: "center",
    gap: 5,
  },
});
