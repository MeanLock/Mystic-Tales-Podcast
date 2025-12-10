import React from "react";
import { ActivityIndicator, ScrollView } from "react-native";
import { useHeaderScroll } from "./_layout";
import { useBottomTabBarHeight } from "@react-navigation/bottom-tabs";
import { View } from "@/src/components/ui/View";
import MixxingText from "@/src/components/ui/MixxingText";
import ShowCarousel from "./components/ShowCarousel/ShowCarousel";
import EpisodeCarousel from "./components/EpisodeCarousel/EpisodeCarousel";
import PodcasterCarousel from "./components/PodcasterCarousel/PodcasterCarousel";
import { useSelector } from "react-redux";
import { RootState } from "@/src/store/store";
import { useGetDiscoveryFeedQuery } from "@/src/core/services/feed/feed.service";
import { Text } from "@/src/components/ui/Text";
import EpisodeContinueCarousel from "./components/EpisodeCarousel/EpisodeContinueCarousel";
import ChannelCarousel from "./components/ChannelCarousel/ChannelCarousel";

const data1 = [
  {
    Id: 1,
    ImageUrl:
      "https://i.pinimg.com/736x/5e/d1/97/5ed197fcebffd71238c9472b72f3c43d.jpg",
  },
  {
    Id: 2,
    ImageUrl:
      "https://i.pinimg.com/736x/62/3b/19/623b1953e2f4a46c9199d40d8698c164.jpg",
  },
  {
    Id: 3,
    ImageUrl:
      "https://i.pinimg.com/736x/c9/ba/d2/c9bad2ea3e9f5961fbdb91cf6acffcbf.jpg",
  },
  {
    Id: 4,
    ImageUrl:
      "https://i.pinimg.com/736x/03/93/7b/03937bb7a29c91227d76a0ab4808987a.jpg",
  },
  {
    Id: 5,
    ImageUrl:
      "https://i.pinimg.com/736x/0c/35/5d/0c355dd83d1a6d9b1e90507af599d637.jpg",
  },
  {
    Id: 6,
    ImageUrl:
      "https://i.pinimg.com/736x/5c/09/1b/5c091b3c4e7fc583aba67d116b14ab1e.jpg",
  },
  {
    Id: 7,
    ImageUrl:
      "https://i.pinimg.com/736x/42/ce/69/42ce690c0639c2e4b13b17e0c2d02018.jpg",
  },
];

const data2 = [
  {
    Id: 1,
    ImageUrl:
      "https://i.pinimg.com/736x/5e/d1/97/5ed197fcebffd71238c9472b72f3c43d.jpg",
    Top: 1,
  },
  {
    Id: 2,
    ImageUrl:
      "https://i.pinimg.com/736x/62/3b/19/623b1953e2f4a46c9199d40d8698c164.jpg",
    Top: 2,
  },
  {
    Id: 3,
    ImageUrl:
      "https://i.pinimg.com/736x/c9/ba/d2/c9bad2ea3e9f5961fbdb91cf6acffcbf.jpg",
    Top: 3,
  },
  {
    Id: 4,
    ImageUrl:
      "https://i.pinimg.com/736x/03/93/7b/03937bb7a29c91227d76a0ab4808987a.jpg",
    Top: 4,
  },
  {
    Id: 5,
    ImageUrl:
      "https://i.pinimg.com/736x/0c/35/5d/0c355dd83d1a6d9b1e90507af599d637.jpg",
    Top: 5,
  },
  {
    Id: 6,
    ImageUrl:
      "https://i.pinimg.com/736x/5c/09/1b/5c091b3c4e7fc583aba67d116b14ab1e.jpg",
    Top: 6,
  },
  {
    Id: 7,
    ImageUrl:
      "https://i.pinimg.com/736x/42/ce/69/42ce690c0639c2e4b13b17e0c2d02018.jpg",
    Top: 7,
  },
];

