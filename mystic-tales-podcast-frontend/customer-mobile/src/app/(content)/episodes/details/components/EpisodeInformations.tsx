import AutoResolvingImageBackground from "@/src/components/autoResolveImage/AutoResolveImageBackground";
import AutoResolvingImage from "@/src/components/autoResolveImage/AutoResolvingImage";
import { EqualizerVariant1 } from "@/src/components/equalizer/Variant1";
import { EqualizerVariant2 } from "@/src/components/equalizer/Variant2";
import Loader from "@/src/components/loaders/Loader";
import { Text } from "@/src/components/ui/Text";
import { View } from "@/src/components/ui/View";
import { usePlayer } from "@/src/core/services/player/usePlayer";
import { EpisodeDetails } from "@/src/core/types/episode.type";
import TimeUtil from "@/src/core/utils/time";
import { formatAudioLength, formatDateRange } from "@/src/lib/format";

import { Feather, MaterialIcons, Octicons } from "@expo/vector-icons";
import { useRouter } from "expo-router";

import { Platform, Pressable, StyleSheet } from "react-native";
import { useDispatch } from "react-redux";

const EpisodeInformations = ({
  episode,
  isSaved,
  onSaveToggle,
  onReport,
}: {
  episode: EpisodeDetails;
  isSaved: boolean;
  onSaveToggle: () => void;
  onReport: () => void;
}) => {
  const router = useRouter();
  const {
    listenFromEpisode,
    state: uiState,
    play,
    pause,
    checkIsCurrentPlay,
  } = usePlayer();

  // FUNCTIONS
  const handlePlayPause = (episodeId: string) => {
    if (uiState.currentAudio) {
      if (uiState.isPlaying && uiState.currentAudio.id === episodeId) {
        pause();
      } else if (uiState.currentAudio.id === episodeId) {
        play();
      } else {
        listenFromEpisode(episodeId, "SpecifyShowEpisodes");
      }
    } else {
      listenFromEpisode(episodeId, "SpecifyShowEpisodes");
    }
  };

  return (
    <View style={styles.containerWrapper}>
      <AutoResolvingImageBackground
        FileKey={episode.MainImageFileKey}
        key={episode.Id}
        type="PodcastPublicSource"
        style={[StyleSheet.absoluteFill]}
        blurRadius={60} // High blur radius
      >
        {/* Dark overlay to dim the background image */}
        <View
          style={[
            StyleSheet.absoluteFill,
            { backgroundColor: "rgba(0, 0, 0, 0.6)" },
          ]}
        />
      </AutoResolvingImageBackground>

      <View style={styles.container}>
        <View style={styles.navigationContainer}>
          <View>
            {Platform.OS === "ios" ? (
              <Pressable onPress={() => router.back()} style={styles.backIcon}>
                <MaterialIcons
                  style={{ padding: 0, margin: 0 }}
                  name="keyboard-arrow-left"
                  color={"#fff"}
                  size={25}
                />
              </Pressable>
            ) : (
              <Pressable onPress={() => router.back()} style={styles.backIcon}>
                <MaterialIcons name="arrow-back" color={"#fff"} />
              </Pressable>
            )}
          </View>
          <View className="flex-row items-center gap-3">
            <Pressable onPress={onSaveToggle} style={styles.backIcon}>
              <MaterialIcons
                name="bookmark"
                color={isSaved ? "#aee339" : "#fff"}
                size={18}
              />
            </Pressable>
            <Pressable onPress={onReport} style={styles.backIcon}>
              <MaterialIcons name="report" color={"#fff"} size={18} />
            </Pressable>
          </View>
        </View>

        <View style={styles.episodeImageContainer}>
          {/* <Image style={styles.episodeImage} source={{ uri: ImageUrl }} /> */}
          <AutoResolvingImage
            FileKey={episode.MainImageFileKey}
            style={styles.episodeImage}
            type="PodcastPublicSource"
          />
        </View>

        <View style={styles.informationContainer}>
          <Text className="text-sm text-gray-300">
            {formatDateRange(episode.ReleaseDate)} • Episode{" "}
            {episode.EpisodeOrder} • Season {episode.SeasonNumber} •{" "}
            {TimeUtil.formatAudioLength(episode.AudioLength, "minuteOnly")}
          </Text>
          <Text numberOfLines={1} className="text-white font-bold text-[20px]">
            {episode.Name}
          </Text>
        </View>

        <View className="w-full flex items-center justify-center">
          {checkIsCurrentPlay(episode.Id) ? (
            <Pressable
              onPress={() => handlePlayPause(episode.Id)}
              className="w-2/3 px-2 py-4 bg-white/40 flex flex-row items-center justify-center gap-5 shadow-sm rounded-full"
            >
              <View className="flex flex-row items-center gap-1">
                <EqualizerVariant1 />
              </View>
              <View className="w-[90px] h-[5px] rounded-full bg-white relative flex flex-row items-center justify-start overflow-hidden">
                <View
                  className="rounded-l-full h-[5px] bg-[#AEE339]"
                  style={{
                    width: `${(uiState.currentTime / uiState.duration) * 100}%`,
                  }}
                ></View>
              </View>
              <View className="flex flex-row items-center gap-1 ml-5">
                <Text className="text-white text-xs font-medium">
                  {TimeUtil.formatAudioLength(
                    uiState.currentTime,
                    "numberOnly"
                  )}
                </Text>
              </View>
            </Pressable>
          ) : uiState &&
            uiState.currentAudio &&
            uiState.currentAudio.id === episode.Id ? (
            <Pressable
              onPress={() => handlePlayPause(episode.Id)}
              className="w-2/3 px-2 py-4 bg-white/40 flex flex-row items-center justify-center gap-5 shadow-sm rounded-full"
            >
              <View className="flex flex-row items-center gap-1">
                <MaterialIcons name="play-arrow" size={14} color="white" />
              </View>
              <View className="w-[90px] h-[5px] rounded-full bg-white relative flex flex-row items-center justify-start overflow-hidden">
                <View
                  className="rounded-l-full h-[5px] bg-[#AEE339]"
                  style={{
                    width: `${(uiState.currentTime / uiState.duration) * 100}%`,
                  }}
                ></View>
              </View>
              <View className="flex flex-row items-center gap-1 ml-5">
                <Text className="text-white text-xs font-medium">
                  {TimeUtil.formatAudioLength(
                    uiState.currentTime,
                    "numberOnly"
                  )}
                </Text>
              </View>
            </Pressable>
          ) : (
            <Pressable
              onPress={() => handlePlayPause(episode.Id)}
              className="w-2/3 p-3 bg-white/40 flex items-center justify-center shadow-sm rounded-full"
            >
              <Text className="text-white font-bold">Play Audio</Text>
            </Pressable>
          )}
        </View>
      </View>
    </View>
  );
};

