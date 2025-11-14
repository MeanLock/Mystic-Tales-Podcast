/* eslint-disable @typescript-eslint/no-unused-vars */
// @ts-nocheck

import type {
  PodcastBookingToneCategoryType,
  PodcastBookingToneType,
  PodcastBuddyUI,
} from "@/core/types/booking";
import { useEffect, useState } from "react";
import { FaArrowLeftLong, FaArrowRightLong } from "react-icons/fa6";
import { IoIosArrowRoundBack } from "react-icons/io";
import { TbCoinFilled } from "react-icons/tb";
import "./style.css";
import BuddyCard from "./components/BuddyCard";
import { Navigate, useNavigate } from "react-router-dom";

interface PodcastBuddySelectProps {
  buddies: PodcastBuddyUI[];
  selectedBuddy: PodcastBuddyUI | null;
  onSelectBuddy: (buddy: PodcastBuddyUI | null) => void;
  availableBookingTones: PodcastBookingToneType[];
  selectedBookingTone: PodcastBookingToneType | null;
  onSelectBookingTone: (tone: PodcastBookingToneType | null) => void;
  availableBookingToneCategories: PodcastBookingToneCategoryType[];
  selectedBookingToneCategory: PodcastBookingToneCategoryType | null;
  onSelectBookingToneCategory: (
    category: PodcastBookingToneCategoryType | null
  ) => void;
}

