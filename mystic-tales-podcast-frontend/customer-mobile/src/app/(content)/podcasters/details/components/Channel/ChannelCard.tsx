import AutoResolvingImage from "@/src/components/autoResolveImage/AutoResolvingImage";
import MixxingText from "@/src/components/ui/MixxingText";
import { Text } from "@/src/components/ui/Text";
import { View } from "@/src/components/ui/View";
import { Channel } from "@/src/core/types/channel.type";
import { MaterialIcons } from "@expo/vector-icons";
import { useRouter } from "expo-router";
import { Image, Pressable, StyleSheet } from "react-native";

interface ShowCardVariant1Props {
  // Define any props if needed in the future
  channel: Channel;
}

const ChannelCard = ({ channel }: ShowCardVariant1Props) => {
  const router = useRouter();

  return (
    <Pressable
      onPress={() => router.push(`/(content)/channels/details/${channel.Id}`)}
      key={channel.Id}
      style={styles.card}
    >
      <AutoResolvingImage
        FileKey={channel.MainImageFileKey}
        type="PodcastPublicSource"
        key={channel.Id}
        style={styles.image}
      />
      <View className="flex flex-col items-start justify-end p-2 rounded-[8px]">
        <MixxingText
          className="line-clamp-1"
          coloredText={channel.Name}
          originalText={channel.Name}
        />
        <Text className="text-[#D9D9D9] text-sm font-bold">
          {channel.PodcastCategory.Name}
        </Text>
        <View className="flex flex-row items-center justify-start gap-1">
          <Text numberOfLines={1} className="text-xs text-[#D9D9D9]">
            {channel.ShowCount} shows • {channel.PodcastSubCategory.Name}
          </Text>
        </View>
      </View>
    </Pressable>
  );
};
export default ChannelCard;

const styles = StyleSheet.create({
  card: {
    overflow: "hidden",
    elevation: 2,
    width: 200,
    borderRadius: 8,
  },
  image: {
    width: 200,
    height: 150,
    resizeMode: "cover",
    borderRadius: 8,
  },
});
