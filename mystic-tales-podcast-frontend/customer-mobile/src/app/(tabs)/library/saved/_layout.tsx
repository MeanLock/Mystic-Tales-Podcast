import { MaterialIcons } from "@expo/vector-icons";
import { BlurView } from "expo-blur";
import { Stack } from "expo-router";
import {
  Pressable,
  StyleSheet,
  Text,
  View,
  Animated,
  ActivityIndicator,
  Platform,
} from "react-native";
import { useHeaderScroll } from "../_layout";
import { useGetSavedEpisodesQuery } from "@/src/core/services/episode/episode.service";

function HeaderGlass() {
  const { scrollY } = useHeaderScroll();
  const opacity = scrollY.interpolate({
    inputRange: [0, 40],
    outputRange: [0, 1],
    extrapolate: "clamp",
  });

  return (
    <Animated.View style={[StyleSheet.absoluteFill, { opacity }]}>
      <BlurView intensity={80} tint="dark" style={StyleSheet.absoluteFill} />
      <View
        style={{
          position: "absolute",
          left: 0,
          right: 0,
          bottom: 0,
          height: StyleSheet.hairlineWidth,
          backgroundColor: "rgba(255,255,255,0.12)",
        }}
      />
    </Animated.View>
  );
}

export default function LibrarySavedLayout() {
  return (
    <>
      <Stack
        screenOptions={{
          headerTransparent: true, // <-- quan trọng
          headerBackground: () => <HeaderGlass />, // <-- glass nền
          headerTitleStyle: { fontWeight: "700", color: "#AEE339" },
          headerTintColor: "#AEE339",
        }}
      >
        <Stack.Screen
          name="index"
          options={({ navigation }) => ({
            headerShown: true,
            headerTitle: "Saved Episodes",
            headerLeft: () => (
              <Pressable
                onPress={() => navigation.goBack()}
                style={{
                  flexDirection: "row",
                  alignItems: "center",
                  gap: 2,
                  paddingHorizontal: 4,
                }}
                hitSlop={10}
              >
                <MaterialIcons
                  name="arrow-back-ios"
                  size={18}
                  color="#AEE339"
                />
                {Platform.OS === "ios" && (
                  <Text
                    style={{
                      color: "#AEE339",
                      fontSize: 16,
                      fontWeight: "600",
                    }}
                  >
                    Library
                  </Text>
                )}
              </Pressable>
            ),
            headerRight: () => (
              <Pressable
                hitSlop={10}
                style={{
                  padding: 4,
                  borderRadius: 9999,
                  backgroundColor: "#333",
                }}
              >
                <MaterialIcons name="more-horiz" size={22} color="#AEE339" />
              </Pressable>
            ),
          })}
        />
      </Stack>
    </>
  );
}
