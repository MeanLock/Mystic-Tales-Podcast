import type React from "react";
import { useState } from "react";
import { Link, useLocation, useNavigate } from "react-router-dom";
import { MdOutlineNavigateNext } from "react-icons/md";
import { useSelector } from "react-redux";
import type { RootState } from "@/redux/store";

interface SubItem {
  icon: React.ReactNode;
  iconActive: React.ReactNode;
  iconWhenSmall: React.ReactNode;
  name: string;
  to: string;
}

interface NavItem {
  icon: React.ReactNode;
  iconActive: React.ReactNode;
  iconWhenSmall: React.ReactNode;
  name: string;
  to: string;
  isSubItemsContain: boolean;
  subItems: SubItem[];
}

interface NavSection {
  title: string;
  isLoginRequired: boolean;
  items: NavItem[];
}

interface SidebarNavItemsProps {
  navItems: NavSection[];
  isLoggedIn?: boolean;
}

export const SidebarNavItems: React.FC<SidebarNavItemsProps> = ({
  navItems,
  isLoggedIn = false,
}) => {
  const location = useLocation();
  const navigate = useNavigate();
  const [expandedItems, setExpandedItems] = useState<string[]>([]);
  const user = useSelector((state: RootState) => state.auth.user);
  const toggleExpand = (itemName: string) => {
    setExpandedItems((prev) =>
      prev.includes(itemName)
        ? prev.filter((name) => name !== itemName)
        : [...prev, itemName]
    );
  };

  const isItemActive = (itemPath: string): boolean => {
    if (!itemPath) return false;
    return location.pathname.startsWith(itemPath);
  };

  return (
    <div
      className="w-full flex-1 flex flex-col gap-2 overflow-y-auto
        [&::-webkit-scrollbar]:hidden
        [-ms-overflow-style:none]
        [scrollbar-width:none]"
    >
      {navItems.map((section) => {
        // Skip sections that require login if user is not logged in
        if (section.isLoginRequired && !isLoggedIn) {
          return null;
        }

        return (
          <div key={section.title} className="flex flex-col gap-3">
            {/* Section Title - Hidden on small screens */}
            <p className="hidden md:block text-[8px] font-semibold text-gray-400 uppercase tracking-wider px-2">
              {section.title}
            </p>

            {/* Nav Items */}
            <div className="flex flex-col gap-2">
              {section.items.map((item) => {
                const isActive = isItemActive(item.to);
                const isExpanded = expandedItems.includes(item.name);

                return (
                  <div key={item.name}>
                    {/* Main Item */}
                    <div
                      className={`
                        flex items-center justify-center md:justify-start gap-3
                        px-2 md:px-3 py-2 md:py-2.5
                        rounded-lg transition-all duration-300 ease-out
                        cursor-pointer group
                        ${
                          isActive
                            ? "bg-white/20 backdrop-blur-sm"
                            : "hover:bg-white/10"
                        }
                      `}
                      onClick={() => {
                        if (item.isSubItemsContain) {
                          toggleExpand(item.name);
                        }
                      }}
                    >
                      {/* Icon Container */}
                      <Link
                        to={item.to || "#"}
                        className="flex items-center justify-center flex-shrink-0"
                      >
                        <div className="transition-all duration-300">
                          {isActive ? item.iconActive : item.icon}
                        </div>
                      </Link>

                      {/* Text - Hidden on small screens */}
                      <Link
                        to={item.to || "#"}
                        className="hidden md:flex flex-1 items-center justify-between"
                      >
                        <span
                          className={`
                            text-sm font-medium transition-colors duration-300
                            ${
                              isActive
                                ? "text-white"
                                : "text-gray-300 group-hover:text-white"
                            }
                          `}
                        >
                          {item.name}
                        </span>
                      </Link>

                      {/* Expand Icon for Sub Items */}
                      {item.isSubItemsContain && (
                        <div className="hidden md:flex ml-auto">
                          <MdOutlineNavigateNext
                            size={18}
                            color={isExpanded ? "#aae339" : "#d9d9d9"}
                            className={`
                              transition-all duration-300 ease-out
                              ${isExpanded ? "rotate-90" : ""}
                            `}
                          />
                        </div>
                      )}
                    </div>

                    {/* Sub Items */}
                    {item.isSubItemsContain && isExpanded && (
                      <div
                        className={`
                          ml-2 md:ml-3 mt-1 flex flex-col gap-1
                          overflow-hidden
                          animate-in fade-in slide-in-from-top-2 duration-300
                        `}
                      >
                        {item.subItems.map((subItem) => {
                          const isSubActive = location.pathname.startsWith(
                            subItem.to
                          );

                          return (
                            <Link
                              key={subItem.name}
                              to={subItem.to}
                              className={`
                                flex items-center justify-center md:justify-start gap-3
                                px-2 md:px-3 py-2 md:py-2
                                rounded-lg transition-all duration-300 ease-out
                                group
                                ${
                                  isSubActive
                                    ? "bg-white/20 backdrop-blur-sm"
                                    : "hover:bg-white/10"
                                }
                              `}
                            >
                              {/* Sub Item Icon */}
                              <div className="flex items-center justify-center flex-shrink-0">
                                <div className="transition-all duration-300">
                                  {isSubActive
                                    ? subItem.iconActive
                                    : subItem.icon}
                                </div>
                              </div>

                              {/* Sub Item Text */}
                              <span
                                className={`
                                  hidden md:block text-xs font-medium transition-colors duration-300
                                  ${
                                    isSubActive
                                      ? "text-mystic-green"
                                      : "text-gray-400 group-hover:text-gray-200"
                                  }
                                `}
                              >
                                {subItem.name}
                              </span>
                            </Link>
                          );
                        })}
                      </div>
                    )}
                  </div>
                );
              })}
            </div>
          </div>
        );
      })}

      {user?.IsPodcaster ? (
        <p
          onClick={() =>
            window.open(
              `${import.meta.env.VITE_PUBLIC_PODCASTER_WEB_URL}`,
              "_blank"
            )
          }
          className="text-mystic-green hidden md:inline-block hover:underline font-poppins italic cursor-pointer"
        >
          Podcaster Studio Website
        </p>
      ) : (
        <p
          onClick={() => navigate("/become-podcaster")}
          className="text-mystic-green hidden md:inline-block hover:underline font-poppins italic cursor-pointer"
        >
          Become Podcaster
        </p>
      )}
    </div>
  );
};
