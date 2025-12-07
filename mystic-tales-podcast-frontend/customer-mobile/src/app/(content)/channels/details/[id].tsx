import AutoResolvingImage from "@/src/components/autoResolveImage/AutoResolvingImage";
import { Text } from "@/src/components/ui/Text";
import { View } from "@/src/components/ui/View";
import {
  useGetActiveChannelSubscriptionQuery,
  useGetChannelDetailsQuery,
} from "@/src/core/services/channel/channel.service";
import { useGetCustomerRegistrationInfoFromChannelQuery } from "@/src/core/services/subscription/subscription.service";
import { ChannelDetails } from "@/src/core/types/channel.type";
import { SubscriptionDetails } from "@/src/core/types/subscription.type";
import {
  Entypo,
  Feather,
  FontAwesome6,
  Ionicons,
  MaterialCommunityIcons,
} from "@expo/vector-icons";
import { BlurView } from "expo-blur";
import { useRouter } from "expo-router";
import { useLocalSearchParams, useSearchParams } from "expo-router/build/hooks";
import { useEffect, useState } from "react";
import {
  Image,
  StyleSheet,
  View as RNView,
  ScrollView,
  Pressable,
  Platform,
  ActivityIndicator,
} from "react-native";
import { SafeAreaInsetsContext } from "react-native-safe-area-context";
import ShowCarousel from "./components/ShowCarousel";
import MixxingText from "@/src/components/ui/MixxingText";
import RatingAndReview from "./components/RatingAndReview";
import HtmlText from "@/src/components/renderHtml/HtmlText";

// Mock
// const channelDetails: ChannelDetails = {
//   Id: "3fa85f64-5717-4562-b3fc-2c963f66afa6",
//   Name: "The Art of Samurice",
//   Description: "",
//   BackgroundImageFileKey:
//     "https://i.pinimg.com/1200x/bc/79/53/bc7953940f9e6f3206e2474b95c16a23.jpg",
//   MainImageFileKey:
//     "https://i.pinimg.com/736x/7c/16/96/7c169655ab155702cfcb5294eb4471aa.jpg",
//   TotalFavorite: 0,
//   ListenCount: 0,
//   ShowCount: 0,
//   Podcaster: {
//     Id: 0,
//     FullName: "string",
//     Email: "string",
//     MainImageFileKey: "string",
//   },
//   PodcastCategory: {
//     Id: 0,
//     Name: "string",
//     MainImageFileKey: "string",
//   },
//   PodcastSubCategory: {
//     Id: 0,
//     Name: "string",
//     PodcastCategoryId: 0,
//   },
//   Hashtags: [
//     {
//       Id: 0,
//       Name: "string",
//     },
//   ],
//   CreatedAt: "2025-12-07T04:04:51.478Z",
//   UpdatedAt: "2025-12-07T04:04:51.478Z",
//   CurrentStatus: {
//     Id: 0,
//     Name: "string",
//   },
//   PodcastSubscriptionList: [
//     {
//       Id: 0,
//       Name: "string",
//       Description: "string",
//       PodcastChannelId: "3fa85f64-5717-4562-b3fc-2c963f66afa6",
//       PodcastShowId: "3fa85f64-5717-4562-b3fc-2c963f66afa6",
//       IsActive: false,
//       CurrentVersion: 0,
//       DeletedAt: "2025-12-07T04:04:51.478Z",
//       CreatedAt: "2025-12-07T04:04:51.478Z",
//       UpdatedAt: "2025-12-07T04:04:51.478Z",
//       PodcastSubscriptionCycleTypePriceList: [
//         {
//           PodcastSubscriptionId: 0,
//           SubscriptionCycleType: {
//             Id: 0,
//             Name: "string",
//           },
//           Version: 0,
//           Price: 0,
//           CreatedAt: "2025-12-07T04:04:51.478Z",
//           UpdatedAt: "2025-12-07T04:04:51.478Z",
//         },
//       ],
//       PodcastSubscriptionBenefitMappingList: [
//         {
//           PodcastSubscriptionId: 0,
//           PodcastSubscriptionBenefit: {
//             Id: 0,
//             Name: "string",
//           },
//           Version: 0,
//           CreatedAt: "2025-12-07T04:04:51.479Z",
//           UpdatedAt: "2025-12-07T04:04:51.479Z",
//         },
//       ],
//     },
//   ],
//   ShowList: [
//     {
//       Id: "3fa85f64-5717-4562-b3fc-2c963f66afa6",
//       Name: "string",
//       Description: "string",
//       Language: "string",
//       ReleaseDate: "2025-12-07T04:04:51.479Z",
//       IsReleased: true,
//       Copyright: "string",
//       UploadFrequency: "string",
//       RatingCount: 0,
//       AverageRating: 0,
//       MainImageFileKey: "string",
//       TrailerAudioFileKey: "string",
//       TotalFollow: 0,
//       ListenCount: 0,
//       EpisodeCount: 0,
//       Podcaster: {
//         Id: 0,
//         FullName: "string",
//         Email: "string",
//         MainImageFileKey: "string",
//       },
//       PodcastCategory: {
//         Id: 0,
//         Name: "string",
//         MainImageFileKey: "string",
//       },
//       PodcastSubCategory: {
//         Id: 0,
//         Name: "string",
//         PodcastCategoryId: 0,
//       },
//       PodcastShowSubscriptionType: {
//         Id: 0,
//         Name: "string",
//       },
//       PodcastChannel: {
//         Id: "3fa85f64-5717-4562-b3fc-2c963f66afa6",
//         Name: "string",
//         Description: "string",
//         MainImageFileKey: "string",
//       },
//       Hashtags: [
//         {
//           Id: 0,
//           Name: "string",
//         },
//       ],
//       TakenDownReason: "string",
//       CreatedAt: "2025-12-07T04:04:51.479Z",
//       UpdatedAt: "2025-12-07T04:04:51.479Z",
//       CurrentStatus: {
//         Id: 0,
//         Name: "string",
//       },
//     },
//   ],
//   IsFavoritedByCurrentUser: true,
// };

