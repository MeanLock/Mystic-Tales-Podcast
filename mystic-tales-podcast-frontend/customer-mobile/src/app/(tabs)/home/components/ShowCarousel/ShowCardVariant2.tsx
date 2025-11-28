import React from "react";
import { Image, Pressable, StyleSheet, Text, View } from "react-native";

interface ShowCardVariant2Props {
  // Define any props if needed in the future
  show: {
    Id: number;
    ImageUrl: string;
    Top: number;
  };
}

const ShowCardVariant2 = ({ show }: ShowCardVariant2Props) => {
  return (
    <Pressable key={show.Id} style={styles.card}>
      <Image source={{ uri: show.ImageUrl }} style={styles.image} />
      <View style={styles.rankContainer}>
        <View style={styles.rankTag}>
          <Text style={styles.rankText}>#{show.Top}</Text>
        </View>
      </View>
    </Pressable>
  );
};
export default ShowCardVariant2;

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
  rankContainer: {
    position: "absolute",
    top: 0,
    right: 0,
  },
  rankTag: {
    backgroundColor: "#AEE339",
    paddingHorizontal: 2,
    paddingVertical: 8,
    borderBottomLeftRadius: 8,
    alignItems: "center",
    justifyContent: "center",
    minWidth: 40,
    minHeight: 50,
  },
  rankText: {
    color: "#000",
    fontWeight: "700",
    fontSize: 16,
  },
});
