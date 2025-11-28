import { View, Text } from "@/src/components/Themed";
import { useBottomTabBarHeight } from "@react-navigation/bottom-tabs";
import EditScreenInfo from "@/src/components/EditScreenInfo";
import { ScrollView } from "react-native";
import { useHeaderScroll } from "./_layout";

export default function Explore() {
  const { onScroll, headerHeight } = useHeaderScroll();
  const tabBarHeight = useBottomTabBarHeight();
  return (
    <ScrollView
      onScroll={onScroll}
      style={{ backgroundColor: "#000" }}
      contentContainerStyle={{
        paddingTop: headerHeight,
        paddingHorizontal: 10,
      }}
      scrollIndicatorInsets={{ top: headerHeight }}
      scrollEventThrottle={16} // Important for smooth animation
    >
      <View style={{ height: 20 }}></View>

      {/* Content gì thì để ở đây */}
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>
      <Text className="text-white">Explore Page</Text>

      <View style={{ height: tabBarHeight + 50 }}></View>
    </ScrollView>
  );
}
