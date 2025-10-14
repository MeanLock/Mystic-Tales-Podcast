import { Text } from "@/src/components/ui/Text";
import { View } from "@/src/components/ui/View";
import {
  Image,
  ImageBackground,
  Platform,
  Pressable,
  StyleSheet,
} from "react-native";
import { ShowDetails } from "../[id]";
import { useEffect, useState } from "react";
import { MaterialIcons } from "@expo/vector-icons";
import { useRouter } from "expo-router";
import { useDispatch, useSelector } from "react-redux";
import { RootState } from "@/src/store/store";

import { BlurView } from "expo-blur";

type ShowInfoProps = Pick<
  ShowDetails,
  | "Id"
  | "Name"
  | "Description"
  | "ReleaseDate"
  | "TotalFollow"
  | "ListenCount"
  | "RatingList"
  | "Language"
  | "UploadFrequency"
  | "Podcaster"
  | "PodcastChannel"
  | "ImageUrl"
  | "TrailerAudioFileKey"
  | "PodcastCategory"
  | "PodcastSubCategory"
  | "PodcastShowsSubscriptionType"
  | "ShowSubscriptionList"
>;

type RatingType = {
  Id: string;
  Title: string;
  Content: string;
  Rating: number;
  Account: {
    Id: number;
    FullName: string;
    Email: string;
    ImageUrl: string;
  };
  IsDeleted: boolean;
  CreatedAt: string;
};

type ShowSubscription = {
  Id: number;
  Name: string;
  Description: string;
  PodcastChannelId: string;
  PodcastShowId: string;
  IsActive: boolean;
  CurrentVersion: number;
  DeletedAt: string | null;
  CreatedAt: string;
  UpdatedAt: string;
  PodcastSubscriptionCycleTypePriceList: {
    PodcastSubscriptionId: number;
    SubscriptionCycleType: {
      Id: number;
      Name: string;
    };
    Version: number;
    Price: number;
    CreatedAt: string;
    UpdatedAt: string;
  }[];
  PodcastSubscriptionBenefitMappingList: {
    PodcastSubscriptionId: number;
    PodcastSubscriptionBenefit: {
      Id: number;
      Name: string;
    };
    Version: number;
    CreatedAt: string;
    UpdatedAt: string;
  }[];
};

const ShowInformations = ({
  Id,
  Name,
  Description,
  ReleaseDate,
  TotalFollow,
  ListenCount,
  Language,
  RatingList,
  UploadFrequency,
  Podcaster,
  PodcastChannel,
  ImageUrl,
  TrailerAudioFileKey,
  PodcastCategory,
  PodcastSubCategory,
  PodcastShowsSubscriptionType,
  ShowSubscriptionList,
}: ShowInfoProps) => {
  // STATES
  const [isSubscribed, setIsSubscribed] = useState(false);

  // HOOKS
  const router = useRouter();
  const dispatch = useDispatch();

  // FUNCTIONS
  const formatRating = (ratingList: RatingType[]) => {
    const ratingCount = ratingList.length;
    const validRatings = ratingList.filter((r) => r.IsDeleted !== true);
    const ratingAvg =
      ratingCount === 0
        ? 0
        : (
            ratingList.reduce((sum, r) => sum + r.Rating, 0) / ratingCount
          ).toFixed(1);
    return { ratingCount, ratingAvg };
  };

  const calculateSubscription = (subscriptionList: ShowSubscription[]) => {
    const activeSubscription = subscriptionList.filter(
      (s) => s.IsActive === true
    )[0];
    const currentPrice =
      activeSubscription.PodcastSubscriptionCycleTypePriceList.filter(
        (p) => p.Version === activeSubscription.CurrentVersion
      )[0].Price;

    if (activeSubscription) {
      return (
        <View className="w-full px-[10px] flex items-center justify-center my-3">
          <Pressable style={style.coloredButton}>
            <Text className="font-extrabold text-black">
              {currentPrice.toLocaleString("vn")} đ / months
            </Text>
          </Pressable>
        </View>
      );
    }
  };

  const renderFooterInformations = (
    ratingList: RatingType[],
    categoryName: string,
    uploadFrequency: string,
    language: string,
    showSubscription: ShowSubscription[]
  ) => {
    let finalString = "";
    const { ratingCount, ratingAvg } = formatRating(ratingList);

    return (
      <View className="w-full flex flex-row items-center justify-between">
        <View className="flex flex-row items-center gap-1">
          <MaterialIcons name="star" color={"#fff"} size={20} />
          <Text className="font-bold">{ratingAvg}</Text>
          <Text>({ratingCount})</Text>
          <Text>
            {" "}
            • {categoryName} • {uploadFrequency} • {language}
          </Text>
        </View>
      </View>
    );
  };

  return (
    <View style={[style.containerWrapper]}>
      {/* Background Image with Overlay */}
      <ImageBackground
        source={{ uri: ImageUrl }}
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
      </ImageBackground>

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
        </View>

        <View style={style.showImageContainer}>
          <Image style={style.showImage} source={{ uri: ImageUrl }} />
        </View>

        <View className="w-full items-center justify-center">
          <Text className="font-bold text-3xl text-white">{Name}</Text>
        </View>

        <View className="flex-row w-full items-center justify-center">
          {PodcastChannel ? (
            <Pressable className="w-full gap-3 h-[35px] flex flex-row items-center justify-center">
              <Image
                source={{ uri: PodcastChannel.ImageUrl }}
                className="w-[20px] h-[20px] rounded-full"
              />
              <Text className="text-[#999999]">{PodcastChannel.Name}</Text>
              <MaterialIcons
                name="keyboard-arrow-right"
                color={"#999999"}
                size={20}
              />
            </Pressable>
          ) : (
            <Pressable className="w-full gap-3 h-[35px] flex flex-row items-center justify-center">
              <Image
                source={{ uri: Podcaster.ImageUrl }}
                className="w-[20px] h-[20px] rounded-full"
              />
              <Text className="text-[#999999]">{Podcaster.FullName}</Text>
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
              <Text className="text-black font-bold">Continue Listening</Text>
            </Pressable>
          ) : (
            <Pressable style={style.whiteButton}>
              <MaterialIcons name="play-arrow" size={20} color={"#000"} />
              <Text className="text-black font-bold">Trailer Audio</Text>
            </Pressable>
          )}
        </View>

        <View className="w-full px-[10px] text-justify py-3">
          <Text numberOfLines={5} className="font-light">
            {Description}
          </Text>
        </View>

        <View className="w-full px-[10px]">
          {renderFooterInformations(
            RatingList,
            PodcastCategory.Name,
            UploadFrequency,
            Language,
            ShowSubscriptionList
          )}
        </View>

        {!isSubscribed && calculateSubscription(ShowSubscriptionList)}
      </View>
    </View>
  );
};

export default ShowInformations;

const style = StyleSheet.create({
  containerWrapper: {
    width: "100%",
    overflow: "hidden",
    borderBottomColor: "#514F4F",
    borderBottomWidth: 0.3,
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
});
