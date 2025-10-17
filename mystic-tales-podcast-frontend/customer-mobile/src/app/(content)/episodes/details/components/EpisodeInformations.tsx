import { Text } from "@/src/components/ui/Text";
import { View } from "@/src/components/ui/View";
import { mockEpisodes } from "@/src/data/mockEpisodes";
import { enqueue, play } from "@/src/features/mediaPlayer/playerSlice";
import { formatAudioLength, formatDateRange } from "@/src/lib/format";
import { EpisodeWithImageUrl } from "@/src/types/episode";
import {
  Feather,
  FontAwesome,
  MaterialIcons,
  Octicons,
} from "@expo/vector-icons";
import { useRouter } from "expo-router";
import { useState } from "react";
import {
  Image,
  ImageBackground,
  Platform,
  Pressable,
  StyleSheet,
} from "react-native";
import { useDispatch } from "react-redux";

type EpisodeInfoProps = Pick<
  EpisodeWithImageUrl,
  "Id" | "Name" | "Description" | "ImageUrl" | "AudioLength" | "ReleaseDate"
>;

const EpisodeInformations = ({
  Id,
  Name,
  Description,
  ImageUrl,
  AudioLength,
  ReleaseDate,
}: EpisodeInfoProps) => {
  const router = useRouter();
  const [isFavorited, setIsFavorited] = useState(false);

  const dispatch = useDispatch();

  const handlePlayEpisode = () => {
    console.log("Force init player with mock data...");
    dispatch(
      play({
        audio: {
          Id: mockEpisodes[0].Id,
          Name: mockEpisodes[0].Name,
          LatestPosition: 0,
          AudioLength: mockEpisodes[0].AudioLength,
          MainFileKey: mockEpisodes[0].AudioFileKey,
          ImageUrl: mockEpisodes[0].ImageUrl,
          PodcasterName: "Hoàng Minh Lộc",
          Show: {
            Id: mockEpisodes[0].PodcastShowId,
            Name: "Khi em lớn",
          },
        },
      })
    );
  };

  const handleAddToQueue = () => {
    console.log("Add To Queue...");
    dispatch(
      enqueue({
        Id: mockEpisodes[1].Id,
        Name: mockEpisodes[1].Name,
        AudioLength: mockEpisodes[1].AudioLength,
        MainFileKey: mockEpisodes[1].AudioFileKey,
        ImageUrl: mockEpisodes[1].ImageUrl,
        PodcasterName: "Wrxdie",
        Show: {
          Id: mockEpisodes[0].PodcastShowId,
          Name: "Cất đi mai chơi nốt",
        },
      })
    );
  };
  const handleAddToQueue2 = () => {
    console.log("Add To Queue...");
    dispatch(
      enqueue({
        Id: mockEpisodes[2].Id,
        Name: mockEpisodes[2].Name,
        AudioLength: mockEpisodes[2].AudioLength,
        MainFileKey: mockEpisodes[2].AudioFileKey,
        ImageUrl: mockEpisodes[2].ImageUrl,
        PodcasterName: "BAN MAI",
        Show: {
          Id: mockEpisodes[2].PodcastShowId,
          Name: "VLEU",
        },
      })
    );
  };
  const handleAddToQueue3 = () => {
    console.log("Add To Queue...");
    dispatch(
      enqueue({
        Id: mockEpisodes[3].Id,
        Name: mockEpisodes[3].Name,
        AudioLength: mockEpisodes[3].AudioLength,
        MainFileKey: mockEpisodes[3].AudioFileKey,
        ImageUrl: mockEpisodes[3].ImageUrl,
        PodcasterName: "Lauv",
        Show: {
          Id: mockEpisodes[3].PodcastShowId,
          Name: "Cất đi mai chơi nốt",
        },
      })
    );
  };
  const handleAddToQueue4 = () => {
    console.log("Add To Queue...");
    dispatch(
      enqueue({
        Id: mockEpisodes[4].Id,
        Name: mockEpisodes[4].Name,
        AudioLength: mockEpisodes[4].AudioLength,
        MainFileKey: mockEpisodes[4].AudioFileKey,
        ImageUrl: mockEpisodes[4].ImageUrl,
        PodcasterName: "Wrxdie",
        Show: {
          Id: mockEpisodes[4].PodcastShowId,
          Name: "Cất đi mai chơi nốt",
        },
      })
    );
  };

  return (
    <View style={styles.containerWrapper}>
      <ImageBackground
        source={{
          uri: ImageUrl,
        }}
        style={[StyleSheet.absoluteFill]}
        blurRadius={60} // High blur radius
      >
        {/* Dark overlay to dim the background image */}
        <View
          style={[
            StyleSheet.absoluteFill,
            { backgroundColor: "rgba(0, 0, 0, 0.6)" },
          ]}
        />
      </ImageBackground>

      <View style={styles.container}>
        <View style={styles.navigationContainer}>
          <View>
            {Platform.OS === "ios" ? (
              <Pressable onPress={() => router.back()} style={styles.backIcon}>
                <MaterialIcons
                  style={{ padding: 0, margin: 0 }}
                  name="keyboard-arrow-left"
                  color={"#fff"}
                  size={25}
                />
              </Pressable>
            ) : (
              <Pressable onPress={() => router.back()} style={styles.backIcon}>
                <MaterialIcons name="arrow-back" color={"#fff"} />
              </Pressable>
            )}
          </View>
          <View className="flex-row items-center gap-3">
            <Pressable
              onPress={() => setIsFavorited(!isFavorited)}
              style={styles.backIcon}
            >
              {isFavorited ? (
                <FontAwesome name="heart" color="#aee339" size={16} />
              ) : (
                <Feather name="heart" color="#fff" size={16} />
              )}
            </Pressable>
            <Pressable style={styles.backIcon}>
              <Octicons name="download" color="#fff" size={18} />
            </Pressable>
            <Pressable style={styles.backIcon}>
              <Feather name="more-horizontal" color="#fff" size={24} />
            </Pressable>
          </View>
        </View>

        <View style={styles.episodeImageContainer}>
          <Image style={styles.episodeImage} source={{ uri: ImageUrl }} />
        </View>

        <View style={styles.informationContainer}>
          <Text className="text-sm text-gray-300">
            {formatDateRange(ReleaseDate)} • Episode 10 •{" "}
            {formatAudioLength(AudioLength)}
          </Text>
          <Text numberOfLines={1} className="text-white font-bold text-[20px]">
            {Name}
          </Text>

          <View className="w-full flex items-center justify-center">
            <Pressable
              onPress={() => handlePlayEpisode()}
              style={styles.playButton}
            >
              <MaterialIcons name="play-arrow" size={25} color={"#aee339"} />
              <Text className="font-bold text-[#aee339]">Play</Text>
            </Pressable>
            <Pressable
              onPress={() => handleAddToQueue()}
              style={styles.playButton}
              className="mt-2"
            >
              <MaterialIcons name="add" size={25} color={"#aee339"} />
              <Text className="font-bold text-[#aee339]">Add To Queue</Text>
            </Pressable>
            <Pressable
              onPress={() => handleAddToQueue2()}
              style={styles.playButton}
              className="mt-2"
            >
              <MaterialIcons name="add" size={25} color={"#aee339"} />
              <Text className="font-bold text-[#aee339]">Add To Queue2</Text>
            </Pressable>
            <Pressable
              onPress={() => handleAddToQueue3()}
              style={styles.playButton}
              className="mt-2"
            >
              <MaterialIcons name="add" size={25} color={"#aee339"} />
              <Text className="font-bold text-[#aee339]">Add To Queue3</Text>
            </Pressable>
            <Pressable
              onPress={() => handleAddToQueue4()}
              style={styles.playButton}
              className="mt-2"
            >
              <MaterialIcons name="add" size={25} color={"#aee339"} />
              <Text className="font-bold text-[#aee339]">Add To Queue4</Text>
            </Pressable>
          </View>
        </View>
      </View>
    </View>
  );
};

