import { View, Text } from "@/src/components/Themed";
import { useBottomTabBarHeight } from "@react-navigation/bottom-tabs";
import EditScreenInfo from "@/src/components/EditScreenInfo";
import { ScrollView } from "react-native";
import { useHeaderScroll } from "../_layout";

export default function Explore() {
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
      <Text className="text-white">Explore Page</Text>

      <View style={{ height: tabBarHeight + 50 }}></View>
    </ScrollView>
  );
}
