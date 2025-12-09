import React, { useEffect } from "react";
import { FlatList, StyleSheet } from "react-native";
import { View } from "@/src/components/ui/View";

// 🔸 Card components (demo — bạn thay bằng component thật của bạn)
import ShowCardVariant1 from "./ShowCardVariant1";
import ShowCardVariant2 from "./ShowCardVariant2";
import { Text } from "@/src/components/ui/Text";
import { MaterialIcons } from "@expo/vector-icons";
import { Show } from "@/src/core/types/show.type";

export type ShowCardNormal = {
  Id: number;
  ImageUrl: string;
};

export type ShowCardTop = {
  Id: number;
  ImageUrl: string;
  Top: number;
};

export interface ShowCarouselProps {
  variant: "normal" | "top";
  title: React.ReactNode;
  shows: Show[];
}

const ITEM_SPACING = 14;

const ShowCarousel = ({ variant, title, shows }: ShowCarouselProps) => {
  // 🔹 renderItem tuỳ theo variant
  const renderItem = ({ item }: { item: Show }) => {
    return <ShowCardVariant1 key={item.Id} show={item} />;
  };

  return (
    <View className="gap-5 mb-10">
      {/* Title */}
      <View className="flex flex-row items-center justify-between w-full">
        {title}

        <View className="flex flex-row justify-center items-center gap-2">
          <Text className="text-white font-medium p-0">See more</Text>
          <MaterialIcons name="arrow-circle-right" size={16} color={"#fff"} />
        </View>
      </View>

      {/* Horizontal FlatList */}
      <FlatList
        data={shows}
        renderItem={renderItem}
        keyExtractor={(item) => item.Id.toString()}
        horizontal
        showsHorizontalScrollIndicator={false}
        contentContainerStyle={styles.contentContainer}
        ItemSeparatorComponent={() => <View style={{ width: ITEM_SPACING }} />}
        // ⚙️ để FlatList chiếm full width component cha
        style={{ width: "100%" }}
      />
    </View>
  );
};

export default ShowCarousel;

const styles = StyleSheet.create({
  contentContainer: {
    paddingHorizontal: 0, // margin 2 bên
  },
});
