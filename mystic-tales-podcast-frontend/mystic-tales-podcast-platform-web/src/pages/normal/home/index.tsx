// @ts-nocheck

import BannerCard from "./components/BannerCard";

import { MdOutlineTune } from "react-icons/md";
import { IoPauseCircle } from "react-icons/io5";
import { MdAutoAwesome } from "react-icons/md";
import { FaFacebook } from "react-icons/fa";
import { FaInstagram } from "react-icons/fa";
import { FaSquareXTwitter } from "react-icons/fa6";
import { FaYoutube } from "react-icons/fa6";

import PodcasterCard from "./components/PodcasterCard";
import TopShowCard from "./components/TrendingShowCard";
import { useNavigate } from "react-router-dom";

const categoryBannerData = [
  {
    title: "Folk Horror Stories",
    description:
      "Step into the chilling world of ancient village legends, cursed rituals, and restless spirits",
    categoryId: "1",
    imageUrl: "/images/home/categories/1.jpg",
  },
  {
    title: "Cultivating Immortality",
    description:
      "Enter the realm of cultivators, mystical sects, and timeless quests for immortality.",
    categoryId: "2",
    imageUrl: "/images/home/categories/2.png",
  },
  {
    title: "Urban Legends",
    description:
      "Uncover the dark secrets hidden in city streets, abandoned buildings, and late-night whispers.",
    categoryId: "3",
    imageUrl: "/images/home/categories/3.jpg",
  },
  {
    title: "Unsolved Mysteries",
    description:
      "Explore the enigmatic cases and puzzling events that history has yet to explain.",
    categoryId: "4",
    imageUrl: "/images/home/categories/4.png",
  },
];

const podcasterData = [
  {
    Id: "123",
    FullName: "Julia Elizabeth",
    ImageUrl:
      "https://i.pinimg.com/736x/35/f2/d3/35f2d38f31a49c125c0e977bb12a10d8.jpg",
    Description:
      "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt explicabo.",
  },
  {
    Id: "456",
    FullName: "Cameron",
    ImageUrl:
      "https://i.pinimg.com/736x/5b/cf/fc/5bcffc5de7d4898d4a44536172d80594.jpg",
    Description:
      "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt explicabo.",
  },
  {
    Id: "789",
    FullName: "Angela Breakly",
    ImageUrl:
      "https://i.pinimg.com/736x/83/d6/56/83d65678c01c214be75d320a9208c45e.jpg",
    Description:
      "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt explicabo.",
  },
  {
    Id: "790",
    FullName: "Angela Breakly",
    ImageUrl:
      "https://i.pinimg.com/736x/83/d6/56/83d65678c01c214be75d320a9208c45e.jpg",
    Description:
      "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt explicabo.",
  },
];

const topShowData = [
  {
    Id: "1",
    Name: "Vạn Quỷ Nhân Gian",
    Podcaster: {
      Id: "123",
      FullName: "Nhân Gian Ký",
    },
    TotalListenCount: 12140200,
    ImageUrl:
      "https://i.pinimg.com/1200x/69/7d/58/697d58d8d3ca8565ecde87fe14f28c5c.jpg",
  },
  {
    Id: "2",
    Name: "Bùa Ngải 101",
    Podcaster: {
      Id: "123",
      FullName: "Chú Ba Duy",
    },
    TotalListenCount: 10500214,
    ImageUrl:
      "https://i.pinimg.com/1200x/03/3a/90/033a90ca6443c7d9bb8fef90ebcf2dda.jpg",
  },
  {
    Id: "3",
    Name: "Sinh Vật Thần Thoại",
    Podcaster: {
      Id: "123",
      FullName: "SAMURICE",
    },
    TotalListenCount: 9220121,
    ImageUrl:
      "https://i.pinimg.com/736x/35/5f/d6/355fd6f9c099ebaa2539fdcf54938770.jpg",
  },
  {
    Id: "4",
    Name: "The Dark Game",
    Podcaster: {
      Id: "123",
      FullName: "Amarando",
    },
    TotalListenCount: 750123,
    ImageUrl:
      "https://i.pinimg.com/1200x/bb/46/39/bb463941e9dd78b5f7acf6edc00e322a.jpg",
  },
  {
    Id: "5",
    Name: "Vũ Động Càn Khôn",
    Podcaster: {
      Id: "123",
      FullName: "Zhang Li",
    },
    TotalListenCount: 340000,
    ImageUrl:
      "https://i.pinimg.com/736x/e1/16/4a/e1164acd25d20448337f381e93de39a6.jpg",
  },
];

