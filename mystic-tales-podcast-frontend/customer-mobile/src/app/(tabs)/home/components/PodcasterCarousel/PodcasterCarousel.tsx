import React from "react";
import { FlatList, StyleSheet, View } from "react-native";
import { Text } from "@/src/components/ui/Text";
import { MaterialIcons } from "@expo/vector-icons";
import PodcasterCard from "./PodcasterCard";

interface PodcasterCarouselProps {
  title: React.ReactNode;
  podcasters: {
    Id: number;
    FullName: string;
    Email: string;
    MainImageFileKey: string;
  }[];
}

const ITEM_SPACING = 20;

const PodcasterCarousel = ({ title, podcasters }: PodcasterCarouselProps) => {
  return (
    <View className="gap-5 mb-10">
      {/* Title with See More */}
      <View className="flex flex-row items-center justify-between w-full">
        {title}

        <View className="flex flex-row justify-center items-center gap-2">
          <Text className="text-white font-medium p-0">See more</Text>
          <MaterialIcons name="arrow-circle-right" size={16} color={"#fff"} />
        </View>
      </View>

      {/* Podcasters Horizontal FlatList */}
      <FlatList
        data={podcasters}
        renderItem={({ item }) => <PodcasterCard podcaster={item} />}
        keyExtractor={(item) => item.Id.toString()}
        horizontal
        showsHorizontalScrollIndicator={false}
        contentContainerStyle={styles.contentContainer}
        ItemSeparatorComponent={() => <View style={{ width: ITEM_SPACING }} />}
        style={{ width: "100%" }}
      />
    </View>
  );
};

export default PodcasterCarousel;

const styles = StyleSheet.create({
  contentContainer: {
    paddingHorizontal: 0,
  },
});