const data3 = [
  {
    Id: "episode1",
    Title: "Bùa Thiên Linh Cái: Từ Mê Tín Đến Man Rợ",
    Description:
      "Có một loại bùa ngải trong dân gian Việt Nam được xem là đáng sợ bậc nhất… Bùa Thiên Linh Cái. Người ta đồn rằng để luyện được thứ bùa này, thầy pháp phải dùng đến sọ người chết yểu...",
    ExplicitContent: true,
    AudioLength: 1867,
    ReleaseDate: "2025-10-08T13:34:33.496Z",
    IsReleased: true,
    ImageUrl:
      "https://i.pinimg.com/736x/be/8e/27/be8e271cdadb7a2d5ee7d2b6be02aed5.jpg",
    AudioFileKey: "string",
    AudioFileSize: 0,
    AudioFingerprint: "string",
    PodcastEpisodeSubscriptionType: { Id: 0, Name: "string" },
    PodcastShowId: "string",
    SeasonNumber: 0,
    TotalSave: 0,
    ListenCount: 0,
    IsAudioPublishable: true,
    TakenDownReason: "string",
    DeletedAt: "2025-10-08T13:34:33.496Z",
    CreatedAt: "2025-10-08T13:34:33.496Z",
    UpdatedAt: "2025-10-08T13:34:33.496Z",
    Top: 1,
  },
  {
    Id: "episode2",
    Title: "Ngôi Làng Ma Ở Tây Nguyên",
    Description:
      "Một ngôi làng nằm sâu trong rừng Tây Nguyên, nơi người ta nói rằng mỗi khi trăng tròn, tiếng khóc than lại vang lên giữa núi rừng…",
    ExplicitContent: true,
    AudioLength: 2134,
    ReleaseDate: "2025-10-08T13:34:33.496Z",
    IsReleased: true,
    ImageUrl:
      "https://i.pinimg.com/1200x/11/a9/79/11a97905373fdfe8aea059f032f4148c.jpg",
    AudioFileKey: "string",
    AudioFileSize: 0,
    AudioFingerprint: "string",
    PodcastEpisodeSubscriptionType: { Id: 0, Name: "string" },
    PodcastShowId: "string",
    SeasonNumber: 0,
    TotalSave: 0,
    ListenCount: 0,
    IsAudioPublishable: true,
    TakenDownReason: "string",
    DeletedAt: "2025-10-08T13:34:33.496Z",
    CreatedAt: "2025-10-08T13:34:33.496Z",
    UpdatedAt: "2025-10-08T13:34:33.496Z",
    Top: 2,
  },
  {
    Id: "episode3",
    Title: "Ngải Quỷ Miên: Huyền Bí Xứ Chùa Tháp",
    Description:
      "Tương truyền ở Campuchia có một loại ngải chỉ dùng để trừng phạt kẻ phản bội. Người trúng ngải sẽ dần hóa điên, rồi biến mất không dấu vết…",
    ExplicitContent: true,
    AudioLength: 1972,
    ReleaseDate: "2025-10-08T13:34:33.496Z",
    IsReleased: true,
    ImageUrl:
      "https://i.pinimg.com/1200x/86/ab/05/86ab05ba59f3ee0a7e26ea2147cc4ae7.jpg",
    AudioFileKey: "string",
    AudioFileSize: 0,
    AudioFingerprint: "string",
    PodcastEpisodeSubscriptionType: { Id: 0, Name: "string" },
    PodcastShowId: "string",
    SeasonNumber: 0,
    TotalSave: 0,
    ListenCount: 0,
    IsAudioPublishable: true,
    TakenDownReason: "string",
    DeletedAt: "2025-10-08T13:34:33.496Z",
    CreatedAt: "2025-10-08T13:34:33.496Z",
    UpdatedAt: "2025-10-08T13:34:33.496Z",
    Top: 3,
  },
  {
    Id: "episode4",
    Title: "Đêm Kinh Hoàng Ở Nhà Số 47",
    Description:
      "Căn nhà số 47 đã bị bỏ hoang hơn 20 năm, nhưng đêm đó, ánh đèn lại bật sáng… và ai đó đang đứng sau cửa sổ tầng hai.",
    ExplicitContent: false,
    AudioLength: 1750,
    ReleaseDate: "2025-10-08T13:34:33.496Z",
    IsReleased: true,
    ImageUrl:
      "https://i.pinimg.com/736x/df/ff/6c/dfff6cf7d8f3f59e4102157aaefb5c93.jpg",
    AudioFileKey: "string",
    AudioFileSize: 0,
    AudioFingerprint: "string",
    PodcastEpisodeSubscriptionType: { Id: 0, Name: "string" },
    PodcastShowId: "string",
    SeasonNumber: 0,
    TotalSave: 0,
    ListenCount: 0,
    IsAudioPublishable: true,
    TakenDownReason: "string",
    DeletedAt: "2025-10-08T13:34:33.496Z",
    CreatedAt: "2025-10-08T13:34:33.496Z",
    UpdatedAt: "2025-10-08T13:34:33.496Z",
    Top: 4,
  },
  {
    Id: "episode5",
    Title: "Tà Thuật Trường Sơn: Linh Hồn Của Núi Rừng",
    Description:
      "Những người thợ săn già kể rằng, có linh hồn của núi rừng đi theo họ suốt đời – nếu họ từng phạm điều cấm kỵ khi săn bắn.",
    ExplicitContent: false,
    AudioLength: 2211,
    ReleaseDate: "2025-10-08T13:34:33.496Z",
    IsReleased: true,
    ImageUrl:
      "https://i.pinimg.com/736x/8d/6c/3b/8d6c3b3c8cad88d0112e52f9570f60b9.jpg",
    AudioFileKey: "string",
    AudioFileSize: 0,
    AudioFingerprint: "string",
    PodcastEpisodeSubscriptionType: { Id: 0, Name: "string" },
    PodcastShowId: "string",
    SeasonNumber: 0,
    TotalSave: 0,
    ListenCount: 0,
    IsAudioPublishable: true,
    TakenDownReason: "string",
    DeletedAt: "2025-10-08T13:34:33.496Z",
    CreatedAt: "2025-10-08T13:34:33.496Z",
    UpdatedAt: "2025-10-08T13:34:33.496Z",
    Top: 5,
  },
  {
    Id: "episode6",
    Title: "Âm Hồn Trên Cầu Cũ",
    Description:
      "Người dân trong vùng tin rằng, mỗi khi có người chết oan, linh hồn họ sẽ quanh quẩn trên chiếc cầu nơi xảy ra bi kịch.",
    ExplicitContent: true,
    AudioLength: 1908,
    ReleaseDate: "2025-10-08T13:34:33.496Z",
    IsReleased: true,
    ImageUrl:
      "https://i.pinimg.com/1200x/ca/3e/e4/ca3ee41771f8e1b1934b0d53caa0717c.jpg",
    AudioFileKey: "string",
    AudioFileSize: 0,
    AudioFingerprint: "string",
    PodcastEpisodeSubscriptionType: { Id: 0, Name: "string" },
    PodcastShowId: "string",
    SeasonNumber: 0,
    TotalSave: 0,
    ListenCount: 0,
    IsAudioPublishable: true,
    TakenDownReason: "string",
    DeletedAt: "2025-10-08T13:34:33.496Z",
    CreatedAt: "2025-10-08T13:34:33.496Z",
    UpdatedAt: "2025-10-08T13:34:33.496Z",
    Top: 6,
  },
  {
    Id: "episode7",
    Title: "Những Đứa Trẻ Trong Gương",
    Description:
      "Căn hộ mới của gia đình Minh có một chiếc gương cổ. Nhưng mỗi tối, Minh đều thấy những đứa trẻ lạ mặt mỉm cười trong đó.",
    ExplicitContent: false,
    AudioLength: 1634,
    ReleaseDate: "2025-10-08T13:34:33.496Z",
    IsReleased: true,
    ImageUrl:
      "https://i.pinimg.com/736x/be/8e/27/be8e271cdadb7a2d5ee7d2b6be02aed5.jpg",
    AudioFileKey: "string",
    AudioFileSize: 0,
    AudioFingerprint: "string",
    PodcastEpisodeSubscriptionType: { Id: 0, Name: "string" },
    PodcastShowId: "string",
    SeasonNumber: 0,
    TotalSave: 0,
    ListenCount: 0,
    IsAudioPublishable: true,
    TakenDownReason: "string",
    DeletedAt: "2025-10-08T13:34:33.496Z",
    CreatedAt: "2025-10-08T13:34:33.496Z",
    UpdatedAt: "2025-10-08T13:34:33.496Z",
    Top: 7,
  },
  {
    Id: "episode8",
    Title: "Ngôi Chùa Không Tiếng Chuông",
    Description:
      "Chùa cổ trên đỉnh núi, chuông đã im lặng hàng trăm năm. Nhưng đêm rằm tháng bảy, người ta vẫn nghe tiếng ngân vang giữa sương.",
    ExplicitContent: false,
    AudioLength: 2050,
    ReleaseDate: "2025-10-08T13:34:33.496Z",
    IsReleased: true,
    ImageUrl:
      "https://i.pinimg.com/736x/be/8e/27/be8e271cdadb7a2d5ee7d2b6be02aed5.jpg",
    AudioFileKey: "string",
    AudioFileSize: 0,
    AudioFingerprint: "string",
    PodcastEpisodeSubscriptionType: { Id: 0, Name: "string" },
    PodcastShowId: "string",
    SeasonNumber: 0,
    TotalSave: 0,
    ListenCount: 0,
    IsAudioPublishable: true,
    TakenDownReason: "string",
    DeletedAt: "2025-10-08T13:34:33.496Z",
    CreatedAt: "2025-10-08T13:34:33.496Z",
    UpdatedAt: "2025-10-08T13:34:33.496Z",
    Top: 8,
  },
  {
    Id: "episode9",
    Title: "Trò Chơi Của Quỷ: Mười Hai Giờ Đêm",
    Description:
      "Một nhóm sinh viên tham gia thử thách triệu hồi quỷ vào đúng 0h. Họ nghĩ đó chỉ là trò đùa… cho đến khi có người biến mất.",
    ExplicitContent: true,
    AudioLength: 2422,
    ReleaseDate: "2025-10-08T13:34:33.496Z",
    IsReleased: true,
    ImageUrl:
      "https://i.pinimg.com/736x/be/8e/27/be8e271cdadb7a2d5ee7d2b6be02aed5.jpg",
    AudioFileKey: "string",
    AudioFileSize: 0,
    AudioFingerprint: "string",
    PodcastEpisodeSubscriptionType: { Id: 0, Name: "string" },
    PodcastShowId: "string",
    SeasonNumber: 0,
    TotalSave: 0,
    ListenCount: 0,
    IsAudioPublishable: true,
    TakenDownReason: "string",
    DeletedAt: "2025-10-08T13:34:33.496Z",
    CreatedAt: "2025-10-08T13:34:33.496Z",
    UpdatedAt: "2025-10-08T13:34:33.496Z",
    Top: 9,
  },
];

