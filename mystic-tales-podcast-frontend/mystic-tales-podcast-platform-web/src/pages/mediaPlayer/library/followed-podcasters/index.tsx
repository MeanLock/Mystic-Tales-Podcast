import { useGetFollowedPodcastersQuery } from "@/core/services/podcasters/podcasters.service";
import type { PodcasterUI } from "@/core/types/podcaster";
import { useEffect, useState } from "react";
import {
  resolveFiles,
  type FileResolveConfig,
} from "@/core/utils/fileResolver.util";
import Loading from "@/components/loading";
import "./styles.css";
import PodcasterCard from "./components/PodcasterCard";
import { Input } from "@/components/ui/input";
import { Search } from "lucide-react";
import { useNavigate } from "react-router-dom";

const PodcasterFileConfig: FileResolveConfig[] = [
  {
    path: "MainImageFileKey",
    output: "ImageUrl",
    type: "AccountPublic",
  },
];

const FollowedPodcastersPage = () => {
  // STATES
  const [podcasters, setPodcasters] = useState<PodcasterUI[] | null>(null);
  const [isFileResolving, setIsFileResolving] = useState(false);
  const [searchQuery, setSearchQuery] = useState("");

  // HOOKS
  const navigate = useNavigate();

  const {
    data: followedPodcastersRaw,
    isLoading: isFollowedPodcastersLoading,
  } = useGetFollowedPodcastersQuery();

  useEffect(() => {
    const resolveData = async () => {
      // Đợi API loading xong
      if (isFollowedPodcastersLoading) {
        return;
      }

      setIsFileResolving(true);

      // Resolve Followed Podcasters
      if (followedPodcastersRaw && followedPodcastersRaw.PodcasterList) {
        const resolvedPromises = followedPodcastersRaw.PodcasterList.map(
          (podcaster) => resolveFiles(podcaster, PodcasterFileConfig)
        );
        const resolvedResults = await Promise.all(resolvedPromises);
        const resolved = resolvedResults.map((r) => r.resolvedData);
        setPodcasters(resolved as unknown as PodcasterUI[]);
      } else {
        setPodcasters([]);
      }

      setIsFileResolving(false);

      // Restore search query from localStorage
      const savedQuery = localStorage.getItem(
        "followed-podcaster-search-query"
      );
      if (savedQuery) {
        setSearchQuery(savedQuery);
      }
    };
    resolveData();
  }, [followedPodcastersRaw, isFollowedPodcastersLoading]);

  // FUNCTIONS
  // Filter podcasters based on search query
  const filteredPodcasters = podcasters?.filter((podcaster) =>
    podcaster.Name.toLowerCase().includes(searchQuery.toLowerCase())
  );

  const handleViewDetails = (podcasterId: number) => {
    localStorage.setItem("followed-podcaster-search-query", searchQuery);
    navigate(`/media-player/podcasters/${podcasterId}`);
  };

  // Loading State
  if (isFollowedPodcastersLoading || isFileResolving) {
    return (
      <div className="w-full h-full flex items-center flex-col justify-center gap-5">
        <Loading />
        <p className="font-poppins text-[#D9D9D9] font-bold">
          Loading Your Followed Podcasters...
        </p>
      </div>
    );
  }

  // Null State
  if (podcasters === null) {
    return (
      <div className="w-full h-full flex items-center flex-col justify-center gap-5">
        <Loading />
        <p className="font-poppins text-[#D9D9D9] font-bold">
          Preparing Your Library...
        </p>
      </div>
    );
  }

  // Empty State
  if (podcasters.length === 0) {
    return (
      <div className="w-full h-full flex items-center flex-col justify-center gap-5">
        <div className="meteor"></div>
        <p className="font-poppins text-white text-3xl font-bold">
          No Followed Podcasters Yet
        </p>
        <p className="font-poppins text-[#D9D9D9]">
          Start following podcasters to see them here!
        </p>
      </div>
    );
  }

  return (
    <div className="w-full flex flex-col relative p-8">
      <div className="w-full flex flex-col items-start justify-center mb-10 gap-2">
        <p className="text-9xl pb-4 font-poppins font-bold text-transparent bg-clip-text bg-gradient-to-r from-[#FFFFFf] via-[#6DD5FA] to-[#2980B9]">
          Followed Podcasters
        </p>
        <p className="font-poppins text-white font-bold">
          Your favorite content creators in one place.
        </p>
        <p className="w-2/3 font-poppins text-[#d9d9d9]">
          Keep up with the podcasters you love and never miss their latest
          content.
        </p>
      </div>

      {/* Search Bar */}
      <div className="w-full md:w-1/2 lg:w-1/3 mb-8">
        <div className="relative">
          <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 h-5 w-5" />
          <Input
            type="text"
            placeholder="Search podcaster by name..."
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            className="pl-10 bg-gray-800 border-gray-700 text-white placeholder:text-gray-400 focus:border-mystic-green"
          />
        </div>
      </div>

      {/* Content */}
      {filteredPodcasters && filteredPodcasters.length > 0 ? (
        <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 xl:grid-cols-5 gap-4">
          {filteredPodcasters.map((podcaster) => (
            <PodcasterCard
              key={podcaster.AccountId}
              podcaster={podcaster}
              onViewDetails={() => handleViewDetails(podcaster.AccountId)}
            />
          ))}
        </div>
      ) : (
        <div className="w-full flex items-center flex-col justify-center gap-5 py-20">
          <p className="font-poppins text-white text-2xl font-bold">
            No podcasters found
          </p>
          <p className="font-poppins text-[#D9D9D9]">
            Try adjusting your search query
          </p>
        </div>
      )}
    </div>
  );
};

export default FollowedPodcastersPage;
