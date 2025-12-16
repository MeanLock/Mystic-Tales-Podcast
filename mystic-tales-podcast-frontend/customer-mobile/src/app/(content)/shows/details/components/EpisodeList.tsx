import { Text } from "@/src/components/ui/Text";
import { View } from "@/src/components/ui/View";
import { MaterialIcons } from "@expo/vector-icons";
import { Image, Pressable, StyleSheet } from "react-native";
import { useMemo } from "react";
import { useRouter } from "expo-router";
import { EpisodeFromShow } from "@/src/core/types/episode.type";
import AutoResolvingImage from "@/src/components/autoResolveImage/AutoResolvingImage";
import PlayButtonVariant1 from "@/src/components/buttons/playButton/playButtonVariant1";
import PlayButtonVariant2 from "@/src/components/buttons/playButton/playButtonVariant2";
import { useDispatch } from "react-redux";
import {
  setEpisodes,
  setEpisodesData,
} from "@/src/features/episode/episodeSlice";
import { playAudio } from "@/src/features/mediaPlayer/playerSlice";
import { usePlayer } from "@/src/core/services/player/usePlayer";
import HtmlText from "@/src/components/renderHtml/HtmlText";

interface EpisodeListProps {
  episodes: EpisodeFromShow[];
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

// Episode Component
const EpisodeComponent = ({ episode }: { episode: EpisodeFromShow }) => {
  const router = useRouter();

  const { listenFromEpisode } = usePlayer();

  const handlePlayEpisode = () => {
    listenFromEpisode(episode.Id, "SpecifyShowEpisodes");
  };

  return (
    <Pressable
      onPress={() => router.push(`/(content)/episodes/details/${episode.Id}`)}
      style={[style.episodeContainer, style.borderBottom]}
    >
      <View className="w-[70%] justify-between">
        <Text style={style.dateText}>{formatDate(episode.ReleaseDate)}</Text>
        <View className="w-full gap-2">
          <Text className="text-white text-[20px] font-bold" numberOfLines={2}>
            {episode.Name}
          </Text>
          <HtmlText html={episode.Description} color="#fff" numberOfLines={3} />
        </View>
        <View className="w-full items-start mt-5">
          <PlayButtonVariant2
            episodeId={episode.Id}
            audioLength={episode.AudioLength}
            onPlayPress={() => handlePlayEpisode()}
          />
        </View>
      </View>
      <View className="flex-1  min-w-[51px] items-end justify-between mt-1">
        <View>
          <AutoResolvingImage
            FileKey={episode.MainImageFileKey}
            type="PodcastPublicSource"
            style={{ width: 80, height: 80 }}
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

  const dispatch = useDispatch();
  const router = useRouter();
  const handleViewMoreEpisodesFromShow = () => {
    // Implement navigation or action to view more episodes from the show
    dispatch(
      setEpisodesData({
        episodes: episodes as EpisodeFromShow[],
        title: `${episodes[0]?.PodcastShow?.Name || "Episodes"}`,
        from: "ShowDetails",
      })
    );
    // Navigate to the episodes list page
    router.push(`/(content)/episodes`);
  };

  return (
    <View className="w-full">
      <Pressable
        style={style.borderBottom}
        onPress={() => handleViewMoreEpisodesFromShow()}
        className="w-full flex-row items-center justify-between pb-6"
      >
        <Text className="text-[30px] font-bold text-white">Episodes</Text>
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
        <Pressable
          onPress={() => handleViewMoreEpisodesFromShow()}
          className="w-full flex flex-row items-center justify-between py-3"
        >
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
