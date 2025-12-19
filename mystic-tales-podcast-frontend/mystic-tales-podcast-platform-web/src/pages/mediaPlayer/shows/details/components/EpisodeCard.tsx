import type { RootState } from "@/redux/store";
import { useDispatch, useSelector } from "react-redux";
import { IoPlay } from "react-icons/io5";
import PlayingWave from "@/components/playingWave/PlayWave";
import { MoreHorizontalIcon, Save } from "lucide-react";
import { useState, useCallback, useMemo } from "react";
import { debouncePromise } from "@/core/utils/debouncePromise";
import ActivityIndicator from "@/components/loader/ActivityIndicator";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuGroup,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@radix-ui/react-dropdown-menu";
import { TbMessageReport } from "react-icons/tb";
import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogDescription,
  DialogClose,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Textarea } from "@/components/ui/textarea";
import { Label } from "@/components/ui/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  useGetEpisodeReportTypesQuery,
  useReportEpisodeMutation,
} from "@/core/services/report/report.service";
import { setError } from "@/redux/slices/errorSlice/errorSlice";
import { useEffect } from "react";
import { useSaveEpisodeMutation } from "@/core/services/episode/episode.service";
import { useLazyGetSubscriptionBenefitsMapListFromEpisodeIdQuery } from "@/core/services/subscription/subscription.service";
import type { EpisodeFromAPI } from "@/core/types/episode";
import { useLazyCheckUserPodcastListenSlotQuery } from "@/core/services/account/account.service";
import { showAlert } from "@/redux/slices/alertSlice/alertSlice";
import { useNavigate } from "react-router-dom";
import { usePlayer } from "@/core/services/player/usePlayer";
import AutoResolveImage from "@/components/fileResolving/AutoResolveImage";

// Helper function to format duration
const formatDuration = (seconds: number): string => {
  const minutes = Math.floor(seconds / 60);
  if (minutes < 1) {
    return `00:${seconds}s`;
  }
  return `${minutes} min`;
};

// Helper function to format time ago
const getTimeAgo = (dateString: string): string => {
  const date = new Date(dateString);
  const now = new Date();
  const diffInDays = Math.floor(
    (now.getTime() - date.getTime()) / (1000 * 60 * 60 * 24)
  );

  if (diffInDays === 0) return "Today";
  if (diffInDays === 1) return "1 day ago";
  return `${diffInDays} days ago`;
};

// Helper function to render description HTML
export function renderDescriptionHTML(description: string | null) {
  if (!description) return "";

  // --- Tách link ---
  const linkRegex = /\$-\[link\]\$-([\s\S]*?)\$-\[link\]\$-/;
  const linkMatch = description.match(linkRegex);
  const link = linkMatch ? linkMatch[1].trim() : null;

  // --- Tách script ---
  const scriptRegex = /\$-\[script\]\$-([\s\S]*?)\$-\[script\]\$-/;
  const scriptMatch = description.match(scriptRegex);
  const scriptContent = scriptMatch ? scriptMatch[1].trim() : null;

  // --- Loại bỏ các phần đặc biệt khỏi phần mô tả còn lại ---
  let cleanDescription = description
    .replace(linkRegex, "")
    .replace(scriptRegex, "")
    .trim();

  // --- Tạo HTML ---
  let html = `<p>${cleanDescription}</p>`;

  if (link) {
    html += `
    <p><strong>Link</strong>: <a href="${link}" target="_blank" rel="noopener noreferrer">${link}</a></p>`;
  }

  if (scriptContent) {
    html += `
    <p><strong>Script</strong>:</p>
    <div style="margin-top: 10px; border: 1px solid #ccc; padding: 10px; border-radius: 5px; background-color: #f9f9f9;">
      ${scriptContent}
    </div>
    `;
  }

  return html.trim();
}