export default EpisodeInformations;

const styles = StyleSheet.create({
  containerWrapper: {
    width: "100%",
    overflow: "hidden",
  },
  container: {
    width: "100%",
    paddingTop: 60,
    paddingBottom: 20,
    paddingHorizontal: 20,
    gap: 10,
  },
  navigationContainer: {
    width: "100%",
    flexDirection: "row",
    justifyContent: "space-between",
    height: 30,
  },
  backIcon: {
    padding: 2,
    borderRadius: 9999,
    backgroundColor: "rgba(217, 217, 217, 0.2)",
    width: 30,
    height: 30,
    alignItems: "center",
    justifyContent: "center",
  },
  episodeImageContainer: {
    width: "100%",
    height: 220,
    alignItems: "center",
    justifyContent: "center",
    paddingVertical: 10,
    resizeMode: "cover",
  },
  episodeImage: {
    width: 220,
    height: 220,
    resizeMode: "cover",
    borderRadius: 8,
  },
  informationContainer: {
    width: "100%",
    alignItems: "center",
    justifyContent: "center",
    marginTop: 10,
    gap: 10,
  },
  playButton: {
    width: "75%",
    paddingVertical: 10,
    borderRadius: 5,
    backgroundColor: "rgba(000, 000, 000, 0.6)",
    alignItems: "center",
    justifyContent: "center",
    flexDirection: "row",
    gap: 5,
  },
});
