import { Card, CardContent } from "@/components/ui/card";
import {
  Carousel,
  CarouselContent,
  CarouselItem,
} from "@/components/ui/carousel";
import { Skeleton } from "@/components/ui/skeleton";
import Autoplay from "embla-carousel-autoplay";
import YouMightLikeItCard from "./components/YouMightLikeItCardCarousel";
import { useSelector } from "react-redux";
import type { RootState } from "@/redux/store";
import { useEffect, useState } from "react";

import { GrFormNext } from "react-icons/gr";
import ShowCard from "./components/ShowCard";
import ShowCardWithRating from "./components/ShowCardWithRating";
import ShowCardWithCategory from "./components/ShowCardWithCategory";
import { ca } from "zod/v4/locales";
import EpisodeCard from "./components/EpisodeCard";

type YouMightLikeItShow = {
  Id: number;
  ImageUrl: string;
};

const youMightLikeItData: YouMightLikeItShow[] = [
  {
    Id: 1,
    ImageUrl:
      "https://i.pinimg.com/736x/3c/af/ff/3cafff2e5ca0a4bcd96116afdae2a78b.jpg",
  },
  {
    Id: 2,
    ImageUrl:
      "https://i.pinimg.com/1200x/5f/f4/83/5ff483cf28501863e5415cd1546951de.jpg",
  },
  {
    Id: 3,
    ImageUrl:
      "https://i.pinimg.com/736x/e6/27/23/e62723569055c73d144400159652c87f.jpg",
  },
  {
    Id: 4,
    ImageUrl:
      "https://i.pinimg.com/736x/a0/fb/4d/a0fb4daeaee5f4422ebc488feff5e4a3.jpg",
  },
  {
    Id: 5,
    ImageUrl:
      "https://i.pinimg.com/736x/1b/b6/e3/1bb6e3635d8f5821603aa40638ae4e69.jpg",
  },
];

type NewShow = {
  Id: number;
  Name: string;
  ImageUrl: string;
  Podcaster: {
    Id: number;
    FullName: string;
  };
};

const newShowsData: NewShow[] = [
  {
    Id: 1,
    Name: "Rồi Ta Sẽ Quên",
    ImageUrl:
      "https://i.pinimg.com/1200x/98/13/c4/9813c4234a5ea4149442a5e2aa3a84df.jpg",
    Podcaster: {
      Id: 1,
      FullName: "Thịnh Suy",
    },
  },
  {
    Id: 2,
    Name: "Sa Mạc Giữa Đại Dương",
    ImageUrl:
      "https://i.pinimg.com/1200x/3f/a7/33/3fa73394e01c499d2d0059bb68a4c6e4.jpg",
    Podcaster: {
      Id: 2,
      FullName: "Angdea Durelen",
    },
  },
  {
    Id: 3,
    Name: "Ngày Chưa Quên Mất",
    ImageUrl:
      "https://i.pinimg.com/736x/be/33/a1/be33a1612ca1402bf0450687f0665ebb.jpg",
    Podcaster: {
      Id: 3,
      FullName: "SAMURICE",
    },
  },
  {
    Id: 4,
    Name: "Tòa Thị Chính",
    ImageUrl:
      "https://i.pinimg.com/1200x/c7/59/0b/c7590b3e16bfd8e841dd33e1b3c87da4.jpg",
    Podcaster: {
      Id: 4,
      FullName: "Kanzo Salona",
    },
  },
  {
    Id: 5,
    Name: "Chiều Không Gian thứ 361",
    ImageUrl:
      "https://i.pinimg.com/736x/22/1d/5a/221d5a5e7f00224ee69ee5252fb054d3.jpg",
    Podcaster: {
      Id: 5,
      FullName: "SAMURICE",
    },
  },
  {
    Id: 6,
    Name: "SkyFall",
    ImageUrl:
      "https://i.pinimg.com/736x/5e/aa/84/5eaa84f7c503a07e7d54c8b8f56321a9.jpg",
    Podcaster: {
      Id: 6,
      FullName: "Thống",
    },
  },
  {
    Id: 7,
    Name: "Cánh Chim Cuối Cùng",
    ImageUrl:
      "https://i.pinimg.com/736x/d4/15/71/d41571849b4aeacb01649b10753306cf.jpg",
    Podcaster: {
      Id: 6,
      FullName: "Better Version",
    },
  },
];

