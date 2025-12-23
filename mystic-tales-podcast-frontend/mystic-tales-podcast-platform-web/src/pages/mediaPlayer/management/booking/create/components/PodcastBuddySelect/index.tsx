import type {
  PodcastBookingTone,
  PodcastBookingToneCategoryType,
  PodcastBuddyFromAPI,
} from "@/core/types/booking";
import { useEffect, useState, useRef } from "react";
import { FaPlay, FaPause } from "react-icons/fa6";
import "./style.css";
import BuddyCard from "./components/BuddyCard";
import { useNavigate } from "react-router-dom";
import type { PodcastBuddyDetails } from "@/core/types/podcaster";
// import AutoResolveImageBackground from "./components/AutoResolveImage";
import TonePill from "./components/TonePill";
import { LiquidButton } from "@/components/ui/shadcn-io/liquid-button";
import { useLazyGetAccountPublicSourceQuery } from "@/core/services/file/file.service";
import AutoResolveImage from "@/components/fileResolving/AutoResolveImage";
import MTPCoinOutline from "@/components/coinIcons/CoinIconOutline";

interface PodcastBuddySelectProps {
  buddies: PodcastBuddyFromAPI[];
  selectedBuddy: PodcastBuddyFromAPI | null;
  selectedBuddyDetails: PodcastBuddyDetails | null | undefined;
  onSelectBuddy: (buddy: PodcastBuddyFromAPI | null) => void;
  availableBookingTones: PodcastBookingTone[];
  selectedBookingTone: PodcastBookingTone | null;
  onSelectBookingTone: (tone: PodcastBookingTone | null) => void;
  availableBookingToneCategories: PodcastBookingToneCategoryType[];
  selectedBookingToneCategory: PodcastBookingToneCategoryType | null;
  onSelectBookingToneCategory: (
    category: PodcastBookingToneCategoryType | null
  ) => void;
}

