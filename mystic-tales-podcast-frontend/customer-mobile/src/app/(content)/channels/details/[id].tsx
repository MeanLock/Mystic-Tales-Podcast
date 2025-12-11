import AutoResolvingImage from "@/src/components/autoResolveImage/AutoResolvingImage";
import { Text } from "@/src/components/ui/Text";
import { View } from "@/src/components/ui/View";
import {
  useFavoriteChannelMutation,
  useGetActiveChannelSubscriptionQuery,
  useGetChannelDetailsQuery,
  useUnfavoriteChannelMutation,
} from "@/src/core/services/channel/channel.service";
import {
  useGetCustomerRegistrationInfoFromChannelQuery,
  useMakeDecisionOnAcceptingNewestVersionMutation,
  useSubscribePodcastSubscriptionMutation,
  useUnsubscribePodcastSubscriptionMutation,
} from "@/src/core/services/subscription/subscription.service";
import { ChannelDetails } from "@/src/core/types/channel.type";
import {
  SubscriptionCycleType,
  SubscriptionDetails,
} from "@/src/core/types/subscription.type";
import {
  Entypo,
  Feather,
  FontAwesome5,
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
  Modal,
} from "react-native";
import { SafeAreaInsetsContext } from "react-native-safe-area-context";
import ShowCarousel from "./components/ShowCarousel";
import MixxingText from "@/src/components/ui/MixxingText";
import RatingAndReview from "./components/RatingAndReview";
import HtmlText from "@/src/components/renderHtml/HtmlText";
import { useDispatch, useSelector } from "react-redux";
import { setDataAndShowAlert } from "@/src/features/alert/alertSlice";
import { RootState } from "@/src/store/store";
import {
  registerAlertAction,
  unregisterAlertAction,
} from "@/src/components/alert/GlobalAlert";

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
  const user = useSelector((state: RootState) => state.auth.user);
  // STATES
  const [isUserSubscribed, setIsUserSubscribed] = useState(false);
  const [isNewVersionAvailable, setIsNewVersionAvailable] = useState(false);
  const [isUserFavorited, setIsUserFavorited] = useState(false);
  const [isSubscriptionOptionsVisible, setIsSubscriptionOptionsVisible] =
    useState(false);
  const [versionCompareModalVisible, setVersionCompareModalVisible] =
    useState(false);
  const [selectedSubscriptionOption, setSelectedSubscriptionOption] =
    useState<number>(1);
  const [showByCategories, setShowByCategories] = useState<ShowByCategory[]>(
    []
  );
  const [selectedCycle, setSelectedCycle] = useState<SubscriptionCycleType>({
    Id: 1,
    Name: "Monthly",
  });

  // HOOKS
  const router = useRouter();
  const dispatch = useDispatch();
  const {
    data: channelDetails,
    isLoading: isChannelDetailsLoading,
    refetch: refetchChannelDetails,
  } = useGetChannelDetailsQuery(
    { ChannelId: id! },
    {
      skip: !id,
      refetchOnFocus: true,
      refetchOnMountOrArgChange: true,
      refetchOnReconnect: true,
    }
  );
  const {
    data: activeSubscription,
    isLoading: isActiveSubscriptionLoading,
    refetch: refetchActiveSubscription,
  } = useGetActiveChannelSubscriptionQuery(
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
    refetch: refetchUserRegistrationInfo,
  } = useGetCustomerRegistrationInfoFromChannelQuery(
    { PodcastChannelId: id! },
    {
      skip: !id,
      refetchOnFocus: true,
      refetchOnMountOrArgChange: true,
      refetchOnReconnect: true,
    }
  );
  const [unSubcribe] = useUnsubscribePodcastSubscriptionMutation();
  const [subcribe, { isLoading: isSubscribing }] =
    useSubscribePodcastSubscriptionMutation();
  const [favorite] = useFavoriteChannelMutation();
  const [unFavorite] = useUnfavoriteChannelMutation();
  const [acceptNewVersion, { isLoading: isAcceptingNewVersion }] =
    useMakeDecisionOnAcceptingNewestVersionMutation();

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
        }
        if (
          customerRegistrationInfo.PodcastSubscriptionRegistration
            .CurrentVersion !==
            activeSubscription?.PodcastSubscription.CurrentVersion &&
          customerRegistrationInfo.PodcastSubscriptionRegistration
            .IsAcceptNewestVersionSwitch === false
        ) {
          setIsNewVersionAvailable(true);
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
  const onChangeCycleView = (option: number) => {
    const cycle =
      activeSubscription?.PodcastSubscription.PodcastSubscriptionCycleTypePriceList.find(
        (cycle) => cycle.SubscriptionCycleType.Id === option
      );
    if (cycle) {
      setSelectedCycle(cycle.SubscriptionCycleType);
    }
  };

  const getSelectedCyclePrice = (cycleId: number) => {
    if (
      !activeSubscription ||
      !activeSubscription.PodcastSubscription
        .PodcastSubscriptionCycleTypePriceList
    ) {
      return 0;
    }
    const priceObj =
      activeSubscription.PodcastSubscription.PodcastSubscriptionCycleTypePriceList.find(
        (price) => price.SubscriptionCycleType.Id === cycleId
      );
    return priceObj ? priceObj.Price : 0;
  };

  const handleToggleFavorite = async (shouldFavorite: boolean) => {
    const restoreValue = isUserFavorited;
    // Gọi API để thêm hoặc bỏ yêu thích channel
    setIsUserFavorited(shouldFavorite);
    if (shouldFavorite) {
      try {
        await favorite({
          PodcastChannelId: channelDetails?.Channel.Id!,
        }).unwrap();
        await refetchChannelDetails();
        await refetchUserRegistrationInfo();
        await refetchActiveSubscription();
      } catch (error) {
        setIsUserFavorited(restoreValue);
      }
    } else {
      try {
        await unFavorite({
          PodcastChannelId: channelDetails?.Channel.Id!,
        }).unwrap();
        await refetchChannelDetails();
        await refetchUserRegistrationInfo();
        await refetchActiveSubscription();
      } catch (error) {
        setIsUserFavorited(restoreValue);
      }
    }
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

  const handleConfirmUnSubscribe = async () => {
    if (
      !customerRegistrationInfo ||
      !customerRegistrationInfo.PodcastSubscriptionRegistration
    ) {
      dispatch(
        setDataAndShowAlert({
          type: "error",
          description: "You have not subscribed to any subscription yet.",
          isCloseable: true,
          isFunctional: false,
          title: "Unsubscribe Failed",
          autoCloseDuration: 10,
        })
      );
      return;
    }
    // Gọi API để huỷ đăng ký
    try {
      await unSubcribe({
        PodcastSubscriptionRegistrationId:
          customerRegistrationInfo.PodcastSubscriptionRegistration.Id,
      }).unwrap();
      setIsUserSubscribed(false);

      // Register action handler
      const actionId = "unsubscribe-success";
      registerAlertAction(actionId, async () => {
        router.reload();
        unregisterAlertAction(actionId);
      });

      dispatch(
        setDataAndShowAlert({
          type: "success",
          description: "You have successfully unsubscribed.",
          isCloseable: true,
          isFunctional: true,
          title: "Unsubscribe Successful",
          autoCloseDuration: 2000,
          functionalButtonText: "Got it",
          actionId,
        })
      );
    } catch (error) {
      dispatch(
        setDataAndShowAlert({
          type: "error",
          description:
            "An error occurred while unsubscribing. Please try again later.",
          isCloseable: true,
          isFunctional: false,
          title: "Unsubscribe Failed",
          autoCloseDuration: 10,
        })
      );
    }
  };

  const handleUnSubscribe = () => {
    if (!isUserSubscribed) return;

    // Register action handler
    const actionId = "confirm-unsubscribe";
    registerAlertAction(actionId, () => {
      handleConfirmUnSubscribe();
      unregisterAlertAction(actionId);
    });

    dispatch(
      setDataAndShowAlert({
        type: "warning",
        title: "Confirm Unsubscribe",
        description:
          "Are you sure you want to unsubscribe from this subscription?",
        isCloseable: true,
        isFunctional: true,
        functionalButtonText: "Yes",
        actionId,
      })
    );
  };

  const handleSubscription = async () => {
    if (!activeSubscription || !activeSubscription.PodcastSubscription) {
      return;
    }
    if (isUserSubscribed) {
      dispatch(
        setDataAndShowAlert({
          type: "info",
          title: "Already Subscribed",
          description: "You are already subscribed to this channel.",
          isCloseable: true,
          isFunctional: false,
        })
      );
      return;
    }
    if (!user) {
      dispatch(
        setDataAndShowAlert({
          type: "warning",
          title: "Login Required",
          description: "Please log in to subscribe to a podcast channel.",
          isCloseable: true,
          isFunctional: false,
        })
      );
      return;
    }
    try {
      await subcribe({
        PodcastSubscriptionId: activeSubscription.PodcastSubscription.Id,
        CycleTypeId: selectedCycle.Id,
      }).unwrap();

      // Register action handler
      const actionId = "subscribe-success";
      registerAlertAction(actionId, async () => {
        router.reload();
        unregisterAlertAction(actionId);
      });

      dispatch(
        setDataAndShowAlert({
          type: "success",
          description: "You have successfully subscribed.",
          isCloseable: true,
          isFunctional: true,
          title: "Subscription Successful",
          autoCloseDuration: 2000,
          functionalButtonText: "Got it",
          actionId,
        })
      );
    } catch (error) {
      dispatch(
        setDataAndShowAlert({
          type: "error",
          description:
            "An error occurred while subscribing. Please try again later.",
          isCloseable: true,
          isFunctional: false,
          title: "Subscription Failed",
          autoCloseDuration: 10,
        })
      );
    }
  };

  const handleViewNewVersion = () => {
    setVersionCompareModalVisible(true);
  };

  const handleAcceptNewVersion = async () => {
    if (
      !customerRegistrationInfo ||
      !customerRegistrationInfo.PodcastSubscriptionRegistration
    ) {
      return;
    }
    try {
      await acceptNewVersion({
        PodcastSubscriptionRegistrationId:
          customerRegistrationInfo.PodcastSubscriptionRegistration.Id,
        IsAccepted: true,
      }).unwrap();

      // Success alert
      dispatch(
        setDataAndShowAlert({
          type: "success",
          description: "You have accepted the newest version.",
          isCloseable: true,
          isFunctional: false,
          title: "Accepted New Version",
          autoCloseDuration: 2000,
        })
      );
      setIsNewVersionAvailable(false);
      setVersionCompareModalVisible(false);
      refetchActiveSubscription();
      refetchUserRegistrationInfo();
    } catch (error) {
      dispatch(
        setDataAndShowAlert({
          type: "error",
          description:
            "An error occurred while accepting the newest version. Please try again later.",
          isCloseable: true,
          isFunctional: false,
          title: "Accept New Version Failed",
          autoCloseDuration: 10,
        })
      );
    }
  };

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
                  onPress={() => setIsSubscriptionOptionsVisible(true)}
                  className="bg-white rounded-full px-3 py-2"
                >
                  <Text className="text-[#252525] font-semibold text-sm">
                    SUBSCRIBE
                  </Text>
                </Pressable>
              </RNView>
            </BlurView>
          )}

        {isUserSubscribed &&
          customerRegistrationInfo &&
          !isNewVersionAvailable && (
            <RNView
              style={{ elevation: 10 }}
              className="absolute bottom-10 rounded-lg left-7 right-7 flex items-center justify-center bg-white gap-2 backdrop-blur-md p-1 z-40"
            >
              <Pressable
                onPress={() => handleUnSubscribe()}
                className="w-full h-full flex items-center justify-center p-2"
              >
                <Text className="text-black font-bold text-lg">
                  Cancel Subscription
                </Text>
              </Pressable>
            </RNView>
          )}

        {isUserSubscribed &&
          customerRegistrationInfo &&
          isNewVersionAvailable && (
            <RNView
              style={{ elevation: 10 }}
              className="absolute bottom-10 rounded-lg left-7 right-7 flex items-center justify-center bg-white gap-2 backdrop-blur-md p-1 z-40"
            >
              <Pressable
                onPress={() => handleViewNewVersion()}
                className="w-full h-full flex items-center justify-center p-2"
              >
                <Text className="text-black font-bold text-lg">
                  New Version Available
                </Text>
              </Pressable>
            </RNView>
          )}
      </View>

      {/* SHOW LIST */}
      <RNView className="px-4 mt-6 mb-10">
        {showByCategories.map((sbc, index) => (
          <ShowCarousel
            key={sbc.Category.Id - index}
            variant="normal"
            title={sbc.Category.Name}
            shows={sbc.ShowList}
          />
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

      {activeSubscription && activeSubscription.PodcastSubscription && (
        <Modal
          transparent={true}
          visible={isSubscriptionOptionsVisible}
          animationType="fade"
          onRequestClose={() => setIsSubscriptionOptionsVisible(false)}
        >
          {/* Overlay tối */}
          <Pressable
            style={styles.overlay}
            onPress={() => setIsSubscriptionOptionsVisible(false)} // bấm ngoài để đóng
          >
            {/* Chặn sự kiện bấm lan xuống overlay */}
            <Pressable style={styles.dialog} onPress={() => {}}>
              <Text style={styles.subscriptionTitle} numberOfLines={1}>
                {activeSubscription?.PodcastSubscription.Name}
              </Text>
              <Text style={styles.message} numberOfLines={3}>
                {activeSubscription?.PodcastSubscription.Description}
              </Text>
              <View className="w-full px-1 mb-2 flex items-center justify-center">
                <View className="w-full h-[1px] bg-[#D9D9D9]" />
              </View>
              <View className="w-full flex flex-col gap-6 py-2">
                <View className="flex flex-row items-center justify-start gap-3">
                  {activeSubscription?.PodcastSubscription.PodcastSubscriptionCycleTypePriceList.map(
                    (cycle) => (
                      <Pressable
                        key={cycle.SubscriptionCycleType.Id}
                        onPress={() =>
                          onChangeCycleView(cycle.SubscriptionCycleType.Id)
                        }
                        className={`${
                          selectedCycle.Id === cycle.SubscriptionCycleType.Id
                            ? "bg-[#AEE339] "
                            : "border border-[#AEE339] "
                        } px-3 py-1 rounded-full text-sm font-medium`}
                      >
                        <Text
                          className={`${
                            selectedCycle.Id === cycle.SubscriptionCycleType.Id
                              ? "text-black"
                              : "text-[#AEE339]"
                          }`}
                        >
                          {cycle.SubscriptionCycleType.Name}
                        </Text>
                      </Pressable>
                    )
                  )}
                </View>

                {/* Benefit List */}
                <View className="w-full flex flex-col items-start justify-between gap-2">
                  <Text className="text-lg font-semibold text-[#D9D9D9]">
                    Includes
                  </Text>
                  {activeSubscription.PodcastSubscription.PodcastSubscriptionBenefitMappingList.map(
                    (benefit) => (
                      <View
                        key={
                          benefit.PodcastSubscriptionId -
                          benefit.PodcastSubscriptionBenefit.Id
                        }
                        className="flex flex-row items-center justify-start gap-4"
                      >
                        <FontAwesome5
                          name="check-circle"
                          size={18}
                          color="#aee339"
                        />
                        <Text className="text-white">
                          {benefit.PodcastSubscriptionBenefit.Name}
                        </Text>
                      </View>
                    )
                  )}
                </View>

                <View className="flex flex-col items-start gap-1">
                  <Text className="text-[#D9D9D9] text-2xl font-semibold">
                    {getSelectedCyclePrice(selectedCycle.Id).toLocaleString()} đ
                  </Text>
                  <Text className="text-sm font-semibold text-[#9CA3AF]">
                    per user /{selectedCycle.Name}
                  </Text>
                </View>
              </View>
              <View style={styles.buttonsRow}>
                <Pressable
                  style={[styles.button, styles.subscriptionButton]}
                  className="flex items-center justify-center"
                  onPress={() => handleSubscription()}
                >
                  <Text style={styles.subscriptionText}>
                    {isSubscribing ? "Processing..." : "Subscribe Now"}
                  </Text>
                </Pressable>
              </View>
            </Pressable>
          </Pressable>
        </Modal>
      )}

      {activeSubscription &&
        activeSubscription.PodcastSubscription &&
        customerRegistrationInfo &&
        customerRegistrationInfo.PodcastSubscriptionRegistration && (
          <Modal
            transparent={true}
            visible={versionCompareModalVisible}
            animationType="fade"
            onRequestClose={() => setVersionCompareModalVisible(false)}
          >
            {/* Overlay tối */}
            <Pressable
              style={styles.overlay}
              onPress={() => setVersionCompareModalVisible(false)} // bấm ngoài để đóng
            >
              {/* Chặn sự kiện bấm lan xuống overlay */}
              <Pressable style={styles.dialogCompare} onPress={() => {}}>
                <Text style={styles.subscriptionTitle} numberOfLines={1}>
                  New Subscription Version Available
                </Text>
                <Text className="text-start">
                  If you don't update, the subscription will be expired at the
                  end of next moth.
                </Text>

                {/* Compare */}
                <View className="mt-5 flex flex-row items-start justify-between px-2">
                  <View className="flex flex-col items-start gap-1">
                    <Text className="text-sm font-bold text-white">
                      Your Subscription
                    </Text>
                    <Text className="text-lg font-bold text-white">
                      {customerRegistrationInfo.PodcastSubscriptionRegistration
                        .Price
                        ? customerRegistrationInfo.PodcastSubscriptionRegistration.Price.toLocaleString()
                        : "100,000"}{" "}
                      đ
                    </Text>
                    <View className="gap-2 mt-2">
                      {customerRegistrationInfo.PodcastSubscriptionRegistration.PodcastSubscriptionBenefitList.map(
                        (benefit) => (
                          <View>
                            <Text className="text-white text-xs">
                              {benefit.Name}
                            </Text>
                          </View>
                        )
                      )}
                    </View>
                  </View>
                  <View className="flex flex-col items-start gap-1">
                    <Text className="text-sm font-bold text-[#aee339]">
                      New Subscription
                    </Text>
                    <Text className="text-lg font-bold text-[#aee339]">
                      {activeSubscription.PodcastSubscription.PodcastSubscriptionCycleTypePriceList.find(
                        (price) =>
                          price.SubscriptionCycleType.Id ===
                          customerRegistrationInfo
                            .PodcastSubscriptionRegistration
                            ?.SubscriptionCycleType.Id
                      )?.Price.toLocaleString()}
                      đ
                    </Text>
                    <View className="gap-2 mt-2">
                      {activeSubscription.PodcastSubscription.PodcastSubscriptionBenefitMappingList.map(
                        (benefit) => (
                          <View>
                            <Text className="text-white text-xs">
                              {benefit.PodcastSubscriptionBenefit.Name}
                            </Text>
                          </View>
                        )
                      )}
                    </View>
                  </View>
                </View>

                {/* Actions */}
                <View className="w-full gap-3 py-4 flex flex-row items-center justify-between mt-5">
                  <Pressable
                    className="w-1/3 py-2 flex items-center justify-center border-2 rounded-md border-zinc-600"
                    onPress={() => setVersionCompareModalVisible(false)}
                  >
                    <Text className="text-zinc-400">Cancel</Text>
                  </Pressable>
                  <Pressable
                    className="flex-1 py-2 flex items-center justify-center border-2 rounded-md border-[#aee339]"
                    onPress={() => handleAcceptNewVersion()}
                  >
                    <Text className="text-[#aee339]">
                      {isAcceptingNewVersion ? "Accepting..." : "Accept"}
                    </Text>
                  </Pressable>
                </View>
              </Pressable>
            </Pressable>
          </Modal>
        )}
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
  overlay: {
    flex: 1,
    backgroundColor: "rgba(0,0,0,0.45)",
    justifyContent: "center",
    alignItems: "center",
  },
  dialog: {
    width: "80%",
    padding: 16,
    borderRadius: 20,
    backgroundColor: "#141414",
  },
  dialogCompare: {
    width: "90%",
    padding: 16,
    borderRadius: 20,
    backgroundColor: "#141414",
  },
  subscriptionTitle: {
    fontSize: 16,
    fontWeight: "600",
    color: "#aee339",
    marginBottom: 6,
  },
  subscriptionContentContainer: {
    width: "100%",
    marginLeft: 2,
    marginRight: 2,
    backgroundColor: "#1f2937",
  },
  message: {
    fontSize: 12,
    color: "#e5e7eb",
    textAlign: "left",
    marginBottom: 16,
  },
  buttonsRow: {
    flexDirection: "row",
    justifyContent: "flex-end",
    gap: 10,
    marginTop: 10,
  },
  button: {
    width: "100%",
    paddingVertical: 8,
    paddingHorizontal: 14,
    borderRadius: 20,
  },
  normalCycleButton: {
    paddingHorizontal: 12,
    paddingVertical: 6,
    borderRadius: 10,
    borderWidth: 1,
    borderColor: "#aee339",
  },
  normalCycleText: {
    color: "#aee339",
  },
  selectedCyclePriceText: {
    fontSize: 40,
    lineHeight: 40, // quan trọng: cho lineHeight = fontSize
    fontWeight: "600",
    color: "#aee339",
  },
  activeCycleButton: {
    paddingHorizontal: 12,
    paddingVertical: 6,
    borderRadius: 10,
    borderWidth: 1,
    borderColor: "#aee339",
    backgroundColor: "#aee339",
  },
  activeCycleText: {
    color: "black",
  },
  cancelButton: {
    backgroundColor: "#374151",
  },
  confirmButton: {
    backgroundColor: "#ef4444",
  },
  cancelText: {
    color: "#e5e7eb",
    fontSize: 14,
  },
  confirmText: {
    color: "#fff",
    fontWeight: "600",
    fontSize: 14,
  },
  subscriptionButton: {
    backgroundColor: "#aee339",
  },
  subscriptionText: {
    color: "black",
    fontWeight: "600",
    fontSize: 14,
  },
});
