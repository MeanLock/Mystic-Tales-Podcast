import { Text } from "@/src/components/ui/Text";
import { View } from "@/src/components/ui/View";
import {
  CurrentAudioType,
  QueuedAudioType,
  pause,
  play,
  moveInQueue,
  moveInQueueSwap,
  seekBy,
  seekTo,
  nextTrack,
  seekPreview,
} from "@/src/features/mediaPlayer/playerSlice";
import { formatAudioLength } from "@/src/lib/format";
import { RootState } from "@/src/store/store";
import {
  Entypo,
  Feather,
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

const MainAudioCard = ({ audio }: { audio: CurrentAudioType }) => {
  return (
    <View style={styles.currentAudioCardContainer}>
      <Image
        style={styles.currentAudioCardImage}
        source={{ uri: audio?.ImageUrl }}
      />
      <View>
        <Text className="text-[#BFC0BA] font-semibold text-[12px]">
          EPISODE 1
        </Text>
        <Text
          numberOfLines={1}
          className="text-[#fff] font-semibold text-[16px] w-3/4"
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

const QueueAudioRow = ({
  audio,
  drag,
  isActive,
}: {
  audio: QueuedAudioType;
  drag: () => void;
  isActive: boolean;
}) => {
  return (
    <View
      style={[
        styles.queueAudioCardContainer,
        { opacity: isActive ? 0.9 : 1, marginBottom: 10 },
      ]}
    >
      <Image
        style={styles.queueAudioCardImage}
        source={{ uri: audio.ImageUrl }}
      />
      <View>
        <View className="w-[250px]">
          <Text numberOfLines={1} className="text-[#fff] text-[15px]">
            {audio.Name}
          </Text>
        </View>
        <Text className="text-[#D9D9D9] text-[12px]">
          Season 1, Episode 12 • {formatAudioLength(audio.AudioLength)}
        </Text>
      </View>

      <View className="flex-1 items-end justify-center">
        <Pressable className="p-1 bg-gray-300/10 rounded-md" onPressIn={drag}>
          <Feather name="menu" color="#9999" size={24} />
        </Pressable>
      </View>
    </View>
  );
};

const RenderQueueList = ({ queue }: { queue: QueuedAudioType[] }) => {
  const dispatch = useDispatch();
  const [localQueue, setLocalQueue] = useState(queue);

  useEffect(() => {
    setLocalQueue(queue);
  }, [queue]);

  if (!queue || queue.length === 0) {
    return (
      <View className="flex-1 items-center justify-center">
        <Text className="font-semibold text-[15px] text-[#fff]">
          Your queue is empty
        </Text>
        <Text className="font-light text-[15px] text-[#D9D9D9] text-center">
          Episodes you add to the queue will appear here.
        </Text>
      </View>
    );
  }

  return (
    <View className="flex-1 gap-2 px-[20px] mt-7">
      <View className="w-full flex-row items-center justify-between">
        <Text className="font-semibold text-white text-[16px]">Your Queue</Text>
        <Pressable>
          <Text className="font-light text-[15px] text-[#d9d9d9]">
            Clear all
          </Text>
        </Pressable>
      </View>

      <DraggableFlatList
        data={localQueue}
        extraData={localQueue.length}
        keyExtractor={(item, index) => `${item.Id}-${index}`}
        containerStyle={{ flexGrow: 1 }}
        contentContainerStyle={{ paddingVertical: 10 }}
        onDragEnd={({ from, to, data }) => {
          setLocalQueue(data);
          dispatch(moveInQueueSwap({ fromIndex: from, toIndex: to }));
        }}
        renderItem={({
          item,
          drag,
          isActive,
        }: RenderItemParams<QueuedAudioType>) => (
          <ScaleDecorator>
            <QueueAudioRow audio={item} drag={drag} isActive={isActive} />
          </ScaleDecorator>
        )}
        activationDistance={0}
        autoscrollThreshold={50}
        autoscrollSpeed={250}
      />
    </View>
  );
};

const MediaPlayerContent = () => {
  const playerState = useSelector((state: RootState) => state.player);
  const dispatch = useDispatch();

  const [displayMode, setDisplayMode] = useState<"current" | "queue">(
    "current"
  );

  const duration = playerState.currentAudio?.AudioLength ?? 0; // giây
  const position = playerState.currentAudio?.LatestPosition ?? 0; // giây
  const clampedPos = Math.min(Math.max(position, 0), duration);
  const remaining = Math.max(duration - clampedPos, 0);
  const progressPct = duration > 0 ? (clampedPos / duration) * 100 : 0;

  const handlePausePress = () => {
    dispatch(pause());
  };

  const handlePlayPress = () => {
    dispatch(play());
  };

  const canNext = playerState.queueAudios.length > 0;

  return (
    <View style={styles.container}>
      <View style={styles.contentContainer}>
        {displayMode === "current" ? (
          <View style={styles.currentContainer}>
            <View style={styles.imageContainer}>
              <Image
                source={{ uri: playerState.currentAudio?.ImageUrl }}
                style={styles.image}
              />
            </View>

            <MainAudioCard audio={playerState.currentAudio} />
          </View>
        ) : (
          <View style={styles.container}>
            <View className="w-full mt-5">
              <MainAudioCard audio={playerState.currentAudio} />
            </View>
            <RenderQueueList queue={playerState.queueAudios} />
          </View>
        )}
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
              dispatch(seekPreview({ position: val }));
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
          {/* === NEW: tua -10s === */}
          <Pressable onPress={() => dispatch(seekBy({ delta: -10 }))}>
            <MaterialCommunityIcons name="rewind-10" color="#fff" size={30} />
          </Pressable>
          {playerState.playerMode.playStatus === "playing" ? (
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
              size={30}
            />
          </Pressable>
        </View>

        {/* Next Audio, Change Layout to Queue Layout */}
        <View
          style={{ gap: 80 }}
          className="w-full h-[120px] flex-row items-center justify-center "
        >
          {/* Next Audio on Queue */}
          {/* === NEW: Next – disable nếu queue rỗng === */}
          <Pressable
            onPress={() => canNext && dispatch(nextTrack())}
            disabled={!canNext}
            style={{ opacity: canNext ? 1 : 0.4 }}
          >
            <Foundation name="next" color="#d9d9d9" size={30} />
          </Pressable>
          {/* Change Layout to Queue Layout */}
          {displayMode === "queue" ? (
            <Pressable
              onPress={() => setDisplayMode("current")}
              className="p-1 rounded-md bg-[#d9d9d9]/30"
            >
              <Entypo name="list" color="#000" size={30} />
            </Pressable>
          ) : (
            <Pressable className="p-1" onPress={() => setDisplayMode("queue")}>
              <Entypo name="list" color="#d9d9d9" size={30} />
            </Pressable>
          )}
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
