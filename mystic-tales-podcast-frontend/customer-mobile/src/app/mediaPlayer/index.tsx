import { Text } from "@/src/components/ui/Text";
import { View } from "@/src/components/ui/View";

import { formatAudioLength } from "@/src/lib/format";
import { RootState } from "@/src/store/store";
import {
  Entypo,
  Feather,
  FontAwesome5,
  Foundation,
  Ionicons,
  MaterialCommunityIcons,
  MaterialIcons,
} from "@expo/vector-icons";
import { useEffect, useState } from "react";
import { Image, Pressable } from "react-native";
import { StyleSheet } from "react-native";
import { ScrollView } from "react-native-gesture-handler";
import { useDispatch, useSelector } from "react-redux";
import DraggableFlatList, {
  RenderItemParams,
  ScaleDecorator,
} from "react-native-draggable-flatlist";
import Slider from "@react-native-community/slider";
import {
  CurrentAudio,
  ListenSessionBookingTracks,
  ListenSessionEpisodes,
} from "@/src/core/types/audio.type";
import AutoResolvingImage from "@/src/components/autoResolveImage/AutoResolvingImage";
import {
  pauseAudio,
  playAudio,
  seekBy,
  seekTo,
  setUIIsAutoPlay,
  setUIPlayOrderMode,
} from "@/src/features/mediaPlayer/playerSlice";
import { usePlayerNavigate } from "@/src/core/services/player/usePlayerNavigate";
import {
  useUpdateBookingTrackLastDurationMutation,
  useUpdateEpisodeLastDurationMutation,
  useUpdatePlayModeMutation,
} from "@/src/core/services/player/playerService";
import { usePlayer } from "@/src/core/services/player/usePlayer";
import {
  SubscriptionBenefit,
  useLazyGetSubscriptionBenefitsMapListFromEpisodeIdQuery,
} from "@/src/core/services/subscription/subscription.service";
import { setDataAndShowAlert } from "@/src/features/alert/alertSlice";
import { playerEngine } from "@/src/core/services/player/playerEngine";

export type CurrentTrack = {
  id: string;
  name: string;
  image?: string;
  podcasterName?: string;
} | null;

const MainAudioCard = ({ audio }: { audio: CurrentTrack }) => {
  return (
    <View style={styles.currentAudioCardContainer}>
      <AutoResolvingImage
        FileKey={audio?.image || ""}
        type="PodcastPublicSource"
        style={styles.currentAudioCardImage}
      />
      <View>
        <Text className="text-[#BFC0BA] font-semibold text-[12px]">
          NOW PLAYING
        </Text>
        <Text
          numberOfLines={1}
          className="text-[#fff] font-semibold text-[16px]"
        >
          {audio?.name}
        </Text>
        <Text className="text-[#BFC0BA] text-[12px]">
          {audio?.podcasterName}
        </Text>
      </View>

      <View className="flex-1 items-end justify-center mr-3">
        <Pressable className="p-1 bg-gray-300/10 rounded-full">
          <Feather name="more-horizontal" color="#fff" size={25} />
        </Pressable>
      </View>
    </View>
  );
};

