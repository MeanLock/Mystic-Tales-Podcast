import React, { useEffect, useRef } from "react";
import { View, Animated, Easing, StyleProp, ViewStyle } from "react-native";

type TinyEqualizerProps = {
  style?: StyleProp<ViewStyle>;
  color?: string;
};

const BAR_WIDTH = 2; // ~ bằng stroke icon nhỏ
const ICON_HEIGHT = 10; // target chiều cao
const MIN_HEIGHT = 2;

export const EqualizerVariant1: React.FC<TinyEqualizerProps> = ({
  style,
  color = "#AEE339",
}) => {
  // 3 thanh bar
  const bars = [
    useRef(new Animated.Value(0)).current,
    useRef(new Animated.Value(0)).current,
    useRef(new Animated.Value(0)).current,
  ];

  useEffect(() => {
    const loops: Animated.CompositeAnimation[] = [];
    const timeouts: NodeJS.Timeout[] = [];

    bars.forEach((value, index) => {
      const loop = Animated.loop(
        Animated.sequence([
          Animated.timing(value, {
            toValue: 1,
            duration: 320,
            easing: Easing.inOut(Easing.ease),
            useNativeDriver: false, // vì animate height
          }),
          Animated.timing(value, {
            toValue: 0,
            duration: 320,
            easing: Easing.inOut(Easing.ease),
            useNativeDriver: false,
          }),
        ])
      );

      loops.push(loop);

      // lệch nhịp mỗi bar 1 chút cho đẹp
      const timeout = setTimeout(() => loop.start(), index * 120);
      timeouts.push(timeout as any);
    });

    return () => {
      timeouts.forEach(clearTimeout);
      loops.forEach((l) => l.stop());
    };
  }, [bars]);

  return (
    <View
      style={[
        {
          flexDirection: "row",
          alignItems: "flex-end",
          height: ICON_HEIGHT,
        },
        style,
      ]}
    >
      {bars.map((value, index) => {
        const height = value.interpolate({
          inputRange: [0, 1],
          // cho mỗi bar max height khác chút nhìn dynamic hơn
          outputRange: [MIN_HEIGHT, ICON_HEIGHT - index * 3],
        });

        return (
          <Animated.View
            key={index}
            style={{
              width: BAR_WIDTH,
              marginHorizontal: 1,
              borderRadius: BAR_WIDTH,
              backgroundColor: color,
              height,
            }}
          />
        );
      })}
    </View>
  );
};
