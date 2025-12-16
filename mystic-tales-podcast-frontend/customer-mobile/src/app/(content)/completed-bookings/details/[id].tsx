import AutoResolvingImage from "@/src/components/autoResolveImage/AutoResolvingImage";
import HtmlText from "@/src/components/renderHtml/HtmlText";
import { Text } from "@/src/components/ui/Text";
import { View } from "@/src/components/ui/View";
import { useGetCompletedBookingDetailQuery } from "@/src/core/services/booking/booking.service";
import { Entypo, MaterialCommunityIcons } from "@expo/vector-icons";
import { useLocalSearchParams, useRouter } from "expo-router";
import { useEffect, useRef } from "react";
import {
  ActivityIndicator,
  Image,
  Platform,
  Pressable,
  ScrollView,
  StyleSheet,
} from "react-native";
import Animated, {
  Easing,
  useAnimatedStyle,
  useSharedValue,
  withRepeat,
  withTiming,
} from "react-native-reanimated";
import BookingTrackRow from "./components/BookingTrackRow";

export default function CompletedBookingDetailsScreen() {
  const { id } = useLocalSearchParams<{ id: string }>();
  const router = useRouter();

  const rotation = useSharedValue(0);

  useEffect(() => {
    rotation.value = withRepeat(
      withTiming(360, {
        duration: 4000,
        easing: Easing.linear,
      }),
      -1, // infinite
      false // không reverse
    );
  }, [rotation]);

  const animatedStyle = useAnimatedStyle(() => ({
    transform: [{ rotate: `${rotation.value}deg` }],
  }));

  // HOOKS
  const { data: bookingData, isLoading } = useGetCompletedBookingDetailQuery(
    { BookingId: parseInt(id!) },
    {
      skip: !id,
      refetchOnFocus: true,
      refetchOnMountOrArgChange: true,
      refetchOnReconnect: true,
    }
  ); // TODO: Fetch booking details by ID

  if (isLoading) {
    return (
      <View className="flex-1 items-center justify-center gap-5 bg-black">
        <ActivityIndicator size="large" color="#AEE339" />
        <Text className="text-white text-lg">Loading Booking Details...</Text>
      </View>
    );
  }

  return (
    <ScrollView showsVerticalScrollIndicator={false}>
      <View className="flex items-center justify-center h-[400px] relative">
        <Image
          source={{
            uri: "https://i.pinimg.com/1200x/40/5e/c2/405ec2a1822a889c48e38b71246993b2.jpg",
          }}
          style={{ width: "100%", height: "100%", objectFit: "cover" }}
        />
        <View className="absolute inset-0 z-10 bg-black/30 flex items-center justify-center"></View>
        <View
          style={{
            left: Platform.OS === "ios" ? 20 : 12,
            right: Platform.OS === "ios" ? 20 : 12,
            top: Platform.OS === "ios" ? 50 : 50,
          }}
          className="absolute z-50 top-14 flex flex-row items-center justify-between"
        >
          <Pressable onPress={() => router.back()} style={styles.actionButton}>
            {Platform.OS === "ios" ? (
              <Entypo name="chevron-small-left" size={24} color="white" />
            ) : (
              <MaterialCommunityIcons
                name="arrow-left"
                size={15}
                color="#fff"
              />
            )}
          </Pressable>
        </View>

        <View className="absolute inset-0 z-20 flex items-center justify-center">
          <Animated.View
            style={animatedStyle}
            className="w-[30%] aspect-square"
          >
            <Image
              source={{
                uri: "https://i.pinimg.com/1200x/40/5e/c2/405ec2a1822a889c48e38b71246993b2.jpg",
              }}
              style={{
                width: "100%",
                objectFit: "cover",
                borderRadius: 150,
              }}
              className="aspect-square"
            />
          </Animated.View>
          <Text className="text-white text-2xl font-bold mt-4">
            {bookingData?.BookingList.Title}
          </Text>
          <HtmlText
            color="white"
            html={bookingData?.BookingList.Description || ""}
          />
        </View>
      </View>
      <View className="px-4 py-6 mb-50 gap-5">
        <Text className="text-white text-2xl font-bold">Track List</Text>
        {bookingData?.BookingList.LastestBookingPodcastTracks.map(
          (track, index) => (
            <BookingTrackRow key={track.Id} index={index} track={track} />
          )
        )}
      </View>
      <View style={{ height: 100 }} />
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  actionButton: {
    padding: 6,
    borderRadius: 50,
    backgroundColor: "rgba(128, 128, 128, 0.7)",
    overflow: "hidden",
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.3,
    shadowRadius: 3,
    elevation: 2,
  },
});
