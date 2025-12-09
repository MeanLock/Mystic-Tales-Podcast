import React, { useState, useRef, useEffect, useCallback } from "react";
import {
  ScrollView,
  StyleSheet,
  Pressable,
  Alert,
  Animated,
  Image,
  Platform,
  TextInput,
  Keyboard,
  TouchableWithoutFeedback,
  ActivityIndicator,
} from "react-native";
import { View } from "@/src/components/ui/View";
import { Text } from "@/src/components/ui/Text";
import { useHeaderScroll } from "./_layout";
import { useBottomTabBarHeight } from "@react-navigation/bottom-tabs";
import { MaterialIcons } from "@expo/vector-icons";
import { useRouter } from "expo-router";
import { useSelector } from "react-redux";
import { RootState } from "@/src/store/store";
import { useSafeAreaInsets } from "react-native-safe-area-context";
import { BlurView } from "expo-blur";
import { useWindowDimensions } from "react-native";
import AutoResolvingImage from "@/src/components/autoResolveImage/AutoResolvingImage";
import { useGetCategoriesQuery } from "@/src/core/services/category/category.service";
import {
  useLazyGetAutocompleteWordRealTimeQuery,
  useLazyGetPodcastContentOnKeywordRealTimeQuery,
} from "@/src/core/services/search/search.service";

const mappingImages = [
  { id: 1, source: require("../../../../assets/images/category/1.png") },
  { id: 2, source: require("../../../../assets/images/category/2.png") },
  { id: 3, source: require("../../../../assets/images/category/3.png") },
  { id: 4, source: require("../../../../assets/images/category/4.png") },
  { id: 5, source: require("../../../../assets/images/category/5.png") },
  { id: 6, source: require("../../../../assets/images/category/6.png") },
  { id: 7, source: require("../../../../assets/images/category/7.png") },
];

// Component to highlight matching text
const HighlightedText = ({ text, query }: { text: string; query: string }) => {
  if (!query.trim()) {
    return <Text style={searchStyles.suggestionTitle}>{text}</Text>;
  }

  const lowerText = text.toLowerCase();
  const lowerQuery = query.toLowerCase();
  const index = lowerText.indexOf(lowerQuery);

  if (index === -1) {
    return <Text style={searchStyles.suggestionTitle}>{text}</Text>;
  }

  const before = text.substring(0, index);
  const match = text.substring(index, index + query.length);
  const after = text.substring(index + query.length);

  return (
    <Text style={searchStyles.suggestionTitle}>
      <Text style={{ color: "#8E8E93" }}>{before}</Text>
      <Text style={{ color: "#fff", fontWeight: "600" }}>{match}</Text>
      <Text style={{ color: "#8E8E93" }}>{after}</Text>
    </Text>
  );
};

// Header Component from the original _layout file
const Header = ({ scrollY }: { scrollY: Animated.Value }) => {
  const insets = useSafeAreaInsets();
  const router = useRouter();
  const authState = useSelector((state: RootState) => state.auth);
  const { width } = useWindowDimensions();

  // Create animated values with more extreme values for clear transition
  const headerOpacity = scrollY.interpolate({
    inputRange: [0, 40],
    outputRange: [0, 1],
    extrapolate: "clamp",
  });

  const titleOpacity = scrollY.interpolate({
    inputRange: [0, 40],
    outputRange: [1, 0],
    extrapolate: "clamp",
  });

  const centeredTitleOpacity = scrollY.interpolate({
    inputRange: [0, 40],
    outputRange: [0, 1],
    extrapolate: "clamp",
  });

  const goToLogin = () => {
    router.push("/(auth)/login");
  };

  const goToProfile = () => {
    router.push("/(user)/profile");
  };

  return (
    <Animated.View
      style={[
        headerStyles.headerContainer,
        {
          paddingTop: insets.top,
          height: insets.top + 60,
        },
      ]}
    >
      {/* Background blur for scrolled state */}
      <Animated.View
        style={[
          StyleSheet.absoluteFill,
          { opacity: headerOpacity },
          headerStyles.headerBackground,
        ]}
      >
        <BlurView intensity={80} tint="dark" style={StyleSheet.absoluteFill} />
      </Animated.View>

      {/* Content container */}
      <View style={[headerStyles.headerContent, { paddingHorizontal: 16 }]}>
        {/* Initial header with title and user */}
        <Animated.View
          style={[
            headerStyles.titleContainer,
            {
              opacity: titleOpacity,
            },
          ]}
        >
          <Text style={[headerStyles.title, { color: "#AEE339" }]}>Search</Text>

          {/* User badge or login button */}
          {authState.user ? (
            <Pressable style={headerStyles.userBadge} onPress={goToProfile}>
              <AutoResolvingImage
                FileKey={authState.user.MainImageFileKey}
                style={headerStyles.userAvatar}
                type="AccountPublicSource"
              />
            </Pressable>
          ) : (
            <Pressable
              style={[headerStyles.loginBtn, { backgroundColor: "#AEE339" }]}
              onPress={goToLogin}
            >
              <Text style={headerStyles.loginText}>Login</Text>
            </Pressable>
          )}
        </Animated.View>

        {/* Centered title for scrolled state */}
        <Animated.View
          style={[
            headerStyles.centeredTitleContainer,
            {
              opacity: centeredTitleOpacity,
              width,
            },
          ]}
        >
          <Text style={[headerStyles.centeredTitle, { color: "#AEE339" }]}>
            Search
          </Text>
        </Animated.View>
      </View>
    </Animated.View>
  );
};

