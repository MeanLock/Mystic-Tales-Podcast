import React from "react";
import { ActivityIndicator, Pressable, ScrollView } from "react-native";
import { useHeaderScroll } from "./_layout";
import { useBottomTabBarHeight } from "@react-navigation/bottom-tabs";
import { View } from "@/src/components/ui/View";
import MixxingText from "@/src/components/ui/MixxingText";
import ShowCarousel from "./components/ShowCarousel/ShowCarousel";
import EpisodeCarousel from "./components/EpisodeCarousel/EpisodeCarousel";
import PodcasterCarousel from "./components/PodcasterCarousel/PodcasterCarousel";
import { useSelector } from "react-redux";
import { RootState } from "@/src/store/store";
import { useGetDiscoveryFeedQuery } from "@/src/core/services/feed/feed.service";
import { Text } from "@/src/components/ui/Text";
import EpisodeContinueCarousel from "./components/EpisodeCarousel/EpisodeContinueCarousel";
import ChannelCarousel from "./components/ChannelCarousel/ChannelCarousel";
import { useRoute } from "@react-navigation/native";
import { useRouter } from "expo-router";

export default function Home() {
  const { onScroll, headerHeight } = useHeaderScroll();
  const tabBarHeight = useBottomTabBarHeight();

  // STATES
  const user = useSelector((state: RootState) => state.auth.user);

  // HOOKS
  const { data: discoveryData, isLoading: isDiscoveryDataLoading } =
    useGetDiscoveryFeedQuery(undefined, {
      refetchOnMountOrArgChange: true,
      refetchOnFocus: true,
      refetchOnReconnect: true,
    });

  const router = useRouter();

  if (isDiscoveryDataLoading || !discoveryData) {
    return (
      <View className="flex-1 flex items-center gap-2 justify-center bg-background-secondary">
        <ActivityIndicator size="large" color="#AEE339" />
        <Text className="text-[#D9D9D9]">Loading...</Text>
      </View>
    );
  } else {
    return (
      <ScrollView
        onScroll={onScroll}
        style={{ backgroundColor: "#000" }}
        contentContainerStyle={{
          paddingTop: headerHeight,
          paddingHorizontal: 10,
        }}
        scrollIndicatorInsets={{ top: headerHeight }}
        scrollEventThrottle={16} // Important for smooth animation
      >
        <View style={{ height: 20 }}></View>

        {/* Base On Your Taste Shows */}
        {user && discoveryData.BasedOnYourTaste.ShowList.length > 0 && (
          <ShowCarousel
            variant="normal"
            title={
              <MixxingText
                originalText="Base On Your Taste"
                coloredText="Your Taste"
              />
            }
            shows={discoveryData.BasedOnYourTaste.ShowList}
            titleString="Base On Your Taste"
          />
        )}

        {/* New Releases Shows */}
        {discoveryData.NewReleases.ShowList.length > 0 && (
          <ShowCarousel
            variant="top"
            title={
              <MixxingText originalText="New Releases" coloredText="New" />
            }
            shows={discoveryData.NewReleases.ShowList}
            titleString = "New Releases"
          />
        )}

        {/* Hot Shows */}
        {discoveryData.HotThisWeek.ShowList.length > 0 && (
          <ShowCarousel
            variant="top"
            title={<MixxingText originalText="Hot Shows" coloredText="Hot" />}
            shows={discoveryData.HotThisWeek.ShowList}
            titleString = "Hot Shows"
          />
        )}

        {/* Top Episodes */}
        {/* <EpisodeCarousel
          title={
            <MixxingText originalText="Top Episodes" coloredText="Episodes" />
          }
          episodes={data3}
        /> */}

        {/* Continue Listening */}
        {user &&
          discoveryData.ContinueListening.ListenSessionList.length > 0 && (
            <EpisodeContinueCarousel
              title={
                <MixxingText
                  originalText="Continue Listening"
                  coloredText="Listening"
                />
              }
              episodes={discoveryData.ContinueListening.ListenSessionList}
            />
          )}

        {/* Hot Channels */}
        {discoveryData.HotThisWeek.ChannelList.length > 0 && (
          <ChannelCarousel
            variant="top"
            title={
              <MixxingText originalText="Hot Channels" coloredText="Hot" />
            }
            channels={discoveryData.HotThisWeek.ChannelList}
            titleString = "Hot Channels"
          />
        )}

        {/* Top Podcasters */}
        {discoveryData.TopPodcasters.PodcasterList.length > 0 && (
          <PodcasterCarousel
            title={
              <MixxingText originalText="Top Podcasters" coloredText="Top" />
            }
            podcasters={discoveryData.TopPodcasters.PodcasterList}
          />
        )}

        {/* Talented Rookies */}
        {discoveryData.TalentedRookies.PodcasterList.length > 0 && (
          <PodcasterCarousel
            title={
              <MixxingText
                originalText="Talented Rookies"
                coloredText="Talented"
              />
            }
            podcasters={discoveryData.TalentedRookies.PodcasterList}
          />
        )}

        {/* Top Sub-Categories */}
        {discoveryData.TopSubCategory &&
          discoveryData.TopSubCategory.PodcastSubCategory &&
          discoveryData.TopSubCategory.ShowList.length > 0 && (
            <ShowCarousel
              variant="normal"
              title={
                <MixxingText
                  originalText={
                    discoveryData.TopSubCategory.PodcastSubCategory.Name
                  }
                  coloredText="\\?\\"
                />
              }
              shows={discoveryData.TopSubCategory.ShowList}
              titleString={discoveryData.TopSubCategory.PodcastSubCategory.Name}
            />
          )}

        {/* Random Category */}
        {discoveryData.RandomCategory &&
          discoveryData.RandomCategory.PodcastCategory &&
          discoveryData.RandomCategory.ShowList.length > 0 && (
            <ShowCarousel
              variant="normal"
              title={
                <MixxingText
                  originalText={
                    discoveryData.RandomCategory.PodcastCategory.Name
                  }
                  coloredText="\\?\\"
                />
              }
              shows={discoveryData.RandomCategory.ShowList}
              titleString={discoveryData.RandomCategory.PodcastCategory.Name}
            />
          )}

        <View style={{ height: tabBarHeight + 50 }}></View>
      </ScrollView>
    );
  }
}
