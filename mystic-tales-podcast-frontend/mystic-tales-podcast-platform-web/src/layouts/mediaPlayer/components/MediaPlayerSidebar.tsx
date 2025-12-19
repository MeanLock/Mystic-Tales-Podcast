/* eslint-disable @typescript-eslint/no-unused-vars */

import { FaHeadphones, FaRegCompass } from "react-icons/fa";
import { FiBarChart2, FiInfo } from "react-icons/fi";
import { BiCategoryAlt } from "react-icons/bi";
import { IoIosSearch, IoMdMicrophone } from "react-icons/io";
import { RiHistoryLine } from "react-icons/ri";
import { RiSlideshow4Line } from "react-icons/ri";
import { CgMediaPodcast } from "react-icons/cg";
import { BsPersonCheck } from "react-icons/bs";
import { PiReceipt } from "react-icons/pi";
import { TbTransactionDollar } from "react-icons/tb";
import {
  MdOutlineAccountBalanceWallet,
  MdOutlinePayments,
} from "react-icons/md";
import { PiHandWithdrawBold } from "react-icons/pi";
import { MdOutlineSubscriptions } from "react-icons/md";
import { AiOutlineFileDone, AiOutlineHome } from "react-icons/ai";
import { MdOutlineNavigateNext } from "react-icons/md";

import { useSelector } from "react-redux";
import type { RootState } from "@/redux/store";
import { SidebarNavItems } from "./SideBarNavItem";
import SearchSuggesstion from "./SearchSuggesstion";
import { GrCircleQuestion } from "react-icons/gr";
import { useNavigate } from "react-router-dom";
import { useEffect, useState } from "react";

import { Skeleton } from "@/components/ui/skeleton";
import type { ContentRealtimeResponse } from "@/core/types/search";
import {
  useGetAutocompleteWordRealTimeQuery,
  useGetPodcastContentOnKeywordRealTimeQuery,
} from "@/core/services/search/search.service";
import { Input } from "@/components/ui/input";
import { LucideFileAudio } from "lucide-react";
import AutoResolveImage from "@/components/fileResolving/AutoResolveImage";
import MTPCoinOutline from "@/components/coinIcons/CoinIconOutline";

