"use client";
import { FaRegCompass } from "react-icons/fa";
import { FiBarChart2 } from "react-icons/fi";
import { BiCategoryAlt } from "react-icons/bi";
import { TbMusicSearch } from "react-icons/tb";
import { IoMdMicrophone } from "react-icons/io";
import { RiHistoryLine } from "react-icons/ri";
import { FaHeart } from "react-icons/fa";
import { RiSlideshow4Line } from "react-icons/ri";
import { CgMediaPodcast } from "react-icons/cg";
import { BsPersonCheck } from "react-icons/bs";
import { PiReceipt } from "react-icons/pi";
import { TbTransactionDollar } from "react-icons/tb";
import { MdOutlinePayments } from "react-icons/md";
import { PiHandWithdrawBold } from "react-icons/pi";
import { MdOutlineSubscriptions } from "react-icons/md";
import { AiOutlineHome } from "react-icons/ai";
import { TbCoinFilled } from "react-icons/tb";
import { MdOutlineNavigateNext } from "react-icons/md";

import { useSelector } from "react-redux";
import type { RootState } from "@/redux/store";
import { SidebarNavItems } from "./SideBarNavItem";

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
        to: "/media-player/category",
        isSubItemsContain: false,
        subItems: [],
      },
      {
        icon: <TbMusicSearch color="#fff" size={11} />,
        iconActive: <TbMusicSearch color="#fff" size={15} />,
        iconWhenSmall: <TbMusicSearch color="#333" size={11} />,
        name: "Search",
        to: "/media-player/search",
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
        name: "Recent",
        to: "/media-player/library/recent",
        isSubItemsContain: false,
        subItems: [],
      },
      {
        icon: <FaHeart color="#fff" size={11} />,
        iconActive: <FaHeart color="#fff" size={15} />,
        iconWhenSmall: <FaHeart color="#333" size={11} />,
        name: "Favorites",
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
        to: "/media-player/management/bookings",
        isSubItemsContain: false,
        subItems: [],
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
    ],
  },
];

const MediaPlayerSidebar = () => {
  const user = useSelector((state: RootState) => state.auth.user);

  return (
    <div
      className="
        bg-white/10 backdrop-blur-[5px] shadow-2xl
        flex flex-col items-center justify-between gap-5
        min-w-[50px] rounded-md py-3 px-2
        sm:w-[80px]
        md:w-[290px] md:rounded-xl md:p-5
        h-full
        md:h-[734px]
        overflow-y-auto
        [&::-webkit-scrollbar]:hidden
        [-ms-overflow-style:none]
        [scrollbar-width:none]
      "
    >
      {/* Mystic Tales Logo */}
      <div
        className="
        flex items-center justify-center gap-2 mb-2
        md:justify-start
        w-full 
      "
      >
        <div className="">
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

      <SidebarNavItems navItems={navItems} isLoggedIn={!!user} />

      {/* User Informations */}
      {user ? (
        <div className="w-full flex items-center justify-center md:justify-start md:gap-2 cursor-pointer">
          <div className="flex items-center justify-center">
            <img
              src={user.ImageUrl || "/placeholder.svg"}
              className="md:w-10 md:h-10 sm:w-9 sm:h-9 w-8 h-8 rounded-full object-cover"
            />
          </div>
          <div className="hidden md:inline-block">
            <p className="font-bold text-white text-[15px]">{user.FullName}</p>
            <div className="flex items-center gap-1">
              <TbCoinFilled color="#aae339" size={18} />
              <p className="text-sm font-bold text-mystic-green">
                {user.Balance.toLocaleString("vn")} đ
              </p>
            </div>
          </div>
          <div className="hidden md:inline-flex flex-1 items-center justify-end">
            <MdOutlineNavigateNext size={30} color="#d9d9d9" />
          </div>
        </div>
      ) : (
        <div></div>
      )}
    </div>
  );
};

export default MediaPlayerSidebar;