type NewEpisode = {
  Id: string;
  Name: string;
  Description: string;
  ReleaseDate: string; // ISO
  MainFileKey: string;
  AudioLength: number;
  ImageUrl: string;
  Show: { Id: number; ImageUrl: string };
};

const newEpisodesData: NewEpisode[] = [
  {
    Id: "1",
    Name: "Kẻ Nuôi Quạ",
    Description:
      "Người ta thường nuôi các loài chim cảnh, hoặc lớn hơn là đại bàng. Nhưng bạn đã bao giờ nghe đến nuôi Quạ chưa ? Một sinh vật mang điềm xấu theo quan niệm nhiều nơi thì có gì đáng để nuôi nhỉ ?",
    ReleaseDate: "2025-10-21T14:32:00Z",
    MainFileKey: "",
    AudioLength: 1825,
    ImageUrl:
      "https://i.pinimg.com/736x/26/94/c8/2694c8749f540f1e8da25d768d2a8ea5.jpg",
    Show: {
      Id: 1,
      ImageUrl:
        "https://i.pinimg.com/736x/c8/91/40/c8914066680ae9720660036e5543524f.jpg",
    },
  },
  {
    Id: "2",
    Name: "Ai đó sau đồi",
    Description:
      "Có vẻ như làng Kanatic này hơi yên bình quá so với người GenZ như Luke, anh ta chẳng có gì làm ngoài ra vườn trồng nốt đám cây",
    ReleaseDate: "2025-10-19T09:48:00Z",
    MainFileKey: "",
    AudioLength: 2473,
    ImageUrl:
      "https://i.pinimg.com/736x/f1/d0/e2/f1d0e253c37d89bcefa61ad7be5ab0c6.jpg",
    Show: {
      Id: 1,
      ImageUrl:
        "https://i.pinimg.com/736x/95/a0/08/95a008e8bebe538c5981d636da632872.jpg",
    },
  },
  {
    Id: "3",
    Name: "Hinatako",
    Description:
      "lorem ipsum dolor sit amet consectetur adipiscing elit mauris mauris malesuada velit sed tristique justo vestibulum ut pellentesque erat curabitur quis rhoncus dui sed vel arcu tempor sem tempor elementum velit in lobortis semper nulla vitae ligula nec felis elit placerat ac nulla auctor interdum neque quisque gravida eros quis felis varius vulputate eget nec metus phasellus",
    ReleaseDate: "2025-10-23T02:15:00Z",
    MainFileKey: "",
    AudioLength: 1532,
    ImageUrl:
      "https://i.pinimg.com/1200x/7e/6d/f1/7e6df141a67c984d997084d9d63938a1.jpg",
    Show: {
      Id: 1,
      ImageUrl:
        "https://i.pinimg.com/1200x/4e/03/e9/4e03e9413dcd7f0b1edd56705f4a9c1b.jpg",
    },
  },
  {
    Id: "4",
    Name: "Lonely King",
    Description:
      "lorem ipsum dolor sit amet consectetur adipiscing elit mauris mauris malesuada velit sed tristique justo vestibulum ut pellentesque erat curabitur quis rhoncus dui sed vel arcu tempor sem tempor elementum velit in lobortis semper nulla vitae ligula nec felis elit placerat ac nulla auctor interdum neque quisque gravida eros quis felis varius vulputate eget nec metus phasellus",
    ReleaseDate: "2025-10-17T18:09:00Z",
    MainFileKey: "",
    AudioLength: 2120,
    ImageUrl:
      "https://i.pinimg.com/736x/c6/54/fb/c654fb003cb1bc60666cc63a5da136aa.jpg",
    Show: {
      Id: 1,
      ImageUrl:
        "https://i.pinimg.com/736x/f7/7a/a8/f77aa8761cd65165c632de69715f02cd.jpg",
    },
  },
  {
    Id: "5",
    Name: "The Killer Paradox",
    Description:
      "lorem ipsum dolor sit amet consectetur adipiscing elit mauris mauris malesuada velit sed tristique justo vestibulum ut pellentesque erat curabitur quis rhoncus dui sed vel arcu tempor sem tempor elementum velit in lobortis semper nulla vitae ligula nec felis elit placerat ac nulla auctor interdum neque quisque gravida eros quis felis varius vulputate eget nec metus phasellus",
    ReleaseDate: "2025-10-20T07:56:00Z",
    MainFileKey: "",
    AudioLength: 2867,
    ImageUrl:
      "https://i.pinimg.com/736x/c5/8f/41/c58f41c11555bfbfa3b983cf8e23b9e7.jpg",
    Show: {
      Id: 1,
      ImageUrl:
        "https://i.pinimg.com/736x/a4/ea/91/a4ea9123bdcc6a4df14ae8b695224a24.jpg",
    },
  },
  {
    Id: "6",
    Name: "Chuyện Cổ Tích Chưa Kể",
    Description:
      "lorem ipsum dolor sit amet consectetur adipiscing elit mauris mauris malesuada velit sed tristique justo vestibulum ut pellentesque erat curabitur quis rhoncus dui sed vel arcu tempor sem tempor elementum velit in lobortis semper nulla vitae ligula nec felis elit placerat ac nulla auctor interdum neque quisque gravida eros quis felis varius vulputate eget nec metus phasellus",
    ReleaseDate: "2025-10-22T11:41:00Z",
    MainFileKey: "",
    AudioLength: 1958,
    ImageUrl:
      "https://i.pinimg.com/736x/20/be/fe/20befe38ec90ab92001ad2f268d9f610.jpg",
    Show: {
      Id: 1,
      ImageUrl:
        "https://i.pinimg.com/474x/50/e1/9e/50e19ec17cd704860ce8abd19f0d2762.jpg",
    },
  },
];

