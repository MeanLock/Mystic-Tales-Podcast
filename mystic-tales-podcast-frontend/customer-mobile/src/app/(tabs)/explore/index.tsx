import { View, Text } from "@/src/components/Themed";
import { useBottomTabBarHeight } from "@react-navigation/bottom-tabs";
import EditScreenInfo from "@/src/components/EditScreenInfo";
import { ActivityIndicator, ScrollView } from "react-native";
import { useHeaderScroll } from "./_layout";
import { useGetTrendingFeedQuery } from "@/src/core/services/feed/feed.service";
import ShowCarousel from "./components/ShowCarousel/ShowCarousel";
import MixxingText from "@/src/components/ui/MixxingText";
import ChannelCarousel from "./components/ChannelCarousel/ChannelCarousel";
import EpisodeCarousel from "./components/EpisodeCarousel/EpisodeCarousel";
import PodcasterCarousel from "./components/PodcasterCarousel/PodcasterCarousel";

export default function Explore() {
  // Sử dụng hook useHeaderScroll để lấy onScroll và headerHeight
  const { onScroll, headerHeight } = useHeaderScroll();
  const tabBarHeight = useBottomTabBarHeight();

  // STATES

  // HOOKS
  const {
    data: trendingData,
    isLoading: trendingIsLoading,
    error: trendingError,
  } = useGetTrendingFeedQuery(undefined, {
    refetchOnMountOrArgChange: true,
    refetchOnFocus: true,
    refetchOnReconnect: true,
  });

  if (trendingIsLoading || !trendingData) {
    return (
      <View className="flex-1 justify-center items-center gap-5 bg-black">
        <ActivityIndicator size="large" color="#AEE339" />
        <Text className="text-[#D9D9D9]">Loading trending feed...</Text>
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

        {/* Content gì thì để ở đây */}

        {/* Popular Shows */}
        {trendingData.PopularShows.ShowList.length > 0 && (
          <ShowCarousel
            variant="normal"
            title={
              <MixxingText originalText="Popular Shows" coloredText="Popular" />
            }
            shows={trendingData.PopularShows.ShowList}
            titleString="Popular Shows"
          />
        )}

        {/* Hot Shows */}
        {trendingData.HotShows.ShowList.length > 0 && (
          <ShowCarousel
            variant="normal"
            title={<MixxingText originalText="Hot Shows" coloredText="Hot" />}
            shows={trendingData.HotShows.ShowList}
            titleString="Hot Shows"
          />
        )}

        {/* Popular Channels */}
        {trendingData.PopularChannels.ChannelList.length > 0 && (
          <ChannelCarousel
            channels={trendingData.PopularChannels.ChannelList}
            title={
              <MixxingText
                originalText="Popular Channels"
                coloredText="Popular"
              />
            }
            variant="normal"
            titleString="Popular Channels"
          />
        )}

        {/* Hot Channels */}
        {trendingData.HotChannels.ChannelList.length > 0 && (
          <ChannelCarousel
            channels={trendingData.HotChannels.ChannelList}
            title={
              <MixxingText originalText="Hot Channels" coloredText="Hot" />
            }
            variant="normal"
            titleString="Hot Channels"
          />
        )}

        {/* Popular Podcasters */}
        {trendingData.PopularPodcasters.PodcasterList.length > 0 && (
          <PodcasterCarousel
            podcasters={trendingData.PopularPodcasters.PodcasterList}
            title={
              <MixxingText
                originalText="Popular Podcasters"
                coloredText="Popular"
              />
            }
            itemSize={150}
            itemSpacing={10}
          />
        )}

        {/* Hot Podcasters */}
        {trendingData.HotPodcasters.PodcasterList.length > 0 && (
          <PodcasterCarousel
            podcasters={trendingData.HotPodcasters.PodcasterList}
            title={
              <MixxingText originalText="Hot Podcasters" coloredText="Hot" />
            }
            itemSize={120}
            itemSpacing={20}
          />
        )}

        {/* New Episodes */}
        {trendingData.NewEpisodes.EpisodeList.length > 0 && (
          <EpisodeCarousel
            episodes={trendingData.NewEpisodes.EpisodeList}
            title={
              <MixxingText originalText="New Episodes" coloredText="New" />
            }
            titleString="New Episodes"
          />
        )}

        {/* Popular Episodes */}
        {trendingData.PopularEpisodes.EpisodeList.length > 0 && (
          <EpisodeCarousel
            episodes={trendingData.PopularEpisodes.EpisodeList}
            title={
              <MixxingText
                originalText="Popular Episodes"
                coloredText="Popular"
              />
            }
            titleString="Popular Episodes"
          />
        )}

        {/* Categories */}
        {trendingData.Category1 &&
          trendingData.Category1.ShowList.length > 0 &&
          trendingData.Category1.PodcastCategory && (
            <ShowCarousel
              variant="normal"
              title={
                <MixxingText
                  originalText={trendingData.Category1.PodcastCategory.Name}
                  coloredText={trendingData.Category1.PodcastCategory.Name}
                />
              }
              shows={trendingData.Category1.ShowList}
              titleString={trendingData.Category1.PodcastCategory.Name}
            />
          )}
        {trendingData.Category2 &&
          trendingData.Category2.ShowList.length > 0 &&
          trendingData.Category2.PodcastCategory && (
            <ShowCarousel
              variant="normal"
              title={
                <MixxingText
                  originalText={trendingData.Category2.PodcastCategory.Name}
                  coloredText={trendingData.Category2.PodcastCategory.Name}
                />
              }
              shows={trendingData.Category2.ShowList}
              titleString={trendingData.Category2.PodcastCategory.Name}
            />
          )}
        {trendingData.Category3 &&
          trendingData.Category3.ShowList.length > 0 &&
          trendingData.Category3.PodcastCategory && (
            <ShowCarousel
              variant="normal"
              title={
                <MixxingText
                  originalText={trendingData.Category3.PodcastCategory.Name}
                  coloredText={trendingData.Category3.PodcastCategory.Name}
                />
              }
              shows={trendingData.Category3.ShowList}
              titleString={trendingData.Category3.PodcastCategory.Name}
            />
          )}
        {trendingData.Category4 &&
          trendingData.Category4.ShowList.length > 0 &&
          trendingData.Category4.PodcastCategory && (
            <ShowCarousel
              variant="normal"
              title={
                <MixxingText
                  originalText={trendingData.Category4.PodcastCategory.Name}
                  coloredText={trendingData.Category4.PodcastCategory.Name}
                />
              }
              shows={trendingData.Category4.ShowList}
              titleString={trendingData.Category4.PodcastCategory.Name}
            />
          )}
        {trendingData.Category5 &&
          trendingData.Category5.ShowList.length > 0 &&
          trendingData.Category5.PodcastCategory && (
            <ShowCarousel
              variant="normal"
              title={
                <MixxingText
                  originalText={trendingData.Category5.PodcastCategory.Name}
                  coloredText={trendingData.Category5.PodcastCategory.Name}
                />
              }
              shows={trendingData.Category5.ShowList}
              titleString={trendingData.Category5.PodcastCategory.Name}
            />
          )}
        {trendingData.Category6 &&
          trendingData.Category6.ShowList.length > 0 &&
          trendingData.Category6.PodcastCategory && (
            <ShowCarousel
              variant="normal"
              title={
                <MixxingText
                  originalText={trendingData.Category6.PodcastCategory.Name}
                  coloredText={trendingData.Category6.PodcastCategory.Name}
                />
              }
              shows={trendingData.Category6.ShowList}
              titleString={trendingData.Category6.PodcastCategory.Name}
            />
          )}
        <View style={{ height: tabBarHeight + 50 }}></View>
      </ScrollView>
    );
  }
}
