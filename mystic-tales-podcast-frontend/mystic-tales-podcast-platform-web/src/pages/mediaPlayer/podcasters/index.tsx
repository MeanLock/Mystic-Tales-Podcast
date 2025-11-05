import {
  Carousel,
  CarouselContent,
  CarouselItem,
} from "@/components/ui/carousel";
import type { PodcastersByCategory, PodcasterUI } from "@/core/types/podcaster";
import type { SearchIcon } from "lucide-react";
import { useEffect, useState } from "react";
import PodcasterCard from "./components/PodcasterCard";
import Autoplay from "embla-carousel-autoplay";
import HighlyRatedCard from "./components/HighlyRatedCard";

export type TopPodcasterUI = PodcasterUI & { Top: number };
const MockTopPodcasters: TopPodcasterUI[] = [
  {
    Id: 1,
    Email: "podcaster1@gmail.com",
    Role: { Id: 1, Name: "Customer" },
    FullName: "Ari Staprans Leff",
    Dob: "08-08-1994",
    Gender: "male",
    Address: "",
    Phone: "",
    Balance: 0,
    ImageUrl:
      "https://i.pinimg.com/736x/72/26/04/722604c09eef53416bf4e38460deb4ba.jpg",
    IsVerified: true,
    GoogleId: null,
    PodcastListenSlot: 0,
    ViolationPoint: 0,
    ViolationLevel: 0,
    LastViolationPointChanged: null,
    LastViolationLevelChanged: null,
    LastPodcastListenSlotChanged: "",
    DeactivatedAt: null,
    CreatedAt: "",
    UpdatedAt: "",
    PodcasterProfile: {
      AccountId: 1,
      Name: "Lauv",
      Description:
        'Ari Staprans Leff, known professionally as Lauv, is an American musician best known for his breakout hit "I Like Me Better".',
      AverageRating: 4.9,
      RatingCount: 2340111,
      TotalFollow: 1200000,
      ListenCount: 30215521456,
      CommitmentDocumentFileKey: "",
      BuddyAudioFileKey: "",
      OwnedBookingStorageSize: 0,
      UsedBookingStorageSize: 0,
      IsVerified: true,
      CreatedAt: "",
      UpdatedAt: "",
    },
    ReviewList: [
      {
        Id: "1",
        Title: "",
        Content: "",
        Rating: 0,
        Account: { Id: 0, FullName: "", Email: "", ImageUrl: "" },
        DeletedAt: null,
        UpdatedAt: null,
      },
    ],
    Top: 1,
  },

  {
    Id: 2,
    Email: "podcaster2@gmail.com",
    Role: { Id: 1, Name: "Customer" },
    FullName: "Adele Laurie Blue Adkins",
    Dob: "05-05-1988",
    Gender: "female",
    Address: "",
    Phone: "",
    Balance: 0,
    ImageUrl:
      "https://i.pinimg.com/736x/16/0b/33/160b33789fd7d8b14bf5be9f22372489.jpg",
    IsVerified: true,
    GoogleId: null,
    PodcastListenSlot: 0,
    ViolationPoint: 0,
    ViolationLevel: 0,
    LastViolationPointChanged: null,
    LastViolationLevelChanged: null,
    LastPodcastListenSlotChanged: "",
    DeactivatedAt: null,
    CreatedAt: "",
    UpdatedAt: "",
    PodcasterProfile: {
      AccountId: 2,
      Name: "Adele",
      Description:
        "Adele is an English singer-songwriter known for her powerful voice and emotional ballads such as 'Hello' and 'Someone Like You'.",
      AverageRating: 4.8,
      RatingCount: 4210055,
      TotalFollow: 3500000,
      ListenCount: 51200000221,
      CommitmentDocumentFileKey: "",
      BuddyAudioFileKey: "",
      OwnedBookingStorageSize: 0,
      UsedBookingStorageSize: 0,
      IsVerified: true,
      CreatedAt: "",
      UpdatedAt: "",
    },
    ReviewList: [
      {
        Id: "2",
        Title: "",
        Content: "",
        Rating: 0,
        Account: { Id: 0, FullName: "", Email: "", ImageUrl: "" },
        DeletedAt: null,
        UpdatedAt: null,
      },
    ],
    Top: 2,
  },

  {
    Id: 3,
    Email: "podcaster3@gmail.com",
    Role: { Id: 1, Name: "Customer" },
    FullName: "Harry Styles",
    Dob: "01-02-1994",
    Gender: "male",
    Address: "",
    Phone: "",
    Balance: 0,
    ImageUrl:
      "https://i.pinimg.com/1200x/8e/2a/e9/8e2ae99c2e87ccb5611eb25fb1141a6e.jpg",
    IsVerified: true,
    GoogleId: null,
    PodcastListenSlot: 0,
    ViolationPoint: 0,
    ViolationLevel: 0,
    LastViolationPointChanged: null,
    LastViolationLevelChanged: null,
    LastPodcastListenSlotChanged: "",
    DeactivatedAt: null,
    CreatedAt: "",
    UpdatedAt: "",
    PodcasterProfile: {
      AccountId: 3,
      Name: "Harry Styles",
      Description:
        "British artist and performer Harry Styles blends pop, rock, and funk elements, known for hits like 'Watermelon Sugar'.",
      AverageRating: 4.7,
      RatingCount: 3150112,
      TotalFollow: 2700000,
      ListenCount: 40785211440,
      CommitmentDocumentFileKey: "",
      BuddyAudioFileKey: "",
      OwnedBookingStorageSize: 0,
      UsedBookingStorageSize: 0,
      IsVerified: true,
      CreatedAt: "",
      UpdatedAt: "",
    },
    ReviewList: [
      {
        Id: "3",
        Title: "",
        Content: "",
        Rating: 0,
        Account: { Id: 0, FullName: "", Email: "", ImageUrl: "" },
        DeletedAt: null,
        UpdatedAt: null,
      },
    ],
    Top: 3,
  },

  {
    Id: 4,
    Email: "podcaster4@gmail.com",
    Role: { Id: 1, Name: "Customer" },
    FullName: "Billie Eilish Pirate Baird O'Connell",
    Dob: "18-12-2001",
    Gender: "female",
    Address: "",
    Phone: "",
    Balance: 0,
    ImageUrl:
      "https://i.pinimg.com/736x/6f/0c/3a/6f0c3a5d61c4ab13760459dfed7e0a72.jpg",
    IsVerified: true,
    GoogleId: null,
    PodcastListenSlot: 0,
    ViolationPoint: 0,
    ViolationLevel: 0,
    LastViolationPointChanged: null,
    LastViolationLevelChanged: null,
    LastPodcastListenSlotChanged: "",
    DeactivatedAt: null,
    CreatedAt: "",
    UpdatedAt: "",
    PodcasterProfile: {
      AccountId: 4,
      Name: "Billie Eilish",
      Description:
        "Billie Eilish is an American singer and songwriter known for her dark pop style and Grammy-winning debut album 'When We All Fall Asleep, Where Do We Go?'.",
      AverageRating: 4.85,
      RatingCount: 3850000,
      TotalFollow: 4200000,
      ListenCount: 62015524110,
      CommitmentDocumentFileKey: "",
      BuddyAudioFileKey: "",
      OwnedBookingStorageSize: 0,
      UsedBookingStorageSize: 0,
      IsVerified: true,
      CreatedAt: "",
      UpdatedAt: "",
    },
    ReviewList: [
      {
        Id: "4",
        Title: "",
        Content: "",
        Rating: 0,
        Account: { Id: 0, FullName: "", Email: "", ImageUrl: "" },
        DeletedAt: null,
        UpdatedAt: null,
      },
    ],
    Top: 4,
  },

  {
    Id: 5,
    Email: "podcaster5@gmail.com",
    Role: { Id: 1, Name: "Customer" },
    FullName: "Ed Sheeran",
    Dob: "17-02-1991",
    Gender: "male",
    Address: "",
    Phone: "",
    Balance: 0,
    ImageUrl:
      "https://i.pinimg.com/736x/46/d2/0a/46d20a9fa668aa1717728176a214d90d.jpg",
    IsVerified: true,
    GoogleId: null,
    PodcastListenSlot: 0,
    ViolationPoint: 0,
    ViolationLevel: 0,
    LastViolationPointChanged: null,
    LastViolationLevelChanged: null,
    LastPodcastListenSlotChanged: "",
    DeactivatedAt: null,
    CreatedAt: "",
    UpdatedAt: "",
    PodcasterProfile: {
      AccountId: 5,
      Name: "Ed Sheeran",
      Description:
        "Ed Sheeran is a British singer-songwriter famous for heartfelt acoustic hits like 'Shape of You' and 'Perfect'.",
      AverageRating: 4.92,
      RatingCount: 5250000,
      TotalFollow: 5500000,
      ListenCount: 80515111523,
      CommitmentDocumentFileKey: "",
      BuddyAudioFileKey: "",
      OwnedBookingStorageSize: 0,
      UsedBookingStorageSize: 0,
      IsVerified: true,
      CreatedAt: "",
      UpdatedAt: "",
    },
    ReviewList: [
      {
        Id: "5",
        Title: "",
        Content: "",
        Rating: 0,
        Account: { Id: 0, FullName: "", Email: "", ImageUrl: "" },
        DeletedAt: null,
        UpdatedAt: null,
      },
    ],
    Top: 5,
  },

  {
    Id: 6,
    Email: "podcaster6@gmail.com",
    Role: { Id: 1, Name: "Customer" },
    FullName: "Taylor Swift",
    Dob: "13-12-1989",
    Gender: "female",
    Address: "",
    Phone: "",
    Balance: 0,
    ImageUrl:
      "https://i.pinimg.com/736x/fb/64/0e/fb640e35d65ca13f7d639ba360caa2c5.jpg",
    IsVerified: true,
    GoogleId: null,
    PodcastListenSlot: 0,
    ViolationPoint: 0,
    ViolationLevel: 0,
    LastViolationPointChanged: null,
    LastViolationLevelChanged: null,
    LastPodcastListenSlotChanged: "",
    DeactivatedAt: null,
    CreatedAt: "",
    UpdatedAt: "",
    PodcasterProfile: {
      AccountId: 6,
      Name: "Taylor Swift",
      Description:
        "Taylor Swift is a global icon, known for narrative-driven songwriting and hit albums across pop and country genres.",
      AverageRating: 4.97,
      RatingCount: 9025111,
      TotalFollow: 8500000,
      ListenCount: 112000000000,
      CommitmentDocumentFileKey: "",
      BuddyAudioFileKey: "",
      OwnedBookingStorageSize: 0,
      UsedBookingStorageSize: 0,
      IsVerified: true,
      CreatedAt: "",
      UpdatedAt: "",
    },
    ReviewList: [
      {
        Id: "6",
        Title: "",
        Content: "",
        Rating: 0,
        Account: { Id: 0, FullName: "", Email: "", ImageUrl: "" },
        DeletedAt: null,
        UpdatedAt: null,
      },
    ],
    Top: 6,
  },

  {
    Id: 7,
    Email: "podcaster7@gmail.com",
    Role: { Id: 1, Name: "Customer" },
    FullName: "Sam Smith",
    Dob: "19-05-1992",
    Gender: "male",
    Address: "",
    Phone: "",
    Balance: 0,
    ImageUrl:
      "https://i.pinimg.com/736x/84/cb/05/84cb053736975950bfb123730f50a2b5.jpg",
    IsVerified: true,
    GoogleId: null,
    PodcastListenSlot: 0,
    ViolationPoint: 0,
    ViolationLevel: 0,
    LastViolationPointChanged: null,
    LastViolationLevelChanged: null,
    LastPodcastListenSlotChanged: "",
    DeactivatedAt: null,
    CreatedAt: "",
    UpdatedAt: "",
    PodcasterProfile: {
      AccountId: 7,
      Name: "Sam Smith",
      Description:
        "Sam Smith is a British singer-songwriter known for soulful pop and emotional tracks like 'Stay With Me'.",
      AverageRating: 4.75,
      RatingCount: 2100000,
      TotalFollow: 1900000,
      ListenCount: 30855000000,
      CommitmentDocumentFileKey: "",
      BuddyAudioFileKey: "",
      OwnedBookingStorageSize: 0,
      UsedBookingStorageSize: 0,
      IsVerified: true,
      CreatedAt: "",
      UpdatedAt: "",
    },
    ReviewList: [
      {
        Id: "7",
        Title: "",
        Content: "",
        Rating: 0,
        Account: { Id: 0, FullName: "", Email: "", ImageUrl: "" },
        DeletedAt: null,
        UpdatedAt: null,
      },
    ],
    Top: 7,
  },

  {
    Id: 8,
    Email: "podcaster8@gmail.com",
    Role: { Id: 1, Name: "Customer" },
    FullName: "Dua Lipa",
    Dob: "22-08-1995",
    Gender: "female",
    Address: "",
    Phone: "",
    Balance: 0,
    ImageUrl:
      "https://i.pinimg.com/736x/24/b4/1b/24b41b947f59b88aba9d43d68f6d060f.jpg",
    IsVerified: true,
    GoogleId: null,
    PodcastListenSlot: 0,
    ViolationPoint: 0,
    ViolationLevel: 0,
    LastViolationPointChanged: null,
    LastViolationLevelChanged: null,
    LastPodcastListenSlotChanged: "",
    DeactivatedAt: null,
    CreatedAt: "",
    UpdatedAt: "",
    PodcasterProfile: {
      AccountId: 8,
      Name: "Dua Lipa",
      Description:
        "Dua Lipa is an English-Albanian singer known for her dance-pop sound and hit tracks like 'Levitating' and 'Don’t Start Now'.",
      AverageRating: 4.83,
      RatingCount: 2650000,
      TotalFollow: 2400000,
      ListenCount: 41225500000,
      CommitmentDocumentFileKey: "",
      BuddyAudioFileKey: "",
      OwnedBookingStorageSize: 0,
      UsedBookingStorageSize: 0,
      IsVerified: true,
      CreatedAt: "",
      UpdatedAt: "",
    },
    ReviewList: [
      {
        Id: "8",
        Title: "",
        Content: "",
        Rating: 0,
        Account: { Id: 0, FullName: "", Email: "", ImageUrl: "" },
        DeletedAt: null,
        UpdatedAt: null,
      },
    ],
    Top: 8,
  },

  {
    Id: 9,
    Email: "podcaster9@gmail.com",
    Role: { Id: 1, Name: "Customer" },
    FullName: "The Weeknd (Abel Tesfaye)",
    Dob: "16-02-1990",
    Gender: "male",
    Address: "",
    Phone: "",
    Balance: 0,
    ImageUrl:
      "https://i.pinimg.com/736x/a3/67/04/a367046f9259229415161584f69ab88c.jpg",
    IsVerified: true,
    GoogleId: null,
    PodcastListenSlot: 0,
    ViolationPoint: 0,
    ViolationLevel: 0,
    LastViolationPointChanged: null,
    LastViolationLevelChanged: null,
    LastPodcastListenSlotChanged: "",
    DeactivatedAt: null,
    CreatedAt: "",
    UpdatedAt: "",
    PodcasterProfile: {
      AccountId: 9,
      Name: "The Weeknd",
      Description:
        "The Weeknd is a Canadian artist blending R&B and pop with dark, cinematic production in hits like 'Blinding Lights'.",
      AverageRating: 4.94,
      RatingCount: 7200000,
      TotalFollow: 6700000,
      ListenCount: 98200000220,
      CommitmentDocumentFileKey: "",
      BuddyAudioFileKey: "",
      OwnedBookingStorageSize: 0,
      UsedBookingStorageSize: 0,
      IsVerified: true,
      CreatedAt: "",
      UpdatedAt: "",
    },
    ReviewList: [
      {
        Id: "9",
        Title: "",
        Content: "",
        Rating: 0,
        Account: { Id: 0, FullName: "", Email: "", ImageUrl: "" },
        DeletedAt: null,
        UpdatedAt: null,
      },
    ],
    Top: 9,
  },

  {
    Id: 10,
    Email: "podcaster10@gmail.com",
    Role: { Id: 1, Name: "Customer" },
    FullName: "Olivia Rodrigo",
    Dob: "20-02-2003",
    Gender: "female",
    Address: "",
    Phone: "",
    Balance: 0,
    ImageUrl:
      "https://i.pinimg.com/736x/fe/c7/53/fec7534d75b4d5547a6ce1284bf3e817.jpg",
    IsVerified: true,
    GoogleId: null,
    PodcastListenSlot: 0,
    ViolationPoint: 0,
    ViolationLevel: 0,
    LastViolationPointChanged: null,
    LastViolationLevelChanged: null,
    LastPodcastListenSlotChanged: "",
    DeactivatedAt: null,
    CreatedAt: "",
    UpdatedAt: "",
    PodcasterProfile: {
      AccountId: 10,
      Name: "Olivia Rodrigo",
      Description:
        "Olivia Rodrigo is an American singer-songwriter who gained fame with her debut single 'drivers license' and the album 'SOUR'.",
      AverageRating: 4.79,
      RatingCount: 2950000,
      TotalFollow: 2800000,
      ListenCount: 37200000000,
      CommitmentDocumentFileKey: "",
      BuddyAudioFileKey: "",
      OwnedBookingStorageSize: 0,
      UsedBookingStorageSize: 0,
      IsVerified: true,
      CreatedAt: "",
      UpdatedAt: "",
    },
    ReviewList: [
      {
        Id: "10",
        Title: "",
        Content: "",
        Rating: 0,
        Account: { Id: 0, FullName: "", Email: "", ImageUrl: "" },
        DeletedAt: null,
        UpdatedAt: null,
      },
    ],
    Top: 10,
  },
];

