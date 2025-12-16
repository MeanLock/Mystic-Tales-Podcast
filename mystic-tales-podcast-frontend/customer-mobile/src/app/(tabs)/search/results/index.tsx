import React, { useState } from "react";
import {
  ScrollView,
  StyleSheet,
  Pressable,
  Platform,
  StatusBar,
} from "react-native";
import { View } from "@/src/components/ui/View";
import { Text } from "@/src/components/ui/Text";
import { useLocalSearchParams, useRouter } from "expo-router";
import { useSafeAreaInsets } from "react-native-safe-area-context";
import { MaterialIcons } from "@expo/vector-icons";
import { useBottomTabBarHeight } from "@react-navigation/bottom-tabs";
import { useGetSearchResultsQuery } from "@/src/core/services/search/search.service";
import ShowCard from "./components/ShowCard";
import EpisodeCard from "./components/EpisodeCard";
import ChannelCard from "./components/ChannelCard";

export default function SearchResultsScreen() {
  const { keyword } = useLocalSearchParams<{ keyword: string }>();
  const router = useRouter();
  const insets = useSafeAreaInsets();
  const tabBarHeight = useBottomTabBarHeight();
  const [viewMode, setViewMode] = useState<
    "top-results" | "channel" | "show" | "episode"
  >("top-results");
  const { data: searchResults, isLoading } = useGetSearchResultsQuery(
    {
      keyword: keyword!,
    },
    { skip: !keyword }
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

          {/* Search Query Display */}
          <View style={styles.searchQueryContainer}>
            <MaterialIcons
              name="search"
              size={20}
              color="#8E8E93"
              style={{ marginRight: 8 }}
            />
            <Text style={styles.searchQueryText} numberOfLines={1}>
              {keyword || "Search"}
            </Text>
          </View>
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
        <View style={styles.content}>
          {/* Results Header */}
          <View style={styles.resultsHeader}>
            <Text style={styles.resultsTitle} className="leading-none">
              Search Results for "{keyword}"
            </Text>
          </View>

          {/* Placeholder content - will be replaced with actual results */}
          {!isLoading && searchResults ? (
            <View className="w-full flex flex-col">
              <View className="w-full flex flex-row items-center gap-5">
                <Pressable
                  onPress={() => setViewMode("top-results")}
                  className={`${
                    viewMode === "top-results"
                      ? "bg-[#aee339]"
                      : "border-2 border-[#aee339] "
                  } py-2 px-3 rounded-full`}
                >
                  <Text
                    className={`${
                      viewMode === "top-results"
                        ? "text-black"
                        : "text-[#aee339]"
                    } font-semibold text-sm`}
                  >
                    Top Results
                  </Text>
                </Pressable>
                <Pressable
                  onPress={() => setViewMode("show")}
                  className={`${
                    viewMode === "show"
                      ? "bg-[#aee339]"
                      : "border-2 border-[#aee339] "
                  } py-2 px-3 rounded-full`}
                >
                  <Text
                    className={`${
                      viewMode === "show" ? "text-black" : "text-[#aee339]"
                    } font-semibold text-sm`}
                  >
                    Shows
                  </Text>
                </Pressable>

                <Pressable
                  onPress={() => setViewMode("episode")}
                  className={`${
                    viewMode === "episode"
                      ? "bg-[#aee339]"
                      : "border-2 border-[#aee339] "
                  } py-2 px-3 rounded-full`}
                >
                  <Text
                    className={`${
                      viewMode === "episode" ? "text-black" : "text-[#aee339]"
                    } font-semibold text-sm`}
                  >
                    Episodes
                  </Text>
                </Pressable>
                <Pressable
                  onPress={() => setViewMode("channel")}
                  className={`${
                    viewMode === "channel"
                      ? "bg-[#aee339]"
                      : "border-2 border-[#aee339] "
                  } py-2 px-3 rounded-full`}
                >
                  <Text
                    className={`${
                      viewMode === "channel" ? "text-black" : "text-[#aee339]"
                    } font-semibold text-sm`}
                  >
                    Channels
                  </Text>
                </Pressable>
              </View>

              {viewMode === "top-results" && (
                <View className="w-full mt-10 flex flex-col gap-2">
                  {searchResults.TopSearchResults.map((result) =>
                    result.Episode ? (
                      <EpisodeCard episode={result.Episode} />
                    ) : result.Show ? (
                      <ShowCard show={result.Show} />
                    ) : (
                      <></>
                    )
                  )}
                </View>
              )}
              {viewMode === "show" && (
                <View className="w-full mt-10 flex flex-col gap-2">
                  {searchResults.ShowList.map((show) => (
                    <ShowCard show={show} />
                  ))}
                </View>
              )}
              {viewMode === "episode" && (
                <View className="w-full mt-10 flex flex-col gap-2">
                  {searchResults.EpisodeList.map((episode) => (
                    <EpisodeCard episode={episode} />
                  ))}
                </View>
              )}
              {viewMode === "channel" && (
                <View className="w-full mt-10 flex flex-col gap-2">
                  {searchResults.ChannelList.map((channel) => (
                    <ChannelCard channel={channel} />
                  ))}
                </View>
              )}
            </View>
          ) : (
            <View style={styles.placeholder}>
              <MaterialIcons name="search" size={64} color="#3A3A3C" />
              <Text style={styles.placeholderText}>
                Loading results for "{keyword}"...
              </Text>
            </View>
          )}
        </View>
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
  searchQueryContainer: {
    flex: 1,
    flexDirection: "row",
    alignItems: "center",
    backgroundColor: "#1C1C1E",
    borderRadius: 10,
    paddingHorizontal: 12,
    paddingVertical: 10,
  },
  searchQueryText: {
    flex: 1,
    color: "#fff",
    fontSize: 16,
  },
  scrollView: {
    flex: 1,
  },
  content: {
    paddingHorizontal: 16,
  },
  resultsHeader: {
    paddingVertical: 20,
  },
  resultsTitle: {
    color: "#fff",
    fontSize: 24,
    fontWeight: "700",
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