type HighlyRatedShow = {
  Id: number;
  Name: string;
  ImageUrl: string;
  Podcaster: {
    Id: number;
    FullName: string;
  };
  Rating: number;
  RatingCount: number;
};

const higlyRatedShowsData: HighlyRatedShow[] = [
  {
    Id: 8,
    Name: "Chạy Khỏi Nhân Gian",
    ImageUrl:
      "https://i.pinimg.com/736x/8a/d5/bd/8ad5bd9c9874dc1220e872eeb35727ae.jpg",
    Podcaster: {
      Id: 8,
      FullName: "Ngô Tuấn Sỹ",
    },
    Rating: 4.9,
    RatingCount: 224100,
  },
  {
    Id: 9,
    Name: "Mất Kết Nối",
    ImageUrl:
      "https://i.pinimg.com/1200x/83/77/ab/8377ab1fcad3cb89f46a74bddb4b2519.jpg",
    Podcaster: {
      Id: 9,
      FullName: "Jean H. Lee",
    },
    Rating: 4.8,
    RatingCount: 1204110,
  },
  {
    Id: 10,
    Name: "Đuối Nước",
    ImageUrl:
      "https://i.pinimg.com/736x/46/fb/58/46fb582a9fa8011d4e07aeebb08947f3.jpg",
    Podcaster: {
      Id: 10,
      FullName: "SAMURICE",
    },
    Rating: 4.8,
    RatingCount: 500210,
  },
  {
    Id: 11,
    Name: "Chuyện Công Viên Nọ",
    ImageUrl:
      "https://i.pinimg.com/736x/e1/b6/1e/e1b61e693884193515a20ba68b422ab7.jpg",
    Podcaster: {
      Id: 11,
      FullName: "Kanzo Salona",
    },
    Rating: 4.9,
    RatingCount: 10214,
  },
  {
    Id: 12,
    Name: "Cảm Xúc Trên Lan Can",
    ImageUrl:
      "https://i.pinimg.com/736x/ca/92/a3/ca92a3676f829a7822182a54c3a6f188.jpg",
    Podcaster: {
      Id: 12,
      FullName: "Thống",
    },
    Rating: 4.6,
    RatingCount: 1256200,
  },
  {
    Id: 13,
    Name: "Mèo Dạy Hải Âu Bay",
    ImageUrl:
      "https://i.pinimg.com/736x/77/40/21/7740210dd5dccfd06c96d7457a5c5874.jpg",
    Podcaster: {
      Id: 13,
      FullName: "Shawty Dun Lie",
    },
    Rating: 4.7,
    RatingCount: 320134,
  },
  {
    Id: 14,
    Name: "Đau Thương Từ Yêu Thương",
    ImageUrl:
      "https://i.pinimg.com/1200x/4d/27/9d/4d279de526e2f89721a026c2eeb41d33.jpg",
    Podcaster: {
      Id: 14,
      FullName: "Better Version",
    },
    Rating: 4.9,
    RatingCount: 98310,
  },
];

