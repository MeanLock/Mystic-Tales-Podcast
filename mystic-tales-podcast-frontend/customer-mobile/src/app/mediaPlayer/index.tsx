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
import { useUpdatePlayModeMutation } from "@/src/core/services/player/playerService";

const MainAudioCard = ({ audio }: { audio: CurrentAudio }) => {
  return (
    <View style={styles.currentAudioCardContainer}>
      <AutoResolvingImage
        FileKey={audio?.MainImageFileKey || ""}
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
          {audio?.Name}
        </Text>
        <Text className="text-[#BFC0BA] text-[12px]">
          {audio?.PodcasterName}
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
  const playerState = useSelector((state: RootState) => state.player);
  const dispatch = useDispatch();
  const { canNavigate, navigateNext, navigatePrevious } = usePlayerNavigate();

  const duration =
    playerState.playbackDuration ?? playerState.currentAudio?.AudioLength ?? 0;

  const position = playerState.playbackPosition ?? 0;

  const clampedPos = Math.min(Math.max(position, 0), duration);
  const remaining = Math.max(duration - clampedPos, 0);
  const progressPct = duration > 0 ? (clampedPos / duration) * 100 : 0;

  const handlePausePress = () => {
    dispatch(pauseAudio());
  };

  const handlePlayPress = () => {
    if (!playerState.listenSessionProcedure || !playerState.currentAudio) {
      return;
    }
    dispatch(
      playAudio({
        sourceType: playerState.listenSessionProcedure.SourceDetail.Type,
        audioId: playerState.currentAudio.Id,
      })
    );
  };

  const [updatePlayMode] = useUpdatePlayModeMutation();

  const setIsAutoPlay = (isAutoPlay: boolean) => {
    const restoreValue = playerState.playMode.isAutoPlay;

    if (!playerState.listenSessionProcedure) return;
    if (!playerState.listenSessionProcedure.PlayOrderMode) return;

    dispatch(setUIIsAutoPlay(isAutoPlay));
    // Gọi API cập nhật ngầm
    try {
      updatePlayMode({
        IsAutoPlay: isAutoPlay,
        PlayOrderMode: playerState.listenSessionProcedure.PlayOrderMode,
        CustomerListenSessionProcedureId: playerState.listenSessionProcedure.Id,
      }).unwrap();
    } catch (error) {
      // Nếu lỗi thì restore lại giá trị cũ
      dispatch(setUIIsAutoPlay(restoreValue));
    }
  };

  const setPlayOrderMode = (mode: "Sequential" | "Random") => {
    const restoreValue = playerState.listenSessionProcedure?.PlayOrderMode;
    if (!playerState.listenSessionProcedure) return;
    dispatch(setUIPlayOrderMode(mode));
    // Gọi API cập nhật ngầm
    try {
      updatePlayMode({
        IsAutoPlay: playerState.playMode.isAutoPlay,
        PlayOrderMode: mode,
        CustomerListenSessionProcedureId: playerState.listenSessionProcedure.Id,
      }).unwrap();
    } catch (error) {
      // Nếu lỗi thì restore lại giá trị cũ
      if (restoreValue) {
        dispatch(setUIPlayOrderMode(restoreValue));
      }
    }
  };

  // SỬA LẠI LOGIC: Disable nút Next nếu
  // player.playMode.isNextSessionNull === true (không có audio tiếp theo)
  // Và
  // player.listenSessionProcedure.ListenObjectsSequentialOrder có số lượng các item có
  // const canNavigate = () => {
  //   if (!playerState.listenSessionProcedure || !playerState.listenSession) {
  //     return false;
  //   }

  //   if (playerState.playMode.isNextSessionNull) {
  //     let availableAudioCount = 0;
  //     if (
  //       playerState.listenSessionProcedure &&
  //       playerState.listenSessionProcedure.ListenObjectsRandomOrder &&
  //       playerState.listenSessionProcedure.ListenObjectsRandomOrder.length > 0
  //     ) {
  //       const availableAudio =
  //         playerState.listenSessionProcedure?.ListenObjectsRandomOrder.map(
  //           (item) => item.IsListenable
  //         );
  //       availableAudioCount = availableAudio?.length ?? 0;
  //     } else if (
  //       playerState.listenSessionProcedure &&
  //       playerState.listenSessionProcedure.ListenObjectsSequentialOrder &&
  //       playerState.listenSessionProcedure.ListenObjectsSequentialOrder.length >
  //         0
  //     ) {
  //       const availableAudio =
  //         playerState.listenSessionProcedure.ListenObjectsSequentialOrder.map(
  //           (item) => item.IsListenable
  //         );
  //       availableAudioCount = availableAudio?.length ?? 0;
  //     }
  //     return availableAudioCount > 0;
  //   } else {
  //     return true;
  //   }
  // };

  return (
    <View style={styles.container}>
      <View style={styles.contentContainer}>
        <View style={styles.currentContainer}>
          <View style={styles.imageContainer}>
            <AutoResolvingImage
              FileKey={playerState.currentAudio?.MainImageFileKey || ""}
              type="PodcastPublicSource"
              style={styles.image}
            />
          </View>

          <MainAudioCard audio={playerState.currentAudio} />
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
            onValueChange={(val) => {
              // chỉ update UI (redux) để thanh chạy mượt, không gọi engine
              // dispatch(seekPreview({ position: val }));
            }}
            onSlidingComplete={(val) => {
              // seek thật: lúc này middleware sẽ gọi engine.seek duy nhất 1 lần
              dispatch(seekTo({ position: val }));
            }}
          />

          {/* Thời gian */}
          <View className="w-full justify-between items-center flex-row">
            <Text>{formatAudioLength(clampedPos)}</Text>
            <Text>-{formatAudioLength(remaining)}</Text>
          </View>
        </View>

        {/* Backward, Forward, Play Buttons */}
        <View
          style={{ gap: 40 }}
          className="w-full py-5 flex-row items-center justify-center"
        >
          <Pressable
            onPress={() => canNavigate && navigatePrevious()}
            disabled={!canNavigate}
            style={{ opacity: canNavigate ? 1 : 0.4 }}
          >
            <Foundation name="previous" color="#d9d9d9" size={30} />
          </Pressable>
          {/* === NEW: tua -10s === */}
          <Pressable onPress={() => dispatch(seekBy({ delta: -10 }))}>
            <MaterialCommunityIcons name="rewind-10" color="#fff" size={25} />
          </Pressable>
          {playerState.playMode.playStatus === "play" ? (
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
          <Pressable onPress={() => dispatch(seekBy({ delta: +10 }))}>
            <MaterialCommunityIcons
              name="fast-forward-10"
              color="#fff"
              size={25}
            />
          </Pressable>
          <Pressable
            onPress={() => canNavigate && navigateNext()}
            disabled={!canNavigate}
            style={{ opacity: canNavigate ? 1 : 0.4 }}
          >
            <Foundation name="next" color="#d9d9d9" size={30} />
          </Pressable>
        </View>

        {/* IsAutoPlay, Sequential/Random */}
        <View
          style={{ gap: 20 }}
          className="w-full h-[100px] flex-row items-center justify-center "
        >
          <Pressable
            onPress={() => setIsAutoPlay(false)}
            disabled={!canNavigate}
            style={{ opacity: canNavigate ? 1 : 0.4 }}
          >
            <MaterialIcons
              name="auto-awesome"
              color={playerState.playMode.isAutoPlay ? "#aee339" : "#d9d9d9"}
              size={20}
            />
          </Pressable>
          <Pressable
            onPress={() => setIsAutoPlay(false)}
            disabled={!canNavigate}
            style={{ opacity: canNavigate ? 1 : 0.4 }}
            className="ml-10"
          >
            <Foundation
              name="loop"
              color={playerState.playMode.isAutoPlay ? "#aee339" : "#d9d9d9"}
              size={20}
            />
          </Pressable>
          <Pressable
            onPress={() => setIsAutoPlay(false)}
            disabled={!canNavigate}
            style={{ opacity: canNavigate ? 1 : 0.4 }}
          >
            <FontAwesome5
              name="random"
              color={playerState.playMode.isAutoPlay ? "#aee339" : "#d9d9d9"}
              size={20}
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
