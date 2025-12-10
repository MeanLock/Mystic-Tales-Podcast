import React, { useEffect } from "react";
import { FlatList, StyleSheet } from "react-native";
import { View } from "@/src/components/ui/View";

import { Text } from "@/src/components/ui/Text";
import { MaterialIcons } from "@expo/vector-icons";
import { Show } from "@/src/core/types/show.type";
import ShowCard from "./ShowCard";

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
  title: string;
  shows: Show[];
}

const ITEM_SPACING = 14;

const ShowCarousel = ({ variant, title, shows }: ShowCarouselProps) => {
  // 🔹 renderItem tuỳ theo variant
  const renderItem = ({ item }: { item: Show }) => {
    return <ShowCard key={item.Id} show={item} />;
  };
  return (
    <View className="gap-5 mb-10">
      {/* Title */}
      <View className="flex flex-row items-center justify-between w-full">
        <Text className="text-[#aee339] text-lg font-bold uppercase leading-none">
          {title}
        </Text>
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