export default function SearchScreen() {
  const { onScroll, headerHeight, scrollY } = useHeaderScroll();
  const tabBarHeight = useBottomTabBarHeight();
  const authState = useSelector((state: RootState) => state.auth);
  const insets = useSafeAreaInsets();

  const router = useRouter();

  // Search states
  const [isSearching, setIsSearching] = useState(false);
  const [searchQuery, setSearchQuery] = useState("");
  const searchInputRef = useRef<TextInput>(null);
  const [keywordSuggestions, setKeywordSuggestions] = useState<string[]>([]);
  const [searchResults, setSearchResults] = useState<any[]>([]);
  const [isLoadingSearch, setIsLoadingSearch] = useState(false);

  // HOOKS
  const { data: categoriesData, isLoading: isLoadingCategories } =
    useGetCategoriesQuery();
  const [getSuggesstKeyword] = useLazyGetAutocompleteWordRealTimeQuery();
  const [getPodcastContentOnKeyword] =
    useLazyGetPodcastContentOnKeywordRealTimeQuery();

  const mappingImageIntoCategory = (categoryId: number) => {
    const mapping = mappingImages.find((img) => img.id === categoryId);
    return mapping ? mapping.source : null;
  };

  // Debounce search API calls
  useEffect(() => {
    if (!searchQuery.trim() || !isSearching) {
      setKeywordSuggestions([]);
      setSearchResults([]);
      return;
    }

    setIsLoadingSearch(true);
    const delayTimer = setTimeout(async () => {
      try {
        // Get keyword suggestions
        const suggestionsResponse = await getSuggesstKeyword({
          keyword: searchQuery,
        }).unwrap();
        setKeywordSuggestions(suggestionsResponse || []);

        // Get content results
        const contentResponse = await getPodcastContentOnKeyword({
          keyword: searchQuery,
        }).unwrap();
        setSearchResults(contentResponse?.SearchItemList || []);
      } catch (error) {
        console.error("Search error:", error);
        setKeywordSuggestions([]);
        setSearchResults([]);
      } finally {
        setIsLoadingSearch(false);
      }
    }, 500); // 500ms debounce

    return () => clearTimeout(delayTimer);
  }, [searchQuery, isSearching]);

  const handleSearchFocus = () => {
    setIsSearching(true);
    searchInputRef.current?.focus();
  };

  const handleSearchCancel = () => {
    setIsSearching(false);
    setSearchQuery("");
    setKeywordSuggestions([]);
    setSearchResults([]);
    Keyboard.dismiss();
  };

  const handleSearchSubmit = () => {
    if (searchQuery.trim()) {
      // Navigate to full search results page
      router.push(
        `/(tabs)/search/results?keyword=${searchQuery}`
      );
      Keyboard.dismiss();
    }
  };

  const handleCategoryPress = (categoryId: number) => {
    router.push(`/(tabs)/search/category/${categoryId}`);
  };

  const handleKeywordSuggestionPress = (keyword: string) => {
    setSearchQuery(keyword);
    router.push(
      `/(tabs)/search/results?keyword=${keyword}`
    );
  };

  const handleContentPress = (item: any) => {
    if (item.Show) {
      router.push(`/(content)/shows/details/${item.Show.Id}`);
    } else if (item.Episode) {
      router.push(`/(content)/episodes/details/${item.Episode.Id}`);
    }
  };

  return (
    <>
      <Header scrollY={scrollY} />
      <View style={{ flex: 1, backgroundColor: "#000" }}>
        {/* Search Bar - Always visible */}
        <View
          style={[
            searchStyles.searchBarContainer,
            {
              paddingTop: headerHeight,
              paddingHorizontal: 16,
              paddingBottom: 12,
            },
          ]}
        >
          <View style={searchStyles.searchBarWrapper}>
            {!isSearching ? (
              // Search bar when not active
              <Pressable
                style={searchStyles.searchBar}
                onPress={handleSearchFocus}
              >
                <MaterialIcons
                  name="search"
                  size={20}
                  color="#8E8E93"
                  style={{ marginRight: 8 }}
                />
                <Text style={searchStyles.searchPlaceholder}>Search</Text>
              </Pressable>
            ) : (
              // Active search bar with input and cancel button
              <View style={searchStyles.activeSearchContainer}>
                <View style={searchStyles.searchInputWrapper}>
                  <MaterialIcons
                    name="search"
                    size={20}
                    color="#8E8E93"
                    style={{ marginRight: 8 }}
                  />
                  <TextInput
                    ref={searchInputRef}
                    style={searchStyles.searchInput}
                    placeholder="Search"
                    placeholderTextColor="#8E8E93"
                    value={searchQuery}
                    onChangeText={setSearchQuery}
                    onSubmitEditing={handleSearchSubmit}
                    returnKeyType="search"
                    autoFocus
                  />
                  {searchQuery.length > 0 && (
                    <Pressable onPress={() => setSearchQuery("")}>
                      <MaterialIcons name="cancel" size={20} color="#8E8E93" />
                    </Pressable>
                  )}
                </View>
                <Pressable
                  onPress={handleSearchCancel}
                  style={searchStyles.cancelButton}
                >
                  <Text style={searchStyles.cancelText}>Cancel</Text>
                </Pressable>
              </View>
            )}
          </View>
        </View>

        {/* Content */}
        {!isSearching ? (
          // Category List - Default view
          <ScrollView
            onScroll={onScroll}
            style={{ flex: 1 }}
            contentContainerStyle={{
              paddingHorizontal: 16,
              paddingBottom: tabBarHeight + 20,
            }}
            scrollIndicatorInsets={{ top: headerHeight }}
            scrollEventThrottle={16}
          >
            <View style={searchStyles.categoryHeader}>
              <Text style={searchStyles.categoryHeaderText}>Categories</Text>
            </View>
            {isLoadingCategories ? (
              <View style={searchStyles.loadingContainer}>
                <ActivityIndicator size="large" color="#AEE339" />
              </View>
            ) : (
              <View style={searchStyles.categoryGrid}>
                {categoriesData?.PodcastCategoryList.map((category) => {
                  const imageSource = mappingImageIntoCategory(category.Id);
                  return (
                    <Pressable
                      key={category.Id}
                      style={searchStyles.categoryCard}
                      onPress={() => handleCategoryPress(category.Id)}
                    >
                      {imageSource && (
                        <Image
                          source={imageSource}
                          style={{ width: 193, height: 110 }}
                          resizeMode="cover"
                        />
                      )}
                    </Pressable>
                  );
                })}
              </View>
            )}
          </ScrollView>
        ) : (
          // Search Results / Suggestions
          <ScrollView
            style={{ flex: 1 }}
            contentContainerStyle={{
              paddingBottom: tabBarHeight + 20,
            }}
            keyboardShouldPersistTaps="handled"
          >
            {searchQuery.length === 0 ? (
              // Empty state
              <View style={searchStyles.emptyState}>
                <MaterialIcons name="search" size={64} color="#3A3A3C" />
                <Text style={searchStyles.emptyStateText}>
                  Start typing to search
                </Text>
              </View>
            ) : isLoadingSearch ? (
              // Loading state
              <View style={searchStyles.loadingContainer}>
                <ActivityIndicator size="large" color="#AEE339" />
                <Text style={searchStyles.loadingText}>Searching...</Text>
              </View>
            ) : (
              // Search results
              <View style={searchStyles.suggestionsContainer}>
                {/* Keyword Suggestions */}
                {keywordSuggestions.length > 0 && (
                  <View style={{ marginBottom: 16 }}>
                    <Text style={searchStyles.suggestionsHeader}>
                      Search Suggestions
                    </Text>
                    {keywordSuggestions.map((keyword, index) => (
                      <Pressable
                        key={`keyword-${index}`}
                        style={searchStyles.suggestionItem}
                        onPress={() => handleKeywordSuggestionPress(keyword)}
                      >
                        <MaterialIcons
                          name="search"
                          size={24}
                          color="#8E8E93"
                          style={{ marginRight: 12 }}
                        />
                        <View style={{ flex: 1 }}>
                          <HighlightedText text={keyword} query={searchQuery} />
                        </View>
                        <MaterialIcons
                          name="north-west"
                          size={20}
                          color="#3A3A3C"
                        />
                      </Pressable>
                    ))}
                  </View>
                )}

                {/* Content Results */}
                {searchResults.length > 0 && (
                  <View>
                    <Text style={searchStyles.suggestionsHeader}>Results</Text>
                    {searchResults.map((item, index) => {
                      const content = item.Show || item.Episode;
                      const type = item.Show ? "Show" : "Episode";

                      if (!content) return null;

                      return (
                        <Pressable
                          key={`content-${index}`}
                          style={searchStyles.contentItem}
                          onPress={() => handleContentPress(item)}
                        >
                          <View style={searchStyles.contentImageContainer}>
                            <AutoResolvingImage
                              FileKey={content.MainImageFileKey}
                              type="PodcastPublicSource"
                              style={searchStyles.contentImage}
                            />
                          </View>
                          <View style={{ flex: 1 }}>
                            <Text
                              style={searchStyles.contentTitle}
                              numberOfLines={2}
                            >
                              {content.Name}
                            </Text>
                            <Text
                              style={searchStyles.contentType}
                              numberOfLines={1}
                            >
                              {type}
                            </Text>
                          </View>
                          <MaterialIcons
                            name="chevron-right"
                            size={24}
                            color="#3A3A3C"
                          />
                        </Pressable>
                      );
                    })}
                  </View>
                )}

                {/* No results */}
                {!isLoadingSearch &&
                  keywordSuggestions.length === 0 &&
                  searchResults.length === 0 && (
                    <View style={searchStyles.noResultsContainer}>
                      <MaterialIcons
                        name="search-off"
                        size={64}
                        color="#3A3A3C"
                      />
                      <Text style={searchStyles.noResultsText}>
                        No results found for "{searchQuery}"
                      </Text>
                    </View>
                  )}
              </View>
            )}
          </ScrollView>
        )}
      </View>
    </>
  );
} // Header styles from the original _layout file
const headerStyles = StyleSheet.create({
  headerContainer: {
    position: "absolute",
    left: 0,
    right: 0,
    top: 0,
    zIndex: 100,
  },
  headerBackground: {
    backgroundColor: "rgba(0,0,0,0.85)",
  },
  headerContent: {
    flex: 1,
    justifyContent: "flex-end",
    paddingBottom: 10,
  },
  titleContainer: {
    flexDirection: "row",
    alignItems: "center",
    justifyContent: "space-between",
    height: 50,
  },
  centeredTitleContainer: {
    position: "absolute",
    top: 0,
    left: 0,
    right: 0,
    bottom: 0,
    alignItems: "center",
    justifyContent: "center",
  },
  title: {
    fontSize: 32,
    fontWeight: "800",
    lineHeight: 40,
  },
  centeredTitle: {
    fontSize: 18,
    fontWeight: "700",
  },
  loginBtn: {
    paddingHorizontal: 16,
    paddingVertical: 8,
    borderRadius: 8,
  },
  loginText: {
    fontWeight: "600",
    color: "#000",
    fontSize: 15,
  },
  userBadge: {
    borderRadius: 9999,
    overflow: "hidden",
  },
  userAvatar: {
    width: 36,
    height: 36,
    borderRadius: 18,
    backgroundColor: "rgba(255,255,255,0.1)",
  },
  userText: {
    color: "#fff",
  },
});

