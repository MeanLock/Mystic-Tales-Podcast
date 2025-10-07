import { View, Text } from "@/src/components/Themed";

import EditScreenInfo from "@/src/components/EditScreenInfo";
import { useHeaderScroll } from "../_layout";
import { useBottomTabBarHeight } from "@react-navigation/bottom-tabs";
import { ScrollView } from "react-native";

export default function Librabry() {
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
      <Text className="text-white">Library Page</Text>

      <View style={{ height: tabBarHeight + 50 }}></View>
    </ScrollView>
  );
}
