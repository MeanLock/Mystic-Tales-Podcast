import AutoResolvingImage from "@/src/components/autoResolveImage/AutoResolvingImage";
import MixxingText from "@/src/components/ui/MixxingText";
import { Text } from "@/src/components/ui/Text";
import { View } from "@/src/components/ui/View";
import { Channel } from "@/src/core/types/channel.type";
import { MaterialIcons } from "@expo/vector-icons";
import { BlurView } from "expo-blur";
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
      <BlurView intensity={2} className="absolute inset-0 z-10" />
      <View className="absolute z-20 inset-0 bg-black/40 flex flex-col items-start justify-end p-2 rounded-[8px]">
        <Text numberOfLines={1} className="text-white font-bold">
          {channel.Name}
        </Text>

        <Text className="text-[#aee339] text-sm font-bold">
          {channel.PodcastCategory.Name}
        </Text>
        <View className="flex flex-row items-center justify-start gap-1 mt-1">
          <MaterialIcons name="favorite" size={12} color={"#fff"} />
          <Text className="text-xs text-[#D9D9D9]">
            {channel.TotalFavorite.toLocaleString()}
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
    height: 150,
    borderRadius: 8,
  },
  image: {
    width: "100%",
    height: "100%",
    resizeMode: "cover",
  },
});
