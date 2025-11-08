import { mockPodcastBuddies } from "@/core/mockData/booking.mockdata";
import {
  type PodcastBookingToneCategoryType,
  type PodcastBookingToneType,
  type PodcastBuddyUI,
} from "@/core/types/booking";
import { useEffect, useState } from "react";
import PodcastBuddySelectComponent from "./components/PodcastBuddySelect";
import Loading from "@/components/loading";
import BookingForm from "./components/BookingForm";

export type BookingRequirementInfo = {
  Name: string;
  Description: string;
  Order: number;
  PodcastBookingToneId: string;
};

type BookingFilterOptions = {
  toneCategoryId: number;
  toneId: string;
};

interface CreateBookingPayloadProps {
  BookingCreateInfo: {
    Title: string;
    Description: string;
    PodcastBuddyId: number;
    BookingRequirementInfos: BookingRequirementInfo[];
  };
  BookingRequirementFiles: File[];
}

const CreateBookingPage = () => {
  // STATES

  // Data States
  const [podcastBuddies, setPodcastBuddies] = useState<PodcastBuddyUI[]>([]);
  const [availableBookingToneCategories, setAvailableBookingToneCategories] =
    useState<PodcastBookingToneCategoryType[]>([]);
  const [availableBookingTones, setAvailableBookingTones] = useState<
    PodcastBookingToneType[]
  >([]);

  // User Selections
  const [selectedBuddy, setSelectedBuddy] = useState<PodcastBuddyUI | null>(
    null
  );
  const [selectedBookingTone, setSelectedBookingTone] =
    useState<PodcastBookingToneType | null>(null);
  const [selectedBookingToneCategory, setSelectedBookingToneCategory] =
    useState<PodcastBookingToneCategoryType | null>(null);
  const [bookingTitle, setBookingTitle] = useState<string>("");
  const [bookingDescription, setBookingDescription] = useState<string>("");
  const [bookingFiles, setBookingFiles] = useState<File[]>([]);
  const [bookingRequirements, setBookingRequirements] = useState<
    BookingRequirementInfo[]
  >([]);

  // UI management states
  const [isFormValid, setIsFormValid] = useState<boolean>(false);
  const [isSubmitting, setIsSubmitting] = useState<boolean>(false);
  // Loading & Error States
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [notFoundPodcasterError, setNotFoundPodcasterError] =
    useState<boolean>(false);

  // HOOKS
  useEffect(() => {
    // Fetch podcaster data here
    fetch();
  }, []);

  // FUNCTIONS
  const fetch = async () => {
    // Fetch podcaster details from API
    setIsLoading(true);
    setTimeout(() => {
      setPodcastBuddies(mockPodcastBuddies);
      const storedPodcaster = localStorage.getItem("selectedPodcaster");
      console.log("storedPodcaster", storedPodcaster);
      console.log(
        "Id: ",
        storedPodcaster ? JSON.parse(storedPodcaster).Id : ""
      );
      let podcaster = null;
      if (storedPodcaster) {
        const selectedPodcasterId = JSON.parse(storedPodcaster).Id;
        podcaster = mockPodcastBuddies.find(
          (pb) => pb.PodcastBuddyProfile.AccountId === selectedPodcasterId
        );
        if (!podcaster) {
          setNotFoundPodcasterError(true);
          setSelectedBuddy(null);
        } else {
          setSelectedBuddy(podcaster);
        }
      } else {
        setSelectedBuddy(null);
      }

      // Gom tất cả PodcastBookingTones, loại trùng theo Id
      const allAvailableTones = Array.from(
        new Map(
          mockPodcastBuddies
            .flatMap((b) => b.PodcastBuddyProfile.PodcastBuddyBookingTone || [])
            .map((tone) => [tone.Id, tone])
        ).values()
      );

      // Gom tất cả PodcastBookingToneCategories, loại trùng theo Id
      const allAvailableBookingToneCategories = Array.from(
        new Map(
          allAvailableTones
            .filter((tone) => tone.PodcastBookingToneCategory) // <-- bảo vệ undefined
            .map((tone) => [
              tone.PodcastBookingToneCategory.Id,
              tone.PodcastBookingToneCategory,
            ])
        ).values()
      );

      // 4️⃣ Set state
      setAvailableBookingTones(allAvailableTones);
      setAvailableBookingToneCategories(allAvailableBookingToneCategories);

      const storedFilterOptions: BookingFilterOptions = JSON.parse(
        localStorage.getItem("bookingFilterOptions") || "{}"
      );
      if (storedFilterOptions) {
        if (storedFilterOptions.toneCategoryId) {
          setSelectedBookingToneCategory(
            allAvailableBookingToneCategories.find(
              (category) => category.Id === storedFilterOptions.toneCategoryId
            ) || null
          );
        }
        if (storedFilterOptions.toneId) {
          setSelectedBookingTone(
            allAvailableTones.find(
              (tone) => tone.Id === storedFilterOptions.toneId
            ) || null
          );
        }
      } else {
        setSelectedBookingToneCategory(null);
        setSelectedBookingTone(null);
      }

      setIsLoading(false);
    }, 2000);
  };

  return (
    <div className="w-full h-full flex flex-col overflow-y-auto scrollbar-hide">
      <p className="text-5xl m-8 font-poppins text-white font-bold">
        Create Booking
      </p>
      {isLoading ? (
        <div className="w-full h-[400px] bg-white/20 flex items-center justify-center">
          <Loading />
        </div>
      ) : (
        <PodcastBuddySelectComponent
          buddies={podcastBuddies}
          selectedBuddy={selectedBuddy}
          onSelectBuddy={setSelectedBuddy}
          availableBookingTones={availableBookingTones}
          selectedBookingTone={selectedBookingTone}
          onSelectBookingTone={setSelectedBookingTone}
          availableBookingToneCategories={availableBookingToneCategories}
          selectedBookingToneCategory={selectedBookingToneCategory}
          onSelectBookingToneCategory={setSelectedBookingToneCategory}
        />
      )}
      {selectedBuddy && <BookingForm />}
    </div>
  );
};
export default CreateBookingPage;
