import AutoResolvingImage from "@/src/components/autoResolveImage/AutoResolvingImage";
import HtmlText from "@/src/components/renderHtml/HtmlText";
import { usePlayer } from "@/src/core/services/player/usePlayer";
import { MaterialCommunityIcons } from "@expo/vector-icons";
import { useRouter } from "expo-router";
import { Pressable, Text, View } from "react-native";

interface Props {
  episode: {
    Id: string;
    Name: string;
    Description: string;
    MainImageFileKey: string;
    ReleaseDate: string;
    IsReleased: boolean;
  };
}
const EpisodeCard = ({ episode }: Props) => {
  const router = useRouter();

  const { play, pause, listenFromEpisode, state } = usePlayer();

  const handlePlayPause = () => {
    if (state.currentAudio && state.currentAudio.id === episode.Id) {
      if (state.isPlaying) {
        pause();
      } else {
        play();
      }
    } else {
      listenFromEpisode(episode.Id, "SpecifyShowEpisodes");
    }
  };

  return (
    <Pressable
      onPress={() => router.push(`/(content)/episodes/details/${episode.Id}`)}
      className="w-full flex flex-row items-center gap-3 p-2 border-b-[0.5px] border-b-[#333]"
    >
      <AutoResolvingImage
        FileKey={episode.MainImageFileKey}
        type="PodcastPublicSource"
        style={{ width: 80, height: 80, borderRadius: 8 }}
      />
      <View className="flex-1 overflow-hidden justify-between">
        <Text className="text-white font-bold" numberOfLines={1}>
          {episode.Name}
        </Text>
        <HtmlText
          html={episode.Description}
          numberOfLines={2}
          fontSize={10}
          color="#D9D9D9"
        />
        <View className="w-full p-2 flex flex-row items-center justify-end">
          {state.currentAudio &&
          state.currentAudio.id === episode.Id &&
          state.isPlaying ? (
            <Pressable
              className="p-2 bg-[#aee339] rounded-full"
              onPress={(e) => {
                e.stopPropagation();
                handlePlayPause();
              }}
            >
              <MaterialCommunityIcons name="pause" size={18} color="#000" />
            </Pressable>
          ) : (
            <Pressable
              className="p-2 bg-[#aee339] rounded-full"
              onPress={(e) => {
                e.stopPropagation();
                handlePlayPause();
              }}
            >
              <MaterialCommunityIcons name="play" size={18} color="#000" />
            </Pressable>
          )}
        </View>
      </View>
    </Pressable>
  );
};
export default EpisodeCard;
