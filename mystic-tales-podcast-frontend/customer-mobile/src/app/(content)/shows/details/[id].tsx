import { Text } from "@/src/components/ui/Text";
import { View } from "@/src/components/ui/View";
import { Feather, FontAwesome5, MaterialIcons } from "@expo/vector-icons";
import { useFocusEffect, useLocalSearchParams, useRouter } from "expo-router";
import { useCallback, useEffect, useState } from "react";
import {
  ActivityIndicator,
  Alert,
  Modal,
  Platform,
  Pressable,
  ScrollView,
  StyleSheet,
} from "react-native";
import ShowInformations from "./components/ShowInformations";
import EpisodeList from "./components/EpisodeList";
import RatingAndReview from "./components/RatingAndReview";
import ShowDescription from "./components/ShowDescription";
import MoreInformations from "./components/MoreInformations";
import { ShowDetails } from "@/src/core/types/show.type";
import {
  useFollowShowMutation,
  useGetActiveShowSubscriptionQuery,
  useGetShowDetailsQuery,
  useUnFollowShowMutation,
} from "@/src/core/services/show/show.service";
import {
  useGetCustomerRegistrationInfoFromShowQuery,
  useMakeDecisionOnAcceptingNewestVersionMutation,
  useSubscribePodcastSubscriptionMutation,
  useUnsubscribePodcastSubscriptionMutation,
} from "@/src/core/services/subscription/subscription.service";
import { useDispatch, useSelector } from "react-redux";
import { RootState } from "@/src/store/store";
import { setDataAndShowAlert } from "@/src/features/alert/alertSlice";

