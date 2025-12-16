import React from "react";
import {
  ScrollView,
  StyleSheet,
  Pressable,
  Alert,
  Animated,
  Image,
  Platform,
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

// Helper function to check if an icon exists
const isValidMaterialIcon = (name: string): boolean => {
  return name in MaterialIcons.glyphMap;
};

const tabNavigation = [
  {
    title: "Shows",
    iconName: "queue-music",
    to: "shows",
  },
  {
    title: "Channels",
    iconName: "podcasts",
    to: "channels",
  },
  {
    title: "Followed Podcasters",
    iconName: "people",
    to: "podcasters",
  },
  {
    title: "Saved",
    iconName: "download",
    to: "saved",
  },
  {
    title: "Bookings",
    iconName: "event-note",
    to: "bookings",
  },
];

const TabComponent = ({
  title,
  iconName,
  to,
}: {
  title: string;
  iconName: string;
  to: string;
}) => {
  const router = useRouter();
  const isValid = isValidMaterialIcon(iconName);

  if (!isValid) {
    console.warn(
      `Icon name '${iconName}' for tab '${title}' is not valid in MaterialIcons`
    );
  }

  const handleNavigateTab = (to: string) => {
    try {
      console.log(`Navigating to: /${to}`);

      switch (to) {
        case "shows":
          router.push("/library/shows");
          break;
        case "channels":
          router.push("/library/channels");
          break;
        case "podcasters":
          router.push("/library/podcasters");
          break;
        case "saved":
          router.push("/library/saved");
          break;
        case "bookings":
          router.push("/library/bookings");
          break;
        default:
          router.push("/library");
      }
    } catch (error) {
      console.error("Navigation error:", error);
      Alert.alert(
        "Navigation Error",
        "Could not navigate to the selected section."
      );
    }
  };

  return (
    <Pressable style={styles.tabItem} onPress={() => handleNavigateTab(to)}>
      <View style={styles.tabLeft}>
        <MaterialIcons
          name={
            (isValid ? iconName : "error") as React.ComponentProps<
              typeof MaterialIcons
            >["name"]
          }
          size={24}
          color="#AEE339"
          style={styles.tabIcon}
        />
        <Text style={styles.tabTitle}>{title}</Text>
      </View>
      <MaterialIcons name="chevron-right" size={24} color="#888" />
    </Pressable>
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
          <Text style={[headerStyles.title, { color: "#AEE339" }]}>
            Library
          </Text>

          {/* User badge or login button */}
          {authState.user ? (
            <Pressable style={headerStyles.userBadge} onPress={goToProfile}>
              {/* <Image
                source={{
                  uri:
                    authState.user.MainImageFileKey ||
                    `https://ui-avatars.com/api/?name=${encodeURIComponent(
                      authState.user.FullName
                    )}`,
                }}
                style={headerStyles.userAvatar}
              /> */}
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
            Library
          </Text>
        </Animated.View>
      </View>
    </Animated.View>
  );
};

export default function Library() {
  const { onScroll, headerHeight, scrollY } = useHeaderScroll();
  const tabBarHeight = useBottomTabBarHeight();
  const authState = useSelector((state: RootState) => state.auth);

  return (
    <>
      <Header scrollY={scrollY} />
      <ScrollView
        onScroll={onScroll}
        style={{ backgroundColor: "#000" }}
        contentContainerStyle={{
          paddingTop: headerHeight,
          paddingHorizontal: 10,
        }}
        scrollIndicatorInsets={{ top: headerHeight }}
        scrollEventThrottle={16}
      >
        {/* Tab Navigation */}
        <View style={styles.tabContainer}>
          {tabNavigation.map((tab, index) => (
            <TabComponent
              key={index}
              iconName={tab.iconName}
              title={tab.title}
              to={tab.to}
            />
          ))}
        </View>

        <View style={{ height: tabBarHeight + 50 }}></View>
      </ScrollView>
    </>
  );
}

// Header styles from the original _layout file
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