const EpisodeCard = ({ episode }: { episode: EpisodeFromAPI }) => {
  const dispatch = useDispatch();
  const user = useSelector((state: RootState) => state.auth.user);

  const {
    playEpisodeFromSpecifyShow,
    play,
    pause,
    state: uiState,
  } = usePlayer();

  // REPORT STATES
  const [episodeReportDialog, setEpisodeReportDialog] = useState(false);
  const [episodeSelectedReportTypeId, setEpisodeSelectedReportTypeId] =
    useState<number | null>(null);
  const [episodeReportContent, setEpisodeReportContent] = useState("");
  const [isEpisodeAlreadyReported, setIsEpisodeAlreadyReported] =
    useState(false);

  const [fetchBenefits] =
    useLazyGetSubscriptionBenefitsMapListFromEpisodeIdQuery();
  const [triggerCheckListenSlot] = useLazyCheckUserPodcastListenSlotQuery();

  // REPORT QUERIES
  const {
    data: episodeAvailableReportTypes,
    isLoading: isEpisodeAvailableReportTypesLoading,
    refetch: refetchEpisodeReportTypes,
  } = useGetEpisodeReportTypesQuery(
    { PodcastEpisodeId: episode.Id },
    { skip: !episodeReportDialog }
  );

  const [reportEpisode, { isLoading: isReportingEpisode }] =
    useReportEpisodeMutation();
  const [saveEpisode] = useSaveEpisodeMutation();

  // Auto-check if episode is already reported when data loads
  useEffect(() => {
    if (!isEpisodeAvailableReportTypesLoading && episodeAvailableReportTypes) {
      if (episodeAvailableReportTypes.EpisodeReportTypeList.length > 0) {
        setIsEpisodeAlreadyReported(false);
      } else {
        setIsEpisodeAlreadyReported(true);
      }
    }
  }, [episodeAvailableReportTypes, isEpisodeAvailableReportTypesLoading]);

  const navigate = useNavigate();

  // Tách phần cần debounce (gọi API) ra riêng
  const playNewEpisode = useCallback(
    async (episodeId: string) => {
      const benefitData = await fetchBenefits({
        PodcastEpisodeId: episodeId,
      }).unwrap();
      const benefitList =
        benefitData.CurrentPodcastSubscriptionRegistrationBenefitList;

      if (benefitList && benefitList.length > 0) {
        const hasNonQuota = benefitList.some(
          (s: any) => s?.Id === 1 || s?.Name === "Non-Quota Listening"
        );
        if (hasNonQuota) {
          playEpisodeFromSpecifyShow({
            audioId: episodeId,
            benefitsList: benefitList,
          });
        } else {
          const listenSlot = await triggerCheckListenSlot().unwrap();
          if (listenSlot > 0) {
            playEpisodeFromSpecifyShow({
              audioId: episodeId,
              benefitsList: benefitList,
            });
          } else {
            dispatch(
              showAlert({
                title: "No Listen Slots Left",
                description:
                  "You have no remaining podcast listen slots. Please wait for your slots to renew.",
                type: "error",
                isAutoClose: true,
                autoCloseDuration: 10,
                isClosable: true,
              })
            );
          }
        }
      } else {
        const listenSlot = await triggerCheckListenSlot().unwrap();
        if (listenSlot > 0) {
          playEpisodeFromSpecifyShow({
            audioId: episodeId,
            benefitsList: [],
          });
        } else {
          dispatch(
            showAlert({
              title: "No Listen Slots Left",
              description:
                "You have no remaining podcast listen slots. Please wait for your slots to renew.",
              type: "error",
              isAutoClose: true,
              autoCloseDuration: 10,
              isClosable: true,
            })
          );
        }
      }
    },
    [
      fetchBenefits,
      playEpisodeFromSpecifyShow,
      triggerCheckListenSlot,
      dispatch,
    ]
  );

  // Memoize debounced function
  const debouncedPlayNew = useMemo(
    () => debouncePromise(playNewEpisode, 1000),
    [playNewEpisode]
  );

  const handlePlayPauseEpisode = useCallback(
    (episodeId: string) => {
      if (!user) {
        dispatch(
          showAlert({
            title: "Login Required",
            description: "You need to login first to play an episode!",
            type: "warning",
            isAutoClose: false,
            isFunctional: true,
            isClosable: true,
            functionalButtonText: "Login Now",
            onClickAction: () => {
              navigate("/auth/login");
            },
          })
        );
        return;
      }

      // Check if current episode is playing or paused
      if (uiState.currentAudio && uiState.currentAudio.id === episodeId) {
        if (uiState.isPlaying) {
          pause();
        } else {
          play();
        }
      } else {
        // Play new episode with debounce
        debouncedPlayNew(episodeId);
      }
    },
    [user, uiState, pause, play, debouncedPlayNew, dispatch, navigate]
  );

  // Handle report episode
  const handleReportEpisode = async () => {
    if (!user) {
      dispatch(
        setError({
          message: "You need to login first to report an episode!",
          autoClose: 10,
        })
      );
      return;
    }
    if (!episodeSelectedReportTypeId || episodeReportContent.trim() === "") {
      return;
    }
    try {
      await reportEpisode({
        PodcastEpisodeId: episode.Id,
        ReportTypeId: episodeSelectedReportTypeId,
        Content: episodeReportContent.trim(),
      }).unwrap();

      // Close dialog and reset form
      setEpisodeReportDialog(false);
      setEpisodeSelectedReportTypeId(null);
      setEpisodeReportContent("");

      // Refetch to update report status
      await refetchEpisodeReportTypes();
    } catch (error) {
      dispatch(
        setError({
          message: `Error while reporting episode: ${error}`,
          autoClose: 20,
        })
      );
    }
  };

  const handleSaveEpisode = async (episodeId: string) => {
    try {
      await saveEpisode({ PodcastEpisodeId: episodeId, IsSave: true }).unwrap();
    } catch (error) {
      dispatch(
        setError({
          message: `Error while saving episode: ${error}`,
          autoClose: 20,
        })
      );
    }
  };

  return (
    <div
      key={episode.Id}
      onClick={() => navigate(`/media-player/episodes/${episode.Id}`)}
      className="px-12 flex h-28 items-center gap-10 p-2 rounded-lg hover:bg-white/10 transition-colors group cursor-pointer"
    >
      <div className="relative aspect-square h-full bg-gray-700 rounded-lg overflow-hidden shrink-0">
        <AutoResolveImage
          FileKey={episode.MainImageFileKey}
          type="PodcastPublicSource"
          className="w-full h-full aspect-square object-cover"
        />

        {uiState.isLoadingSession && uiState.loadingAudioId === episode.Id ? (
          <div className="absolute inset-0 flex bg-black/40 items-center justify-center">
            <div className="p-3 rounded-full bg-mystic-green/80 flex items-center justify-center">
              <ActivityIndicator size={20} color="#fff" />
            </div>
          </div>
        ) : uiState.isPlaying &&
          uiState.currentAudio &&
          uiState.currentAudio?.id === episode.Id ? (
          <div className="absolute inset-0 flex bg-black/30 items-center justify-center">
            <div
              onClick={(e) => {
                e.stopPropagation();
                handlePlayPauseEpisode(episode.Id);
              }}
              className="p-3 rounded-full bg-mystic-green flex items-center justify-center hover:bg-mystic-green cursor-pointer"
            >
              <PlayingWave />
            </div>
          </div>
        ) : (
          <div className="absolute inset-0 hidden group-hover:inline-flex bg-black/30 items-center justify-center">
            <div
              onClick={(e) => {
                e.stopPropagation();
                handlePlayPauseEpisode(episode.Id);
              }}
              className={`p-2 rounded-full bg-gray-400 flex items-center justify-center hover:bg-mystic-green ${
                uiState.isLoadingSession
                  ? "cursor-not-allowed opacity-50"
                  : "cursor-pointer"
              }`}
            >
              <IoPlay size={25} color="#ffffff" />
            </div>
          </div>
        )}
      </div>

      <div className="flex-1 flex items-center justify-between gap-20">
        <div className="flex-1 min-w-0">
          <div className="flex items-start justify-between">
            <div className="flex-1 min-w-0">
              <p className="text-sm text-[#d9d9d9] mb-1">
                {getTimeAgo(episode.ReleaseDate)}
              </p>
              <h4 className="font-bold text-lg text-white mb-2 leading-tight">
                {`Season: ${episode.SeasonNumber}`} - {episode.Name}
              </h4>
              <div
                className="text-white font-light text-sm line-clamp-2 leading-relaxed"
                dangerouslySetInnerHTML={{
                  __html: renderDescriptionHTML(episode.Description),
                }}
              ></div>
            </div>
          </div>
        </div>

        <p className="text-white font-bold text-sm">
          {formatDuration(episode.AudioLength)}
        </p>

        <DropdownMenu modal={false}>
          <DropdownMenuTrigger asChild>
            <button className="text-mystic-green cursor-pointer">
              <MoreHorizontalIcon className="w-5 h-5" />
            </button>
          </DropdownMenuTrigger>
          <DropdownMenuContent
            align="end"
            className="w-48 bg-[#1a1d24] border border-white/10 rounded-lg shadow-xl p-1 z-50"
          >
            <DropdownMenuGroup>
              <DropdownMenuItem
                className="flex items-center gap-3 px-3 py-2 text-white hover:bg-white/10 rounded-md cursor-pointer transition-colors"
                onSelect={() => {
                  setEpisodeReportDialog(true);
                }}
              >
                <div className="flex items-center gap-3">
                  <div className="w-8 h-8 rounded-full bg-white/10 flex items-center justify-center">
                    <TbMessageReport className="text-white" size={16} />
                  </div>
                  <p className="text-sm">Report Episode</p>
                </div>
              </DropdownMenuItem>
              <DropdownMenuItem
                className="flex items-center gap-3 px-3 py-2 text-white hover:bg-white/10 rounded-md cursor-pointer transition-colors"
                onSelect={() => {
                  handleSaveEpisode(episode.Id);
                }}
              >
                <div className="flex items-center gap-3">
                  <div className="w-8 h-8 rounded-full bg-white/10 flex items-center justify-center">
                    <Save className="text-white" size={16} />
                  </div>
                  <p className="text-sm">Save Episode</p>
                </div>
              </DropdownMenuItem>
            </DropdownMenuGroup>
          </DropdownMenuContent>
        </DropdownMenu>

        {/* Report Episode Dialog */}
        <Dialog
          open={episodeReportDialog}
          onOpenChange={setEpisodeReportDialog}
        >
          <DialogContent className="sm:max-w-125 bg-[#0f1115]/95 border-white/10 text-white">
            <DialogHeader>
              <DialogTitle className="text-2xl font-bold text-mystic-green">
                Report Episode
              </DialogTitle>
              <DialogDescription className="text-white/70">
                We truly appreciate your feedback. <br />
                Please select a report type and share the reason so we can
                review and improve this episode.
              </DialogDescription>
            </DialogHeader>

            <div className="space-y-4 py-4">
              {/* Report Type Selection */}
              <div className="space-y-2">
                <Label htmlFor="episode-report-type" className="text-white">
                  Report Type <span className="text-red-500">*</span>
                </Label>
                {isEpisodeAvailableReportTypesLoading ? (
                  <div className="h-10 bg-white/5 rounded-md animate-pulse" />
                ) : isEpisodeAlreadyReported ? (
                  <p className="text-sm text-yellow-500">
                    You have already reported this episode
                  </p>
                ) : (
                  <Select
                    value={episodeSelectedReportTypeId?.toString() || ""}
                    onValueChange={(value) =>
                      setEpisodeSelectedReportTypeId(Number(value))
                    }
                  >
                    <SelectTrigger className="bg-white/5 border-white/10 text-white">
                      <SelectValue placeholder="Select a report type" />
                    </SelectTrigger>
                    <SelectContent className="bg-[#1a1d24] border-white/10 text-white">
                      {episodeAvailableReportTypes?.EpisodeReportTypeList.map(
                        (type) => (
                          <SelectItem
                            key={type.Id}
                            value={type.Id.toString()}
                            className="focus:bg-white/10 focus:text-white"
                          >
                            {type.Name}
                          </SelectItem>
                        )
                      )}
                    </SelectContent>
                  </Select>
                )}
              </div>

              {/* Report Content */}
              <div className="space-y-2">
                <Label htmlFor="episode-report-content" className="text-white">
                  Details <span className="text-red-500">*</span>
                </Label>
                <Textarea
                  id="episode-report-content"
                  placeholder="Please provide more details about the issue..."
                  value={episodeReportContent}
                  onChange={(e: React.ChangeEvent<HTMLTextAreaElement>) =>
                    setEpisodeReportContent(e.target.value)
                  }
                  className="min-h-30 bg-white/5 border-white/10 text-white placeholder:text-white/40 resize-none"
                  disabled={isEpisodeAlreadyReported}
                />
                <p className="text-xs text-white/50">Minimum 10 characters</p>
              </div>
            </div>

            <DialogFooter className="gap-2">
              <DialogClose asChild>
                <Button
                  variant="outline"
                  className="bg-transparent border-white/20 text-white hover:bg-white/10"
                >
                  Cancel
                </Button>
              </DialogClose>
              <Button
                onClick={async () => {
                  await handleReportEpisode();
                }}
                disabled={
                  !episodeSelectedReportTypeId ||
                  episodeReportContent.trim().length < 10 ||
                  isReportingEpisode ||
                  isEpisodeAlreadyReported
                }
                className="bg-mystic-green text-black hover:bg-mystic-green/90"
              >
                {isReportingEpisode ? "Submitting..." : "Submit Report"}
              </Button>
            </DialogFooter>
          </DialogContent>
        </Dialog>
      </div>
    </div>
  );
};

export default EpisodeCard;
