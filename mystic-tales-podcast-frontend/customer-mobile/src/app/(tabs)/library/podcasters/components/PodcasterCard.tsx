import AutoResolvingImage from "@/src/components/autoResolveImage/AutoResolvingImage";
import { PodcasterFromApi } from "@/src/core/types/podcaster.type";
import { useRouter } from "expo-router";
import { Pressable, StyleSheet, Text, View } from "react-native";

const PodcasterCard = ({
  podcaster,
  width,
}: {
  podcaster: PodcasterFromApi;
  width: number;
}) => {
  const router = useRouter();
  return (
    <Pressable
      onPress={() =>
        router.push(`/(content)/podcasters/details/${podcaster.AccountId}`)
      }
      className="flex items-center justify-center gap-2 mb-5"
      style={{ width: width }}
    >
      <AutoResolvingImage
        FileKey={podcaster.MainImageFileKey}
        type="AccountPublicSource"
        style={styles.avatar}
      />
      <Text numberOfLines={1} className=" text-center text-white font-light">
        {podcaster.Name}
      </Text>
    </Pressable>
  );
};

export default PodcasterCard;

const styles = StyleSheet.create({
  avatar: {
    width: 120,
    height: 120,
    borderRadius: 75,
    borderWidth: 2,
    borderColor: "#aee339",
  },
});