const PodcastBuddySelectComponent = ({
  buddies,
  selectedBuddy,
  selectedBuddyDetails,
  onSelectBuddy,
  availableBookingTones,
  availableBookingToneCategories,
  selectedBookingTone,
  selectedBookingToneCategory,
  onSelectBookingTone,
  onSelectBookingToneCategory,
}: PodcastBuddySelectProps) => {
  // STATES
  const [step, setStep] = useState<number>(1);
  const [selectedToneCategoryLocal, setSelectedToneCategoryLocal] =
    useState<PodcastBookingToneCategoryType | null>(
      selectedBookingToneCategory
    );
  const [selectedToneLocal, setSelectedToneLocal] =
    useState<PodcastBookingTone | null>(selectedBookingTone);
  const [isPlayingTrailer, setIsPlayingTrailer] = useState(false);
  const [trailerAudioUrl, setTrailerAudioUrl] = useState<string | null>(null);
  // const [buddiesLocal, setBuddiesLocal] = useState<PodcastBuddyUI[]>(buddies);

  // HOOKS
  const navigate = useNavigate();
  const audioRef = useRef<HTMLAudioElement | null>(null);

  const [getFileUrl] = useLazyGetAccountPublicSourceQuery();

  // Sync step based on parent selections
  useEffect(() => {
    if (selectedBuddy) {
      setStep(4);
    } else if (selectedBookingTone) {
      setStep(3);
    } else if (selectedBookingToneCategory) {
      setStep(2);
    } else {
      setStep(1);
    }
  }, [selectedBuddy, selectedBookingTone, selectedBookingToneCategory]);

  // Sync local states when parent props change (e.g., on mount after navigation back)
  useEffect(() => {
    setSelectedToneCategoryLocal(selectedBookingToneCategory);
  }, [selectedBookingToneCategory]);

  useEffect(() => {
    setSelectedToneLocal(selectedBookingTone);
  }, [selectedBookingTone]);

  // Sync buddies from parent when data changes (parent fetches by selected tone)
  // useEffect(() => {
  //   setBuddiesLocal(buddies);
  // }, [buddies, selectedBookingTone]);

  // FUNCTIONS
  // Lọc danh sách Podcast Buddies dựa trên Podcast Booking Tone đã chọn
  const filteredBookingTones = () => {
    if (!selectedBookingToneCategory) return availableBookingTones;
    if (!availableBookingTones) return [];

    return availableBookingTones.filter(
      (tone) =>
        tone.PodcastBookingToneCategory.Id === selectedBookingToneCategory.Id
    );
  };
  const handleChooseAgain = (fromStep: number) => {
    if (fromStep === 4) {
      onSelectBuddy(null);
      // Step will auto-adjust via useEffect
    } else if (fromStep === 3) {
      onSelectBookingTone(null);
      setSelectedToneLocal(null);
      // Step will auto-adjust via useEffect
    } else if (fromStep === 2) {
      onSelectBookingTone(null);
      setSelectedToneLocal(null);
      setSelectedToneCategoryLocal(null);
      onSelectBookingToneCategory(null);
      // Step will auto-adjust via useEffect
    }
  };

  const handleSetToneCategory = () => {
    onSelectBookingToneCategory(selectedToneCategoryLocal);
  };

  const handleSetTone = () => {
    if (!selectedToneLocal) {
      onSelectBookingTone(null);
      return;
    }
    // Let parent trigger API fetch for buddies by tone
    onSelectBookingTone(selectedToneLocal);
  };

  const handleViewPodcasterDetails = (id: number) => {
    const filterOptionsPayload = {
      toneCategoryId: selectedToneCategoryLocal?.Id,
      toneId: selectedToneLocal?.Id,
    };
    localStorage.setItem(
      "bookingFilterOptions",
      JSON.stringify(filterOptionsPayload)
    );
    localStorage.removeItem("selectedPodcaster");
    navigate(`/media-player/podcasters/${id}`);
  };

  const handleListenToTrailer = async () => {
    if (!selectedBuddyDetails?.PodcastBuddyProfile.BuddyAudioFileKey) {
      console.error("No trailer audio available");
      return;
    }

    try {
      // Nếu đang phát thì pause
      if (isPlayingTrailer && audioRef.current) {
        audioRef.current.pause();
        setIsPlayingTrailer(false);
        return;
      }

      // Nếu chưa có URL thì fetch
      if (!trailerAudioUrl) {
        const fileUrlResponse = await getFileUrl({
          FileKey: selectedBuddyDetails.PodcastBuddyProfile.BuddyAudioFileKey,
        }).unwrap();
        setTrailerAudioUrl(fileUrlResponse.FileUrl);

        // Đợi audio element update rồi mới play
        setTimeout(() => {
          if (audioRef.current) {
            audioRef.current.play();
            setIsPlayingTrailer(true);
          }
        }, 100);
      } else {
        // Đã có URL rồi thì play luôn
        if (audioRef.current) {
          audioRef.current.play();
          setIsPlayingTrailer(true);
        }
      }
    } catch (error) {
      console.error("Error playing trailer:", error);
    }
  };

  // Cleanup audio khi unmount hoặc selectedBuddy thay đổi
  useEffect(() => {
    return () => {
      if (audioRef.current) {
        audioRef.current.pause();
        audioRef.current.src = "";
      }
      setIsPlayingTrailer(false);
      setTrailerAudioUrl(null);
    };
  }, [selectedBuddy]);

  return (
    <div className="w-full flex flex-col">
      {step === 1 && (
        <div className="w-full h-100 bg-white/20 flex flex-col">
          <div className="flex flex-col items-center justify-center h-25 ">
            <p className="font-bold text-2xl">
              Select Booking Tone Category To Continue
            </p>
            <p className="font-semibold text-[#d9d9d9]">
              Lorem ipsum dolor sit amet, consectetur adipiscing elit.
            </p>
          </div>
          <div className="w-full h-57.5 flex items-center justify-center gap-5">
            {availableBookingToneCategories.map((category) => (
              <div
                key={category.Id}
                className={`w-64 h-25 flex items-center justify-center rounded-md cursor-pointer transition-all duration-500 hover:-translate-y-1 ${
                  selectedToneCategoryLocal?.Id === category.Id
                    ? "bg-mystic-green text-black hover:text-black shadow-2xl"
                    : "text-white bg-white/30 border hover:border-2 hover:border-mystic-green hover:text-mystic-green"
                }`}
                onClick={() => setSelectedToneCategoryLocal(category)}
              >
                <p className="text-lg font-bold text-center">{category.Name}</p>
              </div>
            ))}
          </div>

          <div className="h-17.5 py-2 w-full flex items-center justify-center">
            {selectedToneCategoryLocal && (
              <LiquidButton
                onClick={() => handleSetToneCategory()}
                variant="minimalRoundedMd"
              >
                <p>Next</p>
              </LiquidButton>
            )}
          </div>
        </div>
      )}

      {step === 2 && (
        <div className="w-full h-100 bg-white/20 flex flex-col">
          <div className="flex flex-col items-center justify-center h-25 ">
            <p className="font-bold text-2xl">
              Select Booking Tone To Continue
            </p>
            <p className="font-semibold text-[#d9d9d9]">
              Lorem ipsum dolor sit amet, consectetur adipiscing elit.
            </p>
          </div>
          <div className="scrollbar-hide px-5 py-5 w-full h-57.5 overflow-y-auto grid md:grid-cols-4 grid-cols-2 gap-5">
            {filteredBookingTones().map((tone, index) => (
              <TonePill
                onSelect={(selectedTone) => setSelectedToneLocal(selectedTone)}
                key={`${tone.Id}-${index}`}
                bookingTone={tone}
                isSelected={selectedToneLocal?.Id === tone.Id}
              />
            ))}
          </div>
          <div className="px-5 h-17.5 flex items-center justify-between">
            <LiquidButton
              onClick={() => handleChooseAgain(2)}
              variant="minimalRoundedMd"
            >
              <p>Back</p>
            </LiquidButton>
            {selectedToneLocal !== null && (
              <LiquidButton
                onClick={() => handleSetTone()}
                variant="minimalRoundedMd"
              >
                <p>Next</p>
              </LiquidButton>
            )}
          </div>
        </div>
      )}

      {step === 3 && buddies && (
        <div className="w-full py-3 h-100 bg-white/20 flex flex-col items-center justify-center">
          <div className="flex flex-col items-center justify-center h-25">
            <p className="font-bold text-2xl">Select Your Podcaster Now!</p>
            <p className="font-semibold text-[#d9d9d9]">
              Lorem ipsum dolor sit amet, consectetur adipiscing elit.
            </p>
          </div>
          <div className="scrollbar-hide w-full h-57.5 overflow-y-auto mt-5 p-5 grid xl:grid-cols-4 lg:grid-cols-3 md:grid-cols-2 grid-cols-1 gap-5">
            {buddies.length > 0 ? (
              buddies.map((buddy) => (
                <div className="w-full flex items-center justify-center">
                  <BuddyCard
                    key={buddy.Id}
                    buddy={buddy}
                    onViewDetails={handleViewPodcasterDetails}
                    onSelectBuddy={onSelectBuddy}
                  />
                </div>
              ))
            ) : (
              <div className="col-span-full text-center text-white/60">
                No podcasters available for selected tone
              </div>
            )}
          </div>
          <div className="h-17.5 px-5 w-full flex items-center justify-between">
            <LiquidButton
              onClick={() => handleChooseAgain(3)}
              variant="minimalRoundedMd"
            >
              <p>Back</p>
            </LiquidButton>
          </div>
        </div>
      )}

      {step === 4 && selectedBuddy && selectedBuddyDetails && (
        <div className="w-full h-100 flex px-10 relative overflow-hidden">
          <AutoResolveImage
            FileKey={selectedBuddy.MainImageFileKey}
            type="AccountPublicSource"
            className="absolute inset-0 w-full h-full object-cover filter blur-xl scale-110 opacity-80"
          />
          <div className="absolute inset-0 bg-black/50" />
          <div className="relative z-10 w-full flex items-center gap-10">
            <div className="w-78 h-78 rounded-full overflow-hidden shadow-xl">
              <AutoResolveImage
                FileKey={selectedBuddy.MainImageFileKey}
                type="AccountPublicSource"
                className="w-full h-full object-cover"
              />
            </div>
            <div className="flex-1 flex flex-col justify-start text-white gap-3">
              <p className="text-[96px] font-bold p-0 m-0 line-clamp-1">
                {selectedBuddyDetails.PodcastBuddyProfile.Name.toLocaleUpperCase()}
              </p>
              <p className="font-poppins font-bold text-gray-300">
                {selectedBuddyDetails.PodcastBuddyProfile.TotalFollow.toLocaleString()}{" "}
                Followers
              </p>
              <div
                className="text-sm text-gray-400 mt-2 line-clamp-4 w-2/3"
                dangerouslySetInnerHTML={{
                  __html:
                    selectedBuddyDetails.PodcastBuddyProfile.Description || "",
                }}
              />

              {/* Hidden audio element for trailer playback */}
              {trailerAudioUrl && (
                <audio
                  ref={audioRef}
                  src={trailerAudioUrl}
                  onEnded={() => setIsPlayingTrailer(false)}
                  onPause={() => setIsPlayingTrailer(false)}
                  onPlay={() => setIsPlayingTrailer(true)}
                />
              )}
            </div>
            <div className="absolute bottom-5 right-2 left-0 flex items-center justify-end gap-5 ">
              <div className="flex items-center">
                <LiquidButton
                  onClick={() => handleChooseAgain(4)}
                  variant="minimalRoundedMd"
                >
                  <p>Choose Another</p>
                </LiquidButton>
              </div>
              {selectedBuddyDetails.PodcastBuddyProfile.BuddyAudioFileKey && (
                <div className="flex items-center">
                  <LiquidButton
                    onClick={handleListenToTrailer}
                    variant="minimalRoundedMd"
                  >
                    <div className="flex items-center gap-2">
                      {isPlayingTrailer ? <FaPause /> : <FaPlay />}
                      <p>
                        {isPlayingTrailer ? "Pause Trailer" : "Play Trailer"}
                      </p>
                    </div>
                  </LiquidButton>
                </div>
              )}
              <div className="border-2 border-mystic-green rounded-md text-mystic-green shadow-2xl flex items-center gap-1 h-10 px-5">
                <p className="font-bold">
                  {(
                    selectedBuddyDetails.PodcastBuddyProfile
                      .PricePerBookingWord * 1000
                  ).toLocaleString()}
                </p>
                <MTPCoinOutline size={16} color="#aee339" />
                <p className="font-semibold text-sm "> / 1000 words</p>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default PodcastBuddySelectComponent;
