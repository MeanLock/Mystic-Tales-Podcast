import { RootState } from "@/src/store/store";
import { Animated, Dimensions, Text, View } from "react-native";
import { useSelector } from "react-redux";
import { StyleSheet } from "react-native";

export default function EpisodesScreen() {
  const episodeData = useSelector((state: RootState) => state.episode);

  // HOOKS
  const windowWidth = Dimensions.get("window").width;
  const itemWidth = (windowWidth - 16 * 2 - 10) / 2; // Calculate item width based on screen width, padding, and gap

  if (episodeData.episodes.length === 0 || !episodeData.title) {
    return (
      <View>
        <Text>No episodes available.</Text>
      </View>
    );
  }

  return (
    <Animated.ScrollView
      style={styles.container}
      scrollEventThrottle={16}
      contentContainerStyle={{
        paddingTop: 100,
        paddingHorizontal: 16,
        paddingBottom: 40,
      }}
    >
      <View className="w-full flex flex-row items-center py-5 border-b-[0.5px] border-b-[#D9D9D9] mb-10">
        <Text className="text-white font-bold text-3xl">
          {episodeData.title}
        </Text>
      </View>

      {/* Grid layout using flexDirection: row and flexWrap: wrap */}
      {/* <View style={styles.gridColTwoContainer}>
       
       
      </View> */}
      <View className="w-full flex flex-col items-start gap-5">
        {episodeData?.episodes.map((item, index) => (
          //   <EpisodeCard key={item.Id} episode={item} />
          <View key={item.Id} className="w-full">
            <Text className="text-white">{item.Name}</Text>
          </View>
        ))}
      </View>
      <View style={{ height: 50 }}></View>
    </Animated.ScrollView>
  );
}

const styles = StyleSheet.create({
  container: {
    backgroundColor: "#000",
  },
});
