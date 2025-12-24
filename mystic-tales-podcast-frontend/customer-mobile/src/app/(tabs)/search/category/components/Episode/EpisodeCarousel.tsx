import React, { useMemo } from "react";
import { EpisodeCardWithImageProps } from "@/src/types/episode";
import { FlatList, StyleSheet, View, Dimensions } from "react-native";
import EpisodeCard from "./EpisodeCard";
import { Text } from "@/src/components/ui/Text";
import { MaterialIcons } from "@expo/vector-icons";
import { Episode } from "@/src/core/types/episode.type";

interface EpisodeCarouselProps {
  title: React.ReactNode;
  episodes: Episode[];
}

const ITEM_SPACING = 14; // Horizontal spacing between columns
const ITEM_HEIGHT = 80; // Height of each episode card
const ITEM_VERTICAL_SPACING = 14; // Vertical spacing between cards in a column
const itemsPerColumn = 3; // Number of items per column

const EpisodeCarousel = ({ title, episodes }: EpisodeCarouselProps) => {
  // Organize episodes into groups for the grid layout
  const gridData = useMemo(() => {
    // Group episodes into columns (3 items per column)
    const columns = [];
    for (let i = 0; i < episodes.length; i += itemsPerColumn) {
      const column = episodes.slice(i, i + itemsPerColumn);
      columns.push(column);
    }
    return columns;
  }, [episodes]);

  const renderGridColumn = ({
    item,
  }: {
    item: Episode[];
  }) => (
    <View style={styles.gridColumn}>
      {item.map((episode, index) => (
        <View
          key={episode.Id}
          style={[
            styles.episodeContainer,
            // Remove margin from last item
            index === item.length - 1 ? null : styles.episodeMargin,
          ]}
        >
          <EpisodeCard episode={episode} />
        </View>
      ))}
    </View>
  );

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

      {/* Episodes Grid FlatList */}
      <FlatList
        data={gridData}
        renderItem={renderGridColumn}
        keyExtractor={(_, index) => `grid-column-${index}`}
        horizontal
        showsHorizontalScrollIndicator={false}
        contentContainerStyle={styles.contentContainer}
        ItemSeparatorComponent={() => <View style={{ width: ITEM_SPACING }} />}
        style={{ width: "100%" }}
      />
    </View>
  );
};

export default EpisodeCarousel;

const styles = StyleSheet.create({
  contentContainer: {
    paddingHorizontal: 0,
  },
  gridColumn: {
    width: 350, // Width of each column
    // Calculate height based on items and spacing: (itemHeight * itemCount) + (spacing * (itemCount-1))
    height:
      ITEM_HEIGHT * itemsPerColumn +
      ITEM_VERTICAL_SPACING * (itemsPerColumn - 1),
  },
  episodeContainer: {
    height: ITEM_HEIGHT,
  },
  episodeMargin: {
    marginBottom: ITEM_VERTICAL_SPACING,
  },
});
