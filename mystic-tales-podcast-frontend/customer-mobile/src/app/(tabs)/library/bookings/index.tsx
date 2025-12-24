// /(tabs)/library/shows/index.tsx
import React from "react";
import { ActivityIndicator, Animated, Dimensions } from "react-native";
import { View } from "@/src/components/ui/View";
import { Text } from "@/src/components/ui/Text";
import { useHeaderScroll } from "../_layout";
import { StyleSheet } from "react-native";
import { useBottomTabBarHeight } from "@react-navigation/bottom-tabs";
import { useGetCompletedBookingsQuery } from "@/src/core/services/booking/booking.service";
import CompletedBookingRow from "./components/CompletedBookingRow";
import { useRouter } from "expo-router";

export default function CompletedBookingsScreen() {
  const { onScroll } = useHeaderScroll();
  const tabBarHeight = useBottomTabBarHeight();
  const windowWidth = Dimensions.get("window").width;
  const itemWidth = (windowWidth - 16 * 2 - 10) / 2; // Calculate item width based on screen width, padding, and gap

  const { data: completedBookings, isLoading: isLoadingCompletedBookings } =
    useGetCompletedBookingsQuery();
  if (isLoadingCompletedBookings) {
    return (
      <View style={style.centerContainer}>
        <ActivityIndicator size="large" color="#aee339" />
        <Text style={{ color: "#aee339", fontSize: 16, fontWeight: "bold" }}>
          Loading Bookings...
        </Text>
      </View>
    );
  }
  return (
    <Animated.ScrollView
      onScroll={onScroll}
      scrollEventThrottle={16}
      contentContainerStyle={{
        paddingTop: 100,
        paddingHorizontal: 16,
        paddingBottom: 40,
        backgroundColor: "#000",
        minHeight: "100%",
      }}
    >
      <View>
        {completedBookings && completedBookings.BookingList.length > 0 ? (
          completedBookings.BookingList.map((booking) => (
            <CompletedBookingRow booking={booking} key={booking.Id} />
          ))
        ) : (
          <Text style={{ color: "white" }}>No completed bookings found.</Text>
        )}
      </View>
      <View style={{ height: tabBarHeight + 50 }}></View>
    </Animated.ScrollView>
  );
}

const style = StyleSheet.create({
  gridColTwoContainer: {
    flexDirection: "row",
    flexWrap: "wrap",
    justifyContent: "space-between",
    gap: 10, // This sets the gap between grid items
    width: "100%",
  },
  gridItem: {
    height: 150,
    backgroundColor: "#333",
    marginBottom: 5, // Additional gap for vertical spacing
    borderRadius: 8,
    justifyContent: "center",
    alignItems: "center",
  },
  itemText: {
    color: "white",
    fontWeight: "bold",
  },
  centerContainer: {
    flex: 1,
    justifyContent: "center",
    alignItems: "center",
    gap: 20,
    backgroundColor: "#000",
  },
});