type BaseOnListenedShow = {
  Id: number;
  Name: string;
  ImageUrl: string;
  Podcaster: {
    Id: number;
    FullName: string;
  };
  Category: {
    Id: number;
    Name: string;
  };
};

const baseOnListenedShowsData: BaseOnListenedShow[] = [
  {
    Id: 15,
    Name: "Sống Giữa Bầy Cừu",
    ImageUrl:
      "https://i.pinimg.com/1200x/bd/60/28/bd60280b725148a153be71b31ea2de51.jpg",
    Podcaster: {
      Id: 15,
      FullName: "Razor Caluvas",
    },
    Category: {
      Id: 1,
      Name: "Social Psychology",
    },
  },
  {
    Id: 16,
    Name: "The Eyes Man",
    ImageUrl:
      "https://i.pinimg.com/736x/3f/28/02/3f2802e59c9e8629b022dd6a9f213371.jpg",
    Podcaster: {
      Id: 16,
      FullName: "Ozy Nathanellie",
    },
    Category: {
      Id: 2,
      Name: "Horrified",
    },
  },
  {
    Id: 17,
    Name: "Chủ Nghĩa Hư Vô",
    ImageUrl:
      "https://i.pinimg.com/1200x/fb/1e/83/fb1e8397b0a7a7a72fa8a765afcc21c9.jpg",
    Podcaster: {
      Id: 17,
      FullName: "SAMURICE",
    },
    Category: {
      Id: 3,
      Name: "Religion",
    },
  },
  {
    Id: 18,
    Name: "Toàn Trí Độc Giả",
    ImageUrl:
      "https://i.pinimg.com/736x/5e/e3/be/5ee3be0c77450f2cc6b3d303770a2e0e.jpg",
    Podcaster: {
      Id: 18,
      FullName: "SingNSong",
    },
    Category: {
      Id: 4,
      Name: "Cultivation",
    },
  },
  {
    Id: 19,
    Name: "Xác Trắng Trong Rừng Đen",
    ImageUrl:
      "https://i.pinimg.com/736x/de/10/4d/de104d7b4a9518723202d011b93f38dd.jpg",
    Podcaster: {
      Id: 19,
      FullName: "Văn Tùng Siêu Kỳ Án",
    },
    Category: {
      Id: 5,
      Name: "True Crime",
    },
  },
  {
    Id: 20,
    Name: "Bí Ẩn Hội Illuminati",
    ImageUrl:
      "https://i.pinimg.com/1200x/a1/0b/41/a10b41ef05d0884098e95496bf1fa841.jpg",
    Podcaster: {
      Id: 20,
      FullName: "Samurice",
    },
    Category: {
      Id: 6,
      Name: "Urban Legends",
    },
  },
  {
    Id: 21,
    Name: "Mỹ và sự thao túng",
    ImageUrl:
      "https://i.pinimg.com/1200x/79/80/27/798027ef218d175ba97868df457b249c.jpg",
    Podcaster: {
      Id: 21,
      FullName: "Samurice",
    },
    Category: {
      Id: 7,
      Name: "Politics",
    },
  },
];

