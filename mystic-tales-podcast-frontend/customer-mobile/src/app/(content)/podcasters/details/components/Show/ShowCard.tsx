import AutoResolvingImage from "@/src/components/autoResolveImage/AutoResolvingImage";
import { Show } from "@/src/core/types/show.type";
import { useRouter } from "expo-router";
import { Image, Pressable, StyleSheet, Text, View } from "react-native";

interface ShowCardProps {
  // Define any props if needed in the future
  show: Show;
}

const ShowCard = ({ show }: ShowCardProps) => {
  const router = useRouter();

  return (
    <View className="flex flex-col items-start gap-2">
      <Pressable
        onPress={() => router.push(`/(content)/shows/details/${show.Id}`)}
        key={show.Id}
        style={styles.card}
      >
        <AutoResolvingImage
          FileKey={show.MainImageFileKey}
          type="PodcastPublicSource"
          key={show.Id}
          style={styles.image}
        />
      </Pressable>
      <Text numberOfLines={1} className="text-white w-5/6">
        {show.Name}
      </Text>
      <Text numberOfLines={1} className="text-[#D9D9D9] text-sm">
        {show.PodcastSubCategory ? show.PodcastSubCategory.Name : ""}
      </Text>
    </View>
  );
};
export default ShowCard;

const styles = StyleSheet.create({
  card: {
    elevation: 2,
    width: 142,
    height: 142,
    borderRadius: 8,
    position: "relative",
  },
  image: {
    width: "100%",
    resizeMode: "cover",
    aspectRatio: 1,
    borderRadius: 8,
  },
});
