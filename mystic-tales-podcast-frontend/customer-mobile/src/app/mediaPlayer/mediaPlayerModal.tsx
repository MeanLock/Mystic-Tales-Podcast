import {
  useMemo,
  forwardRef,
  useCallback,
  useRef,
  useImperativeHandle,
} from "react";
import { StyleSheet, ImageBackground, Dimensions } from "react-native";
import { useSafeAreaInsets } from "react-native-safe-area-context";
import { BottomSheetModal, BottomSheetView } from "@gorhom/bottom-sheet";
import MediaPlayerContent from "./index";
import { useSelector } from "react-redux";
import { RootState } from "@/src/store/store";
import { BlurView } from "expo-blur";
import { View } from "@/src/components/ui/View";
import AutoResolvingImageBackground from "@/src/components/autoResolveImage/AutoResolveImageBackground";
import { usePlayer } from "@/src/core/services/player/usePlayer";

export interface MediaPlayerModalRef {
  present: () => void;
  close: () => void;
  snapToIndex: (index: number) => void;
}

interface MediaPlayerModalProps {
  onChange?: (index: number) => void;
}

const customBackgroundSheet = () => {
  const {state: uiState} = usePlayer();
  
  if (uiState.currentAudio?.image) {
    return (
      <AutoResolvingImageBackground
        FileKey={uiState.currentAudio.image}
        type="PodcastPublicSource"
        style={styles.backgroundImage}
      >
        <BlurView
          intensity={50}
          tint="dark"
          style={[StyleSheet.absoluteFill, { zIndex: 1 }]}
        />
      </AutoResolvingImageBackground>
    );
  } else {
    return <View style={styles.fallbackBackground} />;
  }
};

const MediaPlayerModal = forwardRef<MediaPlayerModalRef, MediaPlayerModalProps>(
  ({ onChange }, ref) => {
    // Redux selectors and refs
    const playerState = useSelector((state: RootState) => state.player);

    const insets = useSafeAreaInsets();
    const bottomSheetModalRef = useRef<BottomSheetModal>(null);

    // Expose methods to parent
    useImperativeHandle(ref, () => ({
      present: () => {
        bottomSheetModalRef.current?.present();
      },
      close: () => {
        bottomSheetModalRef.current?.close();
      },
      snapToIndex: (index: number) => {
        bottomSheetModalRef.current?.snapToIndex(index);
      },
    }));

    const handleSheetChanges = useCallback(
      (index: number) => {
        onChange?.(index);
      },
      [onChange]
    );

    return (
      <BottomSheetModal
        ref={bottomSheetModalRef}
        onChange={handleSheetChanges}
        index={0}
        snapPoints={useMemo(() => ["100%"], [])} // Có 1 snap points
        enablePanDownToClose={true}
        enableDismissOnClose={true}
        enableDynamicSizing={true}
        enableOverDrag={true}
        backgroundComponent={customBackgroundSheet}
        handleIndicatorStyle={{
          backgroundColor: "rgba(217, 217, 217, 0.6)",
          width: 40,
          height: 4,
          marginTop: 50,
        }}
        backgroundStyle={{ backgroundColor: "#000000" }} // Đen để content không bị transparent
        animationConfigs={{
          duration: 500,
          overshootClamping: true,
        }}
        style={{
          shadowColor: "#000",
          shadowOffset: {
            width: 0,
            height: -4,
          },
          shadowOpacity: 0.25,
          shadowRadius: 4,
          elevation: 5,
        }}
        onAnimate={(_fromIdx, _toIdx, fromPos, toPos) => {
          if (toPos > fromPos) {
            bottomSheetModalRef.current?.close();
          }
        }}
      >
        <BottomSheetView style={styles.sheetContainer}>
          {/* Content - chiếm hết màn hình, padding top để tránh indicator */}
          <View style={[styles.contentContainer]}>
            <MediaPlayerContent />
          </View>
        </BottomSheetView>
      </BottomSheetModal>
    );
  }
);

MediaPlayerModal.displayName = "MediaPlayerModal";

const styles = StyleSheet.create({
  sheetContainer: {
    flex: 1,
    height: Dimensions.get("window").height,
    position: "relative",
    backgroundColor: "transparent", // Fallback background color
  },
  backgroundImage: {
    position: "absolute",
    top: 0,
    left: 0,
    right: 0,
    bottom: 0,
    width: "100%",
    height: "100%",
    zIndex: 0,
  },
  fallbackBackground: {
    position: "absolute",
    top: 0,
    left: 0,
    right: 0,
    bottom: 0,
    width: "100%",
    height: "100%",
    backgroundColor: "#181818",
    zIndex: 0,
  },
  overlay: {
    flex: 1,
    backgroundColor: "rgba(0, 0, 0, 0.5)", // Lớp phủ đen 70% opacity
  },
  contentContainer: {
    flex: 1,
    width: "100%", // Chiếm hết chiều rộng
    height: "100%",
    zIndex: 10, // Cao hơn background và overlay
    backgroundColor: "transparent",
  },
});

export default MediaPlayerModal;
