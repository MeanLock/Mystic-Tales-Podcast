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

const FileConfig: FileResolveConfig[] = [
  {
    path: "MainImageFileKey",
    output: "ImageUrl",
    type: "PodcastPublic",
  },
];

const mockSearchData: SearchResultResponseUI = {
  TopSearchResults: [
    {
      Show: {
        Id: "show1",
        Name: "Mystic Tales: Horror Stories",
        Description: "Dive into the world of supernatural and horror stories",
        ImageUrl:
          "https://i.pinimg.com/736x/6d/fc/98/6dfc98d4b99f8a5cd34f39a3ade230a5.jpg",
        ReleaseDate: "2023-10-01T10:00:00Z",
        IsReleased: true,
      },
      Episode: null,
    },
    {
      Episode: {
        Id: "ep1",
        Name: "The Haunted Mansion - Episode 1",
        Description: "A group of friends explore an abandoned mansion",
        ImageUrl:
          "https://i.pinimg.com/736x/96/cc/f0/96ccf014f1afc8b6d7c35f3e3c0fa0b3.jpg",
        ReleaseDate: "2023-10-05T14:00:00Z",
        IsReleased: true,
      },
      Show: null,
    },
    {
      Show: {
        Id: "show2",
        Name: "True Crime Vietnam",
        Description: "Real crime stories from Vietnam",
        ImageUrl:
          "https://i.pinimg.com/736x/12/45/98/124598c3ef4f1b3af3120048d964eab0.jpg",
        ReleaseDate: "2023-09-15T08:00:00Z",
        IsReleased: true,
      },
      Episode: null,
    },
    {
      Episode: {
        Id: "ep2",
        Name: "Ghost in the Pagoda",
        Description: "Urban legends about haunted temples",
        ImageUrl:
          "https://i.pinimg.com/736x/aa/07/2d/aa072d662735bb21f1ed4d610d112cf0.jpg",
        ReleaseDate: "2023-10-12T16:00:00Z",
        IsReleased: true,
      },
      Show: null,
    },
  ],
  ShowList: [
    {
      Id: "show3",
      Name: "Morning Coffee Talk",
      Description: "Start your day with inspiring stories and conversations",
      MainImageFileKey: "",
      ImageUrl:
        "https://i.pinimg.com/736x/19/76/6c/19766c7b8ad0df7f42e852ccfa324f25.jpg",
      ReleaseDate: "2023-08-01T00:00:00Z",
      IsReleased: true,
    } as any,
    {
      Id: "show4",
      Name: "Tech Insights",
      Description: "Latest technology trends and innovations",
      MainImageFileKey: "",
      ImageUrl:
        "https://i.pinimg.com/736x/46/84/3b/46843b9f2bb286a7a3cd8be6b23607b5.jpg",
      ReleaseDate: "2023-07-20T00:00:00Z",
      IsReleased: true,
    } as any,
    {
      Id: "show5",
      Name: "Business Mindset",
      Description: "Entrepreneurship and business strategies",
      MainImageFileKey: "",
      ImageUrl:
        "https://i.pinimg.com/736x/8f/49/15/8f4915ab6acdf631bc13c56f80c08dc0.jpg",
      ReleaseDate: "2023-06-10T00:00:00Z",
      IsReleased: true,
    } as any,
    {
      Id: "show6",
      Name: "Health & Wellness",
      Description: "Tips for a healthier lifestyle",
      MainImageFileKey: "",
      ImageUrl:
        "https://i.pinimg.com/736x/72/2f/04/722f04dd1d34a8fdaa88840935fc8cda.jpg",
      ReleaseDate: "2023-05-05T00:00:00Z",
      IsReleased: true,
    } as any,
  ],
  EpisodeList: [
    {
      Id: "ep3",
      Name: "The Secret of Success",
      Description: "How to achieve your goals and dreams",
      MainImageFileKey: "",
      ImageUrl:
        "https://i.pinimg.com/736x/e0/85/e9/e085e9b511fa667b04f1c2a2ab0e862b.jpg",
      ReleaseDate: "2023-10-15T10:00:00Z",
      IsReleased: true,
    } as any,
    {
      Id: "ep4",
      Name: "Meditation for Beginners",
      Description: "Learn the basics of meditation and mindfulness",
      MainImageFileKey: "",
      ImageUrl:
        "https://i.pinimg.com/736x/37/c3/1b/37c31b3fe35eede81dd3538f6c9c264e.jpg",
      ReleaseDate: "2023-10-18T09:00:00Z",
      IsReleased: true,
    } as any,
    {
      Id: "ep5",
      Name: "AI Revolution",
      Description: "How artificial intelligence is changing the world",
      MainImageFileKey: "",
      ImageUrl:
        "https://i.pinimg.com/736x/aa/07/2d/aa072d662735bb21f1ed4d610d112cf0.jpg",
      ReleaseDate: "2023-10-20T14:00:00Z",
      IsReleased: true,
    } as any,
    {
      Id: "ep6",
      Name: "Cooking Vietnamese Cuisine",
      Description: "Traditional recipes from Vietnam",
      MainImageFileKey: "",
      ImageUrl:
        "https://i.pinimg.com/736x/6d/fc/98/6dfc98d4b99f8a5cd34f39a3ade230a5.jpg",
      ReleaseDate: "2023-10-22T11:00:00Z",
      IsReleased: true,
    } as any,
  ],
  ChannelList: [
    {
      Id: "channel1",
      Name: "Mystery Channel",
      Description: "Everything about mysteries and unsolved cases",
      MainImageFileKey: "",
      ImageUrl:
        "https://i.pinimg.com/736x/96/cc/f0/96ccf014f1afc8b6d7c35f3e3c0fa0b3.jpg",
    } as any,
    {
      Id: "channel2",
      Name: "Knowledge Hub",
      Description: "Educational content for curious minds",
      MainImageFileKey: "",
      ImageUrl:
        "https://i.pinimg.com/736x/12/45/98/124598c3ef4f1b3af3120048d964eab0.jpg",
    } as any,
    {
      Id: "channel3",
      Name: "Entertainment Plus",
      Description: "Movies, music, and pop culture",
      MainImageFileKey: "",
      ImageUrl:
        "https://i.pinimg.com/736x/46/84/3b/46843b9f2bb286a7a3cd8be6b23607b5.jpg",
    } as any,
    {
      Id: "channel4",
      Name: "Sports World",
      Description: "Latest sports news and analysis",
      MainImageFileKey: "",
      ImageUrl:
        "https://i.pinimg.com/736x/8f/49/15/8f4915ab6acdf631bc13c56f80c08dc0.jpg",
    } as any,
  ],
};

