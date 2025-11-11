"use client";
import { FaRegCompass } from "react-icons/fa";
import { FiBarChart2, FiInfo } from "react-icons/fi";
import { BiCategoryAlt } from "react-icons/bi";
import { TbMusicSearch } from "react-icons/tb";
import { IoIosNotificationsOutline, IoMdMicrophone } from "react-icons/io";
import { RiHistoryLine } from "react-icons/ri";
import { FaHeart } from "react-icons/fa";
import { RiSlideshow4Line } from "react-icons/ri";
import { CgMediaPodcast } from "react-icons/cg";
import { BsPersonCheck } from "react-icons/bs";
import { PiReceipt } from "react-icons/pi";
import { TbTransactionDollar } from "react-icons/tb";
import {
  MdNotificationsNone,
  MdNotificationsPaused,
  MdOutlinePayments,
} from "react-icons/md";
import { PiHandWithdrawBold } from "react-icons/pi";
import { MdOutlineSubscriptions } from "react-icons/md";
import { AiOutlineHome } from "react-icons/ai";
import { TbCoinFilled } from "react-icons/tb";
import { MdOutlineNavigateNext } from "react-icons/md";

import { useSelector } from "react-redux";
import type { RootState } from "@/redux/store";
import { SidebarNavItems } from "./SideBarNavItem";
import { GrCircleQuestion } from "react-icons/gr";
import { IoInformationCircleOutline } from "react-icons/io5";
import { useNavigate } from "react-router-dom";
import { useState } from "react";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
  PopoverPortal,
} from "@radix-ui/react-popover";

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
  const user = useSelector((state: RootState) => state.auth.user);
  const navigate = useNavigate();

  const [showNotificationModal, setShowNotificationModal] = useState(false);
  const [notifications, setNotifications] = useState<Array<any>>([]);

  return (
    <div
      className="
        bg-white/10 backdrop-blur-[10px] shadow-2xl
        flex flex-col items-center justify-between gap-5
        min-w-[50px] rounded-xl py-3 px-2
        sm:w-[80px]
        md:w-[290px] md:rounded-3xl md:p-5
        h-full
        md:h-[734px]
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

        <div className="z-50">
          {/* Notification Modal */}
          <Popover
            open={showNotificationModal}
            onOpenChange={setShowNotificationModal}
          >
            {/* chỉ icon mới toggle */}
            <PopoverTrigger asChild>
              {showNotificationModal ? (
                <div className="flex items-center justify-center p-2 bg-gray-300/30 text-white rounded-full cursor-pointer">
                  <MdNotificationsNone size={25} />
                </div>
              ) : (
                <div className="flex items-center justify-center p-2 hover:bg-gray-300/30 text-[#d9d9d9] hover:text-white rounded-full cursor-pointer">
                  <MdNotificationsNone size={25} />
                </div>
              )}
            </PopoverTrigger>

            <PopoverPortal>
              <PopoverContent
                side="bottom" // mở phía bên trái icon
                align="start" // căn theo giữa icon
                sideOffset={5} // cách icon 12px
                collisionPadding={2}
                className="
                  w-[300px] h-96 rounded-2xl shadow-2xl
                  bg-[#333] backdrop-blur-lg border border-white/10
                  text-white
                  flex flex-col 
                  z-[9999]
                "
              >
                <div className="p-3 w-full flex items-center">
                  <p className="font-semibold">Notifications</p>
                </div>

                {/* Danh sách thông báo */}
                {notifications.length === 0 ? (
                  <div className="flex-1 w-full flex flex-col items-center justify-center p-5">
                    <IoIosNotificationsOutline color="#d9d9d9" size={150} />
                    <p className="text-[#d9d9d9]">No notifications</p>
                    <p className="text-center text-xs text-[#d9d9d9]">
                      Any notifications will be display here...
                    </p>
                  </div>
                ) : (
                  <p>Heheheh</p>
                )}
              </PopoverContent>
            </PopoverPortal>
          </Popover>
        </div>
      </div>

      <SidebarNavItems navItems={navItems} isLoggedIn={!!user} />

      {/* User Informations */}
      {user ? (
        <div
          onClick={() => navigate("/media-player/management/profile")}
          className="w-full flex items-center justify-center md:justify-start md:gap-2 cursor-pointer hover:bg-white/20 py-2 px-2 rounded-lg"
        >
          <div className="flex items-center justify-center">
            <img
              src={user.ImageUrl || "/placeholder.svg"}
              className="md:w-10 md:h-10 sm:w-9 sm:h-9 w-8 h-8 rounded-full object-cover"
            />
          </div>
          <div className="hidden md:inline-block">
            <p className="font-bold text-white text-[15px]">{user.FullName}</p>
            <div className="flex items-center gap-1">
              <TbCoinFilled color="#aae339" size={14} />
              <p className="text-sm font-semibold text-mystic-green">
                {user.Balance.toLocaleString("vn")}
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