export default EpisodeInformations;

const styles = StyleSheet.create({
  containerWrapper: {
    width: "100%",
    overflow: "hidden",
  },
  container: {
    width: "100%",
    paddingTop: 60,
    paddingBottom: 20,
    paddingHorizontal: 20,
    gap: 10,
  },
  navigationContainer: {
    width: "100%",
    flexDirection: "row",
    justifyContent: "space-between",
    height: 30,
  },
  backIcon: {
    padding: 2,
    borderRadius: 9999,
    backgroundColor: "rgba(217, 217, 217, 0.2)",
    width: 30,
    height: 30,
    alignItems: "center",
    justifyContent: "center",
  },
  episodeImageContainer: {
    width: "100%",
    height: 220,
    alignItems: "center",
    justifyContent: "center",
    paddingVertical: 10,
    resizeMode: "cover",
  },
  episodeImage: {
    width: 220,
    height: 220,
    resizeMode: "cover",
    borderRadius: 8,
  },
  informationContainer: {
    width: "100%",
    alignItems: "center",
    justifyContent: "center",
    marginTop: 10,
    gap: 10,
  },
  playButton: {
    width: "75%",
    paddingVertical: 10,
    borderRadius: 5,
    backgroundColor: "rgba(000, 000, 000, 0.6)",
    alignItems: "center",
    justifyContent: "center",
    flexDirection: "row",
    gap: 5,
  },
});
