import { Text } from "@/src/components/ui/Text";
import { View } from "@/src/components/ui/View";
import { Platform, Pressable, StyleSheet } from "react-native";

import { act, useState } from "react";
import { MaterialIcons } from "@expo/vector-icons";
import { useRouter } from "expo-router";
import { useDispatch } from "react-redux";
import { ShowDetails } from "@/src/core/types/show.type";
import AutoResolvingImageBackground from "@/src/components/autoResolveImage/AutoResolveImageBackground";
import AutoResolvingImage from "@/src/components/autoResolveImage/AutoResolvingImage";
import HtmlText from "@/src/components/renderHtml/HtmlText";
import {
  PodcastSubscriptionRegistration,
  SubscriptionDetails,
} from "@/src/core/types/subscription.type";

const ShowInformations = ({
  show,
  activeSubscription,
  isSubscribed,
  setIsSubscriptionInformationsModalVisible,
  onCancelSubscription,
  isFollowed,
  onFollowToggle,
}: {
  show: ShowDetails;
  activeSubscription: SubscriptionDetails | null;
  isSubscribed: boolean;
  setIsSubscriptionInformationsModalVisible: (visible: boolean) => void;
  onCancelSubscription: () => void;
  isFollowed: boolean;
  onFollowToggle: (isFollowed: boolean) => void;
}) => {
  // HOOKS
  const router = useRouter();

  // FUNCTIONS
  const formatRating = (ratingList: ShowDetails["ReviewList"]) => {
    const ratingCount = ratingList.length;
    const ratingAvg =
      ratingCount === 0
        ? 0
        : (
            ratingList.reduce((sum, r) => sum + r.Rating, 0) / ratingCount
          ).toFixed(1);
    return { ratingCount, ratingAvg };
  };

  const calculateSubscription = (activeSubscription: SubscriptionDetails) => {
    return (
      <View className="w-full px-[10px] flex items-center justify-center my-3">
        <Pressable
          style={style.priceTag}
          className="w-full flex items-center justify-center"
        >
          <Text className="font-medium italic text-[#AEE339]">
            Only from{" "}
            {activeSubscription.PodcastSubscriptionCycleTypePriceList[0].Price.toLocaleString(
              "vn"
            )}{" "}
            đ /{" "}
            {
              activeSubscription.PodcastSubscriptionCycleTypePriceList[0]
                .SubscriptionCycleType.Name
            }
          </Text>
        </Pressable>
      </View>
    );
  };

  const renderFooterInformations = (
    ratingList: ShowDetails["ReviewList"],
    categoryName: string,
    uploadFrequency: string,
    language: string
  ) => {
    let finalString = "";
    const { ratingCount, ratingAvg } = formatRating(ratingList);

    return (
      <View className="w-full flex items-start justify-between gap-2">
        <View className="flex flex-row items-center gap-2">
          <MaterialIcons name="star" color={"#fff"} size={20} />
          <Text className="font-bold">{ratingAvg}</Text>
          <Text>({ratingCount})</Text>
        </View>
        <Text numberOfLines={1}>
          {categoryName} • {uploadFrequency} • {language}
        </Text>
      </View>
    );
  };

  return (
    <View style={[style.containerWrapper]}>
      {/* Background Image with Overlay */}
      <AutoResolvingImageBackground
        FileKey={show.MainImageFileKey}
        key={show.Id}
        type="PodcastPublicSource"
        style={[StyleSheet.absoluteFill]}
        blurRadius={60} // High blur radius
      >
        {/* Dark overlay to dim the background image */}
        <View
          style={[
            StyleSheet.absoluteFill,
            { backgroundColor: "rgba(0, 0, 0, 0.6)" },
          ]}
        />
      </AutoResolvingImageBackground>
      {/* Main Content */}
      <View style={style.container}>
        <View style={style.navigationContainer}>
          {Platform.OS === "ios" ? (
            <Pressable onPress={() => router.back()} style={style.backIcon}>
              <MaterialIcons
                style={{ padding: 0, margin: 0 }}
                name="keyboard-arrow-left"
                color={"#fff"}
                size={25}
              />
            </Pressable>
          ) : (
            <Pressable onPress={() => router.back()} style={style.backIcon}>
              <MaterialIcons name="arrow-back" color={"#fff"} />
            </Pressable>
          )}

          {isFollowed ? (
            <Pressable
              onPress={() => onFollowToggle(false)}
              style={style.backIcon}
            >
              <MaterialIcons name="favorite" color={"#AEE339"} size={18} />
            </Pressable>
          ) : (
            <Pressable
              onPress={() => onFollowToggle(true)}
              style={style.backIcon}
            >
              <MaterialIcons name="favorite-border" color={"#fff"} size={18} />
            </Pressable>
          )}
        </View>

        <View style={style.showImageContainer}>
          <AutoResolvingImage
            key={show.Id}
            FileKey={show.MainImageFileKey}
            type="PodcastPublicSource"
            style={style.showImage}
          />
        </View>

        <View className="w-full items-center justify-center">
          <Text className="font-bold text-3xl text-white">{show.Name}</Text>
        </View>

        <View className="flex-row w-full items-center justify-center">
          {show.PodcastChannel ? (
            <Pressable className="w-full gap-3 h-[35px] flex flex-row items-center justify-center">
              {/* <Image
                source={{ uri: PodcastChannel.ImageUrl }}
                className="w-[20px] h-[20px] rounded-full"
              /> */}
              <AutoResolvingImage
                FileKey={show.PodcastChannel.MainImageFileKey}
                type="PodcastPublicSource"
                style={{ width: 20, height: 20, borderRadius: 9999 }}
              />
              <Text className="text-[#999999]">{show.PodcastChannel.Name}</Text>
              <MaterialIcons
                name="keyboard-arrow-right"
                color={"#999999"}
                size={20}
              />
            </Pressable>
          ) : (
            <Pressable className="w-full gap-3 h-[35px] flex flex-row items-center justify-center">
              {/* <Image
                source={{ uri: Podcaster.ImageUrl }}
                className="w-[20px] h-[20px] rounded-full"
              /> */}
              <AutoResolvingImage
                FileKey={show.Podcaster.MainImageFileKey}
                type="PodcastPublicSource"
                style={{ width: 20, height: 20, borderRadius: 9999 }}
              />
              <Text className="text-[#999999]">{show.Podcaster.FullName}</Text>
              <MaterialIcons
                name="keyboard-arrow-right"
                color={"#999999"}
                size={20}
              />
            </Pressable>
          )}
        </View>

        <View className="w-full items-center justify-center p-2">
          {isSubscribed ? (
            <Pressable style={style.whiteButton}>
              <MaterialIcons name="play-arrow" size={20} color={"#000"} />
              <Text className="text-black font-bold">Latest Episode</Text>
            </Pressable>
          ) : (
            <Pressable style={style.whiteButton}>
              <MaterialIcons name="play-arrow" size={20} color={"#000"} />
              <Text className="text-black font-bold">Trailer Audio</Text>
            </Pressable>
          )}
        </View>

        <View className="w-full px-[10px] text-justify py-3">
          {/* <Text numberOfLines={5} className="font-light">
            {Description}
          </Text> */}
          <HtmlText html={show.Description} numberOfLines={5} color="#D9D9D9" />
        </View>

        <View className="w-full px-[10px]">
          {renderFooterInformations(
            show.ReviewList,
            show.PodcastCategory.Name,
            show.UploadFrequency,
            show.Language
          )}
        </View>

        {!isSubscribed &&
          activeSubscription &&
          calculateSubscription(activeSubscription)}
        {activeSubscription &&
          (!isSubscribed ? (
            <View className="w-full px-[10px] flex items-center justify-center my-3">
              <Pressable
                onPress={() => setIsSubscriptionInformationsModalVisible(true)}
                style={style.coloredButton}
              >
                <Text className="font-extrabold text-black">Subscribe Now</Text>
              </Pressable>
            </View>
          ) : (
            <View className="w-full px-[10px] flex items-center justify-center my-3">
              <Pressable
                onPress={() => onCancelSubscription()}
                style={style.coloredButton}
              >
                <Text className="font-extrabold text-black">
                  Cancel Subscription
                </Text>
              </Pressable>
            </View>
          ))}
      </View>
    </View>
  );
};

