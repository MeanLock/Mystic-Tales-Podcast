import { Pressable } from "react-native";
import { Image, StyleSheet } from "react-native";
import { Text } from "@/src/components/ui/Text";
import { View } from "@/src/components/ui/View";
import { useRouter } from "expo-router";

interface PodcasterCardProps {
  podcaster: {
    Id: number;
    ImageUrl: string;
    Fullname: string;
    Followers?: number;
  };
}

// Format follower count with comma separator
const formatFollowerCount = (count: number): string => {
  return count.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
};

const PodcasterCard = ({ podcaster }: PodcasterCardProps) => {
  const router = useRouter();

  return (
    <Pressable style={styles.card}>
      {/* Profile Image */}
      <View style={styles.imageContainer}>
        <Image source={{ uri: podcaster.ImageUrl }} style={styles.image} />
      </View>

      {/* Name and Follower Count */}
      <View style={styles.textContainer}>
        <Text
          numberOfLines={1}
          className="text-white text-center font-semibold"
        >
          {podcaster.Fullname}
        </Text>
        <Text className="text-gray-400 text-center text-xs">
          {formatFollowerCount(podcaster.Followers || 0)} followers
        </Text>
      </View>
    </Pressable>
  );
};

export default PodcasterCard;

const styles = StyleSheet.create({
  card: {
    alignItems: "center",
    // justifyContent: "center",
    gap: 8,
    width: 150,
  },
  imageContainer: {
    width: 120,
    height: 120,
    borderRadius: 99999,
    borderWidth: 2,
    borderColor: "#AEE339",
    overflow: "hidden",
  },
  image: {
    width: "100%",
    height: "100%",
    resizeMode: "cover",
  },
  textContainer: {
    alignItems: "center",
    width: "100%",
  },
});
