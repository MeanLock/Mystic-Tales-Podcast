import { Text } from "@/src/components/ui/Text";
import { View } from "@/src/components/ui/View";
import { MaterialIcons } from "@expo/vector-icons";
import { useFocusEffect, useLocalSearchParams, useRouter } from "expo-router";
import { useCallback, useEffect, useState } from "react";
import {
  ActivityIndicator,
  Alert,
  Platform,
  Pressable,
  ScrollView,
  StyleSheet,
} from "react-native";
import ShowInformations from "./components/ShowInformations";
import EpisodeList from "./components/EpisodeList";
import RatingAndReview from "./components/RatingAndReview";
import ShowDescription from "./components/ShowDescription";
import MoreInformations from "./components/MoreInformations";
import Suggesstion from "./components/Suggesstion";
import SkeletonLoading from "@/src/components/loaders/Skeleton";

export type ShowDetails = {
  Id: number;
  Name: string;
  Description: string;
  ReleaseDate: string;
  Copyright: string;
  TotalFollow: number;
  ListenCount: number;
  Language: string;
  UploadFrequency: string;
  Podcaster: {
    Id: number;
    FullName: string;
    ImageUrl: string;
    Email: string;
  };
  PodcastChannel: {
    Id: string;
    Name: string;
    ImageUrl: string;
  } | null;
  RatingList: {
    Id: string;
    Title: string;
    Content: string;
    Rating: number;
    Account: {
      Id: number;
      FullName: string;
      Email: string;
      ImageUrl: string;
    };
    IsDeleted: boolean;
    CreatedAt: string;
  }[];
  RatingCount: number;
  ImageUrl: string;
  TrailerAudioFileKey: string;
  PodcastCategory: {
    Id: number;
    Name: string;
  };
  PodcastSubCategory: {
    Id: number;
    Name: string;
    PodcastCategoryId: number;
  };
  PodcastShowsSubscriptionType: {
    Id: number;
    Name: string;
  };
  TakenDownReason: string | null;
  DeletedAt: string | null;
  CreateAt: string;
  UpdatedAt: string;
  // Danh sách version của gói subscription
  ShowSubscriptionList: {
    Id: number;
    Name: string;
    Description: string;
    PodcastChannelId: string;
    PodcastShowId: string;
    IsActive: boolean;
    CurrentVersion: number;
    DeletedAt: string | null;
    CreatedAt: string;
    UpdatedAt: string;
    PodcastSubscriptionCycleTypePriceList: {
      PodcastSubscriptionId: number;
      SubscriptionCycleType: {
        Id: number;
        Name: string;
      };
      Version: number;
      Price: number;
      CreatedAt: string;
      UpdatedAt: string;
    }[];
    PodcastSubscriptionBenefitMappingList: {
      PodcastSubscriptionId: number;
      PodcastSubscriptionBenefit: {
        Id: number;
        Name: string;
      };
      Version: number;
      CreatedAt: string;
      UpdatedAt: string;
    }[];
  }[];
  ShowEpisodeList: {
    Id: string;
    Name: string;
    Description: string;
    ExplicitContent: boolean;
    ReleaseDate: string;
    IsReleased: boolean;
    ImageUrl: string;
    AudioFileKey: string;
    AudioFileSize: number;
    AudioLength: number; //Độ dài theo giây
    AudioFingerprint: string;
    PodcastEpisodeSubscriptionType: {
      Id: number;
      Name: string;
    };
    PodcastShowId: string;
    SeasonNumber: number;
    TotalSave: number;
    ListenCount: number;
    IsAudioPublishable: boolean;
    TakenDownReason: string | null;
    DeletedAt: string | null;
    CreatedAt: string;
    UpdatedAt: string;
  }[];
};