export default ShowInformations;

const style = StyleSheet.create({
  containerWrapper: {
    width: "100%",
    overflow: "hidden",
  },
  container: {
    width: "100%",
    paddingTop: 60,
    paddingBottom: 20,
    paddingHorizontal: 20,
  },
  navigationContainer: {
    width: "100%",
    flexDirection: "row",
    justifyContent: "space-between",
    height: 30,
  },
  backIcon: {
    padding: 2,
    borderRadius: 9999,
    backgroundColor: "rgba(217, 217, 217, 0.2)",
    width: 30,
    height: 30,
    alignItems: "center",
    justifyContent: "center",
  },
  showImageContainer: {
    width: "100%",
    height: 220,
    alignItems: "center",
    justifyContent: "center",
    paddingVertical: 10,
    resizeMode: "cover",
  },
  showImage: {
    width: 200,
    height: 200,
    borderRadius: 8,
    resizeMode: "cover",
  },
  whiteButton: {
    backgroundColor: "#fff",
    paddingHorizontal: 80,
    paddingVertical: 10,
    borderRadius: 6,
    flexDirection: "row",
    alignItems: "center",
    justifyContent: "center",
    gap: 5,
  },
  coloredButton: {
    backgroundColor: "#AEE339",
    width: "100%",
    paddingVertical: 10,
    borderRadius: 6,
    alignItems: "center",
    justifyContent: "center",
  },
  priceTag: {
    borderColor: "#AEE339",
    borderWidth: 1,
    backgroundColor: "transparent",
    borderRadius: 999,
    width: "100%",
    paddingVertical: 6,
  },
});