// const activeSubscription: SubscriptionDetails = {
//   Id: 0,
//   Name: "Learning to be a Samurai",
//   Description: null,
//   PodcastChannelId: "3fa85f64-5717-4562-b3fc-2c963f66afa6",
//   PodcastShowId: "3fa85f64-5717-4562-b3fc-2c963f66afa6",
//   IsActive: false,
//   CurrentVersion: 0,
//   DeletedAt: "2025-12-07T05:06:33.134Z",
//   CreatedAt: "2025-12-07T05:06:33.134Z",
//   UpdatedAt: "2025-12-07T05:06:33.134Z",
//   PodcastSubscriptionCycleTypePriceList: [
//     {
//       PodcastSubscriptionId: 0,
//       SubscriptionCycleType: {
//         Id: 1,
//         Name: "Monthly",
//       },
//       Version: 0,
//       Price: 20000000,
//       CreatedAt: "2025-12-07T05:06:33.134Z",
//       UpdatedAt: "2025-12-07T05:06:33.134Z",
//     },
//     {
//       PodcastSubscriptionId: 0,
//       SubscriptionCycleType: {
//         Id: 2,
//         Name: "Annually",
//       },
//       Version: 0,
//       Price: 129900000,
//       CreatedAt: "2025-12-07T05:06:33.134Z",
//       UpdatedAt: "2025-12-07T05:06:33.134Z",
//     },
//   ],
//   PodcastSubscriptionBenefitMappingList: [
//     {
//       PodcastSubscriptionId: 0,
//       PodcastSubscriptionBenefit: {
//         Id: 1,
//         Name: "Non-Quota Listening",
//       },
//       Version: 0,
//       CreatedAt: "2025-12-07T05:06:33.134Z",
//       UpdatedAt: "2025-12-07T05:06:33.134Z",
//     },
//   ],
//   PodcastSubscriptionRegistrationList: null,
// };

type ShowByCategory = {
  Category: {
    Id: number;
    Name: string;
    MainImageFileKey: string;
  };
  ShowList: ChannelDetails["ShowList"];
};

const renderSubscriptionPriceInfo = (subscription: SubscriptionDetails) => {
  // Find monthly and yearly prices
  const monthlyPrice = subscription.PodcastSubscriptionCycleTypePriceList.find(
    (price) => price.SubscriptionCycleType.Id === 1
  );
  const yearlyPrice = subscription.PodcastSubscriptionCycleTypePriceList.find(
    (price) => price.SubscriptionCycleType.Id === 2
  );

  // Format price to Vietnamese dong
  const formatPrice = (price: number) => {
    return new Intl.NumberFormat("vi-VN").format(price);
  };

  // Build price text
  const priceTexts: string[] = [];

  if (monthlyPrice) {
    priceTexts.push(`${formatPrice(monthlyPrice.Price)} đ/month`);
  }

  if (yearlyPrice) {
    priceTexts.push(`${formatPrice(yearlyPrice.Price)} đ/year`);
  }

  // Join with " hoặc " if both exist
  return priceTexts.join(" or ");
};

