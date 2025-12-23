import AutoResolvingImage from "@/src/components/autoResolveImage/AutoResolvingImage";
import PlayButtonVariant2 from "@/src/components/buttons/playButton/playButtonVariant2";
import HtmlText from "@/src/components/renderHtml/HtmlText";
import { usePlayer } from "@/src/core/services/player/usePlayer";
import { Episode } from "@/src/core/types/episode.type";
import { EpisodeCardWithImageProps } from "@/src/types/episode";
import { MaterialIcons } from "@expo/vector-icons";
import { useRouter } from "expo-router";
import { Image, Pressable, StyleSheet, Text, View } from "react-native";

const ExplicitContentTag = () => {
  return (
    <View className="bg-[#AEE339] flex items-center justify-center rounded-sm w-[75px] h-[10px]">
      <Text className="text-black font-bold text-[6px]">Explicit Content</Text>
    </View>
  );
};

const AudioLengthTag = ({ length }: { length: number }) => {
  const formatLength = (length: number) => {
    const hours = Math.floor(length / 60);
    const minutes = length % 60;
    return hours > 0 ? `${hours}h ${minutes}m` : `${minutes}m`;
  };
  return (
    <View className="bg-transparent border-[1px] border-[#AEE339] flex items-center justify-center rounded-sm w-[60px] h-[10px]">
      <Text className="text-[#AEE339] font-bold text-[6px]">
        {formatLength(length)}
      </Text>
    </View>
  );
};

const EpisodeCard = ({ episode }: { episode: Episode }) => {
  const router = useRouter();
  const { state: uiState, play, pause, listenFromEpisode } = usePlayer();
  const handlePlayPause = (episodeId: string) => {
    if (uiState.isAudioLoading) {
      return;
    }
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
    <View style={style.card} key={episode.Id} className="grid grid-cols-12">
      <Pressable
        onPress={() =>
          router.push(`/(content)/episodes/details/${episode.Id})`)
        }
        className="col-span-2 flex items-center justify-center"
      >
        <AutoResolvingImage
          FileKey={episode.MainImageFileKey}
          type="PodcastPublicSource"
          style={style.episodeImage}
        />
      </Pressable>
      {/* <View className="col-span-1 flex items-center justify-center">
        <Text className="text-white font-bold text-[10px]">#{episode.Top}</Text>
      </View> */}
      <View
        style={style.infomations}
        className="col-span-9  gap-2 flex items-center justify-center"
      >
        <View className="flex flex-col gap-1 h-[60px] w-[220px]">
          <Text numberOfLines={1} className="text-white text-[10px] font-bold">
            {episode.Name}
          </Text>
          {/* <HtmlText
            html={episode.Description ?? ""}
            color="#D9D9D9"
            fontSize={7}
            numberOfLines={3}
          /> */}
          <View className="w-full items-start mt-5">
            <PlayButtonVariant2
              audioId={episode.Id}
              audioLength={episode.AudioLength}
              onPlayPress={() => handlePlayPause(episode.Id)}
            />
          </View>
        </View>
      </View>
    </View>
  );
};

export default EpisodeCard;

const style = StyleSheet.create({
  card: {
    padding: 5,
    // width: 250,
    height: 80,
    borderBottomColor: "#999999",
    borderBottomWidth: 1,
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
