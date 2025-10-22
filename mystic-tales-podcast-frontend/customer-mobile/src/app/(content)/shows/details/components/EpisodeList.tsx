import { Text } from "@/src/components/ui/Text";
import { View } from "@/src/components/ui/View";
import { MaterialIcons } from "@expo/vector-icons";
import { Image, Pressable, StyleSheet } from "react-native";
import { useMemo } from "react";
import { useRouter } from "expo-router";

type Episode = {
  Id: string;
  Name: string;
  Description: string;
  ExplicitContent: boolean;
  ReleaseDate: string;
  IsReleased: boolean;
  ImageUrl: string;
  AudioFileKey: string;
  AudioFileSize: number;
  AudioLength: number;
  AudioFingerprint: string;
  PodcastEpisodeSubscriptionType: { Id: number; Name: string };
  PodcastShowId: string;
  SeasonNumber: number;
  TotalSave: number;
  ListenCount: number;
  IsAudioPublishable: boolean;
  TakenDownReason: string | null;
  DeletedAt: string | null;
  CreatedAt: string;
  UpdatedAt: string;
};

interface EpisodeListProps {
  episodes: Episode[];
}

// Format date based on time difference
const formatDate = (dateString: string): string => {
  try {
    const date = new Date(dateString);
    const now = new Date();

    // If invalid date, return the original string
    if (isNaN(date.getTime())) {
      return dateString;
    }

    const diffMs = now.getTime() - date.getTime();
    const diffHours = Math.floor(diffMs / (1000 * 60 * 60));
    const diffDays = Math.floor(diffMs / (1000 * 60 * 60 * 24));

    // Less than 1 day: show hours
    if (diffHours < 24) {
      return diffHours === 0
        ? "Just now"
        : `${diffHours} ${diffHours === 1 ? "hour" : "hours"} ago`;
    }

    // Less than 5 days: show days
    if (diffDays < 5) {
      return `${diffDays} ${diffDays === 1 ? "day" : "days"} ago`;
    }

    // More than 5 days: format as DD/MM/YYYY
    const day = date.getDate().toString().padStart(2, "0");
    const month = (date.getMonth() + 1).toString().padStart(2, "0");
    const year = date.getFullYear();

    return `${day}/${month}/${year}`;
  } catch (error) {
    console.error("Error formatting date:", error);
    return dateString;
  }
};

// Format audio length from seconds to human-readable format
const formatAudioLength = (seconds: number): string => {
  if (!seconds || seconds <= 0) {
    return "0 sec";
  }

  const hours = Math.floor(seconds / 3600);
  const minutes = Math.floor((seconds % 3600) / 60);
  const remainingSeconds = Math.floor(seconds % 60);

  // Format based on the length:
  // 1. If hours > 0: "X hours Y mins"
  // 2. If only minutes > 0: "X mins Y secs"
  // 3. If only seconds > 0: "X secs"

  if (hours > 0) {
    return `${hours} ${hours === 1 ? "hour" : "hours"} ${
      minutes > 0 ? `${minutes} ${minutes === 1 ? "min" : "mins"}` : ""
    }`.trim();
  } else if (minutes > 0) {
    return `${minutes} ${minutes === 1 ? "min" : "mins"} ${
      remainingSeconds > 0
        ? `${remainingSeconds} ${remainingSeconds === 1 ? "sec" : "secs"}`
        : ""
    }`.trim();
  } else {
    return `${remainingSeconds} ${remainingSeconds === 1 ? "sec" : "secs"}`;
  }
};

// Episode Component
const EpisodeComponent = ({ episode }: { episode: Episode }) => {
  const router = useRouter();

  return (
    <Pressable
      onPress={() => router.push(`/(content)/episodes/details/${episode.Id}`)}
      style={[style.episodeContainer, style.borderBottom]}
    >
      <View className="w-[70%] justify-between">
        <Text style={style.dateText}>{formatDate(episode.ReleaseDate)}</Text>
        <View className="w-full gap-2">
          <Text numberOfLines={2} className="font-bold text-white text-[20px]">
            {episode.Name}
          </Text>
          <Text numberOfLines={3}>{episode.Description}</Text>
        </View>
        <View className="w-full items-start mt-5">
          <Pressable style={style.playButton} className="w-8/12">
            <MaterialIcons name="play-arrow" size={15} color={"#AEE339"} />
            <Text
              numberOfLines={1}
              className="text-[10px] font-bold text-[#AEE339]"
            >
              {formatAudioLength(episode.AudioLength)}
            </Text>
          </Pressable>
        </View>
      </View>
      <View className="flex-1  min-w-[51px] items-end justify-between mt-1">
        <View>
          <Image
            source={{ uri: episode.ImageUrl }}
            className="w-[80px] h-[80px]"
          />
        </View>
        <View>
          <Pressable>
            <MaterialIcons name="more-horiz" size={18} color={"#D9D9D9"} />
          </Pressable>
        </View>
      </View>
    </Pressable>
  );
};

const EpisodeList = ({ episodes }: EpisodeListProps) => {
  // Get the 4 most recent episodes sorted by ReleaseDate
  const latestEpisodes = useMemo(() => {
    // Create a copy of episodes to avoid mutating the original array
    return (
      [...episodes]
        // Sort by ReleaseDate in descending order (newest first)
        .sort((a, b) => {
          const dateA = new Date(a.ReleaseDate).getTime();
          const dateB = new Date(b.ReleaseDate).getTime();
          return dateB - dateA; // Descending order
        })
        // Take only the first 4 episodes
        .slice(0, 4)
    );
  }, [episodes]);

  return (
    <View className="w-full">
      <Pressable
        style={style.borderBottom}
        className="w-full flex-row items-center justify-between pb-6"
      >
        <Text className="text-[30px] font-bold">Episodes</Text>
        <View style={style.iconContainer}>
          <MaterialIcons name="keyboard-arrow-right" color={"#fff"} size={25} />
        </View>
      </Pressable>

      {/* Map through the 4 latest episodes */}
      {latestEpisodes.map((episode) => (
        <EpisodeComponent key={episode.Id} episode={episode} />
      ))}

      {/* Show "See all" button if there are more than 4 episodes */}
      {episodes.length > 4 && (
        <Pressable className="w-full flex flex-row items-center justify-between py-3">
          <Text className="font-bold text-[#AEE339]">
            See all ({episodes.length})
          </Text>
          <MaterialIcons
            name="keyboard-arrow-right"
            size={25}
            color={"#D9D9D9"}
          />
        </Pressable>
      )}
    </View>
  );
};

export default EpisodeList;

const style = StyleSheet.create({
  container: {},
  borderBottom: {
    borderBottomWidth: 0.3,
    borderBottomColor: "#514F4F",
  },
  iconContainer: {
    display: "flex",
    alignItems: "center",
    justifyContent: "center",
  },
  episodeContainer: {
    paddingVertical: 10,
    width: "100%",
    flexDirection: "row",
  },
  dateText: {
    color: "#999",
    fontSize: 12,
    marginBottom: 4,
  },
  playButton: {
    backgroundColor: "rgba(217, 217, 217, 0.2)",
    paddingVertical: 5,
    flexDirection: "row",
    alignItems: "center",
    justifyContent: "center",
    gap: 2,
    borderRadius: 8,
  },
});
