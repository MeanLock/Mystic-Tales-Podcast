/* eslint-disable @typescript-eslint/no-unused-vars */
// @ts-nocheck

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
import { useCreateMutation } from "@/core/services/booking/booking.service";
import { useNavigate } from "react-router-dom";

export type BookingRequirementInfo = {
  Name: string;
  Description: string;
  Order: number;
  PodcastBookingToneId: string;
  ContentType: "link" | "file" | "script";
  ContentValue?: string;
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
  const [bookingDeadlineDayCount, setBookingDeadlineDayCount] =
    useState<number>(1);

  // UI management states
  // submission state is provided by the RTK hook (createBooking)
  // Loading & Error States
  const [isLoading, setIsLoading] = useState<boolean>(true);
  // Loading & Error States
  const [notFoundPodcasterError, setNotFoundPodcasterError] =
    useState<boolean>(false);

  const [createBooking] = useCreateMutation();

  // HOOKS
  useEffect(() => {
    // Fetch podcaster data here
    fetch();
  }, []);
  const navigate = useNavigate();

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

  const handleSubmit = async () => {
    // basic validation
    if (!selectedBuddy) {
      alert("Please select a podcast buddy before submitting.");
      return;
    }

    console.log(
      "Parent bookingRequirements before submit:",
      bookingRequirements
    );

    // Transform requirements according to ContentType rules described in comments
    const transformedRequirements = bookingRequirements.map((item) => {
      // shallow clone to avoid mutating state
      const copy: any = { ...item };

      const ct = copy.ContentType;
      const cv = copy.ContentValue;

      if (ct === "file") {
        // For files we remove the helper fields; actual files are sent in BookingRequirementFiles
        delete copy.ContentType;
        delete copy.ContentValue;
      } else if (ct === "link") {
        // Append link marker to description, then remove helper fields
        const desc = copy.Description || "";
        const linkPart = cv ? `$-[link]$-${cv}$-[link]$-` : "";
        copy.Description = `${desc}${desc && linkPart ? "\n" : ""}${linkPart}`;
        delete copy.ContentType;
        delete copy.ContentValue;
      } else if (ct === "script") {
        // Append script marker to description, then remove helper fields
        const desc = copy.Description || "";
        const scriptPart = cv ? `$-[script]$-${cv}$-[script]$-` : "";
        copy.Description = `${desc}${
          desc && scriptPart ? "\n" : ""
        }${scriptPart}`;
        delete copy.ContentType;
        delete copy.ContentValue;
      } else {
        // If no content type, ensure helper fields are not present
        delete copy.ContentType;
        delete copy.ContentValue;
      }

      return copy as BookingRequirementInfo;
    });

    // Build payload
    const payload = {
      BookingCreateInfo: {
        Title: bookingTitle,
        DeadlineDayCount: bookingDeadlineDayCount,
        Description: bookingDescription,
        // selectedBuddy?.PodcastBuddyProfile.AccountId
        PodcastBuddyId: 17,
        BookingRequirementInfo: transformedRequirements,
      },
      BookingRequirementFiles: bookingFiles,
    };

    try {
      // Ensure PodcastBuddyId is a number
      const safePayload = {
        ...payload,
        BookingCreateInfo: {
          ...payload.BookingCreateInfo,
          PodcastBuddyId: 17,
        },
      };
      const formData = new FormData();
      formData.append(
        "BookingCreateInfo",
        JSON.stringify(safePayload.BookingCreateInfo)
      );

      // Append each file individually so the server receives actual File objects
      for (let i = 0; i < safePayload.BookingRequirementFiles.length; i++) {
        const file = safePayload.BookingRequirementFiles[i];
        // field name expected: BookingRequirementFiles (multiple entries)
        formData.append("BookingRequirementFiles", file);
      }

      // Debug: optionally log entries (browser console will not show file content)
      // for (const pair of formData.entries()) console.log(pair[0], pair[1]);

      // Use RTK Query hook to create booking (this wraps kickoffThenWait)
      const result = await createBooking({
        createBookingFormData: formData,
      }).unwrap();
      if (result) {
        alert(result);
        navigate("media-player/management/bookings");
      }
    } catch (err: any) {
      console.error("Create booking failed:", err);
      alert("Create booking failed: " + (err?.message || JSON.stringify(err)));
    }
  };
  return (
    <div className="w-full h-full flex flex-col overflow-y-auto scrollbar-hide">
      <p className="text-5xl m-8 font-poppins text-white font-bold">
        Create Booking
      </p>
      {notFoundPodcasterError && (
        <div className="m-4 p-4 bg-yellow-200 text-yellow-900 rounded">
          Selected podcaster not found. Please choose another podcaster.
        </div>
      )}
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
      {selectedBuddy && (
        <BookingForm
          Title={bookingTitle}
          BookingDeadlineDayCount={bookingDeadlineDayCount}
          onDeadlineDayCountChange={setBookingDeadlineDayCount}
          Description={bookingDescription}
          BookingRequirementFiles={bookingFiles}
          BookingRequirementInfos={bookingRequirements}
          onCreateNewRequirementInfo={(newReq: BookingRequirementInfo) => {
            console.log("onCreateNewRequirementInfo received:", newReq);
            setBookingRequirements((prev) => [...prev, newReq]);
          }}
          onDescriptionChange={setBookingDescription}
          onTitleChange={setBookingTitle}
          // receive a single-updated requirement and merge into parent list
          onUpdateRequirementInfo={(updatedReq: BookingRequirementInfo) => {
            console.log("onUpdateRequirementInfo received:", updatedReq);
            setBookingRequirements((prev) =>
              prev.map((r) => (r.Order === updatedReq.Order ? updatedReq : r))
            );
          }}
          // parent will append/replace the uploaded file; if null, do nothing for now
          onUploadNewFile={(file: File | null) => {
            if (!file) return;
            setBookingFiles((prev) => [
              // remove any file with same order prefix (e.g. "1.")
              ...prev.filter(
                (f) => !f.name.startsWith(`${file.name.split(".")[0]}.`)
              ),
              file,
            ]);
          }}
          selectedBuddy={selectedBuddy}
          onSubmit={handleSubmit}
        />
      )}
    </div>
  );
};
export default CreateBookingPage;