export default function ChannelDetailsScreen() {
  const { id } = useLocalSearchParams<{ id: string }>();

  // STATES
  const [isUserSubscribed, setIsUserSubscribed] = useState(false);
  const [isUserFavorited, setIsUserFavorited] = useState(false);
  const [isSubscriptionOptionsVisible, setIsSubscriptionOptionsVisible] =
    useState(false);
  const [selectedSubscriptionOption, setSelectedSubscriptionOption] =
    useState<number>(1);
  const [showByCategories, setShowByCategories] = useState<ShowByCategory[]>(
    []
  );

  // HOOKS
  const router = useRouter();
  const { data: channelDetails, isLoading: isChannelDetailsLoading } =
    useGetChannelDetailsQuery(
      { ChannelId: id! },
      {
        skip: !id,
        refetchOnFocus: true,
        refetchOnMountOrArgChange: true,
        refetchOnReconnect: true,
      }
    );
  const { data: activeSubscription, isLoading: isActiveSubscriptionLoading } =
    useGetActiveChannelSubscriptionQuery(
      { ChannelId: id! },
      {
        skip: !id,
        refetchOnFocus: true,
        refetchOnMountOrArgChange: true,
        refetchOnReconnect: true,
      }
    );
  const {
    data: customerRegistrationInfo,
    isLoading: isUserRegistrationInfoLoading,
  } = useGetCustomerRegistrationInfoFromChannelQuery(
    { PodcastChannelId: id! },
    {
      skip: !id,
      refetchOnFocus: true,
      refetchOnMountOrArgChange: true,
      refetchOnReconnect: true,
    }
  );
  useEffect(() => {
    if (
      !channelDetails ||
      isChannelDetailsLoading ||
      isUserRegistrationInfoLoading ||
      isActiveSubscriptionLoading
    ) {
      return;
    } else {
      // Update favorited state
      setIsUserFavorited(channelDetails.Channel.IsFavoritedByCurrentUser);
      // Update subscribed state
      if (
        !customerRegistrationInfo ||
        !customerRegistrationInfo?.PodcastSubscriptionRegistration
      ) {
        setIsUserSubscribed(false);
      } else {
        if (
          activeSubscription &&
          activeSubscription.PodcastSubscription &&
          activeSubscription.PodcastSubscription.Id ===
            customerRegistrationInfo.PodcastSubscriptionRegistration
              .PodcastSubscriptionId
        ) {
          setIsUserSubscribed(true);
        } else {
          setIsUserSubscribed(false);
        }
      }

      // Map shows by category
      const categoryMap = new Map<number, ShowByCategory>();

      channelDetails.Channel.ShowList.forEach((show) => {
        const categoryId = show.PodcastCategory.Id;

        if (!categoryMap.has(categoryId)) {
          categoryMap.set(categoryId, {
            Category: {
              Id: show.PodcastCategory.Id,
              Name: show.PodcastCategory.Name,
              MainImageFileKey: show.PodcastCategory.MainImageFileKey,
            },
            ShowList: [],
          });
        }

        categoryMap.get(categoryId)!.ShowList.push(show);
      });

      // Convert map to array and sort by category name
      const categorizedShows = Array.from(categoryMap.values()).sort((a, b) =>
        a.Category.Name.localeCompare(b.Category.Name)
      );

      setShowByCategories(categorizedShows);
    }
  }, [
    channelDetails,
    isChannelDetailsLoading,
    customerRegistrationInfo,
    isUserRegistrationInfoLoading,
    activeSubscription,
    isActiveSubscriptionLoading,
  ]);

  // FUNCTIONS
  const handleToggleFavorite = async (shouldFavorite: boolean) => {
    const restoreValue = isUserFavorited;
    // Gọi API để thêm hoặc bỏ yêu thích channel
    setIsUserFavorited(shouldFavorite);
  };

  if (isChannelDetailsLoading || !channelDetails) {
    return (
      <View className="flex-1 items-center justify-center bg-black gap-5">
        <ActivityIndicator size="large" color="#aee339" />
        <Text className="mt-4 text-white font-medium text-lg">
          Loading Channel Details...
        </Text>
      </View>
    );
  }

  return (
    <ScrollView
      style={styles.container}
      contentContainerStyle={styles.scrollContent}
    >
      <View style={styles.channelHeaderContainer}>
        {/* Background Image */}
        {/* <Image
          source={{ uri: channelDetails.BackgroundImageFileKey }}
          style={styles.channelHeaderBackgroundImage}
        /> */}
        <AutoResolvingImage
          FileKey={channelDetails.Channel.BackgroundImageFileKey}
          type="PodcastPublicSource"
          style={styles.channelHeaderBackgroundImage}
        />
        {/* Dark Overlay */}
        <View className="absolute inset-0 bg-black/50 backdrop-blur-md z-20" />

        {/* Content */}
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

          <RNView className="flex flex-row items-center gap-5">
            <Pressable
              onPress={() => handleToggleFavorite(!isUserFavorited)}
              style={styles.actionButton}
            >
              {Platform.OS === "ios" ? (
                isUserFavorited ? (
                  <Ionicons name="heart-sharp" size={24} color="#aee339" />
                ) : (
                  <Ionicons name="heart-outline" size={24} color="white" />
                )
              ) : (
                <MaterialCommunityIcons
                  name={isUserFavorited ? "heart" : "heart-outline"}
                  size={15}
                  color={isUserFavorited ? "#aee339" : "#fff"}
                />
              )}
            </Pressable>
            <RNView style={styles.actionButton}>
              {Platform.OS === "ios" ? (
                <Feather name="more-horizontal" size={24} color="white" />
              ) : (
                <MaterialCommunityIcons
                  name="dots-vertical"
                  size={15}
                  color="#fff"
                />
              )}
            </RNView>
          </RNView>
        </View>
        <View className="absolute inset-0 flex items-center justify-center z-30 gap-2">
          <Text className="text-white font-bold text-3xl line-clamp-1">
            {channelDetails.Channel.Name}
          </Text>
          <Text className="text-white font-light text-md">
            Channel • {channelDetails.Channel.ShowCount} shows
          </Text>
        </View>

        {/* Subscription Informations */}
        {!isUserSubscribed &&
          activeSubscription &&
          activeSubscription.PodcastSubscription && (
            <BlurView
              style={{ borderRadius: 10, overflow: "hidden", elevation: 5 }}
              className="absolute bottom-10 left-3 right-3 flex gap-2 bg-black/40 backdrop-blur-md p-1 z-40"
            >
              <RNView className=" rounded-full px-4 py-2 flex flex-row items-center justify-between">
                <Text className="text-white text-center uppercase font-extrabold text-xs">
                  {channelDetails.Channel.Name}
                </Text>
                <Text className="text-[#adadad] text-center font-medium text-xs">
                  Subscribe
                </Text>
              </RNView>
              <RNView className="w-full flex flex-row items-center justify-between gap-2 px-4">
                <RNView className="w-16 h-16 rounded-md overflow-hidden flex items-center justify-center">
                  <AutoResolvingImage
                    FileKey={channelDetails.Channel.MainImageFileKey}
                    type="PodcastPublicSource"
                    style={styles.channelImage}
                  />
                </RNView>
                <RNView className="flex-1 flex flex-col items-start justify-center gap-1 ml-2">
                  <Text className="text-white line-clamp-2 font-semibold">
                    {activeSubscription.PodcastSubscription.Name}
                  </Text>

                  <Text className="text-[#D9D9D9] font-medium text-sm line-clamp-2">
                    {renderSubscriptionPriceInfo(
                      activeSubscription.PodcastSubscription
                    )}
                  </Text>
                </RNView>
              </RNView>
              <RNView className="flex-1 flex flex-row p-2 items-center justify-end">
                <Pressable
                  onPress={() => alert("alo")}
                  className="bg-white rounded-full px-3 py-2"
                >
                  <Text className="text-[#252525] font-semibold text-sm">
                    SUBSCRIBE
                  </Text>
                </Pressable>
              </RNView>
            </BlurView>
          )}
      </View>

      {/* SHOW LIST */}
      <RNView className="px-4 mt-6 mb-10">
        {showByCategories.map((sbc, index) => (
          <>
            <ShowCarousel
              key={sbc.Category.Id - index}
              variant="normal"
              title={sbc.Category.Name}
              shows={sbc.ShowList}
            />
          </>
        ))}
      </RNView>

      {/* MORE INFORMATIONS */}
      <RNView className="px-4 mb-52">
        <Text className="text-white text-2xl font-bold leading-none mb-4">
          Description
        </Text>
        <HtmlText
          html={channelDetails.Channel.Description}
          fontSize={18}
          color="#fff"
        />
      </RNView>
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: "#000",
  },
  scrollContent: {
    minHeight: "100%",
  },
  channelHeaderContainer: {
    height: 500,
    padding: 15,
    position: "relative",
  },
  channelHeaderBackgroundImage: {
    position: "absolute",
    top: 0,
    left: 0,
    right: 0,
    bottom: 0,
    resizeMode: "cover",
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
  channelImage: {
    width: 60,
    height: 60,
    resizeMode: "cover",
    borderRadius: 8,
    elevation: 5,
  },
});
