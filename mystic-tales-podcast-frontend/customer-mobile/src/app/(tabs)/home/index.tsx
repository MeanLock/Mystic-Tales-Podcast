import React from "react";
import { Animated, View, Text } from "react-native";
import { useHeaderScroll } from "../_layout";

export default function Home() {
  const { onScroll, headerHeight } = useHeaderScroll();

  return (
    <Animated.FlatList
      data={Array.from({ length: 30 }, (_, i) => i + 1)}
      keyExtractor={(i) => i.toString()}
      renderItem={({ item }) => (
        <View
          style={{
            height: 120,
            backgroundColor: "#222",
            borderRadius: 12,
            margin: 12,
            justifyContent: "center",
            alignItems: "center",
          }}
        >
          <Text style={{ color: "#fff" }}>Item {item}</Text>
        </View>
      )}
      // Không padding đáy toàn bộ → chỉ chèn spacer ở cuối nếu cần
      ListFooterComponent={<View style={{ height: 80 }} />}
      contentContainerStyle={{ paddingTop: headerHeight }}
      scrollIndicatorInsets={{ top: headerHeight }}
      scrollEventThrottle={16}
      onScroll={onScroll}
    />
  );
}
