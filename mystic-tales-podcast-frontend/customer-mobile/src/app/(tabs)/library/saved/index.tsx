import { Text } from "@/src/components/ui/Text";
import { useGetSavedEpisodesQuery } from "@/src/core/services/episode/episode.service";
import {
  ActivityIndicator,
  StyleSheet,
  Text as RNText,
  View,
  Animated,
  Dimensions,
} from "react-native";
import { useHeaderScroll } from "../_layout";
import { useBottomTabBarHeight } from "@react-navigation/bottom-tabs";

export default function SavedScreen() {
  // STATES

  // HOOKS
  const { onScroll } = useHeaderScroll();
  const tabBarHeight = useBottomTabBarHeight();
  const windowWidth = Dimensions.get("window").width;
  const itemWidth = (windowWidth - 16 * 2 - 10) / 2; // Calculate item width based on screen width, padding, and gap

  const { data: savedEpisodes, isLoading: isLoadingSavedEpisodes } =
    useGetSavedEpisodesQuery();

  if (isLoadingSavedEpisodes) {
    return (
      <View
        style={{
          flex: 1,
          justifyContent: "center",
          alignItems: "center",
          gap: 10,
        }}
      >
        <ActivityIndicator size="large" color="#AEE339" />
        <Text>Loading saved episodes...</Text>
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
      }}
    >
      <Text className="text-white font-bold text-4xl my-5 mb-10">
        Saved Episodes
      </Text>

      {/* Grid layout using flexDirection: row and flexWrap: wrap */}
      <View style={styles.gridColTwoContainer}>
        {savedEpisodes?.SavedEpisodes.map((item, index) => (
          <Text key={index} style={styles.itemText}>
            {item.Name}
          </Text>
        ))}
      </View>
      <View style={{ height: tabBarHeight + 50 }}></View>
    </Animated.ScrollView>
  );
}

const styles = StyleSheet.create({
  container: {},
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
});