// Mock data for podcasters
const podcasterData = [
  {
    Id: 1,
    Fullname: "SAMURICE",
    ImageUrl:
      "https://scontent.fsgn5-15.fna.fbcdn.net/v/t39.30808-6/542618171_24917648541194226_1925287616611657518_n.jpg?_nc_cat=111&ccb=1-7&_nc_sid=6ee11a&_nc_ohc=Ga_dPHZVrj4Q7kNvwFVaeEF&_nc_oc=Admw54pEg_9VxbnA2CSgBbP_9v1vj3hyBEyziRp7It9ToqNwY_QGi91FyvPanekWWGU&_nc_zt=23&_nc_ht=scontent.fsgn5-15.fna&_nc_gid=qAYYlX3dNhXf3ZqgmSIONA&oh=00_Affq-0v0_0QWRJYDIAFx6kjXT-4shI-nMlcnZtrTHqvygg&oe=68EC42E3",
    Followers: 1200000,
    Email: "",
    Role: { Id: 1, Name: "Podcaster" },
    Dob: "",
    Gender: "",
    Address: "",
    Phone: "",
    Balance: 0,
    IsVerified: true,
    GoogleId: "",
    VerifyCode: "",
    PodcastListenSlot: 0,
    ViolationPoint: 0,
    ViolationLevel: 0,
    LastViolationPointChanged: "",
    LastViolationLevelChanged: "",
    LastPodcastListenSlotChanged: "",
    DeactivatedAt: "",
    CreatedAt: "",
    UpdatedAt: "",
    IsBeingPunish: false,
  },
  {
    Id: 2,
    Fullname: "ÂM NHẪN STUDIO",
    ImageUrl:
      "https://i.pinimg.com/736x/62/3b/19/623b1953e2f4a46c9199d40d8698c164.jpg",
    Followers: 210000,
    Email: "",
    Role: { Id: 1, Name: "Podcaster" },
    Dob: "",
    Gender: "",
    Address: "",
    Phone: "",
    Balance: 0,
    IsVerified: true,
    GoogleId: "",
    VerifyCode: "",
    PodcastListenSlot: 0,
    ViolationPoint: 0,
    ViolationLevel: 0,
    LastViolationPointChanged: "",
    LastViolationLevelChanged: "",
    LastPodcastListenSlotChanged: "",
    DeactivatedAt: "",
    CreatedAt: "",
    UpdatedAt: "",
    IsBeingPunish: false,
  },
  {
    Id: 3,
    Fullname: "Cải Kính Dị",
    ImageUrl:
      "https://i.pinimg.com/736x/c9/ba/d2/c9bad2ea3e9f5961fbdb91cf6acffcbf.jpg",
    Followers: 165243,
    Email: "",
    Role: { Id: 1, Name: "Podcaster" },
    Dob: "",
    Gender: "",
    Address: "",
    Phone: "",
    Balance: 0,
    IsVerified: true,
    GoogleId: "",
    VerifyCode: "",
    PodcastListenSlot: 0,
    ViolationPoint: 0,
    ViolationLevel: 0,
    LastViolationPointChanged: "",
    LastViolationLevelChanged: "",
    LastPodcastListenSlotChanged: "",
    DeactivatedAt: "",
    CreatedAt: "",
    UpdatedAt: "",
    IsBeingPunish: false,
  },
  {
    Id: 4,
    Fullname: "Horror Stories",
    ImageUrl:
      "https://i.pinimg.com/736x/03/93/7b/03937bb7a29c91227d76a0ab4808987a.jpg",
    Followers: 89500,
    Email: "",
    Role: { Id: 1, Name: "Podcaster" },
    Dob: "",
    Gender: "",
    Address: "",
    Phone: "",
    Balance: 0,
    IsVerified: true,
    GoogleId: "",
    VerifyCode: "",
    PodcastListenSlot: 0,
    ViolationPoint: 0,
    ViolationLevel: 0,
    LastViolationPointChanged: "",
    LastViolationLevelChanged: "",
    LastPodcastListenSlotChanged: "",
    DeactivatedAt: "",
    CreatedAt: "",
    UpdatedAt: "",
    IsBeingPunish: false,
  },
  {
    Id: 5,
    Fullname: "Tales of Terror",
    ImageUrl:
      "https://i.pinimg.com/736x/0c/35/5d/0c355dd83d1a6d9b1e90507af599d637.jpg",
    Followers: 42000,
    Email: "",
    Role: { Id: 1, Name: "Podcaster" },
    Dob: "",
    Gender: "",
    Address: "",
    Phone: "",
    Balance: 0,
    IsVerified: true,
    GoogleId: "",
    VerifyCode: "",
    PodcastListenSlot: 0,
    ViolationPoint: 0,
    ViolationLevel: 0,
    LastViolationPointChanged: "",
    LastViolationLevelChanged: "",
    LastPodcastListenSlotChanged: "",
    DeactivatedAt: "",
    CreatedAt: "",
    UpdatedAt: "",
    IsBeingPunish: false,
  },
];

