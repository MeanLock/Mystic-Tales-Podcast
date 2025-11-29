import { View } from "@/src/components/ui/View";
import { Feather, MaterialIcons } from "@expo/vector-icons";
import {
  FlatList,
  Image,
  StyleSheet,
  Pressable,
  Dimensions,
  Text,
} from "react-native";
import { useMemo } from "react";

type Rating = {
  Id: string;
  Title: string;
  Content: string;
  Rating: number;
  Account: {
    Id: number;
    FullName: string;
    Email: string;
    MainImageFileKey: string;
  };
  PodcastShowId: string;
  DeletedAt: string;
  UpdatedAt: string;
};

interface RatingAndReviewProps {
  ratings: Rating[];
  isCommented: boolean;
}

// Fix 1: Correctly define the RatingTextComponent as a React component with proper props
const RatingTextComponent = ({ ratings }: { ratings: Rating[] }) => {
  // Fix 2: Add null check to prevent division by zero
  const avgRating =
    ratings.length > 0
      ? (
          ratings.reduce((sum, r) => sum + r.Rating, 0) / ratings.length
        ).toFixed(1)
      : "0.0";

  return (
    <View className="flex flex-col items-center justify-center">
      <Text className="text-[48px] font-medium text-white">{avgRating}</Text>
      <Text className="text-[20px] text-white">/5</Text>
    </View>
  );
};

const RatingCard = ({ rating }: { rating: Rating }) => {
  // Format date function
  const formatDate = (dateString: string) => {
    try {
      const date = new Date(dateString);
      return date.toLocaleDateString("en-US", {
        year: "numeric",
        month: "short",
        day: "numeric",
      });
    } catch (error) {
      return "Unknown date";
    }
  };

  return (
    <View style={styles.reviewCard}>
      <View style={styles.reviewHeader}>
        <View style={styles.reviewUser}>
          <View style={styles.userInfo}>
            <Text style={styles.userName}>{rating.Account.FullName}</Text>
            <Text style={styles.reviewDate}>
              {formatDate(rating.UpdatedAt)}
            </Text>
          </View>
        </View>
        <View style={styles.starsContainer}>
          {Array.from({ length: 5 }).map((_, i) => (
            <MaterialIcons
              key={i}
              name={i < rating.Rating ? "star" : "star-outline"}
              size={16}
              color="#AEE339"
            />
          ))}
        </View>
      </View>

      <View style={styles.reviewContent}>
        <Text style={styles.reviewTitle}>{rating.Title}</Text>
        <Text style={styles.reviewText}>{rating.Content}</Text>
      </View>
    </View>
  );
};

// New component for horizontal rating list
const HorizontalRatingList = ({ ratings }: { ratings: Rating[] }) => {
  const screenWidth = Dimensions.get("window").width;
  const cardWidth = screenWidth * 0.8; // Each card takes 80% of screen width

  return (
    <FlatList
      data={ratings}
      keyExtractor={(item) => item.Id}
      horizontal
      showsHorizontalScrollIndicator={false}
      snapToInterval={cardWidth + 10} // Snap to card width + padding
      decelerationRate="fast"
      contentContainerStyle={styles.horizontalListContainer}
      renderItem={({ item, index }) => (
        <View
          style={[
            styles.cardWrapper,
            { width: cardWidth },
            index === 0 && { marginLeft: 0 },
          ]}
        >
          <RatingCard rating={item} />
        </View>
      )}
      ItemSeparatorComponent={() => <View style={{ width: 10 }} />}
    />
  );
};

const RatingChartComponent = ({ ratings }: { ratings: Rating[] }) => {
  // Calculate the count for each rating level (1-5)
  const calculateRatingCounts = () => {
    const counts = [0, 0, 0, 0, 0]; // For ratings 1-5

    if (!ratings || ratings.length === 0) {
      return counts;
    }

    ratings.forEach((rating) => {
      if (rating.Rating >= 1 && rating.Rating <= 5) {
        counts[rating.Rating - 1]++;
      }
    });

    return counts;
  };

  const totalRatings = ratings.length;
  const ratingCounts = calculateRatingCounts();

  // Calculate percentages for progress bars
  const calculatePercentage = (count: number) => {
    if (totalRatings === 0) return 0;
    return (count / totalRatings) * 100;
  };

  // Generate the rating bars
  const renderRatingBars = () => {
    return [5, 4, 3, 2, 1].map((rating) => {
      const count = ratingCounts[rating - 1];
      const percentage = calculatePercentage(count);

      return (
        <View key={rating} style={styles.ratingRow}>
          <View style={styles.ratingLabel}>
            {Array.from({ length: rating }, (_, i) => (
              <MaterialIcons
                key={i}
                name="star"
                size={15}
                color="#D9D9D9"
                style={{ marginRight: 2 }}
              />
            ))}
          </View>

          <View style={styles.progressBarContainer}>
            <View style={styles.progressBarBackground}>
              <View
                style={[styles.progressBarFill, { width: `${percentage}%` }]}
              />
            </View>
          </View>
        </View>
      );
    });
  };

  return (
    <View style={styles.chartContainer}>
      {renderRatingBars()}

      <View style={styles.totalRatingsContainer}>
        <Text style={styles.totalRatingsText}>
          {totalRatings} {totalRatings === 1 ? "rating" : "ratings"}
        </Text>
      </View>
    </View>
  );
};

