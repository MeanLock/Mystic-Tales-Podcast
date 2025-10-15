import Loader from "@/src/components/loaders/Loader";
import { Text } from "@/src/components/ui/Text";
import { View } from "@/src/components/ui/View";
import { EpisodeWithImageUrl } from "@/src/types/episode";
import { useFocusEffect, useLocalSearchParams, useRouter } from "expo-router";
import { useCallback, useEffect, useState } from "react";
import { ActivityIndicator, Pressable, ScrollView } from "react-native";
import EpisodeInformations from "./components/EpisodeInformations";
import EpisodeDescription from "./components/EpisodeDescription";

const epsideMockData: EpisodeWithImageUrl = {
  Id: "a1c2f301-010",
  Name: "Tập 10: Những Đứa Trẻ Không Hồn",
  Description: `<p>Tiếp tục với câu chuyện từ tập trước, Alice làm theo những gì chiếc gương kỳ bí bảo. Cô đi vào ngôi nhà đầy rêu đó, bên trong toàn là những món ăn mà lâu rồi cô chưa được ăn. Đang tự nhủ rằng cô sẽ đánh 1 bữa thật no thì có tiếng động ... Ngôi nhà này <span style="color: var(--ui-editor-text-red)"><strong>không vô chủ</strong></span> ! Alice vội trốn vào một góc tủ, thì cô thấy một đám người gồm 3 đứa trẻ và 1 người lớn. Họ cùng nhau ngồi ăn trưa với đám đồ ăn mà Alice thèm nhỏ dãi.</p><p></p><p>Cô để ý rằng, mắt của bọn trẻ trống rỗng, sâu hoắm vào trong, đầu không 1 sợi tóc. Nhìn chúng thật kinh dị, và quái đản ... và có vẻ như chúng đã nhận ra sự hiện diện của Alice trong căn nhà ...</p><p></p><p></p><ul><li><p>Chào mọi người, tui là Lộc - podcaster đẹp trai của show này ! Mọi thông tin chi tiết xin liên hệ:</p></li><li><p>Facebook: <a target="_blank" rel="noopener noreferrer nofollow" href="https://www.facebook.com/mikely.soryzz"><span style="color: var(--ui-editor-text-green)"><em>https://www.facebook.com/mikely.soryzz</em></span></a></p></li><li><p><strong>Phone<em>:</em></strong><em> 089.689.3636</em></p></li></ul>`,
  ExplicitContent: true,
  ReleaseDate: "2025-03-15T09:00:00.000Z",
  IsReleased: true,
  ImageUrl:
    "https://i.pinimg.com/736x/2e/fd/49/2efd4937b8c2f24ecd7784ad30ad556e.jpg",
  AudioFileKey: "10",
  AudioFileSize: 52000000,
  AudioLength: 3100,
  AudioFingerprint: "©189364",
  PodcastEpisodeSubscriptionType: { Id: 1, Name: "Subcribed Show Only" },
  PodcastShowId: "1",
  SeasonNumber: 1,
  TotalSave: 300,
  ListenCount: 1600,
  IsAudioPublishable: true,
  DeletedAt: "",
  TakenDownReason: "",
  CreatedAt: "2025-02-15T11:56:06.138Z",
  UpdatedAt: "2025-02-15T11:56:06.138Z",
};

export default function EpisdeDetailsScreen() {
  // STATES
  const [episode, setEpisode] = useState<EpisodeWithImageUrl | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  // HOOKS
  const { id } = useLocalSearchParams<{ id: string }>();
  const router = useRouter();

  // useFocusEffect(
  //   useCallback(() => {
  //     if (id && id !== "") {
  //       fetch(id);
  //     }
  //     return () => {};
  //   }, [])
  // );

  useEffect(() => {
    if (id && id !== "") {
      fetch(id);
    }
  }, []);

  const fetch = async (id: string) => {
    setTimeout(() => {
      setEpisode(epsideMockData);
      setIsLoading(false);
    }, 300);
  };

  if (isLoading) {
    return (
      <View className="w-full h-screen items-center justify-center gap-3">
        <ActivityIndicator />
        <Text>Loading ...</Text>
      </View>
    );
  }

  if (!episode || !episode.Id || episode.Id === "") {
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
      <EpisodeInformations {...episode} />
      <EpisodeDescription description={episode.Description} />
    </ScrollView>
  );
}