const MockHighlyRatedPodcasters: PodcasterUI[] = [];

const MockPodcastersByCategory: PodcastersByCategory[] = [];

const PodcastersPage = () => {
  // STATES
  const [topPodcasters, setTopPodcasters] = useState<TopPodcasterUI[]>([]);

  // HOOKS
  useEffect(() => {
    setTopPodcasters(MockTopPodcasters);
  }, []);

  // FUNCTIONS

  return (
    <div className="w-full flex flex-col relative p-8">
      {/* Sticky Header với blur effect */}
      {/* <div className="sticky top-0 z-20">
        <div className="rounded-t-xl w-full h-16 backdrop-blur-md bg-white/20 flex items-center justify-between px-5 ">
          <p className="font-bold text-white text-3xl">Podcasters</p>
        </div>
       
        <div className="absolute bottom-0 left-0 right-0 h-8 bg-gradient-to-b from-transparent to-white/5 pointer-events-none" />
      </div> */}

      {/* Content */}
      {/* Top Podcaster */}
      <div className="mt-5 flex flex-col gap-2">
        <div className="w-full flex flex-col gap-8">
          <div className="w-full flex items-center justify-between">
            <p className="text-5xl font-bold text-white font-poppins">
              <span className="text-mystic-green">Top</span> Podcasters
            </p>
          </div>
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
              {topPodcasters.map((podcaster) => (
                <CarouselItem
                  key={podcaster.Id}
                  className="basis-1/1 md:basis-1/2 lg:basis-1/3"
                >
                  <div className="p-0">
                    <PodcasterCard podcaster={podcaster} />
                  </div>
                </CarouselItem>
              ))}
            </CarouselContent>
          </Carousel>
        </div>
      </div>

      {/* Highly Rated */}
      <div className="flex flex-col gap-2 mt-20">
        <div className="w-full flex flex-col gap-8">
          <div className="w-full flex items-center justify-between">
            <p className="text-3xl font-bold text-white font-poppins">
              <span className="text-mystic-green">Highly Rated</span> Podcasters
            </p>
          </div>
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
              {topPodcasters.map((podcaster) => (
                <CarouselItem
                  key={podcaster.Id}
                  className="basis-1/3 md:basis-1/3 lg:basis-1/5"
                >
                  <div className="p-1">
                    <HighlyRatedCard podcaster={podcaster} />
                  </div>
                </CarouselItem>
              ))}
            </CarouselContent>
          </Carousel>
        </div>
      </div>
    </div>
  );
};

export default PodcastersPage;
