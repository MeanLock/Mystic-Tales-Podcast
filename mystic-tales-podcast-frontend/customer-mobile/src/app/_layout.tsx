// Import gesture the first
import "react-native-gesture-handler";

// Then import the rest
import FontAwesome from "@expo/vector-icons/FontAwesome";
import {
  DarkTheme,
  DefaultTheme,
  ThemeProvider,
  useNavigationContainerRef,
} from "@react-navigation/native";
import { useFonts } from "expo-font";
import { Stack, usePathname, useSegments } from "expo-router";
import * as SplashScreen from "expo-splash-screen";
import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { Provider, useSelector } from "react-redux";
import { useColorScheme } from "@/src/components/useColorScheme";

import "../../global.css";
import { persistor, RootState, store } from "../store/store";
import { bootstrapAuth } from "../utils/helpers/boostrapHelper";
import SetUp from "./setUp";
import { PersistGate } from "redux-persist/integration/react";
import { GestureHandlerRootView } from "react-native-gesture-handler";

import { Animated, Image, Pressable, StyleSheet } from "react-native";
import { useSafeAreaInsets } from "react-native-safe-area-context";
import { BottomSheetModalProvider } from "@gorhom/bottom-sheet";
import PlayerButtonUI from "./mediaPlayer/buttonUI";
import MediaPlayerModal, {
  MediaPlayerModalRef,
} from "./mediaPlayer/mediaPlayerModal";
import { Audio } from "expo-av";
import { GlobalAlert } from "../components/alert/GlobalAlert";
import UpdateAccountMeHook from "./UpdateAccountMeHook";
import { usePlayer } from "../core/services/player/usePlayer";
import useUpdateLastDurationListener from "../core/services/player/useUpdateLastDuration";

export {
  // Catch any errors thrown by the Layout component.
  ErrorBoundary,
} from "expo-router";

// Prevent the splash screen from auto-hiding before asset loading is complete.
SplashScreen.preventAutoHideAsync();

export default function RootLayout() {
  const [loaded, error] = useFonts({
    SpaceMono: require("../../assets/fonts/SpaceMono-Regular.ttf"),
    ...FontAwesome.font,
  });

  // Expo Router uses Error Boundaries to catch errors in the navigation tree.
  useEffect(() => {
    if (error) throw error;
  }, [error]);

  useEffect(() => {
    if (loaded) {
      SplashScreen.hideAsync();
    }
  }, [loaded]);

  if (!loaded) {
    return null;
  }

  return <RootLayoutNav />;
}

// Hook để detect xem có đang ở tab screen không
function useIsTabScreen() {
  const segments = useSegments();
  const isTabScreen = segments[0] === "(tabs)";
  return isTabScreen;
}

function RootLayoutNav() {
  return (
    <GestureHandlerRootView style={{ flex: 1 }}>
      <Provider store={store}>
        <PersistGate
          loading={null}
          persistor={persistor}
          onBeforeLift={() => {
            console.log("🔄 PersistGate: Before lift");
          }}
        >
          <SetUp />
          {/* <PlayerCore /> */}
          <UpdateAccountMeHook />
          <GlobalAlert />
          <ThemeProvider
            value={useColorScheme() === "dark" ? DarkTheme : DefaultTheme}
          >
            <AppBody />
          </ThemeProvider>
        </PersistGate>
      </Provider>
    </GestureHandlerRootView>
  );
}

function AppBody() {
  // Layout Hooks (inside Provider so we can use useSelector)
  const insets = useSafeAreaInsets();
  const isTabScreen = useIsTabScreen();
  const TAB_BAR_HEIGHT = 60;

  const bottomAnim = useRef(
    new Animated.Value(TAB_BAR_HEIGHT + insets.bottom)
  ).current;

  const targetBottom = useMemo(() => {
    if (isTabScreen) return TAB_BAR_HEIGHT + insets.bottom;
    return insets.bottom;
  }, [isTabScreen, insets.bottom]);

  useEffect(() => {
    (async () => {
      await Audio.setAudioModeAsync({
        staysActiveInBackground: true,
        playsInSilentModeIOS: true,
        shouldDuckAndroid: true,
        playThroughEarpieceAndroid: false,
      });
    })();
  }, []);

  useEffect(() => {
    Animated.spring(bottomAnim, {
      toValue: targetBottom,
      useNativeDriver: false,
      tension: 80,
      friction: 10,
    }).start();
  }, [targetBottom, bottomAnim]);

  useEffect(() => {
    bootstrapAuth(store.dispatch);
  }, []);

  // Bottom Sheet Modal state
  const [isButtonVisibleLocal, setIsButtonVisibleLocal] = useState(true);
  const bottomSheetModalRef = useRef<MediaPlayerModalRef>(null);

  const handlePresentModalPress = useCallback(() => {
    setIsButtonVisibleLocal(false);
    bottomSheetModalRef.current?.present();
    setTimeout(() => bottomSheetModalRef.current?.snapToIndex(0), 100);
  }, []);

  const handleSheetChanges = useCallback((index: number) => {
    if (index === -1) setIsButtonVisibleLocal(true);
    else if (index === 0) setIsButtonVisibleLocal(false);
    console.log("handleSheetChanges", index);
  }, []);

  // derive from Redux: hide button when player stopped
  const playStatus = useSelector(
    (s: RootState) => s.player.playMode.playStatus
  );
  const { state: UiState } = usePlayer();

  // Auto-update last duration every 2 seconds when playing
  useUpdateLastDurationListener();

  const isPlayerStopped = UiState.currentAudio === null;

  const isButtonVisible = isButtonVisibleLocal && !isPlayerStopped;

  return (
    <BottomSheetModalProvider>
      <Stack initialRouteName="(tabs)">
        <Stack.Screen
          name="index"
          options={{ headerShown: false, animation: "none" }}
        />
        <Stack.Screen name="(tabs)" options={{ headerShown: false }} />
        <Stack.Screen name="(auth)" options={{ headerShown: false }} />
        <Stack.Screen name="(content)" options={{ headerShown: false }} />
        <Stack.Screen name="(user)" options={{ headerShown: false }} />
      </Stack>

      {isButtonVisible && (
        <Animated.View
          style={{
            position: "absolute",
            bottom: bottomAnim,
            right: 20,
            left: 20,
            zIndex: 99999,
            gap: 10,
          }}
        >
          <Pressable
            onPress={handlePresentModalPress}
            style={{
              backgroundColor: "#282828",
              borderRadius: 10,
              shadowColor: "#000",
              shadowOffset: { width: 0, height: 2 },
              shadowOpacity: 0.25,
              shadowRadius: 3.84,
              elevation: 10,
            }}
          >
            <PlayerButtonUI />
          </Pressable>
        </Animated.View>
      )}

      <MediaPlayerModal
        ref={bottomSheetModalRef}
        onChange={handleSheetChanges}
      />
    </BottomSheetModalProvider>
  );
}

const styles = StyleSheet.create({
  contentContainer: {
    flex: 1,
    alignItems: "center",
  },
});
