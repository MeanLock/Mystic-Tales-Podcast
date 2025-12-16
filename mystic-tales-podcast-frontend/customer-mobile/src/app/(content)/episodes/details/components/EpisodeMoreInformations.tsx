import { Pressable, Text, View } from "react-native";
import { EpisodeDetails } from "@/src/core/types/episode.type";
import TimeUtil from "@/src/core/utils/time";
import { useRouter } from "expo-router";

const EpisodeMoreInformations = ({ episode }: { episode: EpisodeDetails }) => {
  const router = useRouter();
  return (
    <View className="w-full flex flex-col p-4">
      <Text className="text-2xl text-white font-bold mb-6">Informations</Text>
      <View className="border-b-[0.5px] border-gray-800 w-full flex flex-row items-center justify-between py-2">
        <Text className="text-[#D9D9D9] text-lg font-light">Show</Text>
        <Pressable
          onPress={() =>
            router.push(`/(content)/shows/details/${episode.PodcastShow.Id}`)
          }
        >
          <Text className="text-[#aee339] text-lg underline">
            {episode.PodcastShow.Name}
          </Text>
        </Pressable>
      </View>

      <View className="border-b-[0.5px] border-gray-800 w-full flex flex-row items-center justify-between py-2">
        <Text className="text-[#D9D9D9] text-lg font-light">Content Type</Text>
        <Text className="text-white text-lg">
          {episode.ExplicitContent ? "Explicit Content" : "Clean"}
        </Text>
      </View>

      <View className="border-b-[0.5px] border-gray-800 w-full flex flex-row items-center justify-between py-2">
        <Text className="text-[#D9D9D9] text-lg font-light">Release Date</Text>
        <Text className="text-white text-lg">{episode.ReleaseDate}</Text>
      </View>

      <View className="border-b-[0.5px] border-gray-800 w-full flex flex-row items-center justify-between py-2">
        <Text className="text-[#D9D9D9] text-lg font-light">Audio Length</Text>
        <Text className="text-white text-lg">
          {TimeUtil.formatAudioLength(episode.AudioLength, "numberOnly")}
        </Text>
      </View>

      <View className="border-b-[0.5px] border-gray-800 w-full flex flex-row items-center justify-between py-2">
        <Text className="text-[#D9D9D9] text-lg font-light">Listen Count</Text>
        <Text className="text-white text-lg">
          {episode.ListenCount.toLocaleString()} times
        </Text>
      </View>
    </View>
  );
};

export default EpisodeMoreInformations;
