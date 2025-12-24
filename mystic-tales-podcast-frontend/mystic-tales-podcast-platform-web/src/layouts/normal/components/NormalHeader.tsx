import type { RootState } from "@/redux/store";
import { useSelector } from "react-redux";
import { useNavigate } from "react-router-dom";
import { RiMoneyDollarCircleFill } from "react-icons/ri";

import AutoResolveImage from "@/components/fileResolving/AutoResolveImage";
const navigationLinks = [
  { name: "Home", href: "/home" },
  { name: "FAQs", href: "/faqs" },
  { name: "About Us", href: "/about" },
  { name: "Discovery", href: "/media-player/discovery" },
];

const CustomLinkItem = ({
  name,
  current,
}: {
  name: string;
  current: boolean;
}) => {
  return (
    <p
      className={` transition-colors hover:underline cursor-pointer ${
        current ? "text-white font-semibold" : "text-[#D1D5DB] font-medium"
      }`}
    >
      {name}
    </p>
  );
};

const NormalHeader = () => {
  // STATES
  const user = useSelector((state: RootState) => state.auth.user);

  // HOOKS
  const navigate = useNavigate();

  const handleNavigate = (to: string) => {
    navigate(to);
  };

  const isCurrent = (href: string) => {
    if (href === "/home") {
      return (
        window.location.pathname === "/" || window.location.pathname === "/home"
      );
    }
    return window.location.pathname === href;
  };

  return (
    <div
      className="
      rounded-xl 
      py-2 
      px-4 
      flex 
      items-center 
      md:w-[1110px] 
      bg-black/20 
      backdrop-blur-sm 
      border 
      border-white/50 
      shadow-[inset_0_1px_0px_rgba(255,255,255,0.75),0_0_9px_rgba(0,0,0,0.2),0_3px_8px_rgba(0,0,0,0.15)] 
      relative 
      before:absolute 
      before:inset-0 
      before:rounded-lg 
      before:bg-gradient-to-br 
      before:from-white/60 
      before:via-transparent 
      before:to-transparent 
      before:opacity-70 
      before:pointer-events-none 
      after:absolute 
      after:inset-0 
      after:rounded-lg 
      after:bg-gradient-to-tl 
      after:from-white/30 
      after:via-transparent 
      after:to-transparent 
      after:opacity-50 
      after:pointer-events-none"
    >
      {/* Logo and Name */}
      <div className="items-center gap-2 md:inline-flex hidden">
        <div className="flex items-center justify-center ">
          <img
            src="/images/logo/logo.png"
            alt="Logo"
            className="w-[35px] h-[35px]"
          />
        </div>
        <div className="flex flex-col items-start">
          <p className="font-bold text-white text-md">Mystic Tale</p>
          <p className="italic font-light text-gray-300 text-sm">Podcast</p>
        </div>
      </div>

      {/* Navigation Links */}
      <div className="flex-1 flex justify-center gap-6">
        {navigationLinks.map((link) => (
          <div onClick={() => handleNavigate(link.href)}>
            <CustomLinkItem
              key={link.name}
              name={link.name}
              current={isCurrent(link.href)}
            />
          </div>
        ))}
      </div>
      {/* Login Button or Profile Picture */}
      <div className="flex items-center justify-center">
        {user ? (
          <div
            onClick={() => navigate(`/media-player/management/profile`)}
            className="flex items-center gap-5 cursor-pointer"
          >
            <div>
              <AutoResolveImage
                FileKey={user.MainImageFileKey}
                className="w-10 h-10 shadow-2xl rounded-full object-cover"
                imgClassName="w-10 h-10 rounded-full"
                type="AccountPublicSource"
              />
            </div>
            <div className="flex flex-col items-start justify-center">
              <p className="font-bold text-white">{user.FullName}</p>
              <div className="flex items-center gap-1">
                <RiMoneyDollarCircleFill size={20} color="#aae339" />
                <p className="text-mystic-green font-bold text-xs">
                  {user.Balance.toLocaleString("vn")} đ
                </p>
              </div>
            </div>
          </div>
        ) : (
          <div
            onClick={() => handleNavigate("/auth/login")}
            className="md:px-20 sm:px-10 py-2 bg-[#aae339] hover:bg-[#86b42b] rounded-md text-black font-bold md:inline-flex hidden cursor-pointer"
          >
            <p>Login</p>
          </div>
        )}
      </div>
    </div>
  );
};
export default NormalHeader;

/* 
bg-black/20 
backdrop-blur-sm 
border 
border-white/50 
shadow-[inset_0_1px_0px_rgba(255,255,255,0.75),0_0_9px_rgba(0,0,0,0.2),0_3px_8px_rgba(0,0,0,0.15)] 
relative 
before:absolute 
before:inset-0 
before:rounded-lg 
before:bg-gradient-to-br 
before:from-white/60 
before:via-transparent 
before:to-transparent 
before:opacity-70 
before:pointer-events-none 
after:absolute 
after:inset-0 
after:rounded-lg 
after:bg-gradient-to-tl 
after:from-white/30 
after:via-transparent 
after:to-transparent 
after:opacity-50 
after:pointer-events-none
*/
