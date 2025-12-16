import { useBottomTabBarHeight } from "@react-navigation/bottom-tabs";
import { useHeaderScroll } from "../_layout";
import {
  ActivityIndicator,
  Animated,
  Dimensions,
  StyleSheet,
  Text,
  View,
} from "react-native";
import { useGetFollowedPodcastersQuery } from "@/src/core/services/podcasters/podcaster.service";
import PodcasterCard from "./components/PodcasterCard";

export default function PodcastersScreen() {
  const { onScroll } = useHeaderScroll();
  const tabBarHeight = useBottomTabBarHeight();
  const windowWidth = Dimensions.get("window").width;
  const itemWidth = (windowWidth - 16 * 2 - 10) / 2;

  const { data: followedPodcasters, isLoading: isLoadingFollowedPodcasters } =
    useGetFollowedPodcastersQuery(undefined, {
      refetchOnMountOrArgChange: true,
      refetchOnFocus: true,
      refetchOnReconnect: true,
    });
  if (isLoadingFollowedPodcasters) {
    return (
      <View className="flex-1 bg-black flex items-center justify-center gap-5">
        <ActivityIndicator size="large" color="#aee339" />
        <Text className="text-[#aee339] text-lg font-bold">
          Loading Followed Podcasters...
        </Text>
      </View>
    );
  }
  return (
    <Animated.ScrollView
      onScroll={onScroll}
      scrollEventThrottle={16}
      contentContainerStyle={{
        paddingTop: 100,
        paddingHorizontal: 16,
        paddingBottom: 40,
        backgroundColor: "#000",
        minHeight: "100%",
      }}
    >
      <Text className="text-white font-bold text-2xl mt-10 mb-10">
        Followed Podcasters
      </Text>

      {followedPodcasters &&
      followedPodcasters.FollowedPodcasterList.length > 0 ? (
        <View style={styles.gridColTwoContainer}>
          {followedPodcasters.FollowedPodcasterList.map((podcaster) => (
            <PodcasterCard
              width={itemWidth}
              podcaster={podcaster}
              key={podcaster.AccountId}
            />
          ))}
        </View>
      ) : (
        <View style={styles.centerContainer}>
          <Text style={styles.itemText}>No followed podcasters found.</Text>
        </View>
      )}

      {/* Grid layout using flexDirection: row and flexWrap: wrap */}
      <View style={{ height: tabBarHeight + 50 }}></View>
    </Animated.ScrollView>
  );
}

const styles = StyleSheet.create({
  gridColTwoContainer: {
    flexDirection: "row",
    flexWrap: "wrap",
    justifyContent: "space-between",
    gap: 10, // This sets the gap between grid items
    width: "100%",
  },
  gridItem: {
    height: 150,
    backgroundColor: "#333",
    marginBottom: 5, // Additional gap for vertical spacing
    borderRadius: 8,
    justifyContent: "center",
    alignItems: "center",
  },
  itemText: {
    color: "white",
    fontWeight: "bold",
  },
  centerContainer: {
    flex: 1,
    justifyContent: "center",
    alignItems: "center",
    gap: 20,
    backgroundColor: "#000",
  },
});
