import HtmlText from "@/src/components/renderHtml/HtmlText";
import { CompletedBooking } from "@/src/core/types/booking.type";
import { MaterialIcons } from "@expo/vector-icons";
import { router, useRouter } from "expo-router";
import { Image, Pressable, StyleSheet, Text, View } from "react-native";

const gradientImages = [
  {
    uri: "https://i.pinimg.com/736x/0f/e4/03/0fe403dfb8e817a5e79fbe7d36210b74.jpg",
  },
  {
    uri: "https://i.pinimg.com/736x/72/f3/b3/72f3b3f7a96fd5b925a3f6f2b00b19be.jpg",
  },
  {
    uri: "https://i.pinimg.com/736x/5d/87/f9/5d87f94ad62fab09cc2041be69225bdf.jpg",
  },
  {
    uri: "https://i.pinimg.com/736x/88/75/8a/88758a5a7d65789ea14ca54e704c4670.jpg",
  },
  {
    uri: "https://i.pinimg.com/1200x/71/3e/be/713ebecee53669627f6c75a6d4bff6ad.jpg",
  },
  {
    uri: "https://i.pinimg.com/736x/a3/b0/9a/a3b09acecfa4a3fe0321064b74e95b39.jpg",
  },
  {
    uri: "https://i.pinimg.com/736x/9b/98/bd/9b98bdd26e06a9712e7a50bcf00e8378.jpg",
  },
  {
    uri: "https://i.pinimg.com/1200x/40/5e/c2/405ec2a1822a889c48e38b71246993b2.jpg",
  },
];

const CompletedBookingRow = ({ booking }: { booking: CompletedBooking }) => {
  const router = useRouter();
  const randomIndex = Math.floor(Math.random() * gradientImages.length);
  return (
    <View style={styles.container}>
      <View style={styles.avatarContainer}>
        <Image
          source={gradientImages[randomIndex]}
          style={{
            width: 80,
            height: 80,
            borderRadius: 50,
            objectFit: "cover",
          }}
        />
      </View>
      <View className="flex items-start justify-center">
        <Text className="text-white font-bold leading-none">
          {booking.Title}
        </Text>
        <Text>{booking.CompletedAt}</Text>
        <HtmlText
          color="white"
          fontSize={10}
          numberOfLines={3}
          html={booking.Description}
        />
        <Text className="text-[#aee339] font-bold mt-2">
          {booking.CompletedBookingTrackCount} tracks
        </Text>
      </View>
      <View className="flex-1 flex items-end justify-center">
        <Pressable
          onPress={() =>
            router.push(`/(content)/completed-bookings/details/${booking.Id}`)
          }
        >
          <MaterialIcons name="keyboard-arrow-right" size={24} color="#fff" />
        </Pressable>
      </View>
    </View>
  );
};

export default CompletedBookingRow;

const styles = StyleSheet.create({
  container: {
    width: "100%",
    paddingVertical: 10,
    borderBottomWidth: 1,
    borderBottomColor: "#333",
    flexDirection: "row",
    alignItems: "center",
    gap: 12,
  },
  avatarContainer: {
    width: 80,
    height: 80,
    borderRadius: 50,
    overflow: "hidden",
    elevation: 10,
  },
});
