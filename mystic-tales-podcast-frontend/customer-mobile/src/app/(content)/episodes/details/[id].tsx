import Loader from "@/src/components/loaders/Loader";
import { Text } from "@/src/components/ui/Text";
import { View } from "@/src/components/ui/View";
import { EpisodeWithImageUrl } from "@/src/types/episode";
import { useFocusEffect, useLocalSearchParams, useRouter } from "expo-router";
import { useCallback, useEffect, useState } from "react";
import { ActivityIndicator, Alert, Pressable, ScrollView } from "react-native";
import EpisodeInformations from "./components/EpisodeInformations";
import EpisodeDescription from "./components/EpisodeDescription";
import DebugPlayer from "./components/DebugPlayer";
import {
  useGetEpisodeDetailsQuery,
  useSaveEpisodeMutation,
} from "@/src/core/services/episode/episode.service";

export default function EpisdeDetailsScreen() {
  // STATES
  const [isSaved, setIsSaved] = useState<boolean>(false);

  // HOOKS
  const { id } = useLocalSearchParams<{ id: string }>();
  const router = useRouter();

  const {
    data: episodeData,
    isLoading: isEpisodeLoading,
    refetch: refetchEpisode,
  } = useGetEpisodeDetailsQuery({ PodcastEpisodeId: id! }, { skip: !id });

  const [saveEpisode, { isLoading: isSaveEpisodeLoading }] =
    useSaveEpisodeMutation();

  useEffect(() => {
    if (!episodeData || !episodeData.Episode) return;
    setIsSaved(episodeData.Episode.IsSavedByCurrentUser);
  }, [episodeData, isEpisodeLoading]);

  const handleSaveEpisode = async () => {
    if (!episodeData || !episodeData.Episode) return;
    const snapShotValue = episodeData.Episode.IsSavedByCurrentUser;
    setIsSaved(!episodeData.Episode.IsSavedByCurrentUser);
    try {
      await saveEpisode({
        PodcastEpisodeId: episodeData.Episode.Id,
        IsSave: !episodeData.Episode.IsSavedByCurrentUser,
      }).unwrap();

      refetchEpisode();
    } catch (error) {
      Alert.alert("Failed to update saved status. Please try again later.");
      setIsSaved(snapShotValue);
    }
  };

  if (isEpisodeLoading) {
    return (
      <View className="w-full h-screen items-center justify-center gap-5">
        <ActivityIndicator size="large" color="#AEE339" />
        <Text>Loading ...</Text>
      </View>
    );
  }

  if (!episodeData) {
    return (
      <View className="w-full h-screen items-center justify-center gap-3">
        <Text className="text-gray-500 font-bold">
          Can't Find This Episode Due To Some Reasons :(
        </Text>
        <Pressable onPress={() => router.back()}>
          <Text className="text-[#AEE339] underline">Go back</Text>
        </Pressable>
      </View>
    );
  }
  return (
    <ScrollView showsVerticalScrollIndicator={false}>
      <EpisodeInformations
        onSaveToggle={handleSaveEpisode}
        isSaved={isSaved}
        episode={episodeData.Episode}
      />
      <EpisodeDescription description={episodeData.Episode.Description} />
      {/* <DebugPlayer /> */}
    </ScrollView>
  );
}
