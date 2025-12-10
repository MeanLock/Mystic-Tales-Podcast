import React, { useEffect } from "react";
import { FlatList, Pressable, StyleSheet } from "react-native";
import { View } from "@/src/components/ui/View";
import { Text } from "@/src/components/ui/Text";
import { MaterialIcons } from "@expo/vector-icons";
import { Show } from "@/src/core/types/show.type";
import ShowCard from "./ShowCard";
import { useDispatch } from "react-redux";
import { useRouter } from "expo-router";
import { setShowsData } from "@/src/features/show/showSlice";

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
  titleString?: string;
}

const ITEM_SPACING = 14;

const ShowCarousel = ({ variant, title, shows, titleString }: ShowCarouselProps) => {
  // 🔹 renderItem tuỳ theo variant
  const renderItem = ({ item }: { item: Show }) => {
    return <ShowCard key={item.Id} show={item} />;
  };
<<<<<<< HEAD

=======
  console.log("ShowCarousel shows:", shows);

  const dispatch = useDispatch();//thịnh
  const router = useRouter();
  const handleViewMoreShowFromFeed = () => {
    // Implement navigation or action to view more episodes from the show
    dispatch(
      setShowsData({
        shows: shows as Show[],
        title: `${titleString}`,
        from: "Feed",
      })
    );
    // Navigate to the episodes list page
    router.push(`/(content)/shows`);
  };
>>>>>>> d24514da2d90e35a15e7a4d1b3dcfeff93364c25
  return (
    <View className="gap-5 mb-10">
      {/* Title */}
      <Pressable
        onPress={() => handleViewMoreShowFromFeed()}
      >
        <View className="flex flex-row items-center justify-between w-full">
          {title}

          <View className="flex flex-row justify-center items-center gap-2">
            <Text className="text-white font-medium p-0">See more</Text>
            <MaterialIcons name="arrow-circle-right" size={16} color={"#fff"} />
          </View>
        </View>
      </Pressable>
      
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
