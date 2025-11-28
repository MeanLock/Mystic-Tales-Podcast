import React from "react";
import { View, Pressable, ScrollView } from "react-native";
import { Text } from "@/src/components/ui/Text";
import { useSelector, useDispatch } from "react-redux";
import { RootState } from "@/src/store/store";
import { play, enqueue } from "@/src/features/mediaPlayer/playerSlice";

export default function DebugPlayer() {
  const dispatch = useDispatch();
  const { playerMode, currentAudio, queueAudios } = useSelector(
    (s: RootState) => s.player
  );

  const handleTestPlay = () => {
    console.log("🎵 Dispatching play action...");
    dispatch(
      play({
        audio: {
          Id: "test-123",
          Name: "Test Episode - Debug",
          LatestPosition: 0,
          AudioLength: 1800,
          MainFileKey: "test-key",
          ImageUrl:
            "https://i.pinimg.com/736x/2e/fd/49/2efd4937b8c2f24ecd7784ad30ad556e.jpg",
          PodcasterName: "Debug Podcast",
          Show: { Id: "1", Name: "Debug Show" },
        },
      })
    );
  };

  const handleAddToQueue = () => {
    console.log("➕ Adding to queue...");
    dispatch(
      enqueue({
        Id: `queue-${Date.now()}`,
        Name: "Queued Episode " + new Date().getSeconds(),
        AudioLength: 2400,
        MainFileKey: "queue-key",
        ImageUrl:
          "https://i.pinimg.com/564x/9d/0d/d4/9d0dd4b3d8d3c8b3d8d3c8b3d8d3c8b3.jpg",
        PodcasterName: "Queue Test",
        Show: { Id: "2", Name: "Queue Show" },
      })
    );
  };

  return (
    <ScrollView style={{ flex: 1, padding: 16, backgroundColor: "#000" }}>
      <Text className="text-white text-xl font-bold mb-4">
        🐛 Player Debug Panel
      </Text>

      {/* Status */}
      <View className="bg-gray-800 p-4 rounded-lg mb-4">
        <Text className="text-white font-semibold mb-2">Player Status:</Text>
        <Text className="text-green-400">
          Play Status: {playerMode.playStatus}
        </Text>
        <Text className="text-blue-400">Next Mode: {playerMode.nextMode}</Text>
        <Text className="text-yellow-400">
          Has Current Audio: {currentAudio ? "✅ YES" : "❌ NO"}
        </Text>
        <Text className="text-purple-400">
          Queue Length: {queueAudios.length}
        </Text>
      </View>

      {/* Current Audio Info */}
      {currentAudio && (
        <View className="bg-gray-800 p-4 rounded-lg mb-4">
          <Text className="text-white font-semibold mb-2">Current Audio:</Text>
          <Text className="text-gray-300 text-sm">ID: {currentAudio.Id}</Text>
          <Text className="text-gray-300 text-sm">
            Name: {currentAudio.Name}
          </Text>
          <Text className="text-gray-300 text-sm">
            Podcaster: {currentAudio.PodcasterName}
          </Text>
          <Text className="text-gray-300 text-sm">
            Position: {currentAudio.LatestPosition}s /{" "}
            {currentAudio.AudioLength}s
          </Text>
        </View>
      )}

      {/* Queue Info */}
      {queueAudios.length > 0 && (
        <View className="bg-gray-800 p-4 rounded-lg mb-4">
          <Text className="text-white font-semibold mb-2">
            Queue ({queueAudios.length} items):
          </Text>
          {queueAudios.slice(0, 3).map((audio, idx) => (
            <Text key={audio.Id} className="text-gray-300 text-sm">
              {idx + 1}. {audio.Name}
            </Text>
          ))}
          {queueAudios.length > 3 && (
            <Text className="text-gray-400 text-xs mt-1">
              ... and {queueAudios.length - 3} more
            </Text>
          )}
        </View>
      )}

      {/* Action Buttons */}
      <View className="gap-3">
        <Pressable
          onPress={handleTestPlay}
          className="bg-green-600 p-4 rounded-lg"
        >
          <Text className="text-white font-semibold text-center">
            🎵 Test Play Audio
          </Text>
        </Pressable>

        <Pressable
          onPress={handleAddToQueue}
          className="bg-blue-600 p-4 rounded-lg"
        >
          <Text className="text-white font-semibold text-center">
            ➕ Add to Queue
          </Text>
        </Pressable>

        <Pressable
          onPress={() => {
            console.log("📊 Full State:", {
              playerMode,
              currentAudio,
              queueAudios,
            });
          }}
          className="bg-purple-600 p-4 rounded-lg"
        >
          <Text className="text-white font-semibold text-center">
            📊 Log Full State to Console
          </Text>
        </Pressable>
      </View>

      {/* Raw JSON */}
      <View className="bg-gray-900 p-4 rounded-lg mt-4">
        <Text className="text-white font-semibold mb-2">Raw State JSON:</Text>
        <Text className="text-gray-400 text-xs font-mono">
          {JSON.stringify({ playerMode, currentAudio, queueAudios }, null, 2)}
        </Text>
      </View>
    </ScrollView>
  );
}
