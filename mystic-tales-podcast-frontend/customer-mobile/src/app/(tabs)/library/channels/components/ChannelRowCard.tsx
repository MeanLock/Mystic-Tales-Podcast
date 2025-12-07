import AutoResolvingImage from "@/src/components/autoResolveImage/AutoResolvingImage";
import { Text } from "@/src/components/ui/Text";
import { View } from "@/src/components/ui/View";
import { Channel } from "@/src/core/types/channel.type";
import { MaterialCommunityIcons } from "@expo/vector-icons";
import { StyleSheet } from "react-native";

const ChannelRowCard = ({ channel }: { channel: Channel }) => {
  return (
    <View
      style={styles.rowContainer}
      className="transition-all hover:scale-110 duration-500 ease-out"
    >
      <View>
        <AutoResolvingImage
          FileKey={channel.MainImageFileKey}
          type="PodcastPublicSource"
          style={styles.image}
        />
      </View>
      <View style={styles.informationContainer}>
        <Text className="text-white">{channel.Name}</Text>
        <Text className="text-gray-400" numberOfLines={1}>
          {channel.ShowCount} shows
        </Text>
      </View>
      <View className="flex h-full items-center justify-center">
        {/* Navigate Icon */}
        <MaterialCommunityIcons name="chevron-right" size={24} color="white" />
      </View>
    </View>
  );
};

export default ChannelRowCard;
const styles = StyleSheet.create({
  rowContainer: {
    width: "100%",
    flexDirection: "row",
    alignItems: "center",
    paddingVertical: 10,
    borderTopWidth: 0.3,
    borderTopColor: "#333333",
  },
  image: {
    width: 100,
    height: 100,
    resizeMode: "cover",
    borderRadius: 999,
    borderWidth: 1,
    borderColor: "#333333",
  },
  informationContainer: {
    flex: 1,
    marginLeft: 15,
    justifyContent: "center",
  },
});
