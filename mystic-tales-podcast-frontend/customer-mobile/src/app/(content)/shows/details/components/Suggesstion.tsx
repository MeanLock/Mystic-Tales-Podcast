import { Text } from "@/src/components/ui/Text";
import { View } from "@/src/components/ui/View";
import { FlatList, Image, Pressable, StyleSheet } from "react-native";
import { useRouter } from "expo-router";

type Show = {
  Id: string;
  Name: string;
  ImageUrl: string;
  UploadFrequency: string;
};

interface SuggesstionProps {
  shows: Show[];
}

const ShowCard = ({ show }: { show: Show }) => {
  const router = useRouter();

  return (
    <Pressable
      style={styles.cardContainer}
      onPress={() => router.push(`/(content)/shows/details/${show.Id}`)}
    >
      <Image style={styles.cardImage} source={{ uri: show.ImageUrl }} />
      <View className="gap-1">
        <Text numberOfLines={1} className="text-white text-[15px]">
          {show.Name}
        </Text>
        <Text numberOfLines={1} className="text-[#999999] text-[12px]">
          {show.UploadFrequency}
        </Text>
      </View>
    </Pressable>
  );
};

const Suggesstion = ({ shows }: SuggesstionProps) => {
  return (
    <View style={styles.container}>
      <Text className="text-white font-bold text-[20px] mb-4">
        You might also like
      </Text>

      <View>
        {/* Horizontal FlatList */}
        <FlatList
          data={shows}
          renderItem={({ item }) => <ShowCard show={item} />}
          keyExtractor={(item) => item.Id}
          horizontal
          showsHorizontalScrollIndicator={false}
          contentContainerStyle={styles.flatListContent}
          ItemSeparatorComponent={() => <View style={styles.separator} />}
        />
      </View>
    </View>
  );
};

export default Suggesstion;

const styles = StyleSheet.create({
  container: {
    backgroundColor: "#272727",
    paddingLeft: 30,
    paddingTop: 30,
    paddingBottom: 50,
  },
  flatListContent: {
    paddingRight: 30, // Add padding at the end of the list
  },
  separator: {
    width: 16, // Space between items
  },
  cardContainer: {
    width: 184,
    height: 230,
    justifyContent: "space-between",
  },
  cardImage: {
    width: 184,
    height: 184,
    resizeMode: "cover",
    borderRadius: 8,
  },
});
