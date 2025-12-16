// /(tabs)/library/shows/index.tsx
import React from "react";
import { ActivityIndicator, Animated, Dimensions } from "react-native";
import { View } from "@/src/components/ui/View";
import { Text } from "@/src/components/ui/Text";
import { useHeaderScroll } from "../_layout";
import { StyleSheet } from "react-native";
import ShowCard from "./components/ShowCard";
import { useBottomTabBarHeight } from "@react-navigation/bottom-tabs";
import { useGetFollowedShowsQuery } from "@/src/core/services/show/show.service";
import { useGetSubscribedContentsQuery } from "@/src/core/services/subscription/subscription.service";

export default function ShowsScreen() {
  const { onScroll } = useHeaderScroll();
  const tabBarHeight = useBottomTabBarHeight();
  const windowWidth = Dimensions.get("window").width;
  const itemWidth = (windowWidth - 16 * 2 - 10) / 2; // Calculate item width based on screen width, padding, and gap

  const { data: followedShows, isLoading: isLoadingFollowedShows } =
    useGetFollowedShowsQuery(undefined, {
      refetchOnMountOrArgChange: true,
      refetchOnFocus: true,
      refetchOnReconnect: true,
    });

  const { data: subscribedContents, isLoading: isLoadingSubscribedShows } =
    useGetSubscribedContentsQuery(undefined, {
      refetchOnMountOrArgChange: true,
      refetchOnFocus: true,
      refetchOnReconnect: true,
    });

  if (isLoadingFollowedShows || isLoadingSubscribedShows) {
    return (
      <View style={style.centerContainer}>
        <ActivityIndicator size="large" color="#aee339" />
        <Text style={{ color: "#aee339", fontSize: 16, fontWeight: "bold" }}>
          Loading Shows...
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
        Followed Shows
      </Text>

      {/* Grid layout using flexDirection: row and flexWrap: wrap */}
      {followedShows && followedShows.ShowList.length > 0 ? (
        <>
          <View style={style.gridColTwoContainer}>
            {followedShows.ShowList.map((item, index) => (
              <ShowCard width={itemWidth} show={item} key={index} />
            ))}
          </View>
        </>
      ) : (
        <View style={style.centerContainer}>
          <Text style={{ color: "#aee339", fontSize: 20, fontWeight: "bold" }}>
            No Followed Show Available Now
          </Text>
        </View>
      )}

      <Text className="text-white font-bold text-2xl mt-10 mb-10">
        Subscribed Shows
      </Text>

      {/* Grid layout using flexDirection: row and flexWrap: wrap */}
      {subscribedContents &&
      subscribedContents.PodcastShowList &&
      subscribedContents.PodcastShowList.length > 0 ? (
        <>
          <View style={style.gridColTwoContainer}>
            {subscribedContents.PodcastShowList.map((item, index) => (
              <ShowCard width={itemWidth} show={item} key={index} />
            ))}
          </View>
        </>
      ) : (
        <View style={style.centerContainer}>
          <Text style={{ color: "#aee339", fontSize: 20, fontWeight: "bold" }}>
            No Subscribed Shows Available Now
          </Text>
        </View>
      )}
      <View style={{ height: tabBarHeight + 50 }}></View>
    </Animated.ScrollView>
  );
}

const style = StyleSheet.create({
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