export default function Home() {
  const { onScroll, headerHeight } = useHeaderScroll();
  const tabBarHeight = useBottomTabBarHeight();

  // STATES
  const user = useSelector((state: RootState) => state.auth.user);

  // HOOKS
  const { data: discoveryData, isLoading: isDiscoveryDataLoading } =
    useGetDiscoveryFeedQuery(undefined,{
      refetchOnMountOrArgChange: true,
      refetchOnFocus: true,
      refetchOnReconnect: true,
    });

  if (isDiscoveryDataLoading || !discoveryData) {
    return (
      <View className="flex-1 flex items-center gap-2 justify-center bg-background-secondary">
        <ActivityIndicator size="large" color="#AEE339" />
        <Text className="text-[#D9D9D9]">Loading...</Text>
      </View>
    );
  } else {
    return (
      <ScrollView
        onScroll={onScroll}
        style={{ backgroundColor: "#000" }}
        contentContainerStyle={{
          paddingTop: headerHeight,
          paddingHorizontal: 10,
        }}
        scrollIndicatorInsets={{ top: headerHeight }}
        scrollEventThrottle={16} // Important for smooth animation
      >
        <View style={{ height: 20 }}></View>

        {/* Base On Your Taste Shows */}
        {user && discoveryData.BasedOnYourTaste.ShowList.length > 0 && (
          <ShowCarousel
            variant="normal"
            title={
              <MixxingText
                originalText="Base On Your Taste"
                coloredText="Your Taste"
              />
            }
            shows={discoveryData.BasedOnYourTaste.ShowList}
            titleString="Base On Your Taste"
          />
        )}

        {/* New Releases Shows */}
        {discoveryData.NewReleases.ShowList.length > 0 && (
          <ShowCarousel
            variant="top"
            title={
              <MixxingText originalText="New Releases" coloredText="New" />
            }
            shows={discoveryData.NewReleases.ShowList}
            titleString = "New Releases"
          />
        )}

        {/* Hot Shows */}
        {discoveryData.HotThisWeek.ShowList.length > 0 && (
          <ShowCarousel
            variant="top"
            title={<MixxingText originalText="Hot Shows" coloredText="Hot" />}
            shows={discoveryData.HotThisWeek.ShowList}
            titleString = "Hot Shows"
          />
        )}

        {/* Top Episodes */}
        {/* <EpisodeCarousel
          title={
            <MixxingText originalText="Top Episodes" coloredText="Episodes" />
          }
          episodes={data3}
        /> */}

        {/* Continue Listening */}
        {user &&
          discoveryData.ContinueListening.ListenSessionList.length > 0 && (
            <EpisodeContinueCarousel
              title={
                <MixxingText
                  originalText="Continue Listening"
                  coloredText="Listening"
                />
              }
              episodes={discoveryData.ContinueListening.ListenSessionList}
            />
          )}

        {/* Hot Channels */}
        {discoveryData.HotThisWeek.ChannelList.length > 0 && (
          <ChannelCarousel
            variant="top"
            title={
              <MixxingText originalText="Hot Channels" coloredText="Hot" />
            }
            channels={discoveryData.HotThisWeek.ChannelList}
            titleString = "Hot Channels"
          />
        )}

        {/* Top Podcasters */}
        {discoveryData.TopPodcasters.PodcasterList.length > 0 && (
          <PodcasterCarousel
            title={
              <MixxingText originalText="Top Podcasters" coloredText="Top" />
            }
            podcasters={discoveryData.TopPodcasters.PodcasterList}
          />
        )}

        {/* Talented Rookies */}
        {discoveryData.TalentedRookies.PodcasterList.length > 0 && (
          <PodcasterCarousel
            title={
              <MixxingText
                originalText="Talented Rookies"
                coloredText="Talented"
              />
            }
            podcasters={discoveryData.TalentedRookies.PodcasterList}
          />
        )}

        {/* Top Sub-Categories */}
        {discoveryData.TopSubCategory &&
          discoveryData.TopSubCategory.PodcastSubCategory &&
          discoveryData.TopSubCategory.ShowList.length > 0 && (
            <ShowCarousel
              variant="normal"
              title={
                <MixxingText
                  originalText={
                    discoveryData.TopSubCategory.PodcastSubCategory.Name
                  }
                  coloredText="\\?\\"
                />
              }
              shows={discoveryData.TopSubCategory.ShowList}
              titleString={discoveryData.TopSubCategory.PodcastSubCategory.Name}
            />
          )}

        {/* Random Category */}
        {discoveryData.RandomCategory &&
          discoveryData.RandomCategory.PodcastCategory &&
          discoveryData.RandomCategory.ShowList.length > 0 && (
            <ShowCarousel
              variant="normal"
              title={
                <MixxingText
                  originalText={
                    discoveryData.RandomCategory.PodcastCategory.Name
                  }
                  coloredText="\\?\\"
                />
              }
              shows={discoveryData.RandomCategory.ShowList}
              titleString={discoveryData.RandomCategory.PodcastCategory.Name}
            />
          )}

        <View style={{ height: tabBarHeight + 50 }}></View>
      </ScrollView>
    );
  }
}
