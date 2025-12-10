import React, { useEffect } from "react";
import { FlatList, StyleSheet } from "react-native";
import { View } from "@/src/components/ui/View";

import { Text } from "@/src/components/ui/Text";
import { MaterialIcons } from "@expo/vector-icons";
import { Channel } from "@/src/core/types/channel.type";
import ChannelCard from "./ChannelCard";

export type ChannelCardNormal = {
  Id: number;
  ImageUrl: string;
};

export type ChannelCardTop = {
  Id: number;
  ImageUrl: string;
  Top: number;
};

export interface ChannelCarouselProps {
  variant: "normal" | "top";
  title: React.ReactNode;
  channels: Channel[];
}

const ITEM_SPACING = 14;

const ChannelCarousel = ({ variant, title, channels }: ChannelCarouselProps) => {
  // 🔹 renderItem tuỳ theo variant
  const renderItem = ({ item }: { item: Channel }) => {
    return <ChannelCard key={item.Id} channel={item} />;
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
        data={channels}
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

export default ChannelCarousel;

const styles = StyleSheet.create({
  contentContainer: {
    paddingHorizontal: 0, // margin 2 bên
  },
});