const DiscoveryPage = () => {
  const user = useSelector((state: RootState) => state.auth.user);
  const [isLoading, setIsLoading] = useState(false);
  const [youMightLikeItShows, setYouMightLikeItShows] = useState<
    YouMightLikeItShow[]
  >([]);
  const [newShows, setNewShows] = useState<NewShow[]>([]);
  const [highlyRatedShows, setHighlyRatedShows] = useState<HighlyRatedShow[]>(
    []
  );
  const [newEpisodes, setNewEpisodes] = useState([]);
  const [baseOnWhatYouListenTo, setBaseOnWhatYouListenTo] = useState<
    BaseOnListenedShow[]
  >([]);

  useEffect(() => {
    fetch();
  }, []);

  const fetch = async () => {
    setIsLoading(true);
    setTimeout(() => {
      setYouMightLikeItShows(youMightLikeItData);
      setNewShows(newShowsData);
      setHighlyRatedShows(higlyRatedShowsData);
      setNewEpisodes(newEpisodesData as any);
      if (user) {
        setBaseOnWhatYouListenTo(baseOnListenedShowsData);
      }
      setIsLoading(false);
    }, 6000);
  };

  return (
    <div
      className="
      flex flex-col items-center gap-5 mb-20
    "
    >
      {/* You might like it */}
      <div className="w-full flex flex-col gap-4">
        <div className="hidden md:inline-flex w-full items-center justify-between">
          <p className="font-poppins text-white text-4xl">
            You Might <span className="text-mystic-green">Like It</span>
          </p>
          <p className="text-sm font-bold cursor-pointer underline text-gray-300 hover:text-mystic-green">
            See all
          </p>
        </div>

        <div className="md:hidden w-full flex items-center justify-start cursor-pointer">
          <p className="font-poppins font-semibold text-mystic-green text-lg hover:underline">
            You Might Like It
          </p>
          <GrFormNext color="#aae339" size={25} />
        </div>

        {isLoading ? (
          <Carousel
            opts={{
              align: "start",
              loop: true,
            }}
            className="w-full"
          >
            <CarouselContent>
              {Array.from({ length: 5 }).map((_, index) => (
                <CarouselItem
                  key={index}
                  className="basis-1/2 md:basis-1/3 lg:basis-1/4"
                >
                  <div className="p-1">
                    <Skeleton className="w-full h-full aspect-square rounded-lg" />
                  </div>
                </CarouselItem>
              ))}
            </CarouselContent>
          </Carousel>
        ) : (
          <Carousel
            opts={{
              align: "start",
              loop: true,
            }}
            plugins={[
              Autoplay({
                delay: 2000,
              }),
            ]}
            className="w-full"
          >
            <CarouselContent>
              {youMightLikeItData.map((card, index) => (
                <CarouselItem
                  key={index}
                  className="basis-1/2 md:basis-1/3 lg:basis-1/4"
                >
                  <div className="p-1">
                    <YouMightLikeItCard card={card} />
                  </div>
                </CarouselItem>
              ))}
            </CarouselContent>
          </Carousel>
        )}
      </div>

      {/* New Shows */}
      <div className="w-full flex flex-col mt-10 gap-5">
        <div className="hidden md:inline-flex w-full items-center justify-between">
          <p className="font-poppins font-bold text-white text-2xl">
            New Shows
          </p>
          <p className="text-sm font-bold cursor-pointer underline text-gray-300 hover:text-mystic-green">
            See all
          </p>
        </div>

        <div className="md:hidden w-full flex items-center justify-start cursor-pointer">
          <p className="font-poppins font-semibold text-white text-md hover:underline">
            <span className="text-mystic-green">New</span> Shows
          </p>
          <GrFormNext color="#fff" size={25} />
        </div>

        {isLoading ? (
          <Carousel
            opts={{
              align: "start",
              loop: false,
            }}
            className="w-full"
          >
            <CarouselContent>
              {Array.from({ length: 6 }).map((_, index) => (
                <CarouselItem
                  key={index}
                  className="basis-1/3 md:basis-1/4 lg:basis-1/6"
                >
                  <div className="p-1">
                    <Skeleton className="w-full h-full aspect-square rounded-lg mb-2" />
                    <Skeleton className="w-11/12 h-5 rounded-xs mb-2" />
                    <Skeleton className="w-8/12 h-3 rounded-xs" />
                  </div>
                </CarouselItem>
              ))}
            </CarouselContent>
          </Carousel>
        ) : (
          <Carousel
            opts={{
              align: "start",
              loop: true,
            }}
            plugins={[
              Autoplay({
                delay: 3000,
              }),
            ]}
            className="w-full"
          >
            <CarouselContent>
              {newShows.map((card, index) => (
                <CarouselItem
                  key={index}
                  className="basis-1/3 md:basis-1/4 lg:basis-1/6"
                >
                  <div className="p-1">
                    <ShowCard card={card} />
                  </div>
                </CarouselItem>
              ))}
            </CarouselContent>
          </Carousel>
        )}
      </div>

      {/* New Episodes */}
      <div className="w-full flex flex-col mt-10 gap-5">
        <div className="hidden md:inline-flex w-full items-center justify-between">
          <p className="font-poppins font-bold text-white text-2xl">
            New Episodes
          </p>
          <p className="text-sm font-bold cursor-pointer underline text-gray-300 hover:text-mystic-green">
            See all
          </p>
        </div>

        <div className="md:hidden w-full flex items-center justify-start cursor-pointer">
          <p className="font-poppins font-semibold text-white text-md hover:underline">
            <span className="text-mystic-green">New</span> Episodes
          </p>
          <GrFormNext color="#fff" size={25} />
        </div>

        {isLoading ? (
          <Carousel
            opts={{
              align: "start",
              loop: false,
            }}
            className="w-full"
          >
            <CarouselContent>
              {Array.from({ length: 5 }).map((_, index) => (
                <CarouselItem
                  key={index}
                  className="basis-full sm:basis-1/2 md:basis-1/3 lg:basis-1/5"
                >
                  <div className="p-1">
                    <Skeleton className="w-full aspect-[3/4] rounded-lg" />
                  </div>
                </CarouselItem>
              ))}
            </CarouselContent>
          </Carousel>
        ) : (
          <Carousel
            opts={{
              align: "start",
              loop: true,
            }}
            plugins={[
              Autoplay({
                delay: 3000,
              }),
            ]}
            className="w-full"
          >
            <CarouselContent>
              {newEpisodes.map((episode, index) => (
                <CarouselItem
                  key={index}
                  className="basis-full sm:basis-1/2 md:basis-1/3 lg:basis-1/5"
                >
                  <div className="p-1">
                    <EpisodeCard episode={episode} />
                  </div>
                </CarouselItem>
              ))}
            </CarouselContent>
          </Carousel>
        )}
      </div>

      {/* Highly Rated Shows */}
      <div className="w-full flex flex-col mt-10 gap-5">
        <div className="hidden md:inline-flex w-full items-center justify-between">
          <p className="font-poppins font-bold text-white text-2xl">
            Highly Rated Shows
          </p>
          <p className="text-sm font-bold cursor-pointer underline text-gray-300 hover:text-mystic-green">
            See all
          </p>
        </div>

        <div className="md:hidden w-full flex items-center justify-start cursor-pointer">
          <p className="font-poppins font-semibold text-white text-md hover:underline">
            <span className="text-mystic-green">Highly Rated</span> Shows
          </p>
          <GrFormNext color="#fff" size={25} />
        </div>

        {isLoading ? (
          <Carousel
            opts={{
              align: "start",
              loop: false,
            }}
            className="w-full"
          >
            <CarouselContent>
              {Array.from({ length: 6 }).map((_, index) => (
                <CarouselItem
                  key={index}
                  className="basis-1/3 md:basis-1/4 lg:basis-1/6"
                >
                  <div className="p-1">
                    <Skeleton className="w-full h-full aspect-square rounded-lg mb-2" />
                    <Skeleton className="w-11/12 h-5 rounded-xs mb-2" />
                    <Skeleton className="w-8/12 h-3 rounded-xs" />
                  </div>
                </CarouselItem>
              ))}
            </CarouselContent>
          </Carousel>
        ) : (
          <Carousel
            opts={{
              align: "start",
              loop: true,
            }}
            plugins={[
              Autoplay({
                delay: 4000,
              }),
            ]}
            className="w-full"
          >
            <CarouselContent>
              {highlyRatedShows.map((card, index) => (
                <CarouselItem
                  key={index}
                  className="basis-1/3 md:basis-1/4 lg:basis-1/6"
                >
                  <div className="p-1">
                    <ShowCardWithRating card={card} />
                  </div>
                </CarouselItem>
              ))}
            </CarouselContent>
          </Carousel>
        )}
      </div>

      {/* Base On User History Shows */}
      {user && (
        <div className="w-full flex flex-col mt-10 gap-5">
          <div className="hidden md:inline-flex w-full items-center justify-between">
            <p className="font-poppins font-bold text-white text-2xl">
              Base On What You Have Listened
            </p>
            <p className="text-sm font-bold cursor-pointer underline text-gray-300 hover:text-mystic-green">
              See all
            </p>
          </div>

          <div className="md:hidden w-full flex items-center justify-start cursor-pointer">
            <p className="font-poppins font-semibold text-white text-md hover:underline">
              <span className="text-mystic-green">Base On</span> What You Have
              Listened
            </p>
            <GrFormNext color="#fff" size={25} />
          </div>

          {isLoading ? (
            <Carousel
              opts={{
                align: "start",
                loop: false,
              }}
              className="w-full"
            >
              <CarouselContent>
                {Array.from({ length: 6 }).map((_, index) => (
                  <CarouselItem
                    key={index}
                    className="basis-1/3 md:basis-1/4 lg:basis-1/6"
                  >
                    <div className="p-1">
                      <Skeleton className="w-full h-full aspect-square rounded-lg mb-2" />
                      <Skeleton className="w-11/12 h-5 rounded-xs mb-2" />
                      <Skeleton className="w-8/12 h-3 rounded-xs" />
                    </div>
                  </CarouselItem>
                ))}
              </CarouselContent>
            </Carousel>
          ) : (
            <Carousel
              opts={{
                align: "start",
                loop: true,
              }}
              plugins={[
                Autoplay({
                  delay: 3000,
                }),
              ]}
              className="w-full"
            >
              <CarouselContent>
                {baseOnWhatYouListenTo.map((card, index) => (
                  <CarouselItem
                    key={index}
                    className="basis-1/3 md:basis-1/4 lg:basis-1/6"
                  >
                    <div className="p-1">
                      <ShowCardWithCategory card={card} />
                    </div>
                  </CarouselItem>
                ))}
              </CarouselContent>
            </Carousel>
          )}
        </div>
      )}
    </div>
  );
};

export default DiscoveryPage;