const PodcastBuddySelectComponent = ({
  buddies,
  selectedBuddy,
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
  const [availableBookingTonesLocal, setAvailableBookingTonesLocal] = useState<
    PodcastBookingToneType[]
  >(availableBookingTones);
  const [selectedToneLocal, setSelectedToneLocal] =
    useState<PodcastBookingToneType | null>(selectedBookingTone);
  const [buddiesLocal, setBuddiesLocal] = useState<PodcastBuddyUI[]>(buddies);

  // HOOKS
  const navigate = useNavigate();
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

  // Re-filter tones when category changes from parent
  useEffect(() => {
    if (selectedBookingToneCategory) {
      const filteredTones = availableBookingTones.filter(
        (tone) =>
          tone.PodcastBookingToneCategory.Id === selectedBookingToneCategory.Id
      );
      setAvailableBookingTonesLocal(filteredTones);
    } else {
      setAvailableBookingTonesLocal(availableBookingTones);
    }
  }, [selectedBookingToneCategory, availableBookingTones]);

  // Re-filter buddies when tone changes from parent (FIX: này là chỗ quan trọng nhất!)
  useEffect(() => {
    if (selectedBookingTone) {
      const filteredBuddies = buddies.filter((buddy) =>
        (buddy.PodcastBuddyProfile.PodcastBuddyBookingTone || []).some(
          (tone) => tone.Id === selectedBookingTone.Id
        )
      );
      setBuddiesLocal(filteredBuddies);
    } else {
      setBuddiesLocal(buddies);
    }
  }, [selectedBookingTone, buddies]);

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
    const filteredTones = availableBookingTones.filter(
      (tone) =>
        tone.PodcastBookingToneCategory.Id === selectedToneCategoryLocal?.Id
    );
    setAvailableBookingTonesLocal(filteredTones);
    onSelectBookingToneCategory(selectedToneCategoryLocal);
  };

  const handleSetTone = () => {
    if (!selectedToneLocal) {
      setBuddiesLocal(buddies);
      onSelectBookingTone(null);
      return;
    }

    const filteredBuddies = buddies.filter((buddy) =>
      (buddy.PodcastBuddyProfile.PodcastBuddyBookingTone || []).some(
        (tone) => tone.Id === selectedToneLocal.Id
      )
    );

    setBuddiesLocal(filteredBuddies);
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

  return (
    <div className="w-full flex flex-col">
      {step === 1 && (
        <div className="w-full h-[400px] bg-white/20 flex flex-col">
          <div className="flex flex-col items-center justify-center h-[100px] ">
            <p className="font-bold text-2xl">
              Select Booking Tone To Continue
            </p>
            <p className="font-semibold text-[#d9d9d9]">
              Lorem ipsum dolor sit amet, consectetur adipiscing elit.
            </p>
          </div>
          <div className="w-full h-[230px] flex items-center justify-center gap-5">
            {availableBookingToneCategories.map((category) => (
              <div
                key={category.Id}
                className={`w-64 h-[100px] flex items-center justify-center rounded-md cursor-pointer transition-all duration-500 hover:-translate-y-1 ${
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

          <div className="h-[70px] py-2 w-full flex items-center justify-center">
            {selectedToneCategoryLocal && (
              <div
                onClick={() => handleSetToneCategory()}
                className="cursor-pointer bg-transparent text-xl w-50 h-10/12 rounded-sm transition-all duration-500 ease-in-out hover:bg-black hover:-translate-y-1 border-2 border-mystic-green text-mystic-green flex items-center justify-center gap-2"
              >
                <p className="font-bold">Next</p>
                <FaArrowRightLong />
              </div>
            )}
          </div>
        </div>
      )}

      {step === 2 && (
        <div className="w-full h-[400px] bg-white/20 flex flex-col">
          <div className="flex flex-col items-center justify-center h-[100px] ">
            <p className="font-bold text-2xl">
              Select Booking Tone To Continue
            </p>
            <p className="font-semibold text-[#d9d9d9]">
              Lorem ipsum dolor sit amet, consectetur adipiscing elit.
            </p>
          </div>
          <div className="scrollbar-hide px-5 w-full h-[230px] overflow-y-auto grid md:grid-cols-4 grid-cols-2 gap-2">
            {availableBookingTonesLocal.map((tone) => (
              <div
                key={tone.Id}
                className={`flex h-[50px] shadow-[10px_10px_15px_rgba(42,42,42,0.5)]  items-center justify-center rounded-full cursor-pointer transition-all duration-500 hover:-translate-y-1 ${
                  selectedToneLocal?.Id === tone.Id
                    ? "bg-mystic-green text-black hover:text-black"
                    : "text-white border hover:border-2 hover:border-mystic-green hover:text-mystic-green shadow-[inset_2px_2px_2.84px_rgba(255,255,255,0.5),inset_5.68px_5.68px_25.55px_rgba(255,255,255,0.5),inset_-2px_-2px_2.84px_rgba(255,255,255,0.5)]"
                }`}
                onClick={() => setSelectedToneLocal(tone)}
              >
                <p className="text-xs font-bold text-center">{tone.Name}</p>
              </div>
            ))}
          </div>
          <div className="px-5 h-[70px] flex items-center justify-between">
            <div
              onClick={() => handleChooseAgain(2)}
              className="cursor-pointer border-2  px-5 py-2 rounded-sm border-mystic-green text-mystic-green transition-all duration-500 ease-in-out hover:bg-mystic-green hover:text-black hover:-translate-y-1 font-semibold flex items-center justify-center gap-1"
            >
              <FaArrowLeftLong />
              <p>Back</p>
            </div>
            {selectedToneLocal !== null && (
              <div
                onClick={() => handleSetTone()}
                className="cursor-pointer border-2  px-5 py-2 rounded-sm border-mystic-green text-mystic-green transition-all duration-500 ease-in-out hover:bg-mystic-green hover:text-black hover:-translate-y-1 font-semibold flex items-center justify-center gap-1"
              >
                <FaArrowRightLong />
                <p>Next</p>
              </div>
            )}
          </div>
        </div>
      )}

      {step === 3 && (
        <div className="w-full py-3 h-[400px] bg-white/20 flex flex-col items-center justify-center">
          <div className="flex flex-col items-center justify-center h-[100px]">
            <p className="font-bold text-2xl">Select Your Podcaster Now!</p>
            <p className="font-semibold text-[#d9d9d9]">
              Lorem ipsum dolor sit amet, consectetur adipiscing elit.
            </p>
          </div>
          <div className="scrollbar-hide w-full h-[230px] overflow-y-auto mt-5 p-5 grid xl:grid-cols-4 lg:grid-cols-3 md:grid-cols-2 grid-cols-1 gap-5">
            {buddiesLocal.length > 0 ? (
              buddiesLocal.map((buddy) => (
                <BuddyCard
                  key={buddy.PodcastBuddyProfile.AccountId}
                  buddy={buddy}
                  onViewDetails={handleViewPodcasterDetails}
                  onSelectBuddy={onSelectBuddy}
                />
              ))
            ) : (
              <div className="col-span-full text-center text-white/60">
                No podcasters available for selected tone
              </div>
            )}
          </div>
          <div className="h-[70px] px-5 w-full flex items-center justify-between">
            <div
              onClick={() => handleChooseAgain(3)}
              className="cursor-pointer border-2  px-5 py-2 rounded-sm border-mystic-green text-mystic-green transition-all duration-500 ease-in-out hover:bg-mystic-green hover:text-black hover:-translate-y-1 font-semibold flex items-center justify-center gap-1"
            >
              <FaArrowLeftLong />
              <p>Back</p>
            </div>
          </div>
        </div>
      )}

      {step === 4 && selectedBuddy && (
        <div className="w-full h-[400px] flex px-10 relative overflow-hidden">
          <img
            src={selectedBuddy.PodcastBuddyProfile.ImageUrl}
            alt={selectedBuddy.PodcastBuddyProfile.Name}
            className="absolute inset-0 w-full h-full object-cover filter blur-xl scale-110 opacity-80"
            style={{ transformOrigin: "center" }}
          />
          <div className="absolute inset-0 bg-black/50" />
          <div className="relative z-10 w-full flex items-center gap-10">
            <div className="w-[312px] h-[312px] rounded-full overflow-hidden shadow-xl">
              <img
                src={selectedBuddy.PodcastBuddyProfile.ImageUrl}
                alt={selectedBuddy.PodcastBuddyProfile.Name}
                className="w-full h-full object-cover"
              />
            </div>
            <div className="flex-1 flex flex-col justify-start text-white gap-3">
              <p className="text-[96px] font-bold p-0 m-0">
                {selectedBuddy.PodcastBuddyProfile?.Name.toLocaleUpperCase()}
              </p>
              <p className="font-poppins font-bold text-gray-300">
                {selectedBuddy.PodcastBuddyProfile.TotalFollow.toLocaleString()}{" "}
                Followers
              </p>
              <p className="text-sm text-gray-400 mt-2 line-clamp-4 w-2/3">
                {selectedBuddy.PodcastBuddyProfile.Description}
              </p>
              <div className="w-full flex items-center mt-5">
                <div
                  onClick={() => handleChooseAgain(4)}
                  className="cursor-pointer border-2 p-2 rounded-sm border-mystic-green text-mystic-green transition-all duration-500 ease-in-out hover:bg-mystic-green hover:text-black hover:-translate-y-1 font-semibold flex items-center justify-center gap-1"
                >
                  <IoIosArrowRoundBack size={18} />
                  <p>Choose Another</p>
                </div>
              </div>
            </div>
            <div className="absolute bottom-5 right-2">
              <div className="border-2 border-mystic-green  text-mystic-green shadow-2xl px-4 py-2  flex items-center justify-center gap-1">
                <p className="font-bold text-xl">
                  {(
                    selectedBuddy.PodcastBuddyProfile.PricePerBookingWord * 1000
                  ).toLocaleString()}
                </p>
                <TbCoinFilled />
                <p className="font-semibold text-sm ">/1000 words</p>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default PodcastBuddySelectComponent;