const navItems = [
  {
    title: "Explore",
    isLoginRequired: false,
    items: [
      {
        icon: <FaRegCompass color="#fff" size={11} />,
        iconActive: <FaRegCompass color="#fff" size={15} />,
        iconWhenSmall: <FaRegCompass color="#333" size={11} />,
        name: "Discovery",
        to: "/media-player/discovery",
        isSubItemsContain: false,
        subItems: [],
      },
      {
        icon: <FiBarChart2 color="#fff" size={11} />,
        iconActive: <FiBarChart2 color="#fff" size={15} />,
        iconWhenSmall: <FiBarChart2 color="#333" size={11} />,
        name: "Trending",
        to: "/media-player/trending",
        isSubItemsContain: false,
        subItems: [],
      },
      {
        icon: <BiCategoryAlt color="#fff" size={11} />,
        iconActive: <BiCategoryAlt color="#fff" size={15} />,
        iconWhenSmall: <BiCategoryAlt color="#333" size={11} />,
        name: "Category",
        to: "/media-player/categories",
        isSubItemsContain: false,
        subItems: [],
      },
      {
        icon: <IoMdMicrophone color="#fff" size={11} />,
        iconActive: <IoMdMicrophone color="#fff" size={15} />,
        iconWhenSmall: <IoMdMicrophone color="#333" size={11} />,
        name: "Podcasters",
        to: "/media-player/podcasters",
        isSubItemsContain: false,
        subItems: [],
      },
    ],
  },
  {
    title: "Library",
    isLoginRequired: true,
    items: [
      {
        icon: <RiHistoryLine color="#fff" size={11} />,
        iconActive: <RiHistoryLine color="#fff" size={15} />,
        iconWhenSmall: <RiHistoryLine color="#333" size={11} />,
        name: "Listen History",
        to: "/media-player/library/listening-history",
        isSubItemsContain: false,
        subItems: [],
      },
      {
        icon: <LucideFileAudio color="#fff" size={11} />,
        iconActive: <LucideFileAudio color="#fff" size={15} />,
        iconWhenSmall: <LucideFileAudio color="#333" size={11} />,
        name: "Saved Episodes",
        to: "/media-player/library/saved",
        isSubItemsContain: false,
        subItems: [],
      },
      {
        icon: <RiSlideshow4Line color="#fff" size={11} />,
        iconActive: <RiSlideshow4Line color="#fff" size={15} />,
        iconWhenSmall: <RiSlideshow4Line color="#333" size={11} />,
        name: "Shows",
        to: "/media-player/library/subscribed-shows",
        isSubItemsContain: false,
        subItems: [],
      },
      {
        icon: <CgMediaPodcast color="#fff" size={11} />,
        iconActive: <CgMediaPodcast color="#fff" size={15} />,
        iconWhenSmall: <CgMediaPodcast color="#333" size={11} />,
        name: "Channels",
        to: "/media-player/library/subscribed-channels",
        isSubItemsContain: false,
        subItems: [],
      },
      {
        icon: <BsPersonCheck color="#fff" size={11} />,
        iconActive: <BsPersonCheck color="#fff" size={15} />,
        iconWhenSmall: <BsPersonCheck color="#333" size={11} />,
        name: "Followed Podcasters",
        to: "/media-player/library/followed-podcasters",
        isSubItemsContain: false,
        subItems: [],
      },
    ],
  },
  {
    title: "Management",
    isLoginRequired: true,
    items: [
      {
        icon: <PiReceipt color="#fff" size={11} />,
        iconActive: <PiReceipt color="#fff" size={15} />,
        iconWhenSmall: <PiReceipt color="#333" size={11} />,
        name: "Bookings",
        to: "",
        isSubItemsContain: true,
        subItems: [
          {
            icon: <PiReceipt color="#fff" size={9} />,
            iconActive: <PiReceipt color="#aae339" size={9} />,
            iconWhenSmall: <PiReceipt color="#333" size={9} />,
            name: "Bookings Management",
            to: "/media-player/management/bookings",
          },
          {
            icon: <AiOutlineFileDone color="#fff" size={9} />,
            iconActive: <AiOutlineFileDone color="#aae339" size={9} />,
            iconWhenSmall: <AiOutlineFileDone color="#333" size={9} />,
            name: "Completed Bookings",
            to: "/media-player/management/completed-bookings",
          },
        ],
      },
      {
        icon: <TbTransactionDollar color="#fff" size={11} />,
        iconActive: <TbTransactionDollar color="#fff" size={15} />,
        iconWhenSmall: <TbTransactionDollar color="#333" size={11} />,
        name: "Transactions",
        to: "",
        isSubItemsContain: true,
        subItems: [
          {
            icon: <MdOutlinePayments color="#fff" size={9} />,
            iconActive: <MdOutlinePayments color="#aae339" size={9} />,
            iconWhenSmall: <MdOutlinePayments color="#333" size={9} />,
            name: "Top Up",
            to: "/media-player/management/transactions/top-up",
          },
          {
            icon: <PiHandWithdrawBold color="#fff" size={9} />,
            iconActive: <PiHandWithdrawBold color="#aae339" size={9} />,
            iconWhenSmall: <PiHandWithdrawBold color="#333" size={9} />,
            name: "Withdraw",
            to: "/media-player/management/transactions/withdraw",
          },
          {
            icon: <MdOutlineSubscriptions color="#fff" size={9} />,
            iconActive: <MdOutlineSubscriptions color="#aae339" size={9} />,
            iconWhenSmall: <MdOutlineSubscriptions color="#333" size={9} />,
            name: "Subscriptions",
            to: "/media-player/management/transactions/subscriptions",
          },
          {
            icon: <MdOutlineAccountBalanceWallet color="#fff" size={9} />,
            iconActive: (
              <MdOutlineAccountBalanceWallet color="#aae339" size={9} />
            ),
            iconWhenSmall: (
              <MdOutlineAccountBalanceWallet color="#333" size={9} />
            ),
            name: "Account Balance",
            to: "/media-player/management/transactions/account-balance",
          },
        ],
      },
    ],
  },
  {
    title: "Menu",
    isLoginRequired: false,
    items: [
      {
        icon: <AiOutlineHome color="#fff" size={11} />,
        iconActive: <AiOutlineHome color="#fff" size={15} />,
        iconWhenSmall: <AiOutlineHome color="#333" size={11} />,
        name: "Home",
        to: "/home",
        isSubItemsContain: false,
        subItems: [],
      },
      {
        icon: <GrCircleQuestion color="#fff" size={11} />,
        iconActive: <GrCircleQuestion color="#fff" size={15} />,
        iconWhenSmall: <GrCircleQuestion color="#333" size={11} />,
        name: "FAQs",
        to: "/faqs",
        isSubItemsContain: false,
        subItems: [],
      },
      {
        icon: <FiInfo color="#fff" size={11} />,
        iconActive: <FiInfo color="#fff" size={15} />,
        iconWhenSmall: <FiInfo color="#333" size={11} />,
        name: "About Us",
        to: "/about",
        isSubItemsContain: false,
        subItems: [],
      },
    ],
  },
];

