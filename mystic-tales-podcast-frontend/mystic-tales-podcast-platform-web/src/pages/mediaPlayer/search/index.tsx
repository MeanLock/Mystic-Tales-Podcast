import Loading from "@/components/loading";
import { useGetSearchResultsQuery } from "@/core/services/search/search.service";
import { useEffect, useState } from "react";
import { useNavigate, useSearchParams } from "react-router-dom";
import type { SearchResultResponseUI } from "@/core/types/search";
import {
  resolveFiles,
  type FileResolveConfig,
} from "@/core/utils/fileResolver.util";
import { IoIosArrowBack } from "react-icons/io";
import AutoResolveImage from "@/components/fileResolving/AutoResolveImage";

const FileConfig: FileResolveConfig[] = [
  {
    path: "MainImageFileKey",
    output: "ImageUrl",
    type: "PodcastPublic",
  },
];

const SearchPage = () => {
  // STATES
  // const [searchData, setSearchData] = useState<SearchResultResponseUI | null>(
  //   null
  // );
  const [activeTab, setActiveTab] = useState<
    "top" | "channels" | "shows" | "episodes"
  >("top");

  // HOOKS
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const keyword = searchParams.get("keyword");

  const { data: searchDataRaw, isLoading: isSearchDataLoading } =
    useGetSearchResultsQuery(
      { keyword: keyword || "" },
      {
        skip: !keyword || keyword.trim() === "",
      }
    );

  // useEffect(() => {
  //   const resolveSearchData = async () => {
  //     if (!keyword || keyword.trim() === "") {
  //       navigate("/media-player/discovery");
  //       return;
  //     }

  //     if (searchDataRaw) {
  //       // Resolve all images
  //       const resolvedTopResults = await Promise.all(
  //         searchDataRaw.TopSearchResults.map(async (item) => {
  //           if (item.Show) {
  //             const { resolvedData } = await resolveFiles(
  //               item.Show,
  //               FileConfig
  //             );
  //             return { ...item, Show: resolvedData as any };
  //           }
  //           if (item.Episode) {
  //             const { resolvedData } = await resolveFiles(
  //               item.Episode,
  //               FileConfig
  //             );
  //             return { ...item, Episode: resolvedData as any };
  //           }
  //           return item;
  //         })
  //       );

  //       const resolvedShows = await Promise.all(
  //         searchDataRaw.ShowList.map(async (show) => {
  //           const { resolvedData } = await resolveFiles(show, FileConfig);
  //           return resolvedData as any;
  //         })
  //       );

  //       const resolvedEpisodes = await Promise.all(
  //         searchDataRaw.EpisodeList.map(async (episode) => {
  //           const { resolvedData } = await resolveFiles(episode, FileConfig);
  //           return resolvedData as any;
  //         })
  //       );

  //       const resolvedChannels = await Promise.all(
  //         searchDataRaw.ChannelList.map(async (channel) => {
  //           const { resolvedData } = await resolveFiles(channel, FileConfig);
  //           return resolvedData as any;
  //         })
  //       );

  //       const resolvedData: SearchResultResponseUI = {
  //         TopSearchResults: resolvedTopResults as any,
  //         ShowList: resolvedShows,
  //         EpisodeList: resolvedEpisodes,
  //         ChannelList: resolvedChannels,
  //       };

  //       // Merge with mockdata
  //       setSearchData({
  //         TopSearchResults: [...resolvedData.TopSearchResults],
  //         ShowList: [...resolvedData.ShowList],
  //         EpisodeList: [...resolvedData.EpisodeList],
  //         ChannelList: [...resolvedData.ChannelList],
  //       });
  //     } else {
  //       setSearchData(null);
  //     }
  //   };

  //   resolveSearchData();
  // }, [keyword, searchDataRaw, navigate]);

  if (isSearchDataLoading) {
    return (
      <div className="w-full h-full flex flex-col items-center justify-center gap-5">
        <Loading />
        <p className="font-poppins text-[#D9D9D9] font-bold">
          Finding Your Contents...
        </p>
      </div>
    );
  }

  return (
    <div className="w-full h-full flex flex-col overflow-y-auto">
      <div
        onClick={() => navigate(-1)}
        className="px-8 pt-8 text-white font-poppins cursor-pointer hover:underline flex items-center gap-1"
      >
        <IoIosArrowBack size={20} />
        <p>Back</p>
      </div>
      {/* Header */}
      <div className="m-8">
        <p className="font-poppins text-white text-5xl font-bold">
          Search Results: "
          <span className="text-mystic-green font-semibold italic font-sans">
            {keyword}
          </span>
          "
        </p>
      </div>

      {/* Tabs */}
      <div className="px-8 flex gap-4 border-b border-white/20">
        <button
          onClick={() => setActiveTab("top")}
          className={`px-4 py-2 font-semibold transition-all ${
            activeTab === "top"
              ? "text-mystic-green border-b-2 border-mystic-green"
              : "text-white/60 hover:text-white"
          }`}
        >
          Top Results
        </button>
        <button
          onClick={() => setActiveTab("channels")}
          className={`px-4 py-2 font-semibold transition-all ${
            activeTab === "channels"
              ? "text-mystic-green border-b-2 border-mystic-green"
              : "text-white/60 hover:text-white"
          }`}
        >
          Channels
        </button>
        <button
          onClick={() => setActiveTab("shows")}
          className={`px-4 py-2 font-semibold transition-all ${
            activeTab === "shows"
              ? "text-mystic-green border-b-2 border-mystic-green"
              : "text-white/60 hover:text-white"
          }`}
        >
          Shows
        </button>
        <button
          onClick={() => setActiveTab("episodes")}
          className={`px-4 py-2 font-semibold transition-all ${
            activeTab === "episodes"
              ? "text-mystic-green border-b-2 border-mystic-green"
              : "text-white/60 hover:text-white"
          }`}
        >
          Episodes
        </button>
      </div>

      <div className="flex-1 px-8 py-8">
        {/* Top Results Tab */}
        {activeTab === "top" && (
          <div>
            {searchDataRaw?.TopSearchResults &&
            searchDataRaw?.TopSearchResults.length > 0 ? (
              <div className="flex flex-col gap-3">
                {searchDataRaw.TopSearchResults.map((item, index) => {
                  const content = item.Show || item.Episode;
                  if (!content) return null;
                  return (
                    <div
                      key={index}
                      onClick={() => {
                        if (item.Show) {
                          navigate(`/media-player/show/${item.Show.Id}`);
                        } else if (item.Episode) {
                          navigate(`/media-player/episode/${item.Episode.Id}`);
                        }
                      }}
                      className="flex items-start gap-4 p-3 rounded-lg hover:bg-white/10 cursor-pointer transition-all"
                    >
                      <AutoResolveImage
                        FileKey={content.MainImageFileKey}
                        type="PodcastPublicSource"
                        className="w-20 h-20 object-cover rounded-md flex-shrink-0"
                      />
                      <div className="flex-1 min-w-0">
                        <p className="text-white font-semibold text-lg line-clamp-1">
                          {content.Name}
                        </p>
                        <p className="text-gray-400 text-sm line-clamp-2 mt-1">
                          {content.Description}
                        </p>
                        <p className="text-gray-500 text-xs mt-2">
                          {item.Show ? "Show" : "Episode"}
                        </p>
                      </div>
                    </div>
                  );
                })}
              </div>
            ) : (
              <div className="flex flex-col items-center justify-center py-20">
                <p className="text-white text-2xl font-bold">
                  No results found
                </p>
                <p className="text-gray-400 mt-2">
                  Try searching with different keywords
                </p>
              </div>
            )}
          </div>
        )}

        {/* Channels Tab */}
        {activeTab === "channels" && (
          <div>
            {searchDataRaw?.ChannelList &&
            searchDataRaw.ChannelList.length > 0 ? (
              <div className="flex flex-col gap-3">
                {searchDataRaw.ChannelList.map((channel, index) => (
                  <div
                    key={index}
                    onClick={() =>
                      navigate(`/media-player/channel/${channel.Id}`)
                    }
                    className="flex items-start gap-4 p-3 rounded-lg hover:bg-white/10 cursor-pointer transition-all"
                  >
                    <AutoResolveImage
                      FileKey={channel.MainImageFileKey}
                      type="PodcastPublicSource"
                      className="w-20 h-20 object-cover rounded-full flex-shrink-0"
                    />
                    <div className="flex-1 min-w-0">
                      <p className="text-white font-semibold text-lg line-clamp-1">
                        {channel.Name}
                      </p>
                      <p className="text-gray-400 text-sm line-clamp-2 mt-1">
                        {channel.Description}
                      </p>
                    </div>
                  </div>
                ))}
              </div>
            ) : (
              <div className="flex flex-col items-center justify-center py-20">
                <p className="text-white text-2xl font-bold">
                  No channels found
                </p>
                <p className="text-gray-400 mt-2">
                  Try searching with different keywords
                </p>
              </div>
            )}
          </div>
        )}

        {/* Shows Tab */}
        {activeTab === "shows" && (
          <div>
            {searchDataRaw?.ShowList && searchDataRaw.ShowList.length > 0 ? (
              <div className="flex flex-col gap-3">
                {searchDataRaw.ShowList.map((show, index) => (
                  <div
                    key={index}
                    onClick={() => navigate(`/media-player/show/${show.Id}`)}
                    className="flex items-start gap-4 p-3 rounded-lg hover:bg-white/10 cursor-pointer transition-all"
                  >
                    <AutoResolveImage
                      FileKey={show.MainImageFileKey}
                      type="PodcastPublicSource"
                      className="w-20 h-20 object-cover rounded-md flex-shrink-0"
                    />
                    <div className="flex-1 min-w-0">
                      <p className="text-white font-semibold text-lg line-clamp-1">
                        {show.Name}
                      </p>
                      <p className="text-gray-400 text-sm line-clamp-2 mt-1">
                        {show.Description}
                      </p>
                    </div>
                  </div>
                ))}
              </div>
            ) : (
              <div className="flex flex-col items-center justify-center py-20">
                <p className="text-white text-2xl font-bold">No shows found</p>
                <p className="text-gray-400 mt-2">
                  Try searching with different keywords
                </p>
              </div>
            )}
          </div>
        )}

        {/* Episodes Tab */}
        {activeTab === "episodes" && (
          <div>
            {searchDataRaw?.EpisodeList &&
            searchDataRaw.EpisodeList.length > 0 ? (
              <div className="flex flex-col gap-3">
                {searchDataRaw.EpisodeList.map((episode, index) => (
                  <div
                    key={index}
                    onClick={() =>
                      navigate(`/media-player/episode/${episode.Id}`)
                    }
                    className="flex items-start gap-4 p-3 rounded-lg hover:bg-white/10 cursor-pointer transition-all"
                  >
                    <AutoResolveImage
                      FileKey={episode.MainImageFileKey}
                      type="PodcastPublicSource"
                      className="w-20 h-20 object-cover rounded-md flex-shrink-0"
                    />
                    <div className="flex-1 min-w-0">
                      <p className="text-white font-semibold text-lg line-clamp-1">
                        {episode.Name}
                      </p>
                      <p className="text-gray-400 text-sm line-clamp-2 mt-1">
                        {episode.Description}
                      </p>
                    </div>
                  </div>
                ))}
              </div>
            ) : (
              <div className="flex flex-col items-center justify-center py-20">
                <p className="text-white text-2xl font-bold">
                  No episodes found
                </p>
                <p className="text-gray-400 mt-2">
                  Try searching with different keywords
                </p>
              </div>
            )}
          </div>
        )}
      </div>
    </div>
  );
};

export default SearchPage;
