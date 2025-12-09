// /(tabs)/library/shows/_layout.tsx
import React, { useState } from "react";
import { Stack } from "expo-router";
import {
  Pressable,
  View,
  ActionSheetIOS,
  Modal,
  TouchableOpacity,
  Platform,
  StyleSheet,
  Animated,
} from "react-native";
import { Text } from "@/src/components/ui/Text";
import { MaterialIcons } from "@expo/vector-icons";
import { BlurView } from "expo-blur";
import { useHeaderScroll } from "../_layout"; // <- import hook từ layout cha

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

export default function LibraryPodcastersLayout() {
  const [menuOpen, setMenuOpen] = useState(false);

  const openMenu = () => {
    if (Platform.OS === "ios") {
      ActionSheetIOS.showActionSheetWithOptions(
        {
          title: "Actions",
          options: [
            "Cancel",
            "Sort by name",
            "Sort by date",
            "Filter downloaded",
            "Clear filters",
          ],
          cancelButtonIndex: 0,
          userInterfaceStyle: "dark",
        },
        (i) => {
          /* handle */
        }
      );
    } else setMenuOpen(true);
  };

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
            headerTitle: Platform.OS === "ios" ? "Followed Podcasters" : "",
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
                <Text
                  style={{ color: "#AEE339", fontSize: 16, fontWeight: "600" }}
                >
                  Library
                </Text>
              </Pressable>
            ),
          })}
        />
      </Stack>

      {/* Android sheet đơn giản */}
      <Modal
        visible={menuOpen && Platform.OS === "android"}
        transparent
        animationType="fade"
        onRequestClose={() => setMenuOpen(false)}
      >
        <Pressable style={styles.backdrop} onPress={() => setMenuOpen(false)}>
          <View style={styles.sheet}>
            {[
              "Sort by name",
              "Sort by date",
              "Filter downloaded",
              "Clear filters",
            ].map((label) => (
              <TouchableOpacity
                key={label}
                onPress={() => setMenuOpen(false)}
                style={styles.item}
              >
                <Text style={{ color: "#fff", fontSize: 16 }}>{label}</Text>
              </TouchableOpacity>
            ))}
          </View>
        </Pressable>
      </Modal>
    </>
  );
}

const styles = StyleSheet.create({
  backdrop: {
    flex: 1,
    backgroundColor: "rgba(0,0,0,0.4)",
    justifyContent: "flex-end",
  },
  sheet: {
    backgroundColor: "#1a1a1a",
    paddingVertical: 8,
    borderTopLeftRadius: 16,
    borderTopRightRadius: 16,
  },
  item: {
    paddingVertical: 14,
    paddingHorizontal: 20,
    borderBottomWidth: StyleSheet.hairlineWidth,
    borderBottomColor: "#2a2a2a",
  },
});
