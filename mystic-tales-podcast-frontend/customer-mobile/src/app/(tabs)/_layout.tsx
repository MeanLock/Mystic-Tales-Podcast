import React, {
  useMemo,
  useRef,
  useState,
  useContext,
  createContext,
  useEffect,
} from "react";
import { Tabs, usePathname, useRouter } from "expo-router";
import {
  StyleSheet,
  View,
  Text,
  Pressable,
  Platform,
  Animated,
  useWindowDimensions,
  Image,
} from "react-native";
import { BlurView } from "expo-blur";
import MaterialIcons from "@expo/vector-icons/MaterialIcons";
import { useSafeAreaInsets } from "react-native-safe-area-context";

import Colors, {
  primaryThemeColor,
  secondaryThemeColor,
  tintColorDark,
  tintColorLight,
} from "@/src/constants/Colors";
import { useColorScheme } from "@/src/components/useColorScheme";
import { useClientOnlyValue } from "@/src/components/useClientOnlyValue";
import { useSelector } from "react-redux";
import { RootState } from "@/src/store/store";

const HEADER_LARGE = 70; // chiều cao phần body lớn (không tính safeTop)
const HEADER_COMPACT = 56; // compact height (Android/iOS giống nhau)

const libChildTitles: Record<string, string> = {
  shows: "My Shows",
  channels: "Channels",
  favorites: "Favorites",
  downloads: "Downloads",
  bookings: "Bookings",
};

/** ---------- Context: chia sẻ scrollY & headerHeight cho các tab ---------- **/
type HeaderScrollCtx = {
  onScroll: (e: any) => void;
  headerHeight: number;
};
const HeaderScrollContext = createContext<HeaderScrollCtx | null>(null);
export const useHeaderScroll = () => {
  const ctx = useContext(HeaderScrollContext);
  if (!ctx)
    throw new Error("useHeaderScroll must be used inside HeaderScrollContext");
  return ctx;
};

/** ---------- Tính toán kích thước header responsive ---------- **/
function useHeaderMetrics() {
  const insets = useSafeAreaInsets();
  const { width } = useWindowDimensions();

  const titleSize = Math.max(26, Math.min(34, width * 0.085));
  const paddingH = 16;
  const paddingTopExtra = 12;

  const headerLarge = insets.top + paddingTopExtra + HEADER_LARGE;
  const headerCompact = insets.top + paddingTopExtra + HEADER_COMPACT;

  return {
    insetsTop: insets.top,
    titleSize,
    paddingH,
    paddingTopExtra,
    headerLarge,
    headerCompact,
  };
}

export default function TabLayout() {
  const { headerLarge, headerCompact } = useHeaderMetrics();
  const pathname = usePathname();
  const isLibraryChild = pathname.startsWith("/library/");

  const effectiveHeaderHeight = isLibraryChild ? headerCompact : headerLarge;

  const scrollY = useRef(new Animated.Value(0)).current;
  const onScroll = (e: any) => {
    Animated.event([{ nativeEvent: { contentOffset: { y: scrollY } } }], {
      useNativeDriver: false,
    })(e);
  };

  return (
    <HeaderScrollContext.Provider
      value={{ onScroll, headerHeight: effectiveHeaderHeight }}
    >
      <View style={{ flex: 1 }}>
        <Tabs
          initialRouteName="home"
          screenOptions={{
            tabBarActiveTintColor: tintColorDark,
            tabBarInactiveTintColor: Colors.dark.tabIconDefault,
            headerShown: false,

            tabBarStyle: {
              position: "absolute",
              paddingTop: 10,
              borderTopWidth: 0,
              elevation: 0,
              backgroundColor: "transparent",
            },
            tabBarBackground: () => (
              <Animated.View style={StyleSheet.absoluteFill}>
                <BlurView
                  intensity={35}
                  tint="light"
                  style={StyleSheet.absoluteFill}
                />
              </Animated.View>
            ),
          }}
        >
          <Tabs.Screen
            name="home"
            options={{
              title: "Home",
              headerShown: false,
              tabBarIcon: ({ color }) => (
                <MaterialIcons
                  name="home"
                  size={28}
                  style={{ marginBottom: -3 }}
                  color={color}
                />
              ),
            }}
          />
          <Tabs.Screen
            name="explore"
            options={{
              title: "Trending",
              headerShown: false,
              tabBarIcon: ({ color }) => (
                <MaterialIcons
                  name="trending-up"
                  size={28}
                  style={{ marginBottom: -3 }}
                  color={color}
                />
              ),
            }}
          />
          <Tabs.Screen
            name="library"
            options={{
              title: "Library",
              headerShown: false,
              tabBarIcon: ({ color }) => (
                <MaterialIcons
                  name="video-library"
                  size={28}
                  style={{ marginBottom: -3 }}
                  color={color}
                />
              ),
            }}
          />
          <Tabs.Screen
            name="search"
            options={{
              title: "Search",
              headerShown: false,
              tabBarIcon: ({ color }) => (
                <MaterialIcons
                  name="search"
                  size={28}
                  style={{ marginBottom: -3 }}
                  color={color}
                />
              ),
            }}
          />
        </Tabs>
      </View>
    </HeaderScrollContext.Provider>
  );
}

const styles = StyleSheet.create({
  headerContainer: {
    position: "absolute",
    left: 0,
    right: 0,
    top: 0,
    zIndex: 100,
  },
  row: {
    flexDirection: "row",
    alignItems: "center",
    marginBottom: 10,
  },
  title: {
    flex: 1,
    fontWeight: "800",
  },
  loginBtn: {
    paddingHorizontal: 16,
    paddingVertical: 10,
    borderRadius: 10,
  },
  loginText: { fontWeight: "700", color: "#111" },
  userBadge: {
    flexDirection: "row",
    alignItems: "center",
    backgroundColor: "rgba(255,255,255,0.12)",
    borderRadius: 9999,
    padding: 2,
    gap: 6,
  },
  userAvatar: {
    width: 36,
    height: 36,
    borderRadius: 18,
    backgroundColor: "rgba(0,0,0,0.06)", // fallback background while loading
  },
  userText: { color: "#fff", maxWidth: 120 },
  searchBox: {
    flexDirection: "row",
    alignItems: "center",
    borderWidth: StyleSheet.hairlineWidth,
    borderColor: "rgba(255,255,255,0.35)",
    borderRadius: 12,
    paddingHorizontal: 12,
    paddingVertical: Platform.OS === "ios" ? 12 : 10,
  },
  searchPlaceholder: { color: "rgba(255,255,255,0.85)", fontSize: 16 },
});
