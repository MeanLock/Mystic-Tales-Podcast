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
      <View className="absolute inset-0 bg-black/10 bg-opacity-40 justify-end p-2">
        <Text className="text-white ">{show.Name}</Text>
      </View>
    </Pressable>
  );
};
export default ShowCard;

const styles = StyleSheet.create({
  card: {
    overflow: "hidden",
    elevation: 2,
    width: 142,
    height: 142,
    borderRadius: 8,
    position: "relative",
  },
  image: {
    width: "100%",
    height: "100%",
    resizeMode: "cover",
  },
});
