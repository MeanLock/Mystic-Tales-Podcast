import { EqualizerVariant1 } from "@/src/components/equalizer/Variant1";
import { usePlayer } from "@/src/core/services/player/usePlayer";
import { CompletedBookingTrack } from "@/src/core/types/booking.type";
import TimeUtil from "@/src/core/utils/time";
import { MaterialCommunityIcons } from "@expo/vector-icons";
import { Image, Pressable, Text } from "react-native";
import { View } from "react-native";

const BookingTrackRow = ({
  track,
  index,
}: {
  track: CompletedBookingTrack;
  index: number;
}) => {
  const { play, pause, state: uiState, listenFromBookingTrack } = usePlayer();

  const handlePlayPause = () => {
    console.log("Handle play/pause for track:", track.Id);
    if (uiState.currentAudio) {
      if (uiState.isPlaying && uiState.currentAudio.id === track.Id) {
        pause();
      } else if (!uiState.isPlaying && uiState.currentAudio.id === track.Id) {
        play();
      } else {
        listenFromBookingTrack(track.Id, track.BookingId);
      }
    } else {
      listenFromBookingTrack(track.Id, track.BookingId);
    }
  };

  return (
    <View className="w-full h-[100px] flex flex-row items-center justify-center border-b-[0.5px] border-b-[#333]">
      <View className="w-[70px] aspect-square flex items-center justify-center">
        <Image
          source={{
            uri: "https://i.pinimg.com/736x/bc/87/60/bc876086e434230663a42cc57d16c151.jpg",
          }}
          className="w-full h-full rounded-md"
        />
      </View>
      <View className="flex-1 h-[70px] flex flex-col px-4">
        <Text numberOfLines={1} className="text-white font-bold w-5/6">
          Track - {index + 1}
        </Text>
        <Text className="text-sm text-white">
          {TimeUtil.formatAudioLength(track.AudioLength, "numberOnly")}
        </Text>
        <View className="flex-1 items-start justify-end pb-2">
          {uiState.isPlaying &&
          uiState.currentAudio &&
          uiState.currentAudio.id === track.Id ? (
            <Pressable
              onPress={() => handlePlayPause()}
              className="flex flex-row items-center gap-2 bg-white/60 px-2 py-1 rounded-lg"
            >
              <EqualizerVariant1 />
              <View className="w-[30px] h-[4px] bg-white rounded-full flex flex-row items-center justify-start">
                <View
                  style={{
                    width: `${(uiState.currentTime / uiState.duration) * 100}%`,
                  }}
                  className="h-[4px] bg-primaryThemeColor rounded-l-full"
                />
              </View>
              <Text className="text-xs text-white font-bold">
                {TimeUtil.formatAudioLength(uiState.currentTime, "numberOnly")}
              </Text>
            </Pressable>
          ) : uiState.currentAudio && uiState.currentAudio.id === track.Id ? (
            <Pressable
              onPress={() => handlePlayPause()}
              className="flex flex-row items-center gap-2 bg-white/60 px-2 py-1 rounded-lg"
            >
              <MaterialCommunityIcons name="play" size={16} color="white" />
              <View className="w-[30px] h-[4px] bg-white rounded-full flex flex-row items-center justify-start">
                <View
                  style={{
                    width: `${(uiState.currentTime / uiState.duration) * 100}%`,
                  }}
                  className="h-[4px] bg-primaryThemeColor rounded-l-full"
                />
              </View>
              <Text className="text-xs text-white font-bold">
                {TimeUtil.formatAudioLength(uiState.currentTime, "numberOnly")}
              </Text>
            </Pressable>
          ) : (
            <Pressable
              onPress={() => handlePlayPause()}
              className="flex flex-row items-center gap-1 bg-white/60 px-2 py-1 rounded-lg"
            >
              <MaterialCommunityIcons name="play" size={16} color="white" />
              <Text className="text-xs text-white font-bold">Play Track</Text>
            </Pressable>
          )}
        </View>
      </View>
    </View>
  );
};

export default BookingTrackRow;