const MediaPlayerContent = () => {
  const {
    state: uiState,
    play,
    pause,
    seekTo,
    checkIsNaviableInProcedure,
    navigateInBookingTracks,
    navigateInSpecifyShows,
    navigateInSavedEpisodes,
  } = usePlayer();

  const player = useSelector((state: RootState) => state.player);

  const duration = uiState.duration ?? 0;

  const position = uiState.currentTime;

  const clampedPos = Math.min(Math.max(position, 0), duration);
  const remaining = Math.max(duration - clampedPos, 0);

  // STATES
  const [isAutoPlay, setIsAutoPlayState] = useState<boolean>(false);
  const [playOrderMode, setPlayOrderModeState] = useState<
    "Sequential" | "Random"
  >("Sequential");
  // HOOKS

  useEffect(() => {
    if (!player.listenSessionProcedure) return;
    setIsAutoPlayState(player.listenSessionProcedure.IsAutoPlay);
    setPlayOrderModeState(
      player.listenSessionProcedure.PlayOrderMode as "Sequential" | "Random"
    );
  }, [player.listenSessionProcedure]);
  // FUNCTIONS
  const handlePausePress = () => {
    pause();
  };

  const handlePlayPress = () => {
    play();
  };

  const dispatch = useDispatch();

  const [updatePlayMode] = useUpdatePlayModeMutation();

  const handleNavigate = async (navigateType: "Next" | "Previous") => {
    if (!player.listenSessionProcedure || !player.listenSession) {
      return;
    }
    if (uiState.sourceType === "SpecifyShowEpisodes") {
      const ls = player.listenSession as ListenSessionEpisodes;
      await navigateInSpecifyShows(
        navigateType,
        ls,
        player.listenSessionProcedure
      );
    } else if (uiState.sourceType === "SavedEpisodes") {
      const ls = player.listenSession as ListenSessionEpisodes;
      await navigateInSavedEpisodes(
        navigateType,
        ls,
        player.listenSessionProcedure
      );
    } else {
      const ls = player.listenSession as ListenSessionBookingTracks;
      await navigateInBookingTracks(
        navigateType,
        ls,
        player.listenSessionProcedure
      );
    }
  };

  const handleToggleAutoPlay = async (newValue: boolean) => {
    // 1. Cập nhật UI ngay
    const restoreValue = isAutoPlay;
    setIsAutoPlayState(newValue);
    playerEngine.setAutoPlay(newValue);

    // 2. Gọi API cập nhật ngầm
    if (!player.listenSessionProcedure) return;
    updatePlayMode({
      IsAutoPlay: newValue,
      PlayOrderMode: player.listenSessionProcedure.PlayOrderMode,
      CustomerListenSessionProcedureId: player.listenSessionProcedure.Id,
    })
      .unwrap()
      .catch(() => {
        setIsAutoPlayState(restoreValue);
        playerEngine.setAutoPlay(restoreValue);
      });
  };

  const handleSetPlayOrderMode = (mode: "Sequential" | "Random") => {
    const restoreMode = playOrderMode;
    if (mode === playOrderMode) return;
    setPlayOrderModeState(mode);

    if (!player.listenSessionProcedure) return;
    updatePlayMode({
      IsAutoPlay: player.listenSessionProcedure.IsAutoPlay,
      PlayOrderMode: mode,
      CustomerListenSessionProcedureId: player.listenSessionProcedure.Id,
    })
      .unwrap()
      .catch(() => {
        setPlayOrderModeState(restoreMode);
      });
  };

  return (
    <View style={styles.container}>
      <View style={styles.contentContainer}>
        <View style={styles.currentContainer}>
          <View style={styles.imageContainer}>
            <AutoResolvingImage
              FileKey={uiState.currentAudio?.image || ""}
              type="PodcastPublicSource"
              style={styles.image}
            />
          </View>

          <MainAudioCard audio={uiState.currentAudio} />
        </View>
      </View>
      <View style={styles.actionContainer}>
        {/* Audio Length Tracking */}
        <View className="w-full px-[33px] gap-1">
          {/* === NEW: Slider kéo seek === */}
          <Slider
            style={{ width: "100%", height: 28 }}
            value={clampedPos}
            minimumValue={0}
            maximumValue={Math.max(duration, 0.000001)}
            step={1}
            minimumTrackTintColor="#fff"
            maximumTrackTintColor="rgba(217,217,217,0.3)"
            thumbTintColor="#fff"
            onSlidingComplete={(val) => {
              // seek thật: lúc này middleware sẽ gọi engine.seek duy nhất 1 lần
              seekTo(val);
            }}
          />

          {/* Thời gian */}
          <View className="w-full justify-between items-center flex-row ">
            <Text className="text-white">{formatAudioLength(clampedPos)}</Text>
            <Text className="text-white">-{formatAudioLength(remaining)}</Text>
          </View>
        </View>

        {/* Backward, Forward, Play Buttons */}
        <View
          style={{ gap: 40 }}
          className="w-full py-5 flex-row items-center justify-center"
        >
          <Pressable
            onPress={() => handleNavigate("Previous")}
            disabled={!checkIsNaviableInProcedure()}
          >
            <Foundation
              name="previous"
              color={
                checkIsNaviableInProcedure()
                  ? "white"
                  : "rgba(217, 217, 217, 0.4)"
              }
              size={30}
            />
          </Pressable>
          {/* === NEW: tua -10s === */}
          <Pressable onPress={() => seekTo(Math.max(clampedPos - 10, 0))}>
            <MaterialCommunityIcons name="rewind-10" color="#fff" size={25} />
          </Pressable>
          {uiState.isPlaying ? (
            <Pressable
              onPress={() => {
                handlePausePress();
              }}
            >
              <Ionicons name="pause" color="#fff" size={50} />
            </Pressable>
          ) : (
            <Pressable
              onPress={() => {
                handlePlayPress();
              }}
            >
              <Ionicons name="play" color="#fff" size={50} />
            </Pressable>
          )}
          {/* === NEW: tua +10s === */}
          <Pressable
            onPress={() => seekTo(Math.min(clampedPos + 10, duration))}
          >
            <MaterialCommunityIcons
              name="fast-forward-10"
              color="#fff"
              size={25}
            />
          </Pressable>
          <Pressable
            onPress={() => handleNavigate("Next")}
            disabled={!checkIsNaviableInProcedure()}
          >
            <Foundation
              name="next"
              color={
                checkIsNaviableInProcedure()
                  ? "white"
                  : "rgba(217, 217, 217, 0.4)"
              }
              size={30}
            />
          </Pressable>
        </View>

        {/* IsAutoPlay, Sequential/Random */}
        <View
          style={{ gap: 20 }}
          className="w-full h-[100px] flex-row items-center justify-center "
        >
          <Pressable
            onPress={() => handleToggleAutoPlay(!isAutoPlay)}
            disabled={!checkIsNaviableInProcedure()}
            style={{ opacity: checkIsNaviableInProcedure() ? 1 : 0.4 }}
          >
            <MaterialIcons
              name="auto-awesome"
              color={isAutoPlay ? "#aee339" : "#d9d9d9"}
              size={30}
            />
          </Pressable>
          <Pressable
            onPress={() => handleSetPlayOrderMode("Sequential")}
            disabled={!checkIsNaviableInProcedure()}
            style={{ opacity: checkIsNaviableInProcedure() ? 1 : 0.4 }}
            className="ml-10"
          >
            <Foundation
              name="loop"
              color={playOrderMode === "Sequential" ? "#aee339" : "#d9d9d9"}
              size={25}
            />
          </Pressable>
          <Pressable
            onPress={() => handleSetPlayOrderMode("Random")}
            disabled={!checkIsNaviableInProcedure()}
            style={{ opacity: checkIsNaviableInProcedure() ? 1 : 0.4 }}
          >
            <FontAwesome5
              name="random"
              color={playOrderMode === "Random" ? "#aee339" : "#d9d9d9"}
              size={25}
            />
          </Pressable>
        </View>
      </View>
    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    flexDirection: "column",
    alignItems: "center",
    backgroundColor: "transparent",
    width: "100%",
  },
  contentContainer: {
    flex: 1, // Chiếm phần còn lại (60%)
    width: "100%",
  },
  actionContainer: {
    width: "100%",
    height: "40%", // Cố định 40% chiều cao màn hình
    paddingTop: 20,
  },

  currentContainer: {
    width: "100%",
    height: "100%",
    alignItems: "center",
  },
  imageContainer: {
    width: "100%",
    height: 474,
    alignItems: "center",
    justifyContent: "center",
  },
  image: {
    width: 288,
    height: 288,
    borderRadius: 8,
    resizeMode: "cover",

    // Android: hiệu ứng nổi (shadow mặc định theo Material)
    elevation: 10,

    // iOS: tự tạo shadow tương tự
    shadowColor: "#000",
    shadowOffset: { width: 0, height: 4 },
    shadowOpacity: 0.3,
    shadowRadius: 4.65,
    backgroundColor: "#fff", // rất quan trọng: nếu không có sẽ không thấy bóng
  },

  currentAudioCardContainer: {
    width: "100%",
    height: 57,
    flexDirection: "row",
    alignItems: "center",
    paddingHorizontal: 33,
    gap: 14,
  },
  currentAudioCardImage: {
    width: 57,
    height: 57,
    borderRadius: 8,
    resizeMode: "cover",

    // Android: hiệu ứng nổi (shadow mặc định theo Material)
    elevation: 10,

    // iOS: tự tạo shadow tương tự
    shadowColor: "#000",
    shadowOffset: { width: 0, height: 4 },
    shadowOpacity: 0.3,
    shadowRadius: 4.65,
    backgroundColor: "#fff", // rất quan trọng: nếu không có sẽ không thấy bóng
  },

  queueAudioCardContainer: {
    width: "100%",
    height: 50,
    flexDirection: "row",
    alignItems: "center",
    gap: 13,
  },
  queueAudioCardImage: {
    width: 50,
    height: 50,
    borderRadius: 8,
    resizeMode: "cover",

    // Android: hiệu ứng nổi (shadow mặc định theo Material)
    elevation: 10,

    // iOS: tự tạo shadow tương tự
    shadowColor: "#000",
    shadowOffset: { width: 0, height: 4 },
    shadowOpacity: 0.3,
    shadowRadius: 4.65,
    backgroundColor: "#fff", // rất quan trọng: nếu không có sẽ không thấy bóng
  },
});
export default MediaPlayerContent;
