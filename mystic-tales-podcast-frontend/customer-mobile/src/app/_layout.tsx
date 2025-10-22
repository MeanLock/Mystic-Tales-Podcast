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

import { Text } from "../components/ui/Text";
import { View } from "../components/ui/View";

import {
  Animated,
  Image,
  Pressable,
  StyleSheet,
  useWindowDimensions,
} from "react-native";
import { useSafeAreaInsets } from "react-native-safe-area-context";

import { BottomSheetModalProvider } from "@gorhom/bottom-sheet";
import MediaPlayerContent from "./mediaPlayer";
import PlayerButtonUI from "./mediaPlayer/buttonUI";
import MediaPlayerModal, {
  MediaPlayerModalRef,
} from "./mediaPlayer/mediaPlayerModal";
import { Audio } from "expo-av";

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
  // Layout Hooks
  const insets = useSafeAreaInsets();
  const colorScheme = useColorScheme();
  const TAB_BAR_HEIGHT = 60; // Fixed height cho tab bar

  // Detect xem có đang ở tab screen không
  const isTabScreen = useIsTabScreen();

  // Animated value cho bottom position
  const bottomAnim = useRef(
    new Animated.Value(TAB_BAR_HEIGHT + insets.bottom)
  ).current;

  // Tính bottom spacing động
  const targetBottom = useMemo(() => {
    if (isTabScreen) {
      return TAB_BAR_HEIGHT + insets.bottom;
    }
    return insets.bottom; // Chỉ cần thêm padding nhẹ khi không có tab bar
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
      useNativeDriver: false, // bottom không support native driver
      tension: 80, // Độ căng của spring (càng cao càng nhanh)
      friction: 10, // Độ ma sát (càng cao càng ít bounce)
    }).start();
  }, [targetBottom, bottomAnim]);

  useEffect(() => {
    bootstrapAuth(store.dispatch);
  }, []);

  // Bottom Sheet Modal
  // states
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [isButtonVisible, setIsButtonVisible] = useState(true);

  // ref
  const bottomSheetModalRef = useRef<MediaPlayerModalRef>(null);

  // callbacks
  const handlePresentModalPress = useCallback(() => {
    setIsButtonVisible(false);
    bottomSheetModalRef.current?.present();
    // Dùng setTimeout để đảm bảo modal đã mount xong
    setTimeout(() => {
      bottomSheetModalRef.current?.snapToIndex(0); // Snap to 100%
    }, 100);
  }, []);

  const handleSheetChanges = useCallback((index: number) => {
    if (index === -1) {
      setIsButtonVisible(true);
      // Modal đã đóng
    } else if (index === 0) {
      setIsButtonVisible(false);
      // Modal đang mở full screen
    }
    console.log("handleSheetChanges", index);
  }, []);

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

          <ThemeProvider
            value={colorScheme === "dark" ? DarkTheme : DefaultTheme}
          >
            <BottomSheetModalProvider>
              <Stack initialRouteName="(tabs)">
                <Stack.Screen
                  name="index"
                  options={{ headerShown: false, animation: "none" }}
                />
                <Stack.Screen name="(tabs)" options={{ headerShown: false }} />
                <Stack.Screen name="(auth)" options={{ headerShown: false }} />
                <Stack.Screen
                  name="(content)"
                  options={{ headerShown: false }}
                />
                <Stack.Screen name="(user)" options={{ headerShown: false }} />
              </Stack>

              {/* Open Button */}
              {isButtonVisible && (
                <Animated.View
                  style={{
                    position: "absolute",
                    bottom: bottomAnim, // <<-- Sử dụng Animated value
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

              {/* Modal */}
              <MediaPlayerModal
                ref={bottomSheetModalRef}
                onChange={handleSheetChanges}
              />
            </BottomSheetModalProvider>
          </ThemeProvider>
        </PersistGate>
      </Provider>
    </GestureHandlerRootView>
  );
}

const styles = StyleSheet.create({
  contentContainer: {
    flex: 1,
    alignItems: "center",
  },
});
