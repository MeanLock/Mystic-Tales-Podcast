import AutoResolvingImage from "@/src/components/autoResolveImage/AutoResolvingImage";
import { Episode } from "@/src/core/types/episode.type";
import { MaterialIcons } from "@expo/vector-icons";
import { StyleSheet, Text, View } from "react-native";
import { ContinueListenSession } from "./EpisodeContinueCarousel";
import HtmlText from "@/src/components/renderHtml/HtmlText";

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
          <Text numberOfLines={1} className="text-white text-[10px] font-bold">
            {episode.Episode.Name}
          </Text>
          {/* <Text numberOfLines={3} className="text-white text-[7px]">
            {episode.Episode.Description}
          </Text> */}
          <View className="flex-1 flex flex-row items-end gap-2">
            <AudioLengthTag length={episode.Episode.AudioLength} />
          </View>
        </View>
      </View>
      <View className="col-span-1 flex items-center justify-center">
        <MaterialIcons name="more-vert" size={20} color="white" />
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
