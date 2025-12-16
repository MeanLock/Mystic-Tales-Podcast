import AutoResolvingImage from "@/src/components/autoResolveImage/AutoResolvingImage";
import { Episode } from "@/src/core/types/episode.type";
import { EpisodeCardWithImageProps } from "@/src/types/episode";
import { MaterialIcons } from "@expo/vector-icons";
import { Image, StyleSheet, Text, View } from "react-native";

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

const EpisodeCard = ({
  episode,
}: {
  episode: Episode;
}) => {
  return (
    <View style={style.card} key={episode.Id} className="grid grid-cols-12">
      <View className="col-span-2 flex items-center justify-center">
        <AutoResolvingImage
            FileKey={episode.MainImageFileKey}
            type="PodcastPublicSource"
            style={style.episodeImage}
          />
      </View>
      <View
        style={style.infomations}
        className="col-span-8  gap-2 flex items-center justify-center"
      >
        <View className="flex flex-col gap-1 h-[60px] w-[220px]">
          <Text numberOfLines={1} className="text-white text-[10px] font-bold">
            {episode.Name}
          </Text>
          <Text numberOfLines={3} className="text-white text-[7px]">
            {episode.Description}
          </Text>
          <View className="flex-1 flex flex-row items-end gap-2">
            {/* {episode.ExplicitContent && <ExplicitContentTag />} */}
            <AudioLengthTag length={episode.AudioLength} />
          </View>
        </View>
      </View>
      <View className="col-span-1 flex items-center justify-center">
        <MaterialIcons name="more-vert" size={20} color="white" />
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
