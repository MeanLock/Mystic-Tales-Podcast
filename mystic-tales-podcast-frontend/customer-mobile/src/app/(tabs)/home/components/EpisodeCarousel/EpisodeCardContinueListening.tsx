import AutoResolvingImage from "@/src/components/autoResolveImage/AutoResolvingImage";
import { Episode } from "@/src/core/types/episode.type";
import { MaterialIcons } from "@expo/vector-icons";
import { Pressable, StyleSheet, Text, View } from "react-native";
import { ContinueListenSession } from "./EpisodeContinueCarousel";
import HtmlText from "@/src/components/renderHtml/HtmlText";
import { usePlayer } from "@/src/core/services/player/usePlayer";
import TimeUtil from "@/src/core/utils/time";

const AudioLengthTag = ({ length }: { length: number }) => {
  const formatLength = (length: number) => {
    const minutes = Math.floor(length / 60);
    const seconds = length % 60;
    return seconds > 0 ? `0${minutes} : ${seconds}s` : `${minutes}m`;
  };
  return (
    <View className="bg-transparent  flex items-start justify-center rounded-sm w-[60px] h-[10px]">
      <Text className="text-[#D9D9D9] font-medium text-[10px]">
        {formatLength(length)}
      </Text>
    </View>
  );
};

const EpisodeContinueListeningCard = ({
  episode,
}: {
  episode: ContinueListenSession;
}) => {
  const {
    play,
    pause,
    continueListenFromEpisode,
    state: uiState,
  } = usePlayer();

  const handlePlayPause = async () => {
    if (!episode.Episode) return;
    // Đang Playing episode này
    if (uiState.currentAudio?.id === episode.Episode.Id && uiState.isPlaying) {
      pause();
      return;
    }
    // Đang Pause episode này
    if (uiState.currentAudio?.id === episode.Episode.Id && !uiState.isPlaying) {
      play();
      return;
    }
    // Chưa play episode này
    await continueListenFromEpisode(
      episode.Episode.Id,
      episode.PodcastEpisodeListenSession.Id
    );
  };

  return (
    <View
      style={style.card}
      key={episode.Episode.Id}
      className="grid grid-cols-12"
    >
      <View className="col-span-2 flex items-center justify-center">
        <AutoResolvingImage
          FileKey={episode.Episode.MainImageFileKey}
          style={style.episodeImage}
          type="PodcastPublicSource"
        />
      </View>
      <View
        style={style.infomations}
        className="col-span-8  gap-2 flex items-center justify-center"
      >
        <View className="flex flex-col gap-1 h-[60px] w-[220px]">
          <Text numberOfLines={1} className="text-white font-bold">
            {episode.Episode.Name}
          </Text>
          <View className="flex-1 flex flex-row items-end gap-2">
            {uiState.currentAudio?.id === episode.Episode.Id &&
              uiState.isPlaying && (
                <Pressable
                  onPress={() => handlePlayPause()}
                  className="flex flex-row items-center pr-3 pl-1 py-1 gap-2 bg-zinc-50/30 rounded-full"
                >
                  <MaterialIcons
                    name="pause-circle-filled"
                    size={24}
                    color="white"
                  />
                  <View className="w-24 h-2 bg-zinc-50 rounded-full flex flex-row items-center justify-start">
                    <View
                      style={{
                        width: `${
                          (uiState.currentTime / uiState.duration) * 100
                        }%`,
                      }}
                      className="h-2 bg-[#aee339] rounded-l-full"
                    />
                  </View>
                </Pressable>
              )}
            {uiState.currentAudio?.id === episode.Episode.Id &&
              !uiState.isPlaying && (
                <Pressable
                  onPress={() => handlePlayPause()}
                  className="flex flex-row items-center pr-3 pl-1 py-1 gap-2 bg-zinc-50/30 rounded-full"
                >
                  <MaterialIcons
                    name="play-circle-filled"
                    size={24}
                    color="white"
                  />
                  <View className="w-24 h-2 bg-zinc-50 rounded-full flex flex-row items-center justify-start">
                    <View
                      style={{
                        width: `${
                          (uiState.currentTime / uiState.duration) * 100
                        }%`,
                      }}
                      className="h-2 bg-[#aee339] rounded-l-full"
                    />
                  </View>
                </Pressable>
              )}
            {(!uiState.currentAudio ||
              uiState.currentAudio.id !== episode.Episode.Id) && (
              <Pressable
                onPress={() => handlePlayPause()}
                className="flex flex-row items-center pr-3 pl-1 py-1 gap-2 bg-zinc-50/30 rounded-full"
              >
                <MaterialIcons
                  name="play-circle-filled"
                  size={24}
                  color="white"
                />
                <Text className="text-white text-xs">
                  Continue at{" "}
                  {TimeUtil.formatAudioLength(
                    episode.PodcastEpisodeListenSession
                      .LastListenDurationSeconds
                  )}
                </Text>
              </Pressable>
            )}
          </View>
        </View>
      </View>
    </View>
  );
};

export default EpisodeContinueListeningCard;

const style = StyleSheet.create({
  card: {
    padding: 5,
    // width: 250,
    height: 80,
    borderBottomColor: "#999999",
    borderBottomWidth: 0.3,
    backgroundColor: "transparent", // Changed from red to transparent
    flexDirection: "row",
    gap: 10,
  },
  episodeImage: {
    width: 60,
    height: 60,
    borderRadius: 3,
  },

  infomations: {},
});
