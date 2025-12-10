import React, { useState, useRef, createContext, useContext } from "react";
import { View } from "@/src/components/ui/View";
import { MaterialIcons } from "@expo/vector-icons";
import { BlurView } from "expo-blur";
import { Stack, useRouter } from "expo-router";
import {
  Animated,
  Platform,
  Pressable,
  StyleSheet,
  Text,
  Image,
  useWindowDimensions,
} from "react-native";
import { useSafeAreaInsets } from "react-native-safe-area-context";
import { useSelector } from "react-redux";
import { RootState } from "@/src/store/store";
import AutoResolvingImage from "@/src/components/autoResolveImage/AutoResolvingImage";

// Create a context to share the scrollY value
const ScrollContext = createContext<{
  scrollY: Animated.Value;
  headerHeight: number;
}>({
  scrollY: new Animated.Value(0),
  headerHeight: 100,
});

// Create a custom hook for header animations
export const useHeaderScroll = () => {
  const context = useContext(ScrollContext);
  if (!context) {
    throw new Error("useHeaderScroll must be used within a ScrollProvider");
  }
  return {
    ...context,
    onScroll: Animated.event(
      [{ nativeEvent: { contentOffset: { y: context.scrollY } } }],
      { useNativeDriver: false }
    ),
  };
};

const Header = ({ scrollY }: { scrollY: Animated.Value }) => {
  const insets = useSafeAreaInsets();
  const router = useRouter();
  const authState = useSelector((state: RootState) => state.auth);
  const { width } = useWindowDimensions();

  // Create animated values with more extreme values for clear transition
  const headerOpacity = scrollY.interpolate({
    inputRange: [0, 40],
    outputRange: [0, 1],
    extrapolate: "clamp",
  });

  const titleOpacity = scrollY.interpolate({
    inputRange: [0, 40],
    outputRange: [1, 0],
    extrapolate: "clamp",
  });

  const centeredTitleOpacity = scrollY.interpolate({
    inputRange: [0, 40],
    outputRange: [0, 1],
    extrapolate: "clamp",
  });

  const goToLogin = () => {
    router.push("/(auth)/login");
  };

  const goToProfile = () => {
    router.push("/(user)/profile");
  };

  return (
    <Animated.View style={[style.headerContainer, { paddingTop: insets.top }]}>
      {/* Background blur for scrolled state */}
      <Animated.View
        style={[
          StyleSheet.absoluteFill,
          { opacity: headerOpacity },
          style.headerBackground,
        ]}
      >
        <BlurView intensity={80} tint="dark" style={StyleSheet.absoluteFill} />
      </Animated.View>

      {/* Content container */}
      <View style={[style.headerContent, { paddingHorizontal: 10 }]}>
        {/* Initial header with title and user */}
        <Animated.View
          style={[
            style.titleContainer,
            {
              opacity: titleOpacity,
            },
          ]}
        >
          <Text style={[style.title, { color: "#AEE339" }]}>Home</Text>

          {/* User badge or login button */}
          {authState.user ? (
            <Pressable style={style.userBadge} onPress={goToProfile}>
              {/* <Image
                source={{
                  uri: authState.user.ImageUrl,
                }}
                style={style.userAvatar}
              /> */}
              <AutoResolvingImage
                FileKey={authState.user.MainImageFileKey}
                type="AccountPublicSource"
                style={style.userAvatar}
              />
            </Pressable>
          ) : (
            <Pressable
              style={[style.loginBtn, { backgroundColor: "#AEE339" }]}
              onPress={goToLogin}
            >
              <Text style={style.loginText}>Login</Text>
            </Pressable>
          )}
        </Animated.View>

        {/* Centered title for scrolled state */}
        <Animated.View
          style={[
            style.centeredTitleContainer,
            {
              opacity: centeredTitleOpacity,
              width,
            },
          ]}
        >
          <Text style={[style.centeredTitle, { color: "#AEE339" }]}>Home</Text>
        </Animated.View>
      </View>
    </Animated.View>
  );
};

export default function HomeLayout() {
  const scrollY = useRef(new Animated.Value(0)).current;
  const headerHeight = 100; // Adjust based on your design

  return (
    <ScrollContext.Provider value={{ scrollY, headerHeight }}>
      <Header scrollY={scrollY} />
      <Stack
        screenOptions={{
          headerShown: false,
          contentStyle: { backgroundColor: "#000" },
        }}
      >
        <Stack.Screen
          name="index"
          options={{
            headerShown: false,
          }}
        />
      </Stack>
    </ScrollContext.Provider>
  );
}

const style = StyleSheet.create({
  headerContainer: {
    position: "absolute",
    left: 0,
    right: 0,
    top: 0,
    zIndex: 100,
    height: 100, // Fixed height helps with animation
  },
  headerBackground: {
    backgroundColor: "rgba(0,0,0,0.5)", // Semi-transparent background
  },
  headerContent: {
    paddingBottom: 10,
    flex: 1, // Allow content to fill space
  },
  titleContainer: {
    flexDirection: "row",
    alignItems: "center",
    justifyContent: "space-between",
    marginBottom: 8,
    marginTop: 8,
    height: 40, // Fixed height for title container
  },
  centeredTitleContainer: {
    position: "absolute",
    top: 0,
    left: 0,
    right: 0,
    bottom: 0,
    alignItems: "center",
    justifyContent: "center",
  },
  title: {
    fontSize: 32,
    fontWeight: "800",
  },
  centeredTitle: {
    fontSize: 20,
    fontWeight: "800",
  },
  loginBtn: {
    paddingHorizontal: 16,
    paddingVertical: 10,
    borderRadius: 10,
  },
  loginText: {
    fontWeight: "700",
    color: "#000",
  },
  userBadge: {
    flexDirection: "row",
    alignItems: "center",
    backgroundColor: "rgba(255,255,255,0.12)",
    borderRadius: 9999,
    padding: 2,
    gap: 8,
  },
  userInfo: {
    maxWidth: 120,
  },
  userAvatar: {
    width: 36,
    height: 36,
    borderRadius: 18,
    backgroundColor: "rgba(0,0,0,0.06)", // fallback background while loading
  },
  userText: {
    color: "#fff",
  },
});
