import AutoResolvingImage from "@/src/components/autoResolveImage/AutoResolvingImage";
import HtmlText from "@/src/components/renderHtml/HtmlText";
import { usePlayer } from "@/src/core/services/player/usePlayer";
import { MaterialIcons } from "@expo/vector-icons";
import { useRouter } from "expo-router";
import { ActivityIndicator, Pressable, Text, View } from "react-native";
import { EqualizerVariant1 } from "@/src/components/equalizer/Variant1";
import { useRef, useCallback } from "react";

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
  const debounceTimerRef = useRef<number | null>(null);

  const handlePlayPause = useCallback(() => {
    // Clear previous timer if exists
    if (debounceTimerRef.current) {
      clearTimeout(debounceTimerRef.current);
    }

    // Set new timer for debounce
    debounceTimerRef.current = setTimeout(() => {
      if (state.currentAudio && state.currentAudio.id === episode.Id) {
        if (state.isPlaying) {
          pause();
        } else {
          play();
        }
      } else {
        listenFromEpisode(episode.Id, "SpecifyShowEpisodes");
      }
      debounceTimerRef.current = null;
    }, 1000);
  }, [
    episode.Id,
    state.currentAudio,
    state.isPlaying,
    play,
    pause,
    listenFromEpisode,
  ]);

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
          {state.isAudioLoading && state.loadingAudioId === episode.Id ? (
            // Đang load và chính nó đang load
            <Pressable
              className="p-2 bg-[#aee339] rounded-full"
              onPress={(e) => e.stopPropagation()}
            >
              <ActivityIndicator size="small" color="#000" />
            </Pressable>
          ) : state.isAudioLoading ? (
            // Đang load nhưng không phải nó
            <Pressable
              className="p-2 bg-gray-400 rounded-full opacity-50"
              onPress={(e) => e.stopPropagation()}
              disabled
            >
              <MaterialIcons name="play-disabled" size={18} color="#666" />
            </Pressable>
          ) : state.currentAudio && state.currentAudio.id === episode.Id ? (
            // Đây là currentAudio
            <Pressable
              className="p-2 bg-[#aee339] rounded-full"
              onPress={(e) => {
                e.stopPropagation();
                handlePlayPause();
              }}
            >
              {state.isPlaying ? (
                <View className="flex flex-row items-center gap-1">
                  <EqualizerVariant1 color="#000" key={episode.Id} />
                  <View className="w-6 h-[3px] rounded-full bg-black/20 flex flex-row items-center justify-start overflow-hidden ml-1">
                    <View
                      style={{
                        width: `${(state.currentTime / state.duration) * 100}%`,
                      }}
                      className="h-[3px] rounded-l-full bg-black"
                    ></View>
                  </View>
                </View>
              ) : (
                <View className="flex flex-row items-center gap-1">
                  <MaterialIcons name="play-arrow" size={18} color="#000" />
                  <View className="w-6 h-[3px] rounded-full bg-black/20 flex flex-row items-center justify-start overflow-hidden ml-1">
                    <View
                      style={{
                        width: `${(state.currentTime / state.duration) * 100}%`,
                      }}
                      className="h-[3px] rounded-l-full bg-black"
                    ></View>
                  </View>
                </View>
              )}
            </Pressable>
          ) : (
            // Không phải currentAudio
            <Pressable
              className="p-2 bg-[#aee339] rounded-full"
              onPress={(e) => {
                e.stopPropagation();
                handlePlayPause();
              }}
            >
              <MaterialIcons name="play-arrow" size={18} color="#000" />
            </Pressable>
          )}
        </View>
      </View>
    </Pressable>
  );
};
export default EpisodeCard;
