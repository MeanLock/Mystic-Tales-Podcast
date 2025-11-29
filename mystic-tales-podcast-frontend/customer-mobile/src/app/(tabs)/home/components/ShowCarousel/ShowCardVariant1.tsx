import AutoResolvingImage from "@/src/components/autoResolveImage/AutoResolvingImage";
import { Show } from "@/src/core/types/show.type";
import { useRouter } from "expo-router";
import { Image, Pressable, StyleSheet } from "react-native";

interface ShowCardVariant1Props {
  // Define any props if needed in the future
  show: Show;
}

const ShowCardVariant1 = ({ show }: ShowCardVariant1Props) => {
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
    </Pressable>
  );
};
export default ShowCardVariant1;

const styles = StyleSheet.create({
  card: {
    overflow: "hidden",
    elevation: 2,
    width: 142,
    height: 142,
    borderRadius: 8,
  },
  image: {
    width: "100%",
    height: "100%",
    resizeMode: "cover",
  },
});
