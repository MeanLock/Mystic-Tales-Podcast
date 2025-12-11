import AutoResolvingImage from "@/src/components/autoResolveImage/AutoResolvingImage";
import { useGetChannelListFromPodcasterQuery } from "@/src/core/services/channel/channel.service";
import {
  useFollowPodcasterMutation,
  useGetPodcasterDetailsQuery,
  useUnFollowPodcasterMutation,
} from "@/src/core/services/podcasters/podcaster.service";
import { useGetShowListFromPodcasterQuery } from "@/src/core/services/show/show.service";
import { Show } from "@/src/core/types/show.type";
import { Entypo, Ionicons, MaterialCommunityIcons } from "@expo/vector-icons";
import { useLocalSearchParams, useRouter } from "expo-router/build/hooks";
import { useEffect, useMemo, useState } from "react";
import {
  ActivityIndicator,
  Platform,
  Pressable,
  ScrollView,
  StyleSheet,
  Text,
  View,
} from "react-native";
import ShowCarousel from "./components/Show/ShowCarousel";
import ChannelCarousel from "./components/Channel/ChannelCarousel";
import RatingAndReview from "./components/Rating/RatingAndReview";

type ShowsByCategory = {
  Category: {
    Id: number;
    Name: string;
  };
  Shows: Show[];
};

export default function PodcasterDetailsPage() {
  const { id } = useLocalSearchParams<{ id: string }>();

  // STATES
  const [isFollowed, setIsFollowed] = useState<boolean>(false);

  // HOOKS
  const router = useRouter();
  const {
    data: podcaster,
    isLoading: isLoadingPodcaster,
    refetch: refetchPodcasterDetails,
  } = useGetPodcasterDetailsQuery({ podcasterId: Number(id) }, { skip: !id });

  const {
    data: channels,
    isLoading: isLoadingChannels,
    refetch: refetchChannels,
  } = useGetChannelListFromPodcasterQuery(
    { podcasterId: Number(id) },
    { skip: !id }
  );

  const {
    data: shows,
    isLoading: isLoadingShows,
    refetch: refetchShows,
  } = useGetShowListFromPodcasterQuery(
    { podcasterId: Number(id) },
    { skip: !id }
  );

  // Group shows by category
  const showsByCategory = useMemo<ShowsByCategory[]>(() => {
    if (!shows?.ShowList) return [];

    const categoryMap = new Map<number, ShowsByCategory>();

    shows.ShowList.forEach((show) => {
      const categoryId = show.PodcastCategory.Id;

      if (!categoryMap.has(categoryId)) {
        categoryMap.set(categoryId, {
          Category: {
            Id: show.PodcastCategory.Id,
            Name: show.PodcastCategory.Name,
          },
          Shows: [],
        });
      }

      categoryMap.get(categoryId)?.Shows.push(show);
    });

    return Array.from(categoryMap.values());
  }, [shows]);

  useEffect(() => {
    if (podcaster) {
      setIsFollowed(podcaster.IsFollowedByCurrentUser);
    }
  }, [podcaster, isLoadingPodcaster]);

  const [followPodcaster] = useFollowPodcasterMutation();
  const [unFollowPodcaster] = useUnFollowPodcasterMutation();

  // FUNCTIONS
  const handleToggleFollowPodcaster = async (shouldFollow: boolean) => {
    const restoreValue = isFollowed;
    setIsFollowed(shouldFollow);
    // Call API to follow/unfollow podcaster
    // If error, restore previous state
    if (shouldFollow) {
      await followPodcaster({ PodcasterId: Number(id) })
        .unwrap()
        .catch(() => {
          setIsFollowed(restoreValue);
        });
      await refetchPodcasterDetails();
    } else {
      await unFollowPodcaster({ PodcasterId: Number(id) })
        .unwrap()
        .catch(() => {
          setIsFollowed(restoreValue);
        });
      await refetchPodcasterDetails();
    }
  };

  if (isLoadingPodcaster || isLoadingChannels || isLoadingShows) {
    return (
      <View className="flex-1 bg-black flex items-center justify-center gap-5">
        <ActivityIndicator size="large" color="#aee339" />
        <Text className="text-[#aee339] font-bold text-lg">
          Loading Podcaster Details...
        </Text>
      </View>
    );
  }
  return (
    <ScrollView style={styles.container}>
      <View className="relative h-[500px] flex items-center justify-center">
        <AutoResolvingImage
          FileKey={podcaster?.MainImageFileKey}
          type="AccountPublicSource"
          style={{ width: "100%", height: "100%" }}
        />
        <View
          style={{
            left: Platform.OS === "ios" ? 20 : 12,
            right: Platform.OS === "ios" ? 20 : 12,
            top: Platform.OS === "ios" ? 50 : 50,
          }}
          className="absolute z-50 top-14 flex flex-row items-center justify-between"
        >
          <Pressable onPress={() => router.back()} style={styles.actionButton}>
            {Platform.OS === "ios" ? (
              <Entypo name="chevron-small-left" size={24} color="white" />
            ) : (
              <MaterialCommunityIcons
                name="arrow-left"
                size={15}
                color="#fff"
              />
            )}
          </Pressable>
        </View>
        <View className="absolute inset-0 z-10 bg-black/30" />
        <View className="absolute inset-0 z-30 flex flex-row items-end justify-between p-5">
          <Text className="text-white text-3xl font-bold">
            {podcaster?.Name}
          </Text>
          <Pressable
            onPress={() => handleToggleFollowPodcaster(!isFollowed)}
            style={styles.actionButton}
          >
            {Platform.OS === "ios" ? (
              isFollowed ? (
                <Ionicons name="heart-sharp" size={24} color="#aee339" />
              ) : (
                <Ionicons name="heart-outline" size={24} color="white" />
              )
            ) : (
              <MaterialCommunityIcons
                name={isFollowed ? "heart" : "heart-outline"}
                size={15}
                color={isFollowed ? "#aee339" : "#fff"}
              />
            )}
          </Pressable>
        </View>
      </View>

      <View className="p-5 gap-5 mt-10">
        <Text className="text-3xl text-white font-semibold">My Channels</Text>
        {channels && channels.ChannelList.length > 0 && (
          <ChannelCarousel channels={channels.ChannelList} variant="normal" />
        )}
        <Text className="text-3xl text-white font-semibold">My Shows</Text>
        <View className="flex flex-col gap-5">
          {showsByCategory.map((categoryGroup) => (
            <ShowCarousel
              key={categoryGroup.Category.Id}
              variant="normal"
              title={categoryGroup.Category.Name}
              shows={categoryGroup.Shows}
            />
          ))}
        </View>

        {podcaster &&
          podcaster.ReviewList &&
          podcaster.ReviewList.length > 0 && (
            <>
              <Text className="text-3xl text-white font-semibold">
                Ratings & Reviews
              </Text>
              <RatingAndReview
                isCommented={false}
                ratings={podcaster?.ReviewList}
              />
            </>
          )}
      </View>
      <View style={{ height: 100 }} />
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: "#0f0f0f",
  },
  actionButton: {
    padding: 6,
    borderRadius: 50,
    backgroundColor: "rgba(128, 128, 128, 0.7)",
    overflow: "hidden",
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.3,
    shadowRadius: 3,
    elevation: 2,
  },
});
