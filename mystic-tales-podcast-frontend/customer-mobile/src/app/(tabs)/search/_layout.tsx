import React, { useState, useCallback } from "react";
import { Modal, StyleSheet, TouchableOpacity } from "react-native";
import { Stack, useRouter, useFocusEffect } from "expo-router";
import { useSelector } from "react-redux";
import { RootState } from "@/src/store/store";
import { View } from "@/src/components/ui/View";
import { Text } from "@/src/components/ui/Text";
import { BlurView } from "expo-blur";
import { createContext, useContext, useRef } from "react";
import { Animated } from "react-native";

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

export default function SearchLayout() {
  const [showAuthModal, setShowAuthModal] = useState(false);

  // Create and provide the scroll position value
  const scrollY = useRef(new Animated.Value(0)).current;
  const headerHeight = 100;

  return (
    <ScrollContext.Provider value={{ scrollY, headerHeight }}>
      <Stack
        screenOptions={{
          headerStyle: {
            backgroundColor: "#000",
          },
          headerTintColor: "#AEE339",
          headerTitleStyle: {
            fontWeight: "bold",
          },
          headerShown: false,
        }}
      >
        <Stack.Screen
          name="index"
          options={{
            title: "Search",
          }}
        />
      </Stack>
    </ScrollContext.Provider>
  );
}

const styles = StyleSheet.create({
  blurContainer: {
    flex: 1,
    justifyContent: "center",
    alignItems: "center",
  },
  modalContainer: {
    flex: 1,
    justifyContent: "center",
    alignItems: "center",
    width: "100%",
  },
  modalContent: {
    backgroundColor: "#121212",
    borderRadius: 12,
    padding: 24,
    width: "85%",
    borderWidth: 1,
    borderColor: "#333",
    shadowColor: "#000",
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.25,
    shadowRadius: 3.84,
    elevation: 5,
  },
  title: {
    fontSize: 20,
    fontWeight: "bold",
    marginBottom: 12,
    color: "#AEE339",
    textAlign: "center",
  },
  message: {
    fontSize: 16,
    color: "#fff",
    marginBottom: 24,
    textAlign: "center",
    lineHeight: 22,
  },
  buttonContainer: {
    flexDirection: "row",
    justifyContent: "space-between",
    width: "100%",
  },
  button: {
    paddingVertical: 12,
    paddingHorizontal: 24,
    borderRadius: 8,
    minWidth: 120,
    alignItems: "center",
  },
  cancelButton: {
    backgroundColor: "#333",
    borderWidth: 1,
    borderColor: "#444",
  },
  loginButton: {
    backgroundColor: "#AEE339",
  },
  cancelButtonText: {
    color: "#fff",
    fontWeight: "600",
    fontSize: 16,
  },
  loginButtonText: {
    color: "#000",
    fontWeight: "600",
    fontSize: 16,
  },
});
