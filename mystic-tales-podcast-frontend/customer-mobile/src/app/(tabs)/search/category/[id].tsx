import React from "react";
import {
  ScrollView,
  StyleSheet,
  Pressable,
  Platform,
  StatusBar,
  ActivityIndicator,
} from "react-native";
import { View } from "@/src/components/ui/View";
import { Text } from "@/src/components/ui/Text";
import { useLocalSearchParams, useRouter } from "expo-router";
import { useSafeAreaInsets } from "react-native-safe-area-context";
import { MaterialIcons } from "@expo/vector-icons";
import { useBottomTabBarHeight } from "@react-navigation/bottom-tabs";
import { useGetCategoryFeedDataQuery } from "@/src/core/services/category/category.service";
import ShowCarousel from "./components/Show/ShowCarousel";
import EpisodeCarousel from "./components/Episode/EpisodeCarousel";
import ChannelCarousel from "./components/Channel/ChannelCarousel";

export default function CategoryScreen() {
  const { id } = useLocalSearchParams<{ id: string }>();
  const router = useRouter();
  const insets = useSafeAreaInsets();
  const tabBarHeight = useBottomTabBarHeight();

  // Fetch category data
  const {
    data: categoryData,
    isLoading,
    error,
  } = useGetCategoryFeedDataQuery(
    { PodcastCategoryId: Number(id) },
    { skip: !id }
  );

  return (
    <View style={styles.container}>
      <StatusBar barStyle="light-content" />

      {/* Header */}
      <View
        style={[
          styles.header,
          {
            paddingTop: insets.top,
            paddingBottom: 12,
          },
        ]}
      >
        <View style={styles.headerContent}>
          {/* Back Button */}
          <Pressable style={styles.backButton} onPress={() => router.back()}>
            <MaterialIcons
              name={Platform.OS === "ios" ? "arrow-back-ios" : "arrow-back"}
              size={24}
              color="#fff"
            />
          </Pressable>

          {/* Category Title */}
          <View style={styles.titleContainer}>
            <Text style={styles.categoryTitle} numberOfLines={1}>
              {isLoading
                ? "Loading..."
                : categoryData?.PodcastCategory?.Name || "Category"}
            </Text>
          </View>

          {/* Placeholder for balance */}
          <View style={{ width: 40 }} />
        </View>
      </View>

      {/* Content */}
      <ScrollView
        style={styles.scrollView}
        contentContainerStyle={{
          paddingBottom: tabBarHeight + 20,
        }}
        showsVerticalScrollIndicator={true}
      >
        {isLoading ? (
          <View style={styles.loadingContainer}>
            <ActivityIndicator size="large" color="#AEE339" />
            <Text style={styles.loadingText}>Loading category...</Text>
          </View>
        ) : error ? (
          <View style={styles.errorContainer}>
            <MaterialIcons name="error-outline" size={64} color="#FF6B6B" />
            <Text style={styles.errorText}>Failed to load category</Text>
          </View>
        ) : (
          <View style={styles.content}>
            {/* Top Channels Section */}
            {categoryData?.TopChannels &&
              categoryData.TopChannels.length > 0 && (
                <View style={styles.section}>
                  <ChannelCarousel
                    variant="top"
                    title={
                      <Text style={styles.sectionTitle}>Top Channels</Text>
                    }
                    channels={categoryData.TopChannels}
                  />
                </View>
              )}

            {/* Top Shows Section */}
            {categoryData?.TopShows && categoryData.TopShows.length > 0 && (
              <View style={styles.section}>
                <ShowCarousel
                  variant="top"
                  title={<Text style={styles.sectionTitle}>Top Shows</Text>}
                  shows={categoryData.TopShows}
                />
              </View>
            )}

            {/* Hot Shows Section */}
            {categoryData?.HotShows && categoryData.HotShows.length > 0 && (
              <View style={styles.section}>
                <ShowCarousel
                  variant="normal"
                  title={<Text style={styles.sectionTitle}>Hot Shows</Text>}
                  shows={categoryData.HotShows}
                />
              </View>
            )}

            {/* Top Episodes Section */}
            {categoryData?.TopEpisodes &&
              categoryData.TopEpisodes.length > 0 && (
                <View style={styles.section}>
                  <EpisodeCarousel
                    title={
                      <Text style={styles.sectionTitle}>Top Episodes</Text>
                    }
                    episodes={categoryData.TopEpisodes.map(
                      (episode, index) => ({
                        ...episode,
                      })
                    )}
                  />
                </View>
              )}

            {/* SubCategory Sections */}
            {categoryData?.SubCategorySections &&
              categoryData.SubCategorySections.length > 0 &&
              categoryData.SubCategorySections.map((subCategory) => {
                if (!subCategory.ShowList || subCategory.ShowList.length === 0)
                  return null;

                return (
                  <View
                    key={subCategory.PodcastSubCategory.Id}
                    style={styles.section}
                  >
                    <ShowCarousel
                      variant="normal"
                      title={
                        <Text style={styles.sectionTitle}>
                          {subCategory.PodcastSubCategory.Name}
                        </Text>
                      }
                      shows={subCategory.ShowList}
                    />
                  </View>
                );
              })}
          </View>
        )}
      </ScrollView>
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: "#000",
  },
  header: {
    backgroundColor: "#000",
    borderBottomWidth: 1,
    borderBottomColor: "#1C1C1E",
  },
  headerContent: {
    flexDirection: "row",
    alignItems: "center",
    paddingHorizontal: 16,
    gap: 12,
  },
  backButton: {
    width: 40,
    height: 40,
    alignItems: "center",
    justifyContent: "center",
  },
  titleContainer: {
    flex: 1,
    alignItems: "center",
  },
  categoryTitle: {
    color: "#fff",
    fontSize: 18,
    fontWeight: "700",
  },
  scrollView: {
    flex: 1,
  },
  content: {
    paddingHorizontal: 16,
  },
  section: {
    marginBottom: 32,
  },
  sectionTitle: {
    color: "#fff",
    fontSize: 22,
    fontWeight: "700",
  },
  categoryInfo: {
    paddingVertical: 20,
    borderBottomWidth: 1,
    borderBottomColor: "#1C1C1E",
  },
  categoryName: {
    color: "#fff",
    fontSize: 28,
    fontWeight: "700",
    marginBottom: 8,
  },
  categoryDescription: {
    color: "#8E8E93",
    fontSize: 14,
  },
  loadingContainer: {
    flex: 1,
    alignItems: "center",
    justifyContent: "center",
    paddingTop: 100,
  },
  loadingText: {
    color: "#8E8E93",
    fontSize: 16,
    marginTop: 12,
  },
  errorContainer: {
    flex: 1,
    alignItems: "center",
    justifyContent: "center",
    paddingTop: 100,
  },
  errorText: {
    color: "#FF6B6B",
    fontSize: 16,
    marginTop: 16,
    textAlign: "center",
  },
  placeholder: {
    alignItems: "center",
    justifyContent: "center",
    paddingVertical: 100,
  },
  placeholderText: {
    color: "#8E8E93",
    fontSize: 16,
    marginTop: 16,
    textAlign: "center",
  },
});
