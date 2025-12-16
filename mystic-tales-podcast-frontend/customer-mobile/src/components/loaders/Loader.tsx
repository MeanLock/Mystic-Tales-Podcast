"use client";

import { useEffect, useRef } from "react";
import { View, StyleSheet, Animated } from "react-native";

const Loader = () => {
  const bounceAnim = useRef(new Animated.Value(0)).current;
  const flickerAnim = useRef(new Animated.Value(0)).current;
  const eyeAnim = useRef(new Animated.Value(0)).current;
  const shadowAnim = useRef(new Animated.Value(0)).current;

  useEffect(() => {
    // Bounce animation (up and down)
    Animated.loop(
      Animated.sequence([
        Animated.timing(bounceAnim, {
          toValue: 0,
          duration: 250,
          useNativeDriver: true,
        }),
        Animated.timing(bounceAnim, {
          toValue: -3.57,
          duration: 250,
          useNativeDriver: true,
        }),
      ])
    ).start();

    // Flicker animation (for bottom edges)
    Animated.loop(
      Animated.sequence([
        Animated.timing(flickerAnim, {
          toValue: 0,
          duration: 250,
          useNativeDriver: false,
        }),
        Animated.timing(flickerAnim, {
          toValue: 1,
          duration: 250,
          useNativeDriver: false,
        }),
      ])
    ).start();

    // Eye movement animation
    Animated.loop(
      Animated.sequence([
        Animated.timing(eyeAnim, {
          toValue: 0,
          duration: 1500,
          useNativeDriver: true,
        }),
        Animated.timing(eyeAnim, {
          toValue: 3.57,
          duration: 1500,
          useNativeDriver: true,
        }),
      ])
    ).start();

    // Shadow animation
    Animated.loop(
      Animated.sequence([
        Animated.timing(shadowAnim, {
          toValue: 0.5,
          duration: 250,
          useNativeDriver: false,
        }),
        Animated.timing(shadowAnim, {
          toValue: 0.2,
          duration: 250,
          useNativeDriver: false,
        }),
      ])
    ).start();
  }, []);

  const GHOST_COLOR = "#AEE339";
  const PIXEL = 3.571428571; // 50px / 14 columns = 3.571428571px per grid cell

  const flickerColor0 = flickerAnim.interpolate({
    inputRange: [0, 1],
    outputRange: [GHOST_COLOR, "transparent"],
  });

  const flickerColor1 = flickerAnim.interpolate({
    inputRange: [0, 1],
    outputRange: ["transparent", GHOST_COLOR],
  });

  return (
    <View style={styles.container}>
      <Animated.View
        style={[
          styles.ghost,
          {
            transform: [{ translateY: bounceAnim }],
          },
        ]}
      >
        {/* Row 0: top0 spans columns 5-8 (4 cells) */}
        <View
          style={[
            styles.pixel,
            {
              top: 0,
              left: 5 * PIXEL,
              width: 4 * PIXEL,
              height: PIXEL,
              backgroundColor: GHOST_COLOR,
            },
          ]}
        />

        {/* Row 1: top1 spans columns 3-10 (8 cells) */}
        <View
          style={[
            styles.pixel,
            {
              top: 1 * PIXEL,
              left: 3 * PIXEL,
              width: 8 * PIXEL,
              height: PIXEL,
              backgroundColor: GHOST_COLOR,
            },
          ]}
        />

        {/* Row 2: top2 spans columns 2-11 (10 cells) */}
        <View
          style={[
            styles.pixel,
            {
              top: 2 * PIXEL,
              left: 2 * PIXEL,
              width: 10 * PIXEL,
              height: PIXEL,
              backgroundColor: GHOST_COLOR,
            },
          ]}
        />

        {/* Rows 3-5: top3 spans columns 1-12 (12 cells each) */}
        <View
          style={[
            styles.pixel,
            {
              top: 3 * PIXEL,
              left: 1 * PIXEL,
              width: 12 * PIXEL,
              height: PIXEL,
              backgroundColor: GHOST_COLOR,
            },
          ]}
        />
        <View
          style={[
            styles.pixel,
            {
              top: 4 * PIXEL,
              left: 1 * PIXEL,
              width: 12 * PIXEL,
              height: PIXEL,
              backgroundColor: GHOST_COLOR,
            },
          ]}
        />
        <View
          style={[
            styles.pixel,
            {
              top: 5 * PIXEL,
              left: 1 * PIXEL,
              width: 12 * PIXEL,
              height: PIXEL,
              backgroundColor: GHOST_COLOR,
            },
          ]}
        />

        {/* Rows 6-11: top4 spans all 14 columns (full width) */}
        <View
          style={[
            styles.pixel,
            {
              top: 6 * PIXEL,
              left: 0,
              width: 14 * PIXEL,
              height: PIXEL,
              backgroundColor: GHOST_COLOR,
            },
          ]}
        />
        <View
          style={[
            styles.pixel,
            {
              top: 7 * PIXEL,
              left: 0,
              width: 14 * PIXEL,
              height: PIXEL,
              backgroundColor: GHOST_COLOR,
            },
          ]}
        />
        <View
          style={[
            styles.pixel,
            {
              top: 8 * PIXEL,
              left: 0,
              width: 14 * PIXEL,
              height: PIXEL,
              backgroundColor: GHOST_COLOR,
            },
          ]}
        />
        <View
          style={[
            styles.pixel,
            {
              top: 9 * PIXEL,
              left: 0,
              width: 14 * PIXEL,
              height: PIXEL,
              backgroundColor: GHOST_COLOR,
            },
          ]}
        />
        <View
          style={[
            styles.pixel,
            {
              top: 10 * PIXEL,
              left: 0,
              width: 14 * PIXEL,
              height: PIXEL,
              backgroundColor: GHOST_COLOR,
            },
          ]}
        />
        <View
          style={[
            styles.pixel,
            {
              top: 11 * PIXEL,
              left: 0,
              width: 14 * PIXEL,
              height: PIXEL,
              backgroundColor: GHOST_COLOR,
            },
          ]}
        />

        {/* Row 12: Static bottom parts (st0-st5) */}
        <View
          style={[
            styles.pixel,
            {
              top: 12 * PIXEL,
              left: 0,
              width: 2 * PIXEL,
              height: PIXEL,
              backgroundColor: GHOST_COLOR,
            },
          ]}
        />
        <View
          style={[
            styles.pixel,
            {
              top: 12 * PIXEL,
              left: 3 * PIXEL,
              width: 1 * PIXEL,
              height: PIXEL,
              backgroundColor: GHOST_COLOR,
            },
          ]}
        />
        <View
          style={[
            styles.pixel,
            {
              top: 12 * PIXEL,
              left: 5 * PIXEL,
              width: 1 * PIXEL,
              height: PIXEL,
              backgroundColor: GHOST_COLOR,
            },
          ]}
        />
        <View
          style={[
            styles.pixel,
            {
              top: 12 * PIXEL,
              left: 6 * PIXEL,
              width: 2 * PIXEL,
              height: PIXEL,
              backgroundColor: GHOST_COLOR,
            },
          ]}
        />
        <View
          style={[
            styles.pixel,
            {
              top: 12 * PIXEL,
              left: 8 * PIXEL,
              width: 1 * PIXEL,
              height: PIXEL,
              backgroundColor: GHOST_COLOR,
            },
          ]}
        />
        <View
          style={[
            styles.pixel,
            {
              top: 12 * PIXEL,
              left: 10 * PIXEL,
              width: 1 * PIXEL,
              height: PIXEL,
              backgroundColor: GHOST_COLOR,
            },
          ]}
        />
        <View
          style={[
            styles.pixel,
            {
              top: 12 * PIXEL,
              left: 12 * PIXEL,
              width: 2 * PIXEL,
              height: PIXEL,
              backgroundColor: GHOST_COLOR,
            },
          ]}
        />

        {/* Row 12: Flickering parts (flicker1 - transparent first, then visible) */}
        <Animated.View
          style={[
            styles.pixel,
            {
              top: 12 * PIXEL,
              left: 2 * PIXEL,
              width: 1 * PIXEL,
              height: PIXEL,
              backgroundColor: flickerColor1,
            },
          ]}
        />
        <Animated.View
          style={[
            styles.pixel,
            {
              top: 12 * PIXEL,
              left: 4 * PIXEL,
              width: 1 * PIXEL,
              height: PIXEL,
              backgroundColor: flickerColor1,
            },
          ]}
        />
        <Animated.View
          style={[
            styles.pixel,
            {
              top: 12 * PIXEL,
              left: 9 * PIXEL,
              width: 1 * PIXEL,
              height: PIXEL,
              backgroundColor: flickerColor1,
            },
          ]}
        />
        <Animated.View
          style={[
            styles.pixel,
            {
              top: 12 * PIXEL,
              left: 11 * PIXEL,
              width: 1 * PIXEL,
              height: PIXEL,
              backgroundColor: flickerColor1,
            },
          ]}
        />

        {/* Row 13: Flickering parts (flicker0 - visible first, then transparent) */}
        <Animated.View
          style={[
            styles.pixel,
            {
              top: 13 * PIXEL,
              left: 0,
              width: 1 * PIXEL,
              height: PIXEL,
              backgroundColor: flickerColor0,
            },
          ]}
        />
        <Animated.View
          style={[
            styles.pixel,
            {
              top: 13 * PIXEL,
              left: 1 * PIXEL,
              width: 1 * PIXEL,
              height: PIXEL,
              backgroundColor: flickerColor0,
            },
          ]}
        />
        <Animated.View
          style={[
            styles.pixel,
            {
              top: 13 * PIXEL,
              left: 5 * PIXEL,
              width: 1 * PIXEL,
              height: PIXEL,
              backgroundColor: flickerColor0,
            },
          ]}
        />
        <Animated.View
          style={[
            styles.pixel,
            {
              top: 13 * PIXEL,
              left: 9 * PIXEL,
              width: 1 * PIXEL,
              height: PIXEL,
              backgroundColor: flickerColor0,
            },
          ]}
        />
        <Animated.View
          style={[
            styles.pixel,
            {
              top: 13 * PIXEL,
              left: 12 * PIXEL,
              width: 1 * PIXEL,
              height: PIXEL,
              backgroundColor: flickerColor0,
            },
          ]}
        />
        <Animated.View
          style={[
            styles.pixel,
            {
              top: 13 * PIXEL,
              left: 13 * PIXEL,
              width: 1 * PIXEL,
              height: PIXEL,
              backgroundColor: flickerColor0,
            },
          ]}
        />

        {/* Row 13: Flickering parts (flicker1 - transparent first, then visible) */}
        <Animated.View
          style={[
            styles.pixel,
            {
              top: 13 * PIXEL,
              left: 2 * PIXEL,
              width: 1 * PIXEL,
              height: PIXEL,
              backgroundColor: flickerColor1,
            },
          ]}
        />
        <Animated.View
          style={[
            styles.pixel,
            {
              top: 13 * PIXEL,
              left: 3 * PIXEL,
              width: 1 * PIXEL,
              height: PIXEL,
              backgroundColor: flickerColor1,
            },
          ]}
        />
        <Animated.View
          style={[
            styles.pixel,
            {
              top: 13 * PIXEL,
              left: 4 * PIXEL,
              width: 1 * PIXEL,
              height: PIXEL,
              backgroundColor: flickerColor1,
            },
          ]}
        />
        <Animated.View
          style={[
            styles.pixel,
            {
              top: 13 * PIXEL,
              left: 6 * PIXEL,
              width: 2 * PIXEL,
              height: PIXEL,
              backgroundColor: flickerColor1,
            },
          ]}
        />
        <Animated.View
          style={[
            styles.pixel,
            {
              top: 13 * PIXEL,
              left: 8 * PIXEL,
              width: 1 * PIXEL,
              height: PIXEL,
              backgroundColor: flickerColor1,
            },
          ]}
        />
        <Animated.View
          style={[
            styles.pixel,
            {
              top: 13 * PIXEL,
              left: 10 * PIXEL,
              width: 1 * PIXEL,
              height: PIXEL,
              backgroundColor: flickerColor1,
            },
          ]}
        />
        <Animated.View
          style={[
            styles.pixel,
            {
              top: 13 * PIXEL,
              left: 11 * PIXEL,
              width: 1 * PIXEL,
              height: PIXEL,
              backgroundColor: flickerColor1,
            },
          ]}
        />

        {/* Left Eye - white cross shape (::before vertical, ::after horizontal) */}
        <View
          style={[
            styles.pixel,
            {
              top: 3 * PIXEL,
              left: 1 * PIXEL,
              width: 2 * PIXEL,
              height: 5 * PIXEL,
              backgroundColor: "white",
            },
          ]}
        />
        <View
          style={[
            styles.pixel,
            {
              top: 4 * PIXEL,
              left: 1 * PIXEL,
              width: 4 * PIXEL,
              height: 3 * PIXEL,
              backgroundColor: "white",
            },
          ]}
        />

        {/* Right Eye - white cross shape */}
        <View
          style={[
            styles.pixel,
            {
              top: 3 * PIXEL,
              left: 9 * PIXEL,
              width: 2 * PIXEL,
              height: 5 * PIXEL,
              backgroundColor: "white",
            },
          ]}
        />
        <View
          style={[
            styles.pixel,
            {
              top: 4 * PIXEL,
              left: 7 * PIXEL,
              width: 4 * PIXEL,
              height: 3 * PIXEL,
              backgroundColor: "white",
            },
          ]}
        />

        {/* Left Pupil - blue square that moves */}
        <Animated.View
          style={[
            styles.pixel,
            {
              top: 5 * PIXEL,
              left: 1 * PIXEL,
              width: 2 * PIXEL,
              height: 2 * PIXEL,
              backgroundColor: "blue",
              transform: [{ translateX: eyeAnim }],
            },
          ]}
        />

        {/* Right Pupil - blue square that moves */}
        <Animated.View
          style={[
            styles.pixel,
            {
              top: 5 * PIXEL,
              left: 9 * PIXEL,
              width: 2 * PIXEL,
              height: 2 * PIXEL,
              backgroundColor: "blue",
              transform: [{ translateX: eyeAnim }],
            },
          ]}
        />
      </Animated.View>

      {/* Shadow */}
      <Animated.View
        style={[
          styles.shadow,
          {
            opacity: shadowAnim,
          },
        ]}
      />
    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    position: "relative",
    width: 50,
    height: 50,
    justifyContent: "center",
    alignItems: "center",
  },
  ghost: {
    position: "relative",
    width: 50,
    height: 50,
  },
  pixel: {
    position: "absolute",
  },
  shadow: {
    position: "absolute",
    width: 50,
    height: 50,
    backgroundColor: "black",
    borderRadius: 25,
    top: 40,
    transform: [{ scaleY: 0.2 }],
  },
});

export default Loader;
