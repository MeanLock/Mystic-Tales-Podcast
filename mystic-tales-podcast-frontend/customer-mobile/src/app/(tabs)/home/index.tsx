import React from "react";
import { Animated, Text, ScrollView } from "react-native";
import { useHeaderScroll } from "../_layout";
import { useBottomTabBarHeight } from "@react-navigation/bottom-tabs";
import { View } from "@/src/components/ui/View";
import MixxingText from "@/src/components/ui/MixxingText";

export default function Home() {
  const { onScroll, headerHeight } = useHeaderScroll();
  const tabBarHeight = useBottomTabBarHeight();
  return (
    <ScrollView
      onScroll={onScroll}
      contentContainerStyle={{ paddingTop: headerHeight }}
      scrollIndicatorInsets={{ top: headerHeight }}
      scrollEventThrottle={16}
    >
      {/* Content gì thì để ở đây */}
      
      <View style={{ height: tabBarHeight + 50 }}></View>
    </ScrollView>
  );
}
