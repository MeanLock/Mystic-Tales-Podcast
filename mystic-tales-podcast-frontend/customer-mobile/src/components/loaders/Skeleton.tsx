"use client";

import { useEffect, useRef } from "react";
import { View, Animated, StyleSheet } from "react-native";

interface SkeletonLoadingProps {
  width: number;
  height: number;
  borderRadius?: number;
}

export default function SkeletonLoading({
  width,
  height,
  borderRadius = 4,
}: SkeletonLoadingProps) {
  const shimmerAnimation = useRef(new Animated.Value(0)).current;

  useEffect(() => {
    const animation = Animated.loop(
      Animated.timing(shimmerAnimation, {
        toValue: 1,
        duration: 1500,
        useNativeDriver: true,
      })
    );
    animation.start();

    return () => animation.stop();
  }, [shimmerAnimation]);

  const translateX = shimmerAnimation.interpolate({
    inputRange: [0, 1],
    outputRange: [-width, width],
  });

  return (
    <View style={[styles.container, { width, height, borderRadius }]}>
      <Animated.View
        style={[
          styles.shimmer,
          {
            transform: [{ translateX }],
          },
        ]}
      />
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    backgroundColor: "#E1E9EE",
    overflow: "hidden",
    position: "relative",
  },
  shimmer: {
    width: "100%",
    height: "100%",
    backgroundColor: "rgba(255, 255, 255, 0.5)",
  },
});