const showData: ShowDetails = {
  Id: 0,
  Name: "Vườn Địa Đàng Của Quỷ",
  Description:
    "Một ngày sau khi chuyển tới ngôi nhà cũ kỹ ở rìa thị trấn, Alice vô tình phát hiện một khu vườn bị tường rào và dây leo che kín sau sân sau. Ban đầu, đó là nơi trú ẩn yên tĩnh của cô bé: mùi hoa lạ, lối đi lát đá, chiếc xích đu kẽo kẹt và một hồ nước phản chiếu bầu trời. Mỗi chiều, Alice lại trở về, nhặt những mảnh gương vỡ, những chìa khóa gỉ, và nghe tiếng thì thầm như gió len qua lá. Nhưng càng thân thuộc, khu vườn càng lộ ra những điều không bình thường: bóng người thấp thoáng sau bụi hồng, chiếc đồng hồ mặt kính đếm ngược dưới gốc cây, những dấu chân nhỏ xuất hiện rồi biến mất. Khi Alice lần theo chuỗi manh mối dẫn tới căn nhà kính khóa kín từ lâu, cô phát hiện bí ẩn kinh hoàng gắn với chủ cũ ngôi nhà—một lời hứa bị nuốt chửng bởi khu vườn. Từ đó, nơi trú ẩn dịu dàng biến thành mê cung của sự thật và nỗi sợ, buộc Alice phải chọn: rời đi, hay mở cánh cửa cuối cùng.",
  Language: "Vietnamese",
  ReleaseDate: "2019-10-10T11:56:06.138Z",
  Copyright: "© 363636",
  UploadFrequency: "Daily",
  RatingList: [
    {
      Id: "a1f1e0d2-5b4f-4f7a-9f3e-9c2c1a1a0001",
      Title: "Cuốn hút từ chương đầu tiên",
      Content:
        "Cách miêu tả khu vườn và những bí ẩn khiến mình không dứt ra được. Không khí u ám nhưng rất nghệ thuật.",
      Rating: 5,
      Account: {
        Id: 1,
        FullName: "Nguyễn Hồng Ân",
        Email: "an.nguyen@example.com",
        ImageUrl: "/avatars/1.jpg",
      },
      IsDeleted: false,
      CreatedAt: "2025-10-01T09:45:00.000Z",
    },
    {
      Id: "a1f1e0d2-5b4f-4f7a-9f3e-9c2c1a1a0002",
      Title: "Hơi đáng sợ nhưng rất hay",
      Content:
        "Phần mô tả quỷ rất độc đáo, không theo motif cũ. Cảm giác như đang xem một bộ phim tâm linh phương Tây.",
      Rating: 4,
      Account: {
        Id: 2,
        FullName: "Trần Bảo Ngọc",
        Email: "ngoc.tran@example.com",
        ImageUrl: "/avatars/2.jpg",
      },
      IsDeleted: false,
      CreatedAt: "2025-10-02T18:22:00.000Z",
    },
    {
      Id: "a1f1e0d2-5b4f-4f7a-9f3e-9c2c1a1a0003",
      Title: "Cốt truyện độc lạ",
      Content:
        "Chưa từng thấy truyện nào kết hợp yếu tố thần thoại và kinh dị kiểu này. Tác giả rất sáng tạo!",
      Rating: 5,
      Account: {
        Id: 3,
        FullName: "Phạm Đức Thịnh",
        Email: "thinh.pham@example.com",
        ImageUrl: "/avatars/3.jpg",
      },
      IsDeleted: false,
      CreatedAt: "2025-10-03T07:18:00.000Z",
    },
    {
      Id: "a1f1e0d2-5b4f-4f7a-9f3e-9c2c1a1a0004",
      Title: "Tốc độ hơi chậm",
      Content:
        "Một vài chương đầu khá dài dòng, nhưng khi vào cao trào thì thực sự bùng nổ. Đáng để đọc tiếp.",
      Rating: 3,
      Account: {
        Id: 4,
        FullName: "Lê Minh Tuấn",
        Email: "tuan.le@example.com",
        ImageUrl: "/avatars/4.jpg",
      },
      IsDeleted: false,
      CreatedAt: "2025-10-04T15:00:00.000Z",
    },
    {
      Id: "a1f1e0d2-5b4f-4f7a-9f3e-9c2c1a1a0005",
      Title: "Một thế giới ma mị tuyệt đẹp",
      Content:
        "Từng khung cảnh trong truyện như một bức tranh tối – ánh sáng đan xen. Đọc mà nổi da gà.",
      Rating: 5,
      Account: {
        Id: 5,
        FullName: "Hoàng Thị Yến",
        Email: "yen.hoang@example.com",
        ImageUrl: "/avatars/5.jpg",
      },
      IsDeleted: false,
      CreatedAt: "2025-10-05T10:30:00.000Z",
    },
    {
      Id: "a1f1e0d2-5b4f-4f7a-9f3e-9c2c1a1a0006",
      Title: "Không khí truyện rất đặc biệt",
      Content:
        "Kết hợp yếu tố tôn giáo, thần thoại và kinh dị. Cảm giác như đang đọc một tác phẩm phương Tây cổ điển.",
      Rating: 4,
      Account: {
        Id: 6,
        FullName: "Đinh Hoàng Nam",
        Email: "nam.dinh@example.com",
        ImageUrl: "/avatars/6.jpg",
      },
      IsDeleted: false,
      CreatedAt: "2025-10-06T12:40:00.000Z",
    },
    {
      Id: "a1f1e0d2-5b4f-4f7a-9f3e-9c2c1a1a0007",
      Title: "Một chút triết lý ẩn sau kinh dị",
      Content:
        "Không chỉ là câu chuyện quỷ dữ mà còn là ẩn dụ cho dục vọng và sự cứu rỗi. Viết rất sâu sắc.",
      Rating: 5,
      Account: {
        Id: 7,
        FullName: "Trương Hữu Phong",
        Email: "phong.truong@example.com",
        ImageUrl: "/avatars/7.jpg",
      },
      IsDeleted: false,
      CreatedAt: "2025-10-06T21:20:00.000Z",
    },
    {
      Id: "a1f1e0d2-5b4f-4f7a-9f3e-9c2c1a1a0008",
      Title: "Phần giữa hơi lan man",
      Content:
        "Tác giả nên rút gọn đoạn đối thoại, nhưng nhìn chung cốt truyện vẫn rất chắc tay.",
      Rating: 3,
      Account: {
        Id: 8,
        FullName: "Ngô Văn Hậu",
        Email: "hau.ngo@example.com",
        ImageUrl: "/avatars/8.jpg",
      },
      IsDeleted: false,
      CreatedAt: "2025-10-07T08:15:00.000Z",
    },
    {
      Id: "a1f1e0d2-5b4f-4f7a-9f3e-9c2c1a1a0009",
      Title: "Tác phẩm đáng nhớ",
      Content:
        "Đọc xong vẫn còn ám ảnh. Một trong những bộ truyện Việt hiếm hoi khiến mình suy nghĩ về thiện – ác.",
      Rating: 5,
      Account: {
        Id: 9,
        FullName: "Phan Thảo Nhi",
        Email: "nhi.phan@example.com",
        ImageUrl: "/avatars/9.jpg",
      },
      IsDeleted: false,
      CreatedAt: "2025-10-08T14:45:00.000Z",
    },
    {
      Id: "a1f1e0d2-5b4f-4f7a-9f3e-9c2c1a1a0010",
      Title: "Giọng kể rất cuốn",
      Content:
        "Cách dẫn chuyện xen lẫn ký ức và thực tại làm mình thấy như đang lạc trong chính khu vườn địa đàng ấy.",
      Rating: 4,
      Account: {
        Id: 10,
        FullName: "Trịnh Gia Bảo",
        Email: "bao.trinh@example.com",
        ImageUrl: "/avatars/10.jpg",
      },
      IsDeleted: false,
      CreatedAt: "2025-10-09T09:50:00.000Z",
    },
    {
      Id: "a1f1e0d2-5b4f-4f7a-9f3e-9c2c1a1a0011",
      Title: "Không dành cho người yếu tim 😱",
      Content:
        "Một vài cảnh thật sự rùng rợn, đọc ban đêm đúng là thử thách tinh thần.",
      Rating: 4,
      Account: {
        Id: 11,
        FullName: "Võ Thị Linh",
        Email: "linh.vo@example.com",
        ImageUrl: "/avatars/11.jpg",
      },
      IsDeleted: false,
      CreatedAt: "2025-10-10T11:56:06.138Z",
    },
    {
      Id: "a1f1e0d2-5b4f-4f7a-9f3e-9c2c1a1a0012",
      Title: "Chờ phần tiếp theo!",
      Content:
        "Tác phẩm dừng lại đúng lúc gay cấn, mong tác giả sớm ra phần hai để giải đáp mọi bí ẩn.",
      Rating: 5,
      Account: {
        Id: 12,
        FullName: "Đỗ Ngọc Quân",
        Email: "quan.do@example.com",
        ImageUrl: "/avatars/12.jpg",
      },
      IsDeleted: false,
      CreatedAt: "2025-10-11T20:10:00.000Z",
    },
  ],
  RatingCount: 0,
  ImageUrl:
    "https://i.pinimg.com/1200x/80/d3/47/80d347028b34fe1db485a00ecc1c409f.jpg",
  // ImageUrl:
  //   "https://i.pinimg.com/1200x/3a/fd/ab/3afdabfb407e251c19093ee875499a7b.jpg",
  TrailerAudioFileKey: "string",
  TotalFollow: 0,
  ListenCount: 0,
  Podcaster: {
    Id: 1,
    FullName: "SAMURICE",
    Email: "samugao@gmail.com",
    ImageUrl:
      "https://scontent.fsgn5-15.fna.fbcdn.net/v/t39.30808-6/542618171_24917648541194226_1925287616611657518_n.jpg?_nc_cat=111&ccb=1-7&_nc_sid=6ee11a&_nc_ohc=fKlmaf86V9UQ7kNvwGFLt7l&_nc_oc=AdkEcvsAYd7uFkusW5NqQHHYA_t6MwU2lD9_a2eULmFfN4iUgACDntlp0ClDqr7OFc0&_nc_zt=23&_nc_ht=scontent.fsgn5-15.fna&_nc_gid=FzBw82SAPBRZVsLOiVbaHw&oh=00_AfeG58T8yDw6hs5ZqDHDnVj34x7JQgLCf6aVQPN-Bx9Djg&oe=68F269E3",
  },
  PodcastCategory: {
    Id: 1,
    Name: "Religion",
  },
  PodcastSubCategory: {
    Id: 2,
    Name: "Christant",
    PodcastCategoryId: 0,
  },
  PodcastShowsSubscriptionType: {
    Id: 0,
    Name: "string",
  },
  PodcastChannel: {
    Id: "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    Name: "Dị giáo xung quanh ta",
    ImageUrl:
      "https://i.pinimg.com/1200x/f9/52/22/f952223fea25189cc09a7368a812532e.jpg",
  },
  TakenDownReason: "string",
  DeletedAt: "2025-10-10T11:56:06.138Z",
  CreateAt: "2025-10-10T11:56:06.138Z",
  UpdatedAt: "2025-10-10T11:56:06.138Z",
  ShowSubscriptionList: [
    {
      Id: 1,
      Name: "string",
      Description: "",
      PodcastChannelId: "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      PodcastShowId: "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      IsActive: true,
      CurrentVersion: 1,
      DeletedAt: "2025-10-10T11:56:06.138Z",
      CreatedAt: "2025-10-10T11:56:06.138Z",
      UpdatedAt: "2025-10-10T11:56:06.138Z",
      PodcastSubscriptionCycleTypePriceList: [
        {
          PodcastSubscriptionId: 1,
          SubscriptionCycleType: {
            Id: 0,
            Name: "string",
          },
          Version: 1,
          Price: 160000,
          CreatedAt: "2025-10-10T11:56:06.138Z",
          UpdatedAt: "2025-10-10T11:56:06.138Z",
        },
      ],
      PodcastSubscriptionBenefitMappingList: [
        {
          PodcastSubscriptionId: 0,
          PodcastSubscriptionBenefit: {
            Id: 0,
            Name: "string",
          },
          Version: 0,
          CreatedAt: "2025-10-10T11:56:06.138Z",
          UpdatedAt: "2025-10-10T11:56:06.138Z",
        },
      ],
    },
  ],
  ShowEpisodeList: [
    {
      Id: "a1c2f301-001",
      Name: "Tập 1: Gốc Cây Sau Nhà",
      Description:
        "Alice, một cô bé tò mò, vừa chuyển đến ngôi nhà cổ ở rìa rừng cùng gia đình. Trong một buổi chiều u ám, cô phát hiện một gốc cây rỗng kỳ lạ phát ra âm thanh như tiếng thở. Dù bị cảnh báo không được tới gần, cô vẫn bị cuốn hút và quyết định khám phá nó, mở đầu cho một hành trình không thể quay lại.",
      ExplicitContent: false,
      ReleaseDate: "2025-02-01T09:00:00.000Z",
      IsReleased: true,
      ImageUrl:
        "https://i.pinimg.com/736x/bd/0f/1c/bd0f1c284a93974272dbc93aff8be38f.jpg",
      AudioFileKey: "1",
      AudioFileSize: 35600000,
      AudioLength: 1800,
      AudioFingerprint: "©123456",
      PodcastEpisodeSubscriptionType: { Id: 1, Name: "Subcribed Show Only" },
      PodcastShowId: "1",
      SeasonNumber: 1,
      TotalSave: 250,
      ListenCount: 1300,
      IsAudioPublishable: true,
      TakenDownReason: null,
      DeletedAt: null,
      CreatedAt: "2025-01-10T11:56:06.138Z",
      UpdatedAt: "2025-01-10T11:56:06.138Z",
    },
    {
      Id: "a1c2f301-002",
      Name: "Tập 2: Cánh Cửa Dẫn Lối",
      Description:
        "Bên trong gốc cây, Alice nhận ra một luồng sáng xanh dị thường. Khi chạm vào, cô bị hút qua một đường hầm xoáy sâu và rơi xuống một khu rừng lạ. Cảnh vật giống hệt khu rừng sau nhà, nhưng mọi thứ đều mang màu sắc u tối và tĩnh lặng đến đáng sợ. Từ xa, tiếng cười trẻ con vang vọng.",
      ExplicitContent: false,
      ReleaseDate: "2025-02-05T09:00:00.000Z",
      IsReleased: true,
      ImageUrl: "string",
      AudioFileKey: "2",
      AudioFileSize: 41000000,
      AudioLength: 2250,
      AudioFingerprint: "©654321",
      PodcastEpisodeSubscriptionType: { Id: 1, Name: "Subcribed Show Only" },
      PodcastShowId: "1",
      SeasonNumber: 1,
      TotalSave: 190,
      ListenCount: 1150,
      IsAudioPublishable: true,
      TakenDownReason: null,
      DeletedAt: null,
      CreatedAt: "2025-01-12T11:56:06.138Z",
      UpdatedAt: "2025-01-12T11:56:06.138Z",
    },
    {
      Id: "a1c2f301-003",
      Name: "Tập 3: Khu Rừng Không Bóng Người",
      Description:
        "Cô bé lang thang qua những thân cây khổng lồ và hoa cỏ phát sáng. Dù đẹp kỳ lạ, bầu không khí khiến cô cảm thấy có ai đó đang dõi theo. Tiếng gió thổi nghe như lời thì thầm. Mỗi bước đi, những bóng đen trong rừng lại tiến gần hơn.",
      ExplicitContent: false,
      ReleaseDate: "2025-02-10T09:00:00.000Z",
      IsReleased: true,
      ImageUrl: "string",
      AudioFileKey: "3",
      AudioFileSize: 37000000,
      AudioLength: 1950,
      AudioFingerprint: "©238415",
      PodcastEpisodeSubscriptionType: { Id: 1, Name: "Subcribed Show Only" },
      PodcastShowId: "1",
      SeasonNumber: 1,
      TotalSave: 172,
      ListenCount: 980,
      IsAudioPublishable: true,
      TakenDownReason: null,
      DeletedAt: null,
      CreatedAt: "2025-01-15T11:56:06.138Z",
      UpdatedAt: "2025-01-15T11:56:06.138Z",
    },
    {
      Id: "a1c2f301-004",
      Name: "Tập 4: Người Giữ Khu Vườn",
      Description:
        "Alice gặp một người phụ nữ mặc váy đen dài, tự xưng là Người Giữ Vườn. Bà nói rằng Alice đã đến 'Vườn Địa Đàng Của Quỷ' — nơi chỉ những người được chọn mới có thể đặt chân tới. Mọi lời nói của bà ta đều dịu dàng, nhưng đôi mắt lại phản chiếu ánh đỏ rực như máu.",
      ExplicitContent: true,
      ReleaseDate: "2025-02-15T09:00:00.000Z",
      IsReleased: true,
      ImageUrl: "string",
      AudioFileKey: "4",
      AudioFileSize: 42000000,
      AudioLength: 2500,
      AudioFingerprint: "©781249",
      PodcastEpisodeSubscriptionType: { Id: 1, Name: "Subcribed Show Only" },
      PodcastShowId: "1",
      SeasonNumber: 1,
      TotalSave: 145,
      ListenCount: 870,
      IsAudioPublishable: true,
      TakenDownReason: null,
      DeletedAt: null,
      CreatedAt: "2025-01-20T11:56:06.138Z",
      UpdatedAt: "2025-01-20T11:56:06.138Z",
    },
    {
      Id: "a1c2f301-005",
      Name: "Tập 5: Tiếng Cười Trong Đêm",
      Description:
        "Đêm đầu tiên ở khu vườn, Alice bị đánh thức bởi tiếng cười của một đứa trẻ. Khi mở mắt, cô thấy một chiếc mặt nạ đang treo lơ lửng trước giường. Cô bỏ chạy, nhưng mọi lối đi đều dẫn trở lại cùng một nơi — giữa rừng là chiếc bàn ăn với những chiếc ghế tự xoay nhìn về phía cô.",
      ExplicitContent: true,
      ReleaseDate: "2025-02-20T09:00:00.000Z",
      IsReleased: true,
      ImageUrl: "string",
      AudioFileKey: "5",
      AudioFileSize: 52000000,
      AudioLength: 3200,
      AudioFingerprint: "©845193",
      PodcastEpisodeSubscriptionType: { Id: 1, Name: "Subcribed Show Only" },
      PodcastShowId: "1",
      SeasonNumber: 1,
      TotalSave: 260,
      ListenCount: 1550,
      IsAudioPublishable: true,
      TakenDownReason: null,
      DeletedAt: null,
      CreatedAt: "2025-01-23T11:56:06.138Z",
      UpdatedAt: "2025-01-23T11:56:06.138Z",
    },
    {
      Id: "a1c2f301-006",
      Name: "Tập 6: Những Khuôn Mặt Trên Cây",
      Description:
        "Cây cối trong khu vườn bắt đầu thay đổi. Alice phát hiện khuôn mặt người xuất hiện trên thân cây, đôi mắt mở to cầu cứu rồi tan biến khi cô chạm vào. Dường như khu vườn này đang sống — và nó đang đói.",
      ExplicitContent: true,
      ReleaseDate: "2025-02-25T09:00:00.000Z",
      IsReleased: true,
      ImageUrl: "string",
      AudioFileKey: "6",
      AudioFileSize: 47000000,
      AudioLength: 2400,
      AudioFingerprint: "©992834",
      PodcastEpisodeSubscriptionType: { Id: 1, Name: "Subcribed Show Only" },
      PodcastShowId: "1",
      SeasonNumber: 1,
      TotalSave: 199,
      ListenCount: 1110,
      IsAudioPublishable: true,
      TakenDownReason: null,
      DeletedAt: null,
      CreatedAt: "2025-01-28T11:56:06.138Z",
      UpdatedAt: "2025-01-28T11:56:06.138Z",
    },
    {
      Id: "a1c2f301-007",
      Name: "Tập 7: Con Thỏ Đen",
      Description:
        "Một con thỏ đen có đôi mắt người dẫn đường cho Alice. Nó nói bằng giọng trầm của một người đàn ông, bảo rằng cô phải 'ăn bữa tiệc của quỷ' trước khi mặt trời mọc, nếu không sẽ không bao giờ rời được nơi này.",
      ExplicitContent: true,
      ReleaseDate: "2025-03-01T09:00:00.000Z",
      IsReleased: true,
      ImageUrl: "string",
      AudioFileKey: "7",
      AudioFileSize: 49000000,
      AudioLength: 2600,
      AudioFingerprint: "©239485",
      PodcastEpisodeSubscriptionType: { Id: 1, Name: "Subcribed Show Only" },
      PodcastShowId: "1",
      SeasonNumber: 1,
      TotalSave: 223,
      ListenCount: 1275,
      IsAudioPublishable: true,
      TakenDownReason: null,
      DeletedAt: null,
      CreatedAt: "2025-02-02T11:56:06.138Z",
      UpdatedAt: "2025-02-02T11:56:06.138Z",
    },
    {
      Id: "a1c2f301-008",
      Name: "Tập 8: Bữa Tiệc Dưới Trăng Máu",
      Description:
        "Alice được mời tới dự bữa tiệc kỳ lạ, nơi mọi sinh vật quái dị đều mang mặt nạ. Bàn ăn dài vô tận, thức ăn bốc khói và di chuyển như còn sống. Khi cô cắn thử một miếng, vị máu tan nơi đầu lưỡi. Cả bàn tiệc quay sang nhìn, cùng nói: 'Chào mừng thực khách mới.'",
      ExplicitContent: true,
      ReleaseDate: "2025-03-05T09:00:00.000Z",
      IsReleased: true,
      ImageUrl:
        "https://i.pinimg.com/736x/26/e7/81/26e781e85831573679d188f875b66104.jpg",
      AudioFileKey: "8",
      AudioFileSize: 50000000,
      AudioLength: 2700,
      AudioFingerprint: "©119273",
      PodcastEpisodeSubscriptionType: { Id: 1, Name: "Subcribed Show Only" },
      PodcastShowId: "1",
      SeasonNumber: 1,
      TotalSave: 310,
      ListenCount: 1540,
      IsAudioPublishable: true,
      TakenDownReason: null,
      DeletedAt: null,
      CreatedAt: "2025-02-06T11:56:06.138Z",
      UpdatedAt: "2025-02-06T11:56:06.138Z",
    },
    {
      Id: "a1c2f301-009",
      Name: "Tập 9: Chiếc Gương Biết Nói",
      Description:
        "Alice bước vào căn phòng đầy gương. Trong mỗi tấm gương, cô thấy những phiên bản khác nhau của mình – vui vẻ, sợ hãi, tức giận. Một tấm trong số đó mở miệng nói: 'Ta là ngươi khi ngươi ngừng giả vờ.' Cô chạy đi, nhưng tiếng cười trong gương vẫn vang mãi.",
      ExplicitContent: true,
      ReleaseDate: "2025-03-10T09:00:00.000Z",
      IsReleased: true,
      ImageUrl:
        "https://i.pinimg.com/736x/a8/ee/17/a8ee1756b800ac86b0573d330b61cf07.jpg",
      AudioFileKey: "9",
      AudioFileSize: 47000000,
      AudioLength: 2500,
      AudioFingerprint: "©492873",
      PodcastEpisodeSubscriptionType: { Id: 1, Name: "Subcribed Show Only" },
      PodcastShowId: "1",
      SeasonNumber: 1,
      TotalSave: 218,
      ListenCount: 1304,
      IsAudioPublishable: true,
      TakenDownReason: null,
      DeletedAt: null,
      CreatedAt: "2025-02-10T11:56:06.138Z",
      UpdatedAt: "2025-02-10T11:56:06.138Z",
    },
    {
      Id: "a1c2f301-010",
      Name: "Tập 10: Những Đứa Trẻ Không Hồn",
      Description:
        "Alice gặp nhóm trẻ con mặc đồng phục cũ kỹ, gương mặt tái nhợt. Chúng nói rằng đã bị mắc kẹt trong khu vườn hàng trăm năm. Mỗi khi có ai mới đến, khu vườn sẽ lấy đi một ký ức để nuôi sống chính nó. Khi Alice nhìn xuống tay mình, những dòng chữ viết trên tay đã biến mất.",
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
      TakenDownReason: null,
      DeletedAt: null,
      CreatedAt: "2025-02-15T11:56:06.138Z",
      UpdatedAt: "2025-02-15T11:56:06.138Z",
    },
    {
      Id: "a1c2f301-011",
      Name: "Tập 11: Con Đường Không Hồi Kết",
      Description:
        "Alice cố gắng thoát ra khỏi khu vườn, nhưng mỗi con đường lại quay về cùng một chỗ. Cô đánh dấu thân cây bằng dao, song dấu vết biến mất sau vài phút. Từ xa, Người Giữ Khu Vườn đứng nhìn, mỉm cười: 'Không ai rời khỏi nơi này, trừ khi khu vườn muốn.'",
      ExplicitContent: true,
      ReleaseDate: "2025-03-20T09:00:00.000Z",
      IsReleased: true,
      ImageUrl:
        "https://i.pinimg.com/736x/1e/e7/2e/1ee72e7d5a5b3d424e676251f13c40ee.jpg",
      AudioFileKey: "11",
      AudioFileSize: 45000000,
      AudioLength: 2300,
      AudioFingerprint: "©504823",
      PodcastEpisodeSubscriptionType: { Id: 1, Name: "Subcribed Show Only" },
      PodcastShowId: "1",
      SeasonNumber: 1,
      TotalSave: 201,
      ListenCount: 1020,
      IsAudioPublishable: true,
      TakenDownReason: null,
      DeletedAt: null,
      CreatedAt: "2025-02-20T11:56:06.138Z",
      UpdatedAt: "2025-02-20T11:56:06.138Z",
    },
  ],
};