const MediaPlayerSidebar = () => {
  // STATES
  const user = useSelector((state: RootState) => state.auth.user);
  const [isResolveLoading] = useState(false);
  // Search States
  const [keyword, setKeyword] = useState("");
  const [showSearchSuggestion, setShowSearchSuggestion] = useState(false);
  const [suggesstionAutocompleteKeywords, setSuggesstionAutocompleteKeywords] =
    useState<Array<string>>([]);
  const [suggesstionContents, setSuggesstionContents] = useState<
    ContentRealtimeResponse[]
  >([]);

  // HOOKS
  const navigate = useNavigate();
  const shouldSkipQuery = keyword.trim().length === 0;
  const { data: suggestionKeywordData, isLoading: isSuggestionKeywordLoading } =
    useGetAutocompleteWordRealTimeQuery(
      { keyword },
      {
        skip: shouldSkipQuery,
      }
    );
  const { data: suggestionContentData, isLoading: isSuggestionContentLoading } =
    useGetPodcastContentOnKeywordRealTimeQuery(
      { keyword },
      {
        skip: shouldSkipQuery,
      }
    );

  useEffect(() => {
    console.log("Suggestion Keywords Data:", suggestionKeywordData);
    if (suggestionKeywordData) {
      setSuggesstionAutocompleteKeywords(suggestionKeywordData);
    } else {
      setSuggesstionAutocompleteKeywords([]);
    }
    if (suggestionContentData) {
      setSuggesstionContents(suggestionContentData.SearchItemList);
      // Push thêm mockdata vào
      setSuggesstionContents([...suggestionContentData.SearchItemList]);
    } else {
      setSuggesstionContents([]);
      // Push thêm mockdata vào
      setSuggesstionContents([]);
    }
  }, [keyword, suggestionKeywordData, suggestionContentData]);

  // FUNCTIONS

  return (
    <div
      className="
        bg-white/10 backdrop-blur-[10px] shadow-2xl
        flex flex-col items-center justify-between gap-5
        min-w-[50px] rounded-xl py-3 px-2
        sm:w-[80px]
        md:w-[290px] md:rounded-3xl md:p-5
        h-full
        md:h-full
        z-[9999]
      "
    >
      {/* Mystic Tales Logo */}
      <div
        className="
        items-center justify-center gap-2 mb-2
        hidden
        md:inline-flex
        md:justify-between
        w-full 
      "
      >
        <div className="flex items-center justify-center gap-2 mb-2">
          <div>
            <img
              src="/images/logo/logo.png"
              className="
                md:w-12 md:h-12
                sm:w-12 sm:h-12
                w-8 h-8 
                rounded-full object-cover
            "
            />
          </div>
          <div className="hidden md:inline-block">
            <p className="font-poppins font-bold text-white">Mystic Tale</p>
            <p className="text-sm italic font-poppins font-md text-white">
              Podcast
            </p>
          </div>
        </div>

        {user && (
          <div className="z-50 flex items-center justify-center bg-white/30 shadow-md py-1 pl-3 pr-1 gap-3 rounded-full">
            {/* Notification Modal */}
            <p className="font-poppins font-bold text-white">
              {user.PodcastListenSlot}
            </p>
            <div className="flex items-center justify-center flex-col rounded-full p-2 bg-white shadow-sm">
              <FaHeadphones size={15} />
            </div>
          </div>
        )}
      </div>

      {/* Search Components */}
      <div className="w-full flex flex-col">
        <p className="hidden md:block text-[12px] font-bold text-[#d9d9d9] uppercase tracking-wider px-2">
          Search
        </p>
        <div className="w-full p-2 relative">
          <div className="relative w-full">
            <Input
              type="text"
              value={keyword}
              onChange={(e) => {
                const value = e.target.value;
                setKeyword(value);

                // gõ chữ vào: nếu có keyword => mở, nếu xóa hết => đóng
                if (value.trim().length > 0) {
                  setShowSearchSuggestion(true);
                } else {
                  setShowSearchSuggestion(false);
                }
              }}
              onFocus={() => {
                // chỉ mở khi có keyword
                if (keyword.trim().length > 0) {
                  setShowSearchSuggestion(true);
                }
              }}
              onBlur={() => {
                // Chỉ đóng nếu click ra ngoài, không phải vào suggestion
                setTimeout(() => setShowSearchSuggestion(false), 200);
              }}
              onKeyDown={(e) => {
                if (e.key === "Enter" && keyword.trim().length > 0) {
                  navigate(
                    `/media-player/search?keyword=${encodeURIComponent(
                      keyword
                    )}&refresh=${Date.now()}`
                  );

                  setShowSearchSuggestion(false);
                }
              }}
              placeholder="Find Your Contents Here..."
              className="
                w-full
                pl-0
                bg-transparent
                border-0
                border-b
                rounded-none
                border-white/30
                focus-visible:ring-0
                focus-visible:border-white
                focus-visible:border-b-2
                text-white
                placeholder:text-white/30
                pr-12
              "
            />

            <IoIosSearch
              size={20}
              onClick={() => {
                if (keyword.trim()) {
                  navigate(
                    `/media-player/search?keyword=${encodeURIComponent(
                      keyword
                    )}&refresh=${Date.now()}`
                  );

                  setShowSearchSuggestion(false);
                }
              }}
              className="
                absolute right-3 top-1/2 -translate-y-1/2
                cursor-pointer
                text-white/40
                hover:text-white
                transition
              "
            />
          </div>

          {/* Custom Dropdown - không dùng Shadcn Popover */}
          {showSearchSuggestion && keyword.trim().length > 0 && (
            <div
              className="absolute w-[300px] top-full left-0 mt-2 z-[99999]"
              onMouseDown={(e) => {
                // Ngăn blur event khi click vào suggestion
                e.preventDefault();
              }}
            >
              <SearchSuggesstion
                keywordOriginal={keyword}
                keywords={suggesstionAutocompleteKeywords}
                contents={suggesstionContents}
                isKeywordLoading={isSuggestionKeywordLoading}
                isContentLoading={isSuggestionContentLoading}
                onKeywordClick={(kw) => {
                  setKeyword(kw);
                  navigate(
                    `/media-player/search?keyword=${encodeURIComponent(kw)}`
                  );
                  setShowSearchSuggestion(false);
                }}
                onContentClick={(content) => {
                  if (content.Show) {
                    navigate(`/media-player/shows/${content.Show.Id}`);
                  } else if (content.Episode) {
                    navigate(
                      `/media-player/episodes/${content.Episode.Id}`
                    );
                  }
                  setShowSearchSuggestion(false);
                }}
              />
            </div>
          )}
        </div>
      </div>

      {/* Navigation Items */}
      <SidebarNavItems navItems={navItems} isLoggedIn={!!user} />

      {/* User Informations */}
      {user ? (
        <div
          onClick={() => navigate("/media-player/management/profile")}
          className="w-full flex items-center justify-center md:justify-start md:gap-2 cursor-pointer hover:bg-white/20 py-2 px-2 rounded-lg"
        >
          <div className="flex items-center justify-center">
            {isResolveLoading ? (
              <Skeleton className="md:w-8 md:h-8 sm:w-8 sm:h-8 w-8 h-8 rounded-full" />
            ) : (
              <AutoResolveImage
                key={`${user?.MainImageFileKey}-${user?.Id}`}
                FileKey={user?.MainImageFileKey}
                Name={user.FullName}
                className="md:w-10 md:h-10 sm:w-8 sm:h-8 w-8 h-8 rounded-full aspect-square object-cover"
                imgClassName="rounded-full md:w-10 md:h-10 sm:w-8 sm:h-8 w-8 h-8"
                type="AccountPublicSource"
              />
            )}
          </div>
          <div className="hidden md:inline-block">
            <p className="font-bold text-white text-[12px] line-clamp-1 max-w-[120px]">
              {user.FullName}
            </p>
            <div className="flex items-center gap-1 max-w-37.5">
              <MTPCoinOutline size={14} color="#aee339" />
              <p className="text-sm font-semibold text-mystic-green line-clamp-1">
                {user.Balance.toLocaleString("vn")}
              </p>
            </div>
          </div>
          <div className="hidden md:inline-flex flex-1 items-center justify-end">
            <MdOutlineNavigateNext size={30} color="#d9d9d9" />
          </div>
        </div>
      ) : (
        <div className="w-full flex items-center justify-center">
          <div
            className="w-full flex items-center justify-center rounded-md bg-mystic-green py-2 px-4 cursor-pointer hover:brightness-90 transition"
            onClick={() => navigate("/auth/login")}
          >
            <p className="font-poppins text-black font-bold">Login</p>
          </div>
        </div>
      )}
    </div>
  );
};

export default MediaPlayerSidebar;
