import { RootState } from "@/src/store/store";
import { Animated, Dimensions, Pressable, Text, View } from "react-native";
import { useSelector } from "react-redux";
import { StyleSheet } from "react-native";
import PlayButtonVariant2 from "@/src/components/buttons/playButton/playButtonVariant2";
import AutoResolvingImage from "@/src/components/autoResolveImage/AutoResolvingImage";
import { MaterialIcons } from "@expo/vector-icons";
import { useRouter } from "expo-router";
import HtmlText from "@/src/components/renderHtml/HtmlText";
import ShowCard from "./components/ShowCard";

const formatDate = (dateString: string): string => {
  try {
    const date = new Date(dateString);
    const now = new Date();

    // If invalid date, return the original string
    if (isNaN(date.getTime())) {
      return dateString;
    }

    const diffMs = now.getTime() - date.getTime();
    const diffHours = Math.floor(diffMs / (1000 * 60 * 60));
    const diffDays = Math.floor(diffMs / (1000 * 60 * 60 * 24));

    // Less than 1 day: show hours
    if (diffHours < 24) {
      return diffHours === 0
        ? "Just now"
        : `${diffHours} ${diffHours === 1 ? "hour" : "hours"} ago`;
    }

    // Less than 5 days: show days
    if (diffDays < 5) {
      return `${diffDays} ${diffDays === 1 ? "day" : "days"} ago`;
    }

    // More than 5 days: format as DD/MM/YYYY
    const day = date.getDate().toString().padStart(2, "0");
    const month = (date.getMonth() + 1).toString().padStart(2, "0");
    const year = date.getFullYear();

    return `${day}/${month}/${year}`;
  } catch (error) {
    console.error("Error formatting date:", error);
    return dateString;
  }
};
export default function ShowsScreen() {
  const showData = useSelector((state: RootState) => state.show);
  const router = useRouter();
  const handlePlayShow = () => {
    console.log("Play show with ID:", showData.shows[1]);
  };
  // HOOKS
  const windowWidth = Dimensions.get("window").width;
  const itemWidth = (windowWidth - 16 * 2 - 10) / 2; // Calculate item width based on screen width, padding, and gap

  if (showData.shows.length === 0 || !showData.title) {
    return (
      <View style={styles.centerContainer}>
        <Text style={{ color: "#aee339", fontSize: 20, fontWeight: "bold" }}>
          No Show Available Now
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
        {showData.from === "ChannelDetails" && (
          <Pressable onPress={() => router.back()}>
            <View className="flex flex-row items-center gap-2">
              <MaterialIcons
                name="arrow-back-ios-new"
                size={18}
                color={"#AEE339"}
              />
              <Text className="text-[#AEE339] font-medium text-xl">
                {showData.title}
              </Text>
            </View>
          </Pressable>
        )}

        {showData.from === "Feed" && (
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
              {showData.title}
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

      {showData.from === "ChannelDetails" && (
        <View style={style.gridColTwoContainer}>
          {showData?.shows.map((item, index) => (
            <ShowCard width={itemWidth} show={item} key={index} />
          ))}
        </View>
      )}

      {showData.from === "Feed" && (
        <View style={style.gridColTwoContainer}>
          {showData?.shows.map((item, index) => (
            <ShowCard width={itemWidth} show={item} key={index} />
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
