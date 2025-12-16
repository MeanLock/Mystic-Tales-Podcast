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

export default function LibraryLayout() {
  const router = useRouter();
  const authState = useSelector((state: RootState) => state.auth);
  const [showAuthModal, setShowAuthModal] = useState(false);

  // Create and provide the scroll position value
  const scrollY = useRef(new Animated.Value(0)).current;
  const headerHeight = 100;

  // Use useFocusEffect to check auth on focus
  useFocusEffect(
    useCallback(() => {
      if (!authState.user) {
        setShowAuthModal(true);
      } else {
        setShowAuthModal(false);
      }

      return () => {
        // Optional cleanup if needed
      };
    }, [authState.user])
  );

  const handleCancel = () => {
    setShowAuthModal(false);
    router.replace("/(tabs)/home");
  };

  const handleLogin = () => {
    setShowAuthModal(false);
    router.push("/(auth)/login");
  };

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
            title: "Library",
          }}
        />
      </Stack>

      {/* Custom Authentication Alert Modal */}
      <Modal
        visible={showAuthModal}
        transparent={true}
        animationType="fade"
        onRequestClose={handleCancel}
      >
        <BlurView intensity={80} tint="dark" style={styles.blurContainer}>
          <View style={styles.modalContainer}>
            <View style={styles.modalContent}>
              {/* Title */}
              <Text style={styles.title}>Authentication Required</Text>

              {/* Message */}
              <Text style={styles.message}>
                You need to be logged in to access this features.
              </Text>

              {/* Buttons */}
              <View style={styles.buttonContainer}>
                <TouchableOpacity
                  style={[styles.button, styles.cancelButton]}
                  onPress={handleCancel}
                >
                  <Text style={styles.cancelButtonText}>Cancel</Text>
                </TouchableOpacity>

                <TouchableOpacity
                  style={[styles.button, styles.loginButton]}
                  onPress={handleLogin}
                >
                  <Text style={styles.loginButtonText}>Login</Text>
                </TouchableOpacity>
              </View>
            </View>
          </View>
        </BlurView>
      </Modal>
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
