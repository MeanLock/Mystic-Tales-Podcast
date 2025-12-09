// /(tabs)/library/shows/index.tsx
import React from "react";
import { ActivityIndicator, Animated, Dimensions } from "react-native";
import { View } from "@/src/components/ui/View";
import { Text } from "@/src/components/ui/Text";
import { useHeaderScroll } from "../_layout";
import { StyleSheet } from "react-native";

import { useBottomTabBarHeight } from "@react-navigation/bottom-tabs";
import { useGetFollowedShowsQuery } from "@/src/core/services/show/show.service";
import { useGetSubscribedContentsQuery } from "@/src/core/services/subscription/subscription.service";
import { useGetFavoritedChannelsQuery } from "@/src/core/services/channel/channel.service";
import ChannelRowCard from "./components/ChannelRowCard";

export default function ChannelsScreen() {
  const { onScroll } = useHeaderScroll();
  const tabBarHeight = useBottomTabBarHeight();
  const windowWidth = Dimensions.get("window").width;
  const itemWidth = (windowWidth - 16 * 2 - 10) / 2; // Calculate item width based on screen width, padding, and gap

  const { data: favoriteChannels, isLoading: isLoadingFavoriteChannels } =
    useGetFavoritedChannelsQuery();

  const { data: subscribedContents, isLoading: isLoadingSubscribedChannels } =
    useGetSubscribedContentsQuery();

  if (isLoadingFavoriteChannels || isLoadingSubscribedChannels) {
    return (
      <View style={style.centerContainer}>
        <ActivityIndicator size="large" color="#AEE339" />
        <Text style={{ color: "#AEE339", fontSize: 16, fontWeight: "bold" }}>
          Loading Channels...
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
        Favorite Channels
      </Text>

      {/* Grid layout using flexDirection: row and flexWrap: wrap */}
      {favoriteChannels && favoriteChannels.ChannelList.length > 0 ? (
        <>
          <View className="w-full flex flex-col items-start gap-1">
            {favoriteChannels.ChannelList.map((item, index) => (
              <ChannelRowCard key={`Favorties-${item.Id}`} channel={item} />
            ))}
          </View>
        </>
      ) : (
        <View className="w-full h-56 flex items-center justify-center">
          <Text
            style={{ color: "#aee339", fontSize: 12, fontWeight: "medium" }}
          >
            No Followed Show Available Now
          </Text>
        </View>
      )}

      <Text className="text-white font-bold text-2xl mt-10 mb-10">
        Subscribed Shows
      </Text>
      {subscribedContents &&
      subscribedContents.PodcastChannelList.length > 0 ? (
        <>
          <View className="w-full flex flex-col items-start gap-1">
            {subscribedContents.PodcastChannelList.map((item, index) => (
              <ChannelRowCard key={`Subscribed-${item.Id}`} channel={item} />
            ))}
          </View>
        </>
      ) : (
        <View className="w-full h-56 flex items-center justify-center">
          <Text
            style={{ color: "#aee339", fontSize: 12, fontWeight: "medium" }}
          >
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