const suggesstionData = [
  {
    Id: "uuid1",
    Name: "Show 1",
    ImageUrl:
      "https://i.pinimg.com/1200x/18/0a/91/180a91cf15f65bb31f729b4488119eae.jpg",
    UploadFrequency: "Daily",
  },
  {
    Id: "uuid2",
    Name: "Show 2",
    ImageUrl:
      "https://i.pinimg.com/1200x/4a/9b/8d/4a9b8d86f5f5da147e030b11398ccc78.jpg",
    UploadFrequency: "Monthly",
  },
  {
    Id: "uuid3",
    Name: "Show 3",
    ImageUrl:
      "https://i.pinimg.com/1200x/d2/30/f3/d230f3c8e238b76ac7c16efed5f88b6e.jpg",
    UploadFrequency: "Daily",
  },
  {
    Id: "uuid4",
    Name: "Show 4",
    ImageUrl:
      "https://i.pinimg.com/1200x/d1/25/2a/d1252ae0bd823fa0fc052572d992f4a6.jpg",
    UploadFrequency: "Every Even Days",
  },
  {
    Id: "uuid5",
    Name: "Show 5",
    ImageUrl:
      "https://i.pinimg.com/1200x/09/90/93/099093e23510ec24d8e747bb57d62fbe.jpg",
    UploadFrequency: "Daily",
  },
];

