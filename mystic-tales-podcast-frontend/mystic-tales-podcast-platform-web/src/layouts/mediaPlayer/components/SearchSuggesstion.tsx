import type { ContentRealtimeResponse } from "@/core/types/search";
import { IoIosSearch } from "react-icons/io";

import { useCallback, useEffect, useMemo, useState } from "react";

import {
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger,
} from "@/components/ui/tooltip";

import { IoPlay } from "react-icons/io5";
import PlayingWave from "@/components/playingWave/PlayWave";
import AutoResolveImage from "@/components/fileResolving/AutoResolveImage";
import { usePlayer } from "@/core/services/player/usePlayer";
import { useLazyGetSubscriptionBenefitsMapListFromEpisodeIdQuery } from "@/core/services/subscription/subscription.service";
import { debouncePromise } from "@/core/utils/debouncePromise";
import ActivityIndicator from "@/components/loader/ActivityIndicator";

type SearchSuggestionProps = {
  keywordOriginal: string;
  keywords: string[];
  contents: ContentRealtimeResponse[];
  isKeywordLoading: boolean;
  isContentLoading: boolean;
  onKeywordClick: (keyword: string) => void;
  onContentClick: (content: ContentRealtimeResponse) => void;
};

const SearchSuggesstion = ({
  keywordOriginal,
  keywords,
  contents,
  isKeywordLoading,
  isContentLoading,
  onKeywordClick,
  onContentClick,
}: SearchSuggestionProps) => {
  const [resolvedContents, setResolvedContents] = useState<
    ContentRealtimeResponse[]
  >([]);

  // HOOKS
  const {
    play,
    pause,
    state: uiState,
    playEpisodeFromSpecifyShow,
  } = usePlayer();

  const [getBenefitList] =
    useLazyGetSubscriptionBenefitsMapListFromEpisodeIdQuery();

  useEffect(() => {
    const resolveContent = async () => {
      if (contents.length > 0) {
        setResolvedContents(contents);
      } else {
        setResolvedContents([]);
      }
    };

    resolveContent();
  }, [contents]);

  // FUNCTIONS
  const highlightText = (text: string, keyword: string) => {
    if (!keyword.trim()) return text;

    const index = text.toLowerCase().indexOf(keyword.toLowerCase());
    if (index === -1) return text;

    const before = text.substring(0, index);
    const match = text.substring(index, index + keyword.length);
    const after = text.substring(index + keyword.length);

    return (
      <>
        {before}
        <span className="font-bold">{match}</span>
        {after}
      </>
    );
  };

  // Tách phần cần debounce (gọi API) ra riêng - KHÔNG depend vào uiState
  const playNewEpisode = useCallback(
    async (audioId: string) => {
      const benefitList =
        (await getBenefitList({ PodcastEpisodeId: audioId }).unwrap())
          .CurrentPodcastSubscriptionRegistrationBenefitList || [];

      playEpisodeFromSpecifyShow({
        audioId: audioId,
        benefitsList: benefitList,
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
      if (uiState.currentAudio && uiState.currentAudio.id === audioId) {
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

  // Always return content to keep popover open
  return (
    <div className="w-full z-9999 bg-white backdrop-blur-3xl rounded-sm shadow-xl border border-white/20">
      {/* Default search with original keyword */}
      {!isKeywordLoading &&
        keywords.length === 0 &&
        !isContentLoading &&
        resolvedContents.length === 0 && (
          <div className="p-1">
            <div
              onClick={() => onKeywordClick(keywordOriginal)}
              className="text-black cursor-pointer py-1 px-3 rounded-xs flex items-center justify-between hover:bg-gray-300"
            >
              <p className="text-sm">
                {highlightText(keywordOriginal, keywordOriginal)}
              </p>
              <IoIosSearch />
            </div>
          </div>
        )}

      {/* Keywords Section */}
      {isKeywordLoading ? (
        <div className="p-3 border-b border-white/10">
          <p className="text-sm text-gray-500">Searching keywords...</p>
        </div>
      ) : keywords.length > 0 ? (
        <div className="p-3 border-b border-white/10">
          <div className="flex flex-col gap-1">
            {keywords.map((keyword, index) => (
              <TooltipProvider key={index}>
                <Tooltip>
                  <TooltipTrigger asChild>
                    <div
                      onClick={() => onKeywordClick(keyword)}
                      className="group text-black cursor-pointer py-1 px-3 rounded-xs flex items-center justify-between hover:bg-gray-300"
                    >
                      <p className="text-sm line-clamp-1 max-w-55">
                        {highlightText(keyword, keywordOriginal)}
                      </p>
                      <IoIosSearch />
                    </div>
                  </TooltipTrigger>

                  <TooltipContent
                    side="right"
                    align="start"
                    className="max-w-xs absolute z-9999 w-125"
                  >
                    {keyword}
                  </TooltipContent>
                </Tooltip>
              </TooltipProvider>
            ))}
          </div>
        </div>
      ) : null}

      {/* Contents Section */}
      {isContentLoading ? (
        <div className="p-3 border-t border-t-[#252525]">
          <p className="text-sm text-gray-500">Loading contents...</p>
        </div>
      ) : resolvedContents.length > 0 ? (
        <div className="p-3 border-t border-t-[#252525]">
          <div className="flex flex-col gap-2">
            {resolvedContents.map((content, index) => {
              const item = content.Show || content.Episode;
              if (!item) return null;

              // Show UI
              if (content.Show) {
                return (
                  <div
                    key={index}
                    onClick={() => onContentClick(content)}
                    className="flex items-start gap-3 px-2 py-1 rounded-md cursor-pointer transition-colors hover:bg-gray-300"
                  >
                    {/* TODO: Design Show UI */}
                    <AutoResolveImage
                      FileKey={item.MainImageFileKey}
                      type="PodcastPublicSource"
                      className="w-10 h-10 rounded-full object-cover shadow-md"
                    />
                    <div className="flex-1 h-10 flex items-center min-w-0">
                      <p className="text-[#252525] text-sm font-semibold line-clamp-1">
                        {item.Name}
                      </p>
                      <div
                        className="text-gray-700 text-[9px] line-clamp-1 mt-1"
                        dangerouslySetInnerHTML={{ __html: item.Description }}
                      />
                    </div>
                  </div>
                );
              }

              // Episode UI
              if (content.Episode && content.Episode !== null) {
                return (
                  <div
                    key={index}
                    onClick={() => onContentClick(content)}
                    className="flex items-start gap-3 px-2 py-1 rounded-md cursor-pointer transition-colors hover:bg-gray-300"
                  >
                    <div className="group w-10 aspect-square rounded-sm flex items-center justify-center relative">
                      <AutoResolveImage
                        FileKey={content.Episode.MainImageFileKey}
                        type="PodcastPublicSource"
                        className="w-10 h-10 rounded-sm shadow-md object-cover"
                      />
                      {uiState.isLoadingSession &&
                      uiState.loadingAudioId === content.Episode.Id ? (
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
                            handlePlayPause(
                              content.Episode ? content.Episode.Id : null
                            );
                          }}
                          className={`z-10 absolute inset-0 bg-black/40 rounded-sm items-center justify-center ${
                            uiState.isPlaying &&
                            uiState.currentAudio &&
                            uiState.currentAudio?.id === content.Episode?.Id
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
                          uiState.currentAudio?.id === content.Episode?.Id ? (
                            <PlayingWave />
                          ) : (
                            <IoPlay className="text-white w-4 h-4" />
                          )}
                        </div>
                      )}
                    </div>
                    <div className="flex-1 h-10 min-w-0">
                      <div className="flex items-center gap-2">
                        <p className="text-[#252525] text-sm font-semibold line-clamp-1">
                          {item.Name}
                        </p>
                      </div>
                      <div
                        className="text-gray-700 text-xs line-clamp-1 mt-1"
                        dangerouslySetInnerHTML={{
                          __html: item.Description || "",
                        }}
                      />
                      <div />
                    </div>
                  </div>
                );
              }

              return null;
            })}
          </div>
        </div>
      ) : null}
    </div>
  );
};

export default SearchSuggesstion;