const SearchPage = () => {
  // STATES
  const [searchData, setSearchData] = useState<SearchResultResponseUI | null>(
    null
  );
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

  useEffect(() => {
    const resolveSearchData = async () => {
      if (!keyword || keyword.trim() === "") {
        navigate("/media-player/discovery");
        return;
      }

      if (searchDataRaw) {
        // Resolve all images
        const resolvedTopResults = await Promise.all(
          searchDataRaw.TopSearchResults.map(async (item) => {
            if (item.Show) {
              const { resolvedData } = await resolveFiles(
                item.Show,
                FileConfig
              );
              return { ...item, Show: resolvedData as any };
            }
            if (item.Episode) {
              const { resolvedData } = await resolveFiles(
                item.Episode,
                FileConfig
              );
              return { ...item, Episode: resolvedData as any };
            }
            return item;
          })
        );

        const resolvedShows = await Promise.all(
          searchDataRaw.ShowList.map(async (show) => {
            const { resolvedData } = await resolveFiles(show, FileConfig);
            return resolvedData as any;
          })
        );

        const resolvedEpisodes = await Promise.all(
          searchDataRaw.EpisodeList.map(async (episode) => {
            const { resolvedData } = await resolveFiles(episode, FileConfig);
            return resolvedData as any;
          })
        );

        const resolvedChannels = await Promise.all(
          searchDataRaw.ChannelList.map(async (channel) => {
            const { resolvedData } = await resolveFiles(channel, FileConfig);
            return resolvedData as any;
          })
        );

        const resolvedData: SearchResultResponseUI = {
          TopSearchResults: resolvedTopResults as any,
          ShowList: resolvedShows,
          EpisodeList: resolvedEpisodes,
          ChannelList: resolvedChannels,
        };

        // Merge with mockdata
        setSearchData({
          TopSearchResults: [
            ...resolvedData.TopSearchResults,
            ...mockSearchData.TopSearchResults,
          ],
          ShowList: [...resolvedData.ShowList, ...mockSearchData.ShowList],
          EpisodeList: [
            ...resolvedData.EpisodeList,
            ...mockSearchData.EpisodeList,
          ],
          ChannelList: [
            ...resolvedData.ChannelList,
            ...mockSearchData.ChannelList,
          ],
        });
      } else {
        // Nếu không có data từ API, dùng mockdata
        setSearchData(mockSearchData);
      }
    };

    resolveSearchData();
  }, [keyword, searchDataRaw, navigate]);

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
            {searchData?.TopSearchResults &&
            searchData.TopSearchResults.length > 0 ? (
              <div className="flex flex-col gap-3">
                {searchData.TopSearchResults.map((item, index) => {
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
                      <img
                        src={
                          (content as any).ImageUrl ||
                          "/images/unknown/podcast.png"
                        }
                        alt={content.Name}
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
            {searchData?.ChannelList && searchData.ChannelList.length > 0 ? (
              <div className="flex flex-col gap-3">
                {searchData.ChannelList.map((channel, index) => (
                  <div
                    key={index}
                    onClick={() =>
                      navigate(`/media-player/channel/${channel.Id}`)
                    }
                    className="flex items-start gap-4 p-3 rounded-lg hover:bg-white/10 cursor-pointer transition-all"
                  >
                    <img
                      src={
                        (channel as any).ImageUrl ||
                        "/images/unknown/podcast.png"
                      }
                      alt={channel.Name}
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
            {searchData?.ShowList && searchData.ShowList.length > 0 ? (
              <div className="flex flex-col gap-3">
                {searchData.ShowList.map((show, index) => (
                  <div
                    key={index}
                    onClick={() => navigate(`/media-player/show/${show.Id}`)}
                    className="flex items-start gap-4 p-3 rounded-lg hover:bg-white/10 cursor-pointer transition-all"
                  >
                    <img
                      src={
                        (show as any).ImageUrl || "/images/unknown/podcast.png"
                      }
                      alt={show.Name}
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
            {searchData?.EpisodeList && searchData.EpisodeList.length > 0 ? (
              <div className="flex flex-col gap-3">
                {searchData.EpisodeList.map((episode, index) => (
                  <div
                    key={index}
                    onClick={() =>
                      navigate(`/media-player/episode/${episode.Id}`)
                    }
                    className="flex items-start gap-4 p-3 rounded-lg hover:bg-white/10 cursor-pointer transition-all"
                  >
                    <img
                      src={
                        (episode as any).ImageUrl ||
                        "/images/unknown/podcast.png"
                      }
                      alt={episode.Name}
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
