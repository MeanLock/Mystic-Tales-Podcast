import React, {
  useMemo,
  useRef,
  useState,
  useContext,
  createContext,
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
} from "react-native";
import { BlurView } from "expo-blur";
import MaterialIcons from "@expo/vector-icons/MaterialIcons";
import { useSafeAreaInsets } from "react-native-safe-area-context";

import Colors, { primaryThemeColor } from "@/src/constants/Colors";
import { useColorScheme } from "@/src/components/useColorScheme";
import { useClientOnlyValue } from "@/src/components/useClientOnlyValue";

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

  // title size: responsive theo chiều ngang
  const titleSize = Math.max(26, Math.min(34, width * 0.085));
  const titleLine = titleSize * 1.2;

  const verticalGap = 10; // khoảng cách giữa các hàng
  const paddingH = 16; // padding ngang
  const paddingTopExtra = 12; // khoảng extra dưới safe-area

  const headerBodyHeight = titleLine + verticalGap + verticalGap; // 2 hàng
  const headerHeight = insets.top + paddingTopExtra + headerBodyHeight;

  return {
    insetsTop: insets.top,
    titleSize,
    paddingH,
    paddingTopExtra,
    headerHeight,
  };
}

/** ---------- Header sticky đổi nền theo scroll ---------- **/
function StickyHeader({ scrollY }: { scrollY: Animated.Value }) {
  const pathname = usePathname();
  const router = useRouter();
  const { titleSize, paddingH, paddingTopExtra, headerHeight, insetsTop } =
    useHeaderMetrics();

  // const user = useSelector((s: RootState) => s.auth.user);
  const user = null;

  const title = useMemo(() => {
    if (pathname.startsWith("/explore")) return "Explore";
    if (pathname.startsWith("/library")) return "Library";
    if (pathname.startsWith("/search")) return "Search";
    return "Home";
  }, [pathname]);

  // ===== Scroll → progress 0..1
  const THRESH = 40;
  const END = THRESH + 60;
  const collapseProgress = scrollY.interpolate({
    inputRange: [0, THRESH, END],
    outputRange: [0, 0.5, 1],
    extrapolate: "clamp",
  });

  // ===== Background
  const blackOpacity = collapseProgress.interpolate({
    inputRange: [0, 1],
    outputRange: [1, 0],
  });
  const glassOpacity = collapseProgress.interpolate({
    inputRange: [0, 1],
    outputRange: [0, 0.85],
  });

  // ===== Expanded row (title-left + auth)
  const rowOpacity = collapseProgress.interpolate({
    inputRange: [0, 1],
    outputRange: [1, 0],
  });
  const rowTranslateY = collapseProgress.interpolate({
    inputRange: [0, 1],
    outputRange: [0, -12],
  });

  // ===== Center title (only when collapsed)
  const centerOpacity = collapseProgress;
  const centerTranslateY = collapseProgress.interpolate({
    inputRange: [0, 1],
    outputRange: [20, 0], // nhẹ nhàng trồi lên giữa
  });

  return (
    <View style={[styles.headerContainer, { height: headerHeight }]}>
      {/* nền đen lúc đầu */}
      <Animated.View
        style={[
          StyleSheet.absoluteFill,
          { backgroundColor: "black", opacity: blackOpacity },
        ]}
      />

      {/* nền glass sau khi scroll */}
      <Animated.View
        style={[StyleSheet.absoluteFill, { opacity: glassOpacity }]}
      >
        <BlurView
          intensity={80} // 👈 tăng lên 80 hoặc 90 để “nhòe” hơn
          tint="dark"
          style={StyleSheet.absoluteFill}
        />
        <View
          style={[
            StyleSheet.absoluteFill,
            {
              backgroundColor: "rgba(255,255,255,0.1)", // overlay trắng nhẹ
              borderBottomColor: "rgba(255,255,255,0.25)", // viền sáng hơn
              borderBottomWidth: StyleSheet.hairlineWidth,
            },
          ]}
        />
      </Animated.View>

      {/* content header */}
      <View
        style={{
          paddingTop: insetsTop + paddingTopExtra,
          paddingHorizontal: paddingH,
          height: headerHeight,
          justifyContent: "flex-start",
        }}
      >
        {/* Expanded row: Title trái + Login/Profile */}
        <Animated.View
          style={[
            styles.row,
            { opacity: rowOpacity, transform: [{ translateY: rowTranslateY }] },
          ]}
        >
          <Text style={[styles.title, { fontSize: titleSize }]}>{title}</Text>

          {user ? (
            <Pressable
              onPress={() => router.push("/(user)/profile/index")}
              style={styles.userBadge}
            >
              <MaterialIcons name="person" size={18} />
              <Text style={styles.userText} numberOfLines={1}>
                Profile
              </Text>
            </Pressable>
          ) : (
            <Pressable
              onPress={() => router.push("/(auth)/login")}
              style={styles.loginBtn}
            >
              <Text style={styles.loginText}>Login</Text>
            </Pressable>
          )}
        </Animated.View>

        {/* Collapsed title: giữa màn hình, không có nút */}
        <Animated.View
          pointerEvents="none"
          style={{
            position: "absolute",
            left: 0,
            right: 0,
            top: insetsTop + paddingTopExtra,
            alignItems: "center",
            opacity: centerOpacity,
            transform: [{ translateY: centerTranslateY }],
          }}
        >
          <Text
            style={{
              fontSize: Math.max(18, titleSize * 0.5),
              fontWeight: "800",
              color: "#fff",
            }}
            numberOfLines={1}
          >
            {title}
          </Text>
        </Animated.View>
      </View>
    </View>
  );
}

export default function TabLayout() {
  const colorScheme = useColorScheme();
  const { headerHeight } = useHeaderMetrics();
  const insets = useSafeAreaInsets();
  // Animated scrollY — các tab sẽ bắn sự kiện vào đây
  const scrollY = useRef(new Animated.Value(0)).current;

  // Context cung cấp onScroll cho các tab
  const onScroll = (e: any) => {
    Animated.event([{ nativeEvent: { contentOffset: { y: scrollY } } }], {
      useNativeDriver: false,
    })(e);
  };

  return (
    <HeaderScrollContext.Provider value={{ onScroll, headerHeight }}>
      <View style={{ flex: 1 }}>
        <StickyHeader scrollY={scrollY} />

        <Tabs
          screenOptions={{
            tabBarActiveTintColor: Colors[colorScheme ?? "light"].tint,
            headerShown: false,

            // Đẩy content xuống dưới đúng bằng headerHeight
            sceneStyle: {
              // paddingTop: headerHeight,
            },

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
              title: "Explore",
              headerShown: false,
              tabBarIcon: ({ color }) => (
                <MaterialIcons
                  name="explore"
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
    color: primaryThemeColor,
  },
  loginBtn: {
    paddingHorizontal: 16,
    paddingVertical: 10,
    backgroundColor: primaryThemeColor,
    borderRadius: 10,
  },
  loginText: { fontWeight: "700", color: "#111" },
  userBadge: {
    flexDirection: "row",
    alignItems: "center",
    paddingHorizontal: 12,
    paddingVertical: 8,
    backgroundColor: "rgba(255,255,255,0.12)",
    borderRadius: 10,
    gap: 6,
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