const HomePage = () => {
  const navigate = useNavigate();

  return (
    <div className="w-full flex flex-col items-center">
      {/* Banner */}
      <div className="w-10/12 flex flex-col items-center py-5 gap-10">
        <p className="text-center font-bold text-white md:text-7xl sm:text-4xl xs:2xl">
          STEP INTO <span className="text-[#aae339]">THE REALM OF GHOSTS</span>{" "}
          AND <span className="text-[#aae339]">IMMORTALS</span>{" "}
        </p>

        <div className="bg-[#aae339] hover:bg-[#86b42b] px-20 py-2 flex items-center justify-center rounded-4xl cursor-pointer">
          <p className="text-black text-xl font-bold m-0 p-0">Explore Now</p>
        </div>
      </div>

      {/* Category Banners */}
      <div className="w-11/12 mt-10 flex items-center justify-between">
        {categoryBannerData.map((category) => (
          <BannerCard
            key={category.categoryId}
            title={category.title}
            description={category.description}
            categoryId={category.categoryId}
            imageUrl={category.imageUrl}
          />
        ))}
      </div>

      {/* What Makes Us Standout */}
      <div className="mt-20  w-full flex flex-col items-center gap-10">
        <p className="text-white font-bold text-5xl">What Makes Us Stand Out</p>

        <div className="w-11/12 flex items-center justify-between">
          {/* 1. MYSTERIOUS CONTENTS */}
          <div className="flex flex-col gap-4 w-[700px] h-[500px] bg-black/20 backdrop-blur-sm border border-white/50 rounded-xl shadow-[inset_0_1px_0px_rgba(255,255,255,0.75),0_0_9px_rgba(0,0,0,0.2),0_3px_8px_rgba(0,0,0,0.15)] p-4 text-white relative before:absolute before:inset-0 before:rounded-lg before:bg-gradient-to-br before:from-white/60 before:via-transparent before:to-transparent before:opacity-70 before:pointer-events-none after:absolute after:inset-0 after:rounded-lg after:bg-gradient-to-tl after:from-white/30 after:via-transparent after:to-transparent after:opacity-50 after:pointer-events-none">
            <p className="text-[#D1D5DB]">MYSTERIOUS CONTENTS</p>
            <p>
              Every story is more than just words — it’s a{" "}
              <span className="font-bold text-[#aae339]">
                gateway into the unknown.
              </span>
            </p>
            <p>
              We curate tales that unsettle, intrigue, and ignite your
              curiosity, bringing you into realms where the line between myth
              and reality fades.
            </p>
            <p>
              With haunting visuals and soundscapes, we deliver a podcast
              experience that feels as if you’re walking through the shadows
              yourself.
            </p>

            <div className="w-full flex items-center justify-between">
              <img
                src="https://i.pinimg.com/474x/a4/b4/60/a4b4607f94868632e68c966d9c7c86a1.jpg"
                alt="Ghost Icon"
                className="z-30 w-[326px] h-[260px] rounded-xl bg-cover"
              />
              <img
                src="https://i.pinimg.com/1200x/18/db/35/18db358dc162bd67c7ba04f2355f76f8.jpg"
                alt="Ghost Icon"
                className="z-30 w-[326px] h-[260px] rounded-xl bg-cover"
              />
            </div>
          </div>

          {/* 2. PREMIUM LISTENING EXPERIENCE */}
          <div className="flex flex-col gap-4 w-[700px] h-[500px] bg-black/20 backdrop-blur-sm border border-white/50 rounded-xl shadow-[inset_0_1px_0px_rgba(255,255,255,0.75),0_0_9px_rgba(0,0,0,0.2),0_3px_8px_rgba(0,0,0,0.15)] p-4 text-white relative before:absolute before:inset-0 before:rounded-lg before:bg-gradient-to-br before:from-white/60 before:via-transparent before:to-transparent before:opacity-70 before:pointer-events-none after:absolute after:inset-0 after:rounded-lg after:bg-gradient-to-tl after:from-white/30 after:via-transparent after:to-transparent after:opacity-50 after:pointer-events-none">
            <p>PREMIUM LISTENING EXPERIENCE</p>
            <p>
              Your listening experience matters. That’s why we’ve built an{" "}
              <span className="font-bold text-[#aae339]">
                advanced audio system
              </span>
              :
            </p>

            <div className="flex items-center gap-3">
              <MdOutlineTune color="#aae339" size={20} />
              <p className=" text-white">
                Custom EQ tuning for crystal-clear narration and spine-tingling
                effects.
              </p>
            </div>

            <div className="flex items-center gap-3">
              <IoPauseCircle color="#aae339" size={20} />
              <p className=" text-white">
                Auto-pause audio for you to listen before sleep.
              </p>
            </div>

            <div className="flex items-center gap-3">
              <MdAutoAwesome color="#aae339" size={20} />
              <p className=" text-white">
                Intelligent content discovery that learns your taste.
              </p>
            </div>

            <div className="w-full flex items-center justify-between">
              <img
                src="https://i.pinimg.com/1200x/0b/6e/25/0b6e2571821e275c68a7e544c0e3b78a.jpg"
                alt="Ghost Icon"
                className="w-[326px] h-[260px] rounded-xl bg-cover"
              />
              <img
                src="https://i.pinimg.com/1200x/37/e2/2e/37e22ebe079a6b440358a793c96c0981.jpg"
                alt="Ghost Icon"
                className="w-[326px] h-[260px] rounded-xl bg-cover"
              />
            </div>
          </div>
        </div>
      </div>

      {/* Top Podcasters */}
      <div className="mt-20 w-full flex flex-col items-center gap-10">
        <div className="w-11/12 flex items-center justify-between">
          <div className="w-1/3">
            <p className="text-5xl font-bold text-white">
              Meet our <span className="text-[#aae339]">top-tier</span>{" "}
              Podcasters
            </p>
          </div>
          <div className="bg-[#aae339] hover:bg-[#86b42b] px-10 py-2 flex items-center justify-center rounded-4xl cursor-pointer">
            <p className="text-black text-xl font-bold m-0 p-0">More</p>
          </div>
        </div>

        <div className="w-11/12 py-5 flex items-center justify-around">
          {podcasterData.map((podcaster) => (
            <PodcasterCard key={podcaster.Id} podcaster={podcaster} />
          ))}
        </div>
      </div>

      {/* Top Shows */}
      <div className="mt-20 w-full flex flex-col items-center gap-10 pb-20">
        <div className="w-11/12 flex items-center justify-between">
          <div className="w-1/3">
            <p className="text-5xl font-bold text-white">
              <span className="text-[#aae339]">Top</span> Shows
            </p>
          </div>
          <div className="bg-[#aae339] hover:bg-[#86b42b] px-10 py-2 flex items-center justify-center rounded-4xl cursor-pointer">
            <p className="text-black text-xl font-bold m-0 p-0">More</p>
          </div>
        </div>

        <div className="w-11/12 py-5 flex items-center justify-between">
          {topShowData.map((show) => (
            <TopShowCard key={show.Id} show={show} />
          ))}
        </div>
      </div>

      {/* Advertising */}
      {/* Ad 1 */}
      <div className="w-11/12 h-[790px] relative flex items-center justify-center rounded-2xl bg-neutral-300/20 border border-neutral-400/20 backdrop-blur-md p-2">
        <div className="w-full h-full flex md:justify-end sm:justify-center items-end">
          <img
            src="/images/home/advertising/1.png"
            className="h-11/12 object-cover bg-cover"
          />
        </div>

        {/* Overlay */}
        <div className="absolute inset-0 z-10 bg-black/20 rounded-2xl"></div>

        {/* Content */}
        <div className="absolute inset-0 z-20 flex flex-col md:items-start sm:items-center justify-center gap-5 md:w-2/3 sm:w-full px-10 ">
          <p className="text-[#aae339] md:text-[20px] sm:text-[15px] font-bold md:text-left sm:text-center">
            ENJOY YOUR OWN PODCAST
          </p>
          <p className="text-white md:text-[40px] sm:text-[30px] font-bold md:text-left sm:text-center">
            BOOKING YOUR FAVORITE PODCASTER WITH CUSTOM CONTENT
          </p>
          <p className="text-white md:text-[15px] sm:text-[12px] md:text-left sm:text-center">
            Connect with your favorite podcaster today and order custom-made
            podcasts! Tailor the content your way, enjoy total privacy, and
            experience your favorite stories in the voice you love!
          </p>
        </div>
      </div>

      {/* Ad 2 */}
      <div className="mt-15 w-11/12 h-[790px] relative flex items-center justify-center rounded-2xl bg-neutral-300/20 border border-neutral-400/20 backdrop-blur-md p-2 mb-20">
        <div className="w-full h-full flex md:justify-start sm:justify-center items-center md:p-10 sm:p-0">
          <img
            src="/images/home/advertising/2.png"
            className="h-11/12 object-cover bg-cover"
          />
        </div>

        {/* Overlay */}
        <div className="absolute inset-0 z-10 bg-black/20 rounded-2xl"></div>

        {/* Content */}
        <div className="absolute top-0 bottom-0 left-1/3 z-20 flex flex-col items-start justify-center gap-5 w-2/3 px-10 ">
          <p className="text-[#aae339] text-[20px] font-bold">
            CONTENT THAT FINDS YOU
          </p>
          <p className="text-white text-[40px] font-bold">
            “SIT BACK, RELAX, AND LET FRESH, PERSONALIZED CONTENT COME KNOCKING
            AT YOUR DOOR!
          </p>
          <p className="text-white">
            No need to search — our smart AI brings the right podcasts straight
            to you, perfectly matched to your taste
          </p>
        </div>
      </div>

      {/* Footer */}
      <div className="w-full px-10 py-12 border-t-[1px] border-b-[1px] border-[#D9D9D9] grid grid-cols-2">
        <div className="w-full flex flex-col justify-center gap-[20px]">
          <div className="flex items-center gap-2">
            <div className="flex items-center justify-center ">
              <img
                src="/images/logo/logo.png"
                alt="Logo"
                className="w-[50px] h-[50px]"
              />
            </div>
            <div className="flex flex-col items-start">
              <p className="font-bold text-white text-lg">Mystic Tale</p>
              <p className="italic font-light text-gray-300 text-md">Podcast</p>
            </div>
          </div>

          <div className="w-8/12">
            <p className="text-white text-[16px]">
              Dive into mysterious, bizarre, and captivating content like never
              before. Mystic Podcast Tales — where untold stories come to life
            </p>
          </div>
        </div>

        <div className="w-full flex items-center gap-60">
          <div className="flex flex-col items-start gap-3">
            <p className="font-bold text-[#d9d9d9]">Navigation</p>
            <p
              onClick={() => navigate("/home")}
              className="text-white font-light hover:underline cursor-pointer"
            >
              Home
            </p>
            <p
              onClick={() => navigate("/media-player/discovery")}
              className="text-white font-light hover:underline cursor-pointer"
            >
              Explore
            </p>
            <p
              onClick={() => navigate("/faqs")}
              className="text-white font-light hover:underline cursor-pointer"
            >
              FAQs
            </p>
            <p
              onClick={() => navigate("/about")}
              className="text-white font-light hover:underline cursor-pointer"
            >
              About
            </p>
          </div>

          <div className="flex flex-col items-start gap-3">
            <p className="font-bold text-[#d9d9d9]">Social</p>

            <div
              className="flex items-center gap-2"
              onClick={() =>
                (window.location.href =
                  "https://www.facebook.com/mikely.soryzz")
              }
            >
              <FaFacebook color="white" size={20} />
              <p className="text-white font-light hover:underline cursor-pointer">
                Facebook
              </p>
            </div>
            <div
              className="flex items-center gap-2"
              onClick={() =>
                (window.location.href = "https://www.instagram.com/cbum/")
              }
            >
              <FaInstagram color="white" size={20} />
              <p className="text-white font-light hover:underline cursor-pointer">
                Instagram
              </p>
            </div>
            <div
              className="flex items-center gap-2"
              onClick={() => (window.location.href = "#")}
            >
              <FaSquareXTwitter color="white" size={20} />
              <p className="text-white font-light hover:underline cursor-pointer">
                X (Twitter)
              </p>
            </div>
            <div
              className="flex items-center gap-2"
              onClick={() => (window.location.href = "#")}
            >
              <FaYoutube color="white" size={20} />
              <p className="text-white font-light hover:underline cursor-pointer">
                Youtube
              </p>
            </div>
          </div>
        </div>
      </div>

      <div className="w-full px-7 py-8 flex items-center justify-between">
        <p className="font-light text-[13px] text-[#d9d9d9]">
          © 2025 - Mystic Tale Podcast
        </p>
        <p className="font-light text-[13px] text-[#d9d9d9]">
          mtpodcast@gmail.com - Hotline: +84 123 456 7213
        </p>
      </div>
    </div>
  );
};

export default HomePage;