// Original styles for tab navigation
const styles = StyleSheet.create({
  header: {
    padding: 16,
    borderBottomWidth: 1,
    borderBottomColor: "#222",
  },
  headerTitle: {
    fontSize: 28,
    fontWeight: "bold",
    color: "#AEE339",
  },
  tabContainer: {
    width: "100%",
  },
  tabItem: {
    flexDirection: "row",
    alignItems: "center",
    justifyContent: "space-between",
    paddingVertical: 16,
    paddingHorizontal: 16,
    borderBottomWidth: 1,
    borderBottomColor: "#222",
  },
  tabLeft: {
    flexDirection: "row",
    alignItems: "center",
  },
  tabIcon: {
    marginRight: 16,
  },
  tabTitle: {
    fontSize: 18,
    color: "white",
    fontWeight: "500",
  },
});

// Search-specific styles
const searchStyles = StyleSheet.create({
  searchBarContainer: {
    backgroundColor: "#000",
    zIndex: 10,
  },
  searchBarWrapper: {
    width: "100%",
  },
  searchBar: {
    flexDirection: "row",
    alignItems: "center",
    backgroundColor: "#1C1C1E",
    borderRadius: 10,
    paddingHorizontal: 12,
    paddingVertical: 10,
  },
  searchPlaceholder: {
    color: "#8E8E93",
    fontSize: 16,
  },
  activeSearchContainer: {
    flexDirection: "row",
    alignItems: "center",
    gap: 8,
  },
  searchInputWrapper: {
    flex: 1,
    flexDirection: "row",
    alignItems: "center",
    backgroundColor: "#1C1C1E",
    borderRadius: 10,
    paddingHorizontal: 12,
    paddingVertical: 10,
  },
  searchInput: {
    flex: 1,
    color: "#fff",
    fontSize: 16,
    padding: 0,
  },
  cancelButton: {
    paddingLeft: 8,
  },
  cancelText: {
    color: "#AEE339",
    fontSize: 16,
    fontWeight: "400",
  },
  categoryHeader: {
    paddingVertical: 16,
  },
  categoryHeaderText: {
    color: "#fff",
    fontSize: 22,
    fontWeight: "700",
  },
  categoryGrid: {
    flexDirection: "row",
    flexWrap: "wrap",
    gap: 12,
  },
  categoryCard: {
    width: "48%",
    aspectRatio: 1.5,
    borderRadius: 12,
    marginTop: 12,
    overflow: "hidden",
    backgroundColor: "#1C1C1E",
  },
  categoryOverlay: {
    ...StyleSheet.absoluteFillObject,
    backgroundColor: "rgba(0,0,0,0.3)",
    justifyContent: "center",
    alignItems: "center",
    padding: 16,
  },
  categoryName: {
    color: "#fff",
    fontSize: 16,
    fontWeight: "700",
    textAlign: "center",
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
  emptyState: {
    flex: 1,
    alignItems: "center",
    justifyContent: "center",
    paddingTop: 100,
  },
  emptyStateText: {
    color: "#8E8E93",
    fontSize: 16,
    marginTop: 16,
  },
  suggestionsContainer: {
    paddingHorizontal: 16,
    paddingTop: 8,
  },
  suggestionsHeader: {
    color: "#8E8E93",
    fontSize: 13,
    fontWeight: "600",
    textTransform: "uppercase",
    marginBottom: 8,
    paddingHorizontal: 4,
  },
  suggestionItem: {
    flexDirection: "row",
    alignItems: "center",
    paddingVertical: 12,
    borderBottomWidth: 1,
    borderBottomColor: "#1C1C1E",
  },
  suggestionTitle: {
    color: "#fff",
    fontSize: 16,
    fontWeight: "400",
  },
  suggestionType: {
    color: "#8E8E93",
    fontSize: 13,
    marginTop: 2,
  },
  contentItem: {
    flexDirection: "row",
    alignItems: "center",
    paddingVertical: 12,
    borderBottomWidth: 1,
    borderBottomColor: "#1C1C1E",
    gap: 12,
  },
  contentImageContainer: {
    width: 60,
    height: 60,
    borderRadius: 8,
    overflow: "hidden",
    backgroundColor: "#1C1C1E",
  },
  contentImage: {
    width: "100%",
    height: "100%",
  },
  contentTitle: {
    color: "#fff",
    fontSize: 16,
    fontWeight: "500",
  },
  contentType: {
    color: "#8E8E93",
    fontSize: 13,
    marginTop: 4,
  },
  noResultsContainer: {
    alignItems: "center",
    justifyContent: "center",
    paddingTop: 60,
    paddingHorizontal: 20,
  },
  noResultsText: {
    color: "#8E8E93",
    fontSize: 16,
    marginTop: 16,
    textAlign: "center",
  },
});