export default function ShowDetailsScreen() {
  const { id } = useLocalSearchParams<{ id: string }>();
  const router = useRouter();

  // REDUX
  const user = useSelector((state: RootState) => state.auth.user);

  // STATES
  const [isSubscribed, setIsSubscribed] = useState<boolean>(false);
  const [isCommented, setIsCommented] = useState<boolean>(false);
  const [isFollowed, setIsFollowed] = useState<boolean>(false);
  const [isNewVersionAvailable, setIsNewVersionAvailable] =
    useState<boolean>(false);
  const [versionCompareModalVisible, setVersionCompareModalVisible] =
    useState<boolean>(false);
  const [
    isSubscriptionInformationsModalVisible,
    setIsSubscriptionInformationsModalVisible,
  ] = useState<boolean>(false);
  const [viewCycleName, setViewCycleName] = useState<string>("Monthly");

  const [selectedCycle, setSelectedCycle] = useState<any>(null);
  // HOOKS
  const dispatch = useDispatch();
  const {
    data: showData,
    isLoading: showLoading,
    refetch: refetchShowData,
  } = useGetShowDetailsQuery(
    { PodcastShowId: id! },
    {
      skip: !id,
      refetchOnMountOrArgChange: true,
      refetchOnFocus: true,
      refetchOnReconnect: true,
    }
  );
  const {
    data: activeSubscription,
    isLoading: activeSubscriptionLoading,
    refetch: refetchActiveSubscription,
  } = useGetActiveShowSubscriptionQuery(
    { ShowId: id! },
    {
      skip: !id,
      refetchOnMountOrArgChange: true,
      refetchOnFocus: true,
      refetchOnReconnect: true,
    }
  );

  const {
    data: customerRegistrationInfo,
    isLoading: customerRegistrationInfoLoading,
    refetch: refetchUserRegistrationInfo,
  } = useGetCustomerRegistrationInfoFromShowQuery(
    { PodcastShowId: id! },
    {
      skip: !id,
      refetchOnMountOrArgChange: true,
      refetchOnFocus: true,
      refetchOnReconnect: true,
    }
  );

  const [subscribeShow, { isLoading: subscribeLoading }] =
    useSubscribePodcastSubscriptionMutation();

  const [unSubscribeShow, { isLoading: unSubscribeLoading }] =
    useUnsubscribePodcastSubscriptionMutation();

  const [followShow, { isLoading: followShowLoading }] =
    useFollowShowMutation();
  const [unfollowShow, { isLoading: unfollowShowLoading }] =
    useUnFollowShowMutation();
  const [acceptNewVersion, { isLoading: isAcceptingNewVersion }] =
    useMakeDecisionOnAcceptingNewestVersionMutation();

  useEffect(() => {
    if (
      !showData ||
      showLoading ||
      activeSubscriptionLoading ||
      customerRegistrationInfoLoading
    ) {
      return;
    }
    if (
      !customerRegistrationInfo ||
      !customerRegistrationInfo.PodcastSubscriptionRegistration
    ) {
      setIsSubscribed(false);
    } else {
      if (!activeSubscription) {
        setIsSubscribed(false);
      } else if (
        customerRegistrationInfo.PodcastSubscriptionRegistration
          .PodcastSubscriptionId === activeSubscription.PodcastSubscription.Id
      ) {
        setIsSubscribed(true);
        if (
          customerRegistrationInfo.PodcastSubscriptionRegistration
            .CurrentVersion !==
            activeSubscription.PodcastSubscription.CurrentVersion &&
          customerRegistrationInfo.PodcastSubscriptionRegistration
            .IsAcceptNewestVersionSwitch === false
        ) {
          // User chưa chấp nhận phiên bản mới
          setIsNewVersionAvailable(true);
        }
      } else {
        setIsSubscribed(false);
      }
    }
    // Set default selected cycle
    if (
      activeSubscription &&
      activeSubscription.PodcastSubscription &&
      activeSubscription.PodcastSubscription
        .PodcastSubscriptionCycleTypePriceList.length > 0
    ) {
      setViewCycleName(
        activeSubscription.PodcastSubscription
          .PodcastSubscriptionCycleTypePriceList[0].SubscriptionCycleType.Name
      );
      setSelectedCycle(
        activeSubscription.PodcastSubscription
          .PodcastSubscriptionCycleTypePriceList[0]
      );
    }

    // Check if user has commented
    if (showData && showData.Show && showData.Show.ReviewList && user) {
      const commented = showData.Show.ReviewList.some(
        (review) => review.Account.Id === user.Id
      );
      setIsCommented(commented);
    }

    // Check if user has followed
    if (showData && showData.Show && user) {
      setIsFollowed(showData.Show.IsFollowedByCurrentUser);
    }
  }, [
    showData,
    showLoading,
    activeSubscription,
    customerRegistrationInfo,
    activeSubscriptionLoading,
    customerRegistrationInfoLoading,
  ]);

  // FUNCTIONS
  const handleSubscription = async () => {
    if (!selectedCycle) {
      Alert.alert("Please select a subscription cycle");
      return;
    }
    if (!user) {
      Alert.alert("You need to login to subscribe.");
      return;
    }
    try {
      const result = await subscribeShow({
        PodcastSubscriptionId: selectedCycle.PodcastSubscriptionId,
        CycleTypeId: selectedCycle.SubscriptionCycleType.Id,
      }).unwrap();

      if (result) {
        Alert.alert("Subscription successful!");
        setIsSubscriptionInformationsModalVisible(false);
        refetchShowData();
        refetchActiveSubscription();
        refetchUserRegistrationInfo();
      }
    } catch (error) {
      Alert.alert("Subscription failed. Please try again later.");
    }
  };

  const handleCancelSubscription = async () => {
    Alert.alert(
      "Cancel Subscription",
      "Are you sure you want to cancel your subscription?",
      [
        {
          text: "Cancel",
          style: "cancel", // iOS: nút cancel
        },
        {
          text: "Confirm",
          style: "destructive", // iOS: màu đỏ
          onPress: async () => {
            try {
              await onConfirmCancelSubscription();
            } catch (e) {
              console.log("Cancel subscription error", e);
            }
          },
        },
      ],
      {
        cancelable: true, // bấm ra ngoài để đóng (Android)
      }
    );
  };

  const onConfirmCancelSubscription = async () => {
    if (
      !customerRegistrationInfo ||
      !customerRegistrationInfo.PodcastSubscriptionRegistration
    ) {
      Alert.alert("You do not have an active subscription to cancel.");
      return;
    } else {
      try {
        await unSubscribeShow({
          PodcastSubscriptionRegistrationId:
            customerRegistrationInfo?.PodcastSubscriptionRegistration?.Id,
        }).unwrap();
        Alert.alert("Subscription cancelled successfully.");
        refetchShowData();
        refetchActiveSubscription();
        refetchUserRegistrationInfo();
      } catch (error) {
        Alert.alert("Failed to cancel subscription. Please try again later.");
      }
    }
  };

  const onChangeCycleView = (cycle: any) => {
    setViewCycleName(cycle.SubscriptionCycleType.Name);
    setSelectedCycle(cycle);
  };

  const handelFollowToggle = (isFollow: boolean) => {
    const snapShotValue = isFollowed;
    setIsFollowed(isFollow);
    if (isFollow) {
      followShow({ PodcastShowId: id! })
        .unwrap()
        .then(() => {
          refetchShowData();
          refetchActiveSubscription();
          refetchUserRegistrationInfo();
        })
        .catch(() => {
          Alert.alert("Failed to follow the show. Please try again later.");
          setIsFollowed(snapShotValue);
        });
    } else {
      unfollowShow({ PodcastShowId: id! })
        .unwrap()
        .then(() => {
          refetchActiveSubscription();
          refetchShowData();
          refetchUserRegistrationInfo();
        })
        .catch(() => {
          Alert.alert("Failed to unfollow the show. Please try again later.");
          setIsFollowed(snapShotValue);
        });
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

  if (showLoading) {
    return (
      <View className="w-full bg-black h-screen justify-center items-center gap-3">
        <ActivityIndicator />
        <Text className="text-[#D9D9D9] font-bold text-lg">
          Loading Show ...
        </Text>
      </View>
    );
  }
  // ...render theo id
  if (!showData || !showData.Show) {
    return (
      <View className="w-full bg-black h-full flex items-center justify-center">
        <Text className="text-[#D9D9D9] font-bold text-lg">
          Cannot Find Show :(
        </Text>
        <Pressable
          className="flex flex-row items-center gap-2 mt-4 bg-[#AEE339] px-4 py-2 rounded-full"
          onPress={() => router.back()}
        >
          <Text className="text-white">Go Back</Text>
        </Pressable>
      </View>
    );
  } else {
    return (
      <ScrollView style={styles.container}>
        <ShowInformations
          show={showData.Show}
          activeSubscription={activeSubscription?.PodcastSubscription || null}
          isSubscribed={isSubscribed}
          setIsSubscriptionInformationsModalVisible={
            setIsSubscriptionInformationsModalVisible
          }
          onCancelSubscription={handleCancelSubscription}
          isFollowed={isFollowed}
          onFollowToggle={handelFollowToggle}
          isNewVersionAvailable={isNewVersionAvailable}
          customerRegistrationInfo={customerRegistrationInfo}
          handleViewNewVersion={handleViewNewVersion}
        />
        <View className="p-[30px] gap-10">
          <EpisodeList episodes={showData.Show?.EpisodeList} />
          <RatingAndReview
            isCommented={isCommented}
            ratings={showData.Show.ReviewList}
          />
          <ShowDescription description={showData.Show.Description} />
          <MoreInformations
            Copyright={showData.Show.Copyright}
            Podcaster={showData.Show.Podcaster}
            PodcastCategory={showData.Show.PodcastCategory}
            PodcastSubCategory={showData.Show.PodcastSubCategory}
            PodcastChannel={showData.Show.PodcastChannel}
            ReleaseDate={showData.Show.ReleaseDate}
            ShowEpisodeList={showData.Show.EpisodeList}
          />
        </View>

        {/* Subscription Informations Modal */}
        {activeSubscription &&
          activeSubscription.PodcastSubscription &&
          selectedCycle &&
          !isNewVersionAvailable && (
            <Modal
              transparent={true}
              visible={isSubscriptionInformationsModalVisible}
              animationType="fade"
              onRequestClose={() =>
                setIsSubscriptionInformationsModalVisible(false)
              }
            >
              {/* Overlay tối */}
              <Pressable
                style={styles.overlay}
                onPress={() => setIsSubscriptionInformationsModalVisible(false)} // bấm ngoài để đóng
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
                            onPress={() => onChangeCycleView(cycle)}
                            className={`${
                              viewCycleName === cycle.SubscriptionCycleType.Name
                                ? "bg-[#AEE339] "
                                : "border border-[#AEE339] "
                            } px-3 py-1 rounded-full text-sm font-medium`}
                          >
                            <Text
                              className={`${
                                viewCycleName ===
                                cycle.SubscriptionCycleType.Name
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
                        {selectedCycle.Price.toLocaleString()} đ
                      </Text>
                      <Text className="text-sm font-semibold text-[#9CA3AF]">
                        per user /{selectedCycle.SubscriptionCycleType.Name}
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
                        {subscribeLoading ? "Processing..." : "Subscribe Now"}
                      </Text>
                    </Pressable>

                    {/* <Pressable
                    style={[styles.button, styles.confirmButton]}
                    onPress={() => {
                      // TODO: gọi API, xử lý gì đó...
                      handleSubscription();
                    }}
                  >
                    <Text style={styles.confirmText}>Delete</Text>
                  </Pressable> */}
                  </View>
                </Pressable>
              </Pressable>
            </Modal>
          )}

        {activeSubscription &&
          activeSubscription.PodcastSubscription &&
          customerRegistrationInfo &&
          customerRegistrationInfo.PodcastSubscriptionRegistration &&
          isNewVersionAvailable && (
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
                        {customerRegistrationInfo
                          .PodcastSubscriptionRegistration.Price
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
        <View className="h-36" />
      </ScrollView>
    );
  }
}

const styles = StyleSheet.create({
  container: {
    backgroundColor: "black",
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
