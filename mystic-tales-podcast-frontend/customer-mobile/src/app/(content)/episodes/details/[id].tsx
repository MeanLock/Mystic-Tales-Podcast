import { Text } from "@/src/components/ui/Text";
import { View } from "@/src/components/ui/View";
import { useLocalSearchParams, useRouter } from "expo-router";
import { useState } from "react";

export default function EpisdeDetailsScreen() {
  // STATES
  const [episode, setEpisode] = useState(null);
  
  // HOOKS
  const { id } = useLocalSearchParams<{ id: string }>();
  const router = useRouter();

  return (
    <View>
      <Text>Episode: {id}</Text>
    </View>
  );
}
