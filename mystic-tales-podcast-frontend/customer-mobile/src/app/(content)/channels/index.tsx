import { RootState } from "@/src/store/store";
import { Animated, Dimensions, Pressable, Text, View } from "react-native";
import { useSelector } from "react-redux";
import { StyleSheet } from "react-native";
import PlayButtonVariant2 from "@/src/components/buttons/playButton/playButtonVariant2";
import AutoResolvingImage from "@/src/components/autoResolveImage/AutoResolvingImage";
import { MaterialIcons } from "@expo/vector-icons";
import { useRouter } from "expo-router";
import HtmlText from "@/src/components/renderHtml/HtmlText";
import ChannelCard from "./components/ChannelCard";

export default function ChannelsScreen() {
  const channelData = useSelector((state: RootState) => state.channel);
  const router = useRouter();
  const handlePlayChannel = () => {
    console.log("Play channel with ID:", channelData.channels[1]);
  };
  // HOOKS
  const windowWidth = Dimensions.get("window").width;
  const itemWidth = (windowWidth - 16 * 2 - 10) / 2; // Calculate item width based on screen width, padding, and gap

  if (channelData.channels.length === 0 || !channelData.title) {
    return (
      <View style={styles.centerContainer}>
        <Text style={{ color: "#aee339", fontSize: 20, fontWeight: "bold" }}>
          No Channel Available Now
        </Text>
        <Pressable onPress={() => router.back()}>
          <View className="flex flex-row items-center gap-2">
            <MaterialIcons
              name="arrow-back-ios-new"
              size={18}
              color={"#ffffff"}
            />
            <Text className="text-white font-medium text-xl">Back</Text>
          </View>
        </Pressable>
      </View>
    );
  }

  return (
    <Animated.ScrollView
      style={styles.container}
      scrollEventThrottle={16}
      contentContainerStyle={{
        paddingTop: 50,
        paddingHorizontal: 16,
        paddingBottom: 40,
      }}
    >
      <View className="w-full flex flex-row items-center py-5 ">
        {channelData.from === "Feed" && (
          <View className="flex flex-row items-center justify-between w-full">
            <Pressable onPress={() => router.back()}>
              <View className="flex flex-row items-center gap-2">
                <MaterialIcons
                  name="arrow-back-ios-new"
                  size={18}
                  color={"#AEE339"}
                />
                <Text className="text-[#AEE339] font-medium text-xl">Home</Text>
              </View>
            </Pressable>
            <Text className="text-white font-bold text-xl">
              {channelData.title}
            </Text>
            {/* tạo khoảng cách cho title ở giữa:))) */}
            <View className="flex flex-row items-center gap-2">
              <MaterialIcons
                name="arrow-back-ios-new"
                size={18}
                color={"#000"}
              />
              <Text className="text-[#000] font-medium text-xl">Home</Text>
            </View>
          </View>
        )}
      </View>

      {channelData.from === "Feed" && (
        <View style={style.gridColTwoContainer}>
          {channelData?.channels.map((item, index) => (
            <ChannelCard width={itemWidth} channel={item} key={index} />
          ))}
        </View>
      )}

      <View style={{ height: 50 }}></View>
    </Animated.ScrollView>
  );
}

const styles = StyleSheet.create({
  container: {
    backgroundColor: "#000",
  },
  centerContainer: {
    flex: 1,
    justifyContent: "center",
    alignItems: "center",
    backgroundColor: "#000",
    gap: 20,
  },
});
const style = StyleSheet.create({
  gridColTwoContainer: {
    flexDirection: "row",
    flexWrap: "wrap",
    justifyContent: "space-between",
    gap: 10, // This sets the gap between grid items
    width: "100%",
  },
});
