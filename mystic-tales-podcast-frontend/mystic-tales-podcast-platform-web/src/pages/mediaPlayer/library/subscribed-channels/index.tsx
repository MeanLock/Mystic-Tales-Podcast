import { useGetFavoritedChannelsQuery } from "@/core/services/channel/channel.service";
import { useState } from "react";
import ChannelCard from "./components/ChannelCard";
import { useGetSubscribedContentsQuery } from "@/core/services/subscription/subscription.service";
import FireLoading from "@/components/fireLoading";

import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover";
import { Checkbox } from "@/components/ui/checkbox";
import { Label } from "@/components/ui/label";

const SubscribedChannelsPage = () => {
  // STATES
  const [favoritedSearchQuery, setFavoritedSearchQuery] = useState<string>("");
  const [subscribedSearchQuery, setSubscribedSearchQuery] =
    useState<string>("");
  const [favoritedSelectedCategories, setFavoritedSelectedCategories] =
    useState<string[]>([]);
  const [subscribedSelectedCategories, setSubscribedSelectedCategories] =
    useState<string[]>([]);

  // HOOKS
  const { data: favoritedChannels, isLoading: isFavoritedChannelsLoading } =
    useGetFavoritedChannelsQuery(undefined, {
      refetchOnMountOrArgChange: true,
      refetchOnFocus: true,
      refetchOnReconnect: true,
    });

  const { data: subscribedChannels, isLoading: isSubscribedContentsLoading } =
    useGetSubscribedContentsQuery(undefined, {
      refetchOnMountOrArgChange: true,
      refetchOnFocus: true,
      refetchOnReconnect: true,
    });

  // Filter favorited channels by search query and category
  const filteredFavoritedChannels = favoritedChannels?.ChannelList.filter(
    (channel) => {
      const matchesSearch = channel.Name.toLowerCase().includes(
        favoritedSearchQuery.toLowerCase()
      );
      const matchesCategory =
        favoritedSelectedCategories.length === 0 ||
        favoritedSelectedCategories.includes(channel.PodcastCategory.Name);
      return matchesSearch && matchesCategory;
    }
  );

  // Get unique categories from favorited channels
  const favoritedCategories = Array.from(
    new Set(favoritedChannels?.ChannelList.map((ch) => ch.PodcastCategory.Name))
  );

  // Filter subscribed channels by search query and category
  const filteredSubscribedChannels =
    subscribedChannels?.PodcastChannelList.filter((channel) => {
      const matchesSearch = channel.Name.toLowerCase().includes(
        subscribedSearchQuery.toLowerCase()
      );
      const matchesCategory =
        subscribedSelectedCategories.length === 0 ||
        subscribedSelectedCategories.includes(channel.PodcastCategory.Name);
      return matchesSearch && matchesCategory;
    });

  // Get unique categories from subscribed channels
  const subscribedCategories = Array.from(
    new Set(
      subscribedChannels?.PodcastChannelList.map(
        (ch) => ch.PodcastCategory.Name
      )
    )
  );

  // Toggle category selection for favorited
  const toggleFavoritedCategory = (category: string) => {
    setFavoritedSelectedCategories((prev) =>
      prev.includes(category)
        ? prev.filter((c) => c !== category)
        : [...prev, category]
    );
  };

  // Toggle category selection for subscribed
  const toggleSubscribedCategory = (category: string) => {
    setSubscribedSelectedCategories((prev) =>
      prev.includes(category)
        ? prev.filter((c) => c !== category)
        : [...prev, category]
    );
  };

  return (
    <div className="w-full h-full flex flex-col py-10 gap-20">
      <div className="w-full flex flex-col gap-5">
        <p className="mx-8 font-poppins text-6xl text-white font-semibold">
          <span className="text-transparent bg-clip-text bg-linear-to-r from-[#C6FFDD] via-[#FBD786] to-[#f7797d]">
            Favorited
          </span>{" "}
          Channels
        </p>

        {favoritedChannels && favoritedChannels.ChannelList.length > 0 ? (
          <div className="mx-2 px-6 flex items-center justify-start gap-20 shadow-2xl py-3">
            {/* Search Query Input */}
            <input
              type="text"
              placeholder="Search by channel name..."
              value={favoritedSearchQuery}
              onChange={(e) => setFavoritedSearchQuery(e.target.value)}
              className="px-2 py-2 bg-transparent border-b-2 border-white/20 text-white placeholder:text-[#D9D9D9] focus:outline-none focus:border-b-2 focus:border-white w-80"
            />
            {/* Category Filter Dropdown */}
            <Popover>
              <PopoverTrigger asChild>
                <div
                  className="
                px-6 py-2 
                bg-transparent border-mystic-green 
                border rounded-md 
                text-mystic-green text-sm font-semibold font-poppins
                transition-all duration-500 ease-out hover:bg-mystic-green hover:text-white hover:-translate-y-0.5 cursor-pointer
                "
                >
                  <p>Categories: {favoritedSelectedCategories.length}</p>
                </div>
              </PopoverTrigger>
              <PopoverContent
                align="start"
                sideOffset={8}
                className="w-80 bg-white/20 backdrop-blur-sm border-none"
              >
                <div className="rounded-md flex flex-col gap-4">
                  <p className="text-mystic-green font-poppins font-semibold">
                    Categories
                  </p>
                  <div className="grid grid-cols-2 w-full gap-2">
                    {favoritedCategories.map((category) => (
                      <div key={category} className="flex items-center gap-2">
                        <Checkbox
                          id={`favorited-category-${category}`}
                          checked={favoritedSelectedCategories.includes(
                            category
                          )}
                          onCheckedChange={() =>
                            toggleFavoritedCategory(category)
                          }
                          className="
                            border-mystic-green
                            data-[state=checked]:bg-[#aee339]
                            data-[state=checked]:border-[#aee339]
                            data-[state=checked]:text-black
                          "
                        />
                        <Label
                          className="text-white font-medium cursor-pointer"
                          htmlFor={`favorited-category-${category}`}
                        >
                          {category}
                        </Label>
                      </div>
                    ))}
                  </div>
                </div>
              </PopoverContent>
            </Popover>
          </div>
        ) : (
          <div className="flex items-center text-start">
            <p className="mx-8 font-poppins text-white font-light">
              You have not favorited any channels yet. Explore and favorite to
              your favorite channels to see them here!
            </p>
          </div>
        )}

        {isFavoritedChannelsLoading ? (
          <div className="mx-8 flex flex-col items-center justify-end h-48 p-5 relative">
            <FireLoading />
            <p className="font-poppins font-semibold text-[#D9D9D9]">
              Searching For Your Favorite Channels ...
            </p>
          </div>
        ) : (
          <div className="mx-8 grid grid-cols-6">
            {filteredFavoritedChannels?.map((channel) => (
              <div
                key={channel.Id}
                className="flex items-center justify-center p-5"
              >
                <ChannelCard channel={channel} />
              </div>
            ))}
          </div>
        )}
      </div>
      <div className="w-full flex flex-col gap-5">
        <p className="mx-8 font-poppins text-6xl text-white font-semibold">
          <span className="text-transparent bg-clip-text bg-linear-to-r from-[#74ebd5]  to-[#ACB6E5]">
            Subscribed
          </span>{" "}
          Channels
        </p>
        {subscribedChannels &&
        subscribedChannels.PodcastChannelList.length > 0 ? (
          <div className="mx-2 px-6 flex items-center justify-start gap-20 shadow-2xl py-3">
            {/* Search Query Input */}
            <input
              type="text"
              placeholder="Search by channel name..."
              value={subscribedSearchQuery}
              onChange={(e) => setSubscribedSearchQuery(e.target.value)}
              className="px-2 py-2 bg-transparent border-b-2 border-white/20 text-white placeholder:text-[#D9D9D9] focus:outline-none focus:border-b-2 focus:border-white w-80"
            />
            {/* Category Filter Dropdown */}
            <Popover>
              <PopoverTrigger asChild>
                <div
                  className="
                px-6 py-2 
                bg-transparent border-mystic-green 
                border rounded-md 
                text-mystic-green text-sm font-semibold font-poppins
                transition-all duration-500 ease-out hover:bg-mystic-green hover:text-white hover:-translate-y-0.5 cursor-pointer
                "
                >
                  <p>Categories: {subscribedSelectedCategories.length}</p>
                </div>
              </PopoverTrigger>
              <PopoverContent
                align="start"
                sideOffset={8}
                className="w-80 bg-white/20 backdrop-blur-sm border-none"
              >
                <div className="rounded-md flex flex-col gap-4">
                  <p className="text-mystic-green font-poppins font-semibold">
                    Categories
                  </p>
                  <div className="grid grid-cols-2 w-full gap-2">
                    {subscribedCategories.map((category) => (
                      <div key={category} className="flex items-center gap-2">
                        <Checkbox
                          id={`subscribed-category-${category}`}
                          checked={subscribedSelectedCategories.includes(
                            category
                          )}
                          onCheckedChange={() =>
                            toggleSubscribedCategory(category)
                          }
                          className="
                            border-mystic-green
                            data-[state=checked]:bg-[#aee339]
                            data-[state=checked]:border-[#aee339]
                            data-[state=checked]:text-black
                          "
                        />
                        <Label
                          className="text-white font-medium cursor-pointer"
                          htmlFor={`subscribed-category-${category}`}
                        >
                          {category}
                        </Label>
                      </div>
                    ))}
                  </div>
                </div>
              </PopoverContent>
            </Popover>
          </div>
        ) : (
          <div className="flex items-center text-start">
            <p className="mx-8 font-poppins text-white font-light">
              You have not subscribed to any channels yet. Explore and subscribe
              to your favorite channels to see them here!
            </p>
          </div>
        )}
        {isSubscribedContentsLoading ? (
          <div className="mx-8 flex flex-col items-center justify-end h-48 p-5 relative">
            <FireLoading />
            <p className="font-poppins font-semibold text-[#D9D9D9]">
              Searching For Your Subscribed Channels ...
            </p>
          </div>
        ) : (
          <div className="mx-8 w-full grid grid-cols-6">
            {filteredSubscribedChannels?.map((channel) => (
              <div
                key={channel.Id}
                className="flex items-center justify-center p-5"
              >
                <ChannelCard channel={channel} />
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
};

export default SubscribedChannelsPage;
