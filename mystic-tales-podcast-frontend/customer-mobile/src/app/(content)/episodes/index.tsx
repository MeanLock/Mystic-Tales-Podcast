import { RootState } from "@/src/store/store";
import { Animated, Dimensions, Pressable, Text, View } from "react-native";
import { useSelector } from "react-redux";
import { StyleSheet } from "react-native";
import PlayButtonVariant2 from "@/src/components/buttons/playButton/playButtonVariant2";
import AutoResolvingImage from "@/src/components/autoResolveImage/AutoResolvingImage";
import { MaterialIcons } from "@expo/vector-icons";
import { useRouter } from "expo-router";
import HtmlText from "@/src/components/renderHtml/HtmlText";

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
export default function EpisodesScreen() {
  const episodeData = useSelector((state: RootState) => state.episode);
  const router = useRouter();
  const handlePlayEpisode = () => {
    console.log("Play episode with ID:", episodeData.episodes[1]);
  };
  // HOOKS
  const windowWidth = Dimensions.get("window").width;
  const itemWidth = (windowWidth - 16 * 2 - 10) / 2; // Calculate item width based on screen width, padding, and gap
  if (episodeData.episodes.length === 0 || !episodeData.title) {
    return (
      <View style={styles.centerContainer}>
        <Text style={{ color: "#aee339", fontSize: 20, fontWeight: "bold" }}>
          No Episode Available Now
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
        {episodeData.from === "ShowDetails" && (
          <Pressable onPress={() => router.back()}>
            <View className="flex flex-row items-center gap-2">
              <MaterialIcons
                name="arrow-back-ios-new"
                size={18}
                color={"#AEE339"}
              />
              <Text className="text-[#AEE339] font-medium text-xl">
                {episodeData.title}
              </Text>
            </View>
          </Pressable>
        )}

        {episodeData.from === "Feed" && (
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
              {episodeData.title}
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

      {episodeData.from === "ShowDetails" && (
        <>
          <Text className="text-white font-bold text-xl mb-6">Episodes</Text>
          <View className="w-full flex flex-col items-start gap-5">
            {episodeData?.episodes.map((item, index) => (
              <Pressable
                key={item.Id}
                onPress={() =>
                  router.push(`/(content)/episodes/details/${item.Id}`)
                }
                style={[style.episodeContainer, style.borderBottom]}
              >
                <View className="w-[70%] justify-between ">
                  <Text style={style.dateText}>
                    {formatDate(item.ReleaseDate)}
                  </Text>
                  <View className="w-full gap-2">
                    <Text
                      className="text-white text-[18px] font-bold"
                      numberOfLines={2}
                    >
                      {item.Name}
                    </Text>
                    <HtmlText
                      html={item.Description}
                      numberOfLines={3}
                      color="#9c9a9aff"
                    />
                  </View>
                  <View className="w-2/3 items-start mt-5">
                    <PlayButtonVariant2
                      episodeId={item.Id}
                      audioLength={item.AudioLength}
                      onPlayPress={() => handlePlayEpisode()}
                    />
                  </View>
                </View>
                <View className="flex-1  min-w-[51px] items-end justify-between mt-1">
                  <View>
                    <AutoResolvingImage
                      FileKey={item.MainImageFileKey}
                      type="PodcastPublicSource"
                      style={{ width: 80, height: 80 }}
                    />
                  </View>
                  <View>
                    <Pressable>
                      <MaterialIcons
                        name="more-horiz"
                        size={18}
                        color={"#D9D9D9"}
                      />
                    </Pressable>
                  </View>
                </View>
              </Pressable>
            ))}
          </View>
        </>
      )}

      {episodeData.from === "Feed" && (
        <>
          <View className="w-full flex flex-col items-start">
            {episodeData?.episodes.map((item, index) => (
              <Pressable
                key={item.Id}
                onPress={() =>
                  router.push(`/(content)/episodes/details/${item.Id}`)
                }
                style={[style.episodeContainer, style.borderTop]}
                className="flex gap-4"
              >
                <View className="min-w-[51px] ">
                  <AutoResolvingImage
                    FileKey={item.MainImageFileKey}
                    type="PodcastPublicSource"
                    style={{ width: 80, height: 80, borderRadius: 6 }}
                  />
                </View>
                <View className="w-[70%] justify-between ">
                  <Text style={style.dateText}>
                    {formatDate(item.ReleaseDate)}
                  </Text>
                  <View className="w-full gap-2">
                    <Text
                      className="text-white text-[18px] font-bold"
                      numberOfLines={2}
                    >
                      {item.Name}
                    </Text>
                    <HtmlText
                      html={item.Description}
                      numberOfLines={1}
                      color="#9c9a9aff"
                    />
                  </View>
                  <View className=" flex flex-row items-center justify-between items-start mt-5">
                    <View className="w-2/3">
                      <PlayButtonVariant2
                        episodeId={item.Id}
                        audioLength={item.AudioLength}
                        onPlayPress={() => handlePlayEpisode()}
                      />
                    </View>
                    <Pressable>
                      <MaterialIcons
                        name="more-horiz"
                        size={18}
                        color={"#D9D9D9"}
                      />
                    </Pressable>
                  </View>
                </View>
              </Pressable>
            ))}
          </View>
        </>
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
  container: {},
  borderBottom: {
    borderBottomWidth: 0.3,
    borderBottomColor: "#514F4F",
  },
  borderTop: {
    borderTopWidth: 0.3,
    borderTopColor: "#514F4F",
  },
  iconContainer: {
    display: "flex",
    alignItems: "center",
    justifyContent: "center",
  },
  episodeContainer: {
    paddingVertical: 16,
    width: "100%",
    flexDirection: "row",
  },
  dateText: {
    color: "#999",
    fontSize: 12,
    marginBottom: 4,
  },
  playButton: {
    backgroundColor: "rgba(217, 217, 217, 0.2)",
    paddingVertical: 5,
    flexDirection: "row",
    alignItems: "center",
    justifyContent: "center",
    gap: 2,
    borderRadius: 8,
  },
});