const RatingAndReview = ({ ratings, isCommented }: RatingAndReviewProps) => {
  // Fix 3: Add proper null/empty check
  if (!ratings || ratings.length === 0) {
    return (
      <View style={styles.container}>
        <View className="flex flex-row items-center justify-between">
          <Text className="text-[30px] font-bold text-white">
            Rating & Reviews
          </Text>
          <MaterialIcons name="keyboard-arrow-right" size={30} color={"#fff"} />
        </View>
        <View className="w-full justify-center">
          <Text className="text-white text-center my-5">No ratings yet</Text>
        </View>
      </View>
    );
  }

  // Get the most recent rating (if any)
  const mostRecentRating =
    ratings.length > 0
      ? // Sort by date and get the first one
        [...ratings].sort(
          (a, b) =>
            new Date(b.UpdatedAt).getTime() - new Date(a.UpdatedAt).getTime()
        )[0]
      : null;

  // Get top rated reviews (5 and 4 stars)
  const topRatings = useMemo(() => {
    return ratings
      .filter((rating) => rating.Rating >= 4)
      .sort((a, b) => b.Rating - a.Rating)
      .slice(0, 5); // Get up to 5 top ratings
  }, [ratings]);

  return (
    <View style={styles.container}>
      <View className="flex flex-row items-center justify-between">
        <Text
          style={{ color: "#fff" }}
          className="text-[30px] font-bold text-white"
        >
          Rating & Reviews
        </Text>
        <Pressable>
          <MaterialIcons name="keyboard-arrow-right" size={30} color={"#fff"} />
        </Pressable>
      </View>

      <View style={styles.borderBottom} className="w-full flex-row mt-5 gap-5">
        <View className="w-1/3 items-start justify-start">
          <RatingTextComponent ratings={ratings} />
        </View>
        <View className="w-2/3 items-start justify-center">
          <RatingChartComponent ratings={ratings} />
        </View>
      </View>

      {/* Featured Reviews Section */}
      <View style={styles.reviewsSection}>
        {/* Horizontal Scrolling Review Cards */}
        {ratings.length > 0 ? (
          <HorizontalRatingList ratings={ratings} />
        ) : (
          <Text style={styles.noReviewsText}>No reviews yet</Text>
        )}
      </View>

      {!isCommented && (
        <Pressable className="w-full py-3 flex flex-row items-center gap-1">
          <Feather name="edit" color="#AEE339" size={15} />
          <Text className="text-[#AEE339]">Write a comment</Text>
        </Pressable>
      )}
    </View>
  );
};

export default RatingAndReview;

// const style = StyleSheet.create({
//   container: {
//     width: "100%",
//   },
// });

const styles = StyleSheet.create({
  container: {
    width: "100%",
  },
  chartContainer: {
    width: "100%",
    paddingHorizontal: 8,
  },
  ratingRow: {
    flexDirection: "row",
    alignItems: "center",
    marginBottom: 8,
    width: "100%",
  },
  ratingLabel: {
    flexDirection: "row",
    alignItems: "center",
    justifyContent: "flex-end",
    marginRight: 10,
    width: 25,
  },
  progressBarContainer: {
    flex: 1,
    marginHorizontal: 5,
  },
  progressBarBackground: {
    height: 6,
    backgroundColor: "#282828", // Dark background as specified
    borderRadius: 3,
  },
  progressBarFill: {
    height: "100%",
    backgroundColor: "#D9D9D9", // Light gray fill as specified
    borderRadius: 3,
  },
  countText: {
    color: "white",
    fontSize: 12,
    marginLeft: 5,
    width: 30,
    textAlign: "right",
  },
  totalRatingsContainer: {
    marginTop: 5,
    alignItems: "flex-end",
  },
  totalRatingsText: {
    color: "#999",
    fontSize: 12,
  },
  horizontalListContainer: {
    paddingVertical: 15,
    paddingRight: 20, // Add extra padding at the end
  },
  cardWrapper: {
    marginLeft: 5,
  },
  reviewsSection: {
    marginTop: 24,
  },
  sectionTitleContainer: {
    flexDirection: "row",
    justifyContent: "space-between",
    alignItems: "center",
    marginBottom: 10,
  },
  sectionTitle: {
    color: "#fff",
    fontSize: 18,
    fontWeight: "600",
  },
  seeAllButton: {
    flexDirection: "row",
    alignItems: "center",
    gap: 4,
  },
  seeAllText: {
    color: "#AEE339",
    fontSize: 14,
  },
  reviewCard: {
    backgroundColor: "rgba(255, 255, 255, 0.05)",
    borderRadius: 8,
    padding: 16,
    height: 201, // Fixed height for consistent cards
  },
  reviewHeader: {
    flexDirection: "row",
    justifyContent: "space-between",
    alignItems: "center",
    marginBottom: 12,
  },
  reviewUser: {
    flexDirection: "row",
    alignItems: "center",
  },
  userInfo: {
    justifyContent: "center",
  },
  userName: {
    color: "#fff",
    fontWeight: "600",
    fontSize: 14,
  },
  reviewDate: {
    color: "#999",
    fontSize: 12,
    marginTop: 2,
  },
  starsContainer: {
    flexDirection: "row",
  },
  reviewContent: {
    marginLeft: 4,
  },
  reviewTitle: {
    color: "#fff",
    fontWeight: "600",
    fontSize: 16,
    marginBottom: 8,
  },
  reviewText: {
    color: "#ccc",
    fontSize: 14,
    lineHeight: 20,
    maxHeight: 80, // Limit text height
  },
  noReviewsText: {
    color: "#999",
    fontStyle: "italic",
    textAlign: "center",
    paddingVertical: 16,
  },
  borderBottom: {
    borderBottomWidth: 0.3,
    borderColor: "#514F4F",
    paddingBottom: 10,
  },
});
