import { Text } from "@/src/components/ui/Text";
import { View } from "@/src/components/ui/View";
import { Platform, Pressable, StyleSheet, View as RNView } from "react-native";

import { useState, useEffect } from "react";
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
import { useLazyGetPodcastPublicSourceQuery } from "@/src/core/services/file/file.service";
import { Audio } from "expo-av";

const ShowInformations = ({
  show,
  activeSubscription,
  isSubscribed,
  setIsSubscriptionInformationsModalVisible,
  onCancelSubscription,
  isFollowed,
  onFollowToggle,
  isNewVersionAvailable,
  customerRegistrationInfo,
  handleViewNewVersion,
}: {
  show: ShowDetails;
  activeSubscription: SubscriptionDetails | null;
  isSubscribed: boolean;
  setIsSubscriptionInformationsModalVisible: (visible: boolean) => void;
  onCancelSubscription: () => void;
  isFollowed: boolean;
  onFollowToggle: (isFollowed: boolean) => void;
  isNewVersionAvailable: boolean;
  customerRegistrationInfo: any;
  handleViewNewVersion: () => void;
}) => {
  // HOOKS
  const router = useRouter();

  const [triggerGetTrailerAudioFileUrl] = useLazyGetPodcastPublicSourceQuery();

  // Local trailer player state
  const [trailerSound, setTrailerSound] = useState<Audio.Sound | null>(null);
  const [isTrailerPlaying, setIsTrailerPlaying] = useState(false);
  const [isTrailerLoading, setIsTrailerLoading] = useState(false);
  const [trailerPosition, setTrailerPosition] = useState(0);
  const [trailerDuration, setTrailerDuration] = useState(0);

  // Cleanup sound on unmount
  useEffect(() => {
    return () => {
      if (trailerSound) {
        trailerSound.unloadAsync();
      }
    };
  }, [trailerSound]);

  // FUNCTIONS
  const formatTime = (seconds: number) => {
    const mins = Math.floor(seconds / 60);
    const secs = Math.floor(seconds % 60);
    return `${mins}:${secs.toString().padStart(2, "0")}`;
  };

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
    const { ratingCount, ratingAvg } = formatRating(ratingList);

    return (
      <View className="w-full flex items-start justify-between gap-2">
        <View className="flex flex-row items-center gap-2">
          <MaterialIcons name="star" color={"#fff"} size={20} />
          <Text className="font-bold text-white">{ratingAvg}</Text>
          <Text className="text-white">({ratingCount})</Text>
        </View>
        <Text className="text-white" numberOfLines={1}>
          {categoryName} • {uploadFrequency} • {language}
        </Text>
      </View>
    );
  };

  const handleListenToTrailerAudio = async () => {
    try {
      if (!show.TrailerAudioFileKey) return;

      // If already playing, stop it
      if (isTrailerPlaying && trailerSound) {
        await trailerSound.stopAsync();
        await trailerSound.unloadAsync();
        setTrailerSound(null);
        setIsTrailerPlaying(false);
        setTrailerPosition(0);
        setTrailerDuration(0);
        return;
      }

      // If sound exists but paused, resume
      if (trailerSound && !isTrailerPlaying) {
        await trailerSound.playAsync();
        setIsTrailerPlaying(true);
        return;
      }

      // Load and play new sound
      setIsTrailerLoading(true);
      const result = await triggerGetTrailerAudioFileUrl({
        FileKey: show.TrailerAudioFileKey,
      }).unwrap();
      const trailerAudioUrl = result.FileUrl;

      // Configure audio mode
      await Audio.setAudioModeAsync({
        playsInSilentModeIOS: true,
        staysActiveInBackground: false,
        shouldDuckAndroid: true,
      });

      // Create and load sound
      const { sound } = await Audio.Sound.createAsync(
        { uri: trailerAudioUrl },
        { shouldPlay: true }
      );

      // Set up playback status listener
      sound.setOnPlaybackStatusUpdate((status) => {
        if (status.isLoaded) {
          setIsTrailerPlaying(status.isPlaying);
          setTrailerPosition(status.positionMillis / 1000); // Convert to seconds
          setTrailerDuration(
            status.durationMillis ? status.durationMillis / 1000 : 0
          );

          // Auto cleanup when finished
          if (status.didJustFinish) {
            sound.unloadAsync();
            setTrailerSound(null);
            setIsTrailerPlaying(false);
            setTrailerPosition(0);
            setTrailerDuration(0);
          }
        }
      });
      setTrailerSound(sound);
      setIsTrailerPlaying(true);
      setIsTrailerLoading(false);
    } catch (error) {
      console.log("Error fetching/playing trailer audio:", error);
      setIsTrailerLoading(false);
      setIsTrailerPlaying(false);
    }
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
          <View className="w-full items-center justify-center p-2">
            <AutoResolvingImage
              FileKey={show.MainImageFileKey}
              type="PodcastPublicSource"
              style={{ width: 200, height: 200, borderRadius: 8 }}
            />
          </View>
        </View>

        <View className="w-full items-center justify-center">
          <Text className="font-bold text-3xl text-white">{show.Name}</Text>
        </View>

        <View className="flex-row w-full items-center justify-center">
          {show.PodcastChannel ? (
            <Pressable
              onPress={() =>
                router.push(
                  `/(content)/channels/details/${show.PodcastChannel.Id}`
                )
              }
              className="w-full gap-3 h-[35px] flex flex-row items-center justify-center"
            >
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
          {show.TrailerAudioFileKey && (
            <>
              <Pressable
                style={style.whiteButton}
                onPress={handleListenToTrailerAudio}
                disabled={isTrailerLoading}
              >
                {isTrailerLoading ? (
                  <MaterialIcons
                    name="hourglass-empty"
                    size={20}
                    color={"#000"}
                  />
                ) : isTrailerPlaying ? (
                  <MaterialIcons name="stop" size={20} color={"#000"} />
                ) : (
                  <MaterialIcons name="play-arrow" size={20} color={"#000"} />
                )}
                <Text className="text-black font-bold">
                  {isTrailerLoading
                    ? "Loading..."
                    : isTrailerPlaying
                    ? "Stop Trailer"
                    : "Trailer Audio"}
                </Text>
              </Pressable>

              {/* Progress Bar - Apple Music Style */}
              {(isTrailerPlaying || trailerDuration > 0) && (
                <View className="w-full mt-3 px-2">
                  <View className="flex-row items-center justify-between mb-1">
                    <Text className="text-white/70 text-xs">
                      {formatTime(trailerPosition)}
                    </Text>
                    <Text className="text-white/70 text-xs">
                      {formatTime(trailerDuration)}
                    </Text>
                  </View>
                  <View className="w-full h-1 bg-white/20 rounded-full overflow-hidden">
                    <View
                      className="h-full bg-white rounded-full"
                      style={{
                        width: `${
                          trailerDuration > 0
                            ? (trailerPosition / trailerDuration) * 100
                            : 0
                        }%`,
                      }}
                    />
                  </View>
                </View>
              )}
            </>
          )}
        </View>

        <View className="w-full px-[10px] text-justify py-3">
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
        {activeSubscription && !isSubscribed && (
          <View className="w-full px-[10px] flex items-center justify-center my-3">
            <Pressable
              onPress={() => setIsSubscriptionInformationsModalVisible(true)}
              style={style.coloredButton}
            >
              <Text className="font-extrabold text-black">Subscribe Now</Text>
            </Pressable>
          </View>
        )}

        {isSubscribed && customerRegistrationInfo && !isNewVersionAvailable && (
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
        )}

        {isSubscribed && customerRegistrationInfo && isNewVersionAvailable && (
          <View className="w-full px-[10px] flex items-center justify-center my-3">
            <Pressable
              onPress={() => handleViewNewVersion()}
              style={style.coloredButton}
            >
              <Text className="font-extrabold text-black">
                New Version Available
              </Text>
            </Pressable>
          </View>
        )}
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