export default function ShowDetailsScreen() {
  const { id } = useLocalSearchParams<{ id: string }>();
  const router = useRouter();

  // STATES
  const [show, setShow] = useState<ShowDetails | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(true);

  // HOOKS
  useFocusEffect(
    useCallback(() => {
      if (id && id !== "") {
        fetch(id);
      }
      return () => {};
    }, [])
  );

  // FUNCTIONS
  const fetch = async (id: string) => {
    setIsLoading(true);
    setTimeout(() => {
      setShow(showData);
      setIsLoading(false);
    }, 300);
  };

  if (isLoading) {
    return (
      <View className="w-full h-screen justify-center items-center gap-3">
        <ActivityIndicator />
        <Text>Loading ...</Text>
      </View>
    );
  }
  // ...render theo id
  if (!show) {
    return (
      <View className="w-full h-full flex items-center justify-center">
        <Text>Cannot Find Show :(</Text>
      </View>
    );
  } else {
    return (
      <ScrollView style={style.container}>
        <ShowInformations {...show} />
        <View className="p-[30px] gap-10">
          <EpisodeList episodes={show.ShowEpisodeList} />
          <RatingAndReview ratings={show.RatingList} />
          <ShowDescription description={show.Description} />
          <MoreInformations
            Copyright={show.Copyright}
            Podcaster={show.Podcaster}
            PodcastCategory={show.PodcastCategory}
            PodcastSubCategory={show.PodcastSubCategory}
            PodcastChannel={show.PodcastChannel}
            ReleaseDate={show.ReleaseDate}
            ShowEpisodeList={show.ShowEpisodeList}
          />
        </View>
        <Suggesstion shows={suggesstionData} />
      </ScrollView>
    );
  }
}

const style = StyleSheet.create({
  container: {},
});
