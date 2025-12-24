// @ts-nocheck
import Loading from "@/components/loading";
import { useGetSearchResultsQuery } from "@/core/services/search/search.service";
import { useCallback, useEffect, useMemo, useState } from "react";
import { useNavigate, useSearchParams } from "react-router-dom";
import { IoIosArrowBack } from "react-icons/io";
import AutoResolveImage from "@/components/fileResolving/AutoResolveImage";
import { usePlayer } from "@/core/services/player/usePlayer";
import { useLazyGetSubscriptionBenefitsMapListFromEpisodeIdQuery } from "@/core/services/subscription/subscription.service";
import PlayingWave from "@/components/playingWave/PlayWave";
import { IoPlay } from "react-icons/io5";
import ActivityIndicator from "@/components/loader/ActivityIndicator";
import { debouncePromise } from "@/core/utils/debouncePromise";

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
  const refresh = searchParams.get("refresh");

  const { data: searchDataRaw, isFetching: isSearchDataLoading } =
    useGetSearchResultsQuery(
      { keyword: keyword || "", refresh: refresh || undefined },
      {
        skip: !keyword || keyword.trim() === "",
        // Chỉ refetch nếu có refresh param (search mới) hoặc data cũ hơn 10s
        refetchOnMountOrArgChange: refresh ? true : 20,
      }
    );

  // Loại bỏ refresh param khỏi URL sau khi đã fetch xong để navigate(-1) không refetch lại
  useEffect(() => {
    if (refresh && !isSearchDataLoading && keyword) {
      // Replace URL without refresh param
      navigate(`/media-player/search?keyword=${encodeURIComponent(keyword)}`, {
        replace: true,
      });
    }
  }, [refresh, isSearchDataLoading, keyword, navigate]);

  const {
    play,
    pause,
    state: uiState,
    playEpisodeFromSpecifyShow,
  } = usePlayer();
  const [getBenefitList] =
    useLazyGetSubscriptionBenefitsMapListFromEpisodeIdQuery();

  // const handlePlayPause = async (episodeId: string) => {
  //   if (uiState.currentAudio && uiState.currentAudio?.id === episodeId) {
  //     if (uiState.isPlaying) {
  //       pause();
  //     } else {
  //       play();
  //     }
  //   } else {
  //     const benefitList = await getBenefitList({
  //       PodcastEpisodeId: episodeId,
  //     }).unwrap();
  //     playEpisodeFromSpecifyShow({
  //       audioId: episodeId,
  //       benefitsList:
  //         benefitList.CurrentPodcastSubscriptionRegistrationBenefitList || [],
  //     });
  //   }
  // };

  // Tách phần cần debounce (gọi API) ra riêng
  const playNewEpisode = useCallback(
    async (audioId: string) => {
      const benefitList = await getBenefitList({
        PodcastEpisodeId: audioId,
      }).unwrap();
      playEpisodeFromSpecifyShow({
        audioId: audioId,
        benefitsList:
          benefitList.CurrentPodcastSubscriptionRegistrationBenefitList || [],
      });
    },
    [getBenefitList, playEpisodeFromSpecifyShow]
  );

  // Memoize debounced function - chỉ depend vào playNewEpisode (ổn định)
  const debouncedPlayNew = useMemo(
    () => debouncePromise(playNewEpisode, 1000),
    [playNewEpisode]
  );

  // Handler check state trước khi gọi - có thể tạo lại không sao
  const handlePlayPause = useCallback(
    (audioId: string | null) => {
      if (!audioId) return;
      if (uiState.currentAudio && uiState.currentAudio?.id === audioId) {
        if (uiState.isPlaying) {
          pause();
        } else {
          play();
        }
      } else {
        debouncedPlayNew(audioId);
      }
    },
    [uiState, pause, play, debouncedPlayNew]
  );

  if (isSearchDataLoading) {
    return (
      <div className="scrollbar-hide w-full h-full flex flex-col items-center justify-center gap-5">
        <Loading />
        <p className="font-poppins text-[#D9D9D9] font-bold">
          Finding Your Contents...
        </p>
      </div>
    );
  }

  return (
    <div className="scrollbar-hide w-full h-full flex flex-col overflow-y-auto">
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
                  const isEpisode = !!item.Episode;
                  if (!content) return null;
                  return (
                    <div
                      key={index}
                      onClick={() => {
                        if (item.Show) {
                          navigate(`/media-player/shows/${item.Show.Id}`);
                        } else if (item.Episode) {
                          navigate(
                            `/media-player/episodes/${item.Episode.Id}`
                          );
                        }
                      }}
                      className="flex items-start gap-4 p-3 rounded-lg hover:bg-white/10 cursor-pointer transition-all"
                    >
                      <div className="w-20 h-20 group flex items-center justify-center relative">
                        <AutoResolveImage
                          FileKey={content.MainImageFileKey}
                          type="PodcastPublicSource"
                          className="w-20 h-20 object-cover rounded-md flex-shrink-0"
                        />
                        {uiState.isLoadingSession &&
                        uiState.loadingAudioId === content.Id &&
                        isEpisode ? (
                          <div
                            onClick={(e) => {
                              e.stopPropagation();
                            }}
                            className={`z-10 absolute inset-0 bg-black/40 rounded-sm flex items-center justify-center cursor-not-allowed`}
                          >
                            <ActivityIndicator size={16} color="#fff" />
                          </div>
                        ) : isEpisode ? (
                          <div
                            onClick={(e) => {
                              e.stopPropagation();
                              handlePlayPause(
                                content && isEpisode ? content.Id : null
                              );
                            }}
                            className={`z-10 absolute inset-0 bg-black/40 rounded-sm items-center justify-center ${
                              uiState.isPlaying &&
                              uiState.currentAudio &&
                              uiState.currentAudio?.id === content?.Id
                                ? "flex"
                                : "hidden group-hover:inline-flex"
                            }
                            ${
                              uiState.isLoadingSession
                                ? "cursor-not-allowed"
                                : "cursor-pointer"
                            }
                          `}
                          >
                            {uiState.isPlaying &&
                            uiState.currentAudio &&
                            uiState.currentAudio?.id === content?.Id ? (
                              <PlayingWave />
                            ) : (
                              <IoPlay className="text-white w-4 h-4" />
                            )}
                          </div>
                        ) : null}
                      </div>

                      <div className="flex-1 min-w-0">
                        <p className="text-white font-semibold text-lg line-clamp-1">
                          {content.Name}
                        </p>
                        <div
                          className="text-gray-400 text-sm line-clamp-2 mt-1"
                          dangerouslySetInnerHTML={{
                            __html: content.Description,
                          }}
                        />
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
                      navigate(`/media-player/channels/${channel.Id}`)
                    }
                    className="flex items-start gap-4 p-3 rounded-lg hover:bg-white/10 cursor-pointer transition-all"
                  >
                    <AutoResolveImage
                      FileKey={channel.MainImageFileKey}
                      type="PodcastPublicSource"
                      className="w-20 h-20 object-cover rounded-full shrink-0"
                    />
                    <div className="flex-1 min-w-0">
                      <p className="text-white font-semibold text-lg line-clamp-1">
                        {channel.Name}
                      </p>
                      <div
                        className="text-gray-400 text-sm line-clamp-2 mt-1"
                        dangerouslySetInnerHTML={{
                          __html: channel.Description,
                        }}
                      />
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
                      <div
                        className="text-gray-400 text-sm line-clamp-2 mt-1"
                        dangerouslySetInnerHTML={{ __html: show.Description }}
                      />
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
                      navigate(`/media-player/episodes/${episode.Id}`)
                    }
                    className="flex items-start gap-4 p-3 rounded-lg hover:bg-white/10 cursor-pointer transition-all"
                  >
                    <div className="w-20 h-20 group flex items-center justify-center relative">
                      <AutoResolveImage
                        FileKey={episode.MainImageFileKey}
                        type="PodcastPublicSource"
                        className="w-20 h-20 object-cover rounded-md shrink-0"
                      />
                      {uiState.isLoadingSession &&
                      uiState.loadingAudioId === episode.Id ? (
                        <div
                          onClick={(e) => {
                            e.stopPropagation();
                          }}
                          className={`z-10 absolute inset-0 bg-black/40 rounded-sm flex items-center justify-center cursor-not-allowed`}
                        >
                          <ActivityIndicator size={16} color="#fff" />
                        </div>
                      ) : (
                        <div
                          onClick={(e) => {
                            e.stopPropagation();
                            handlePlayPause(episode ? episode.Id : null);
                          }}
                          className={`z-10 absolute inset-0 bg-black/40 rounded-sm items-center justify-center ${
                            uiState.isPlaying &&
                            uiState.currentAudio &&
                            uiState.currentAudio?.id === episode?.Id
                              ? "flex"
                              : "hidden group-hover:inline-flex"
                          }
                            ${
                              uiState.isLoadingSession
                                ? "cursor-not-allowed"
                                : "cursor-pointer"
                            }
                          `}
                        >
                          {uiState.isPlaying &&
                          uiState.currentAudio &&
                          uiState.currentAudio?.id === episode?.Id ? (
                            <PlayingWave />
                          ) : (
                            <IoPlay className="text-white w-4 h-4" />
                          )}
                        </div>
                      )}
                    </div>

                    <div className="flex-1 min-w-0">
                      <p className="text-white font-semibold text-lg line-clamp-1">
                        {episode.Name}
                      </p>
                      <div
                        className="text-gray-400 text-sm line-clamp-2 mt-1"
                        dangerouslySetInnerHTML={{
                          __html: episode.Description,
                        }}
                      />
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
