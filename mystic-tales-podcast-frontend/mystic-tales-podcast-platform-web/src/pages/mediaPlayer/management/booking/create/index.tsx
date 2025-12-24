import { useEffect, useState } from "react";
import PodcastBuddySelectComponent from "./components/PodcastBuddySelect";
import Loading from "@/components/loading";
import BookingForm from "./components/BookingForm";
import {
  useCreateMutation,
  useGetPodcastBookingTonesQuery,
  useGetPodcastBuddiesByBookingToneQuery,
} from "@/core/services/booking/booking.service";
import { useNavigate } from "react-router-dom";
import type {
  PodcastBookingTone,
  PodcastBookingToneCategoryType,
  PodcastBuddyFromAPI,
} from "@/core/types/booking";
import { useGetPodcastBuddyDetailsQuery } from "@/core/services/podcasters/podcasters.service";
import { useDispatch } from "react-redux";
import { showAlert } from "@/redux/slices/alertSlice/alertSlice";

export type BookingRequirementInfo = {
  Name: string;
  Description: string;
  Order: number;
  PodcastBookingToneId: string;
  ContentType: "link" | "file" | "script";
  ContentValue?: string;
};

const CreateBookingPage = () => {
  // STATES

  // Data States
  const [availableBookingToneCategories, setAvailableBookingToneCategories] =
    useState<PodcastBookingToneCategoryType[]>([]);
  const [availableBookingTones, setAvailableBookingTones] = useState<
    PodcastBookingTone[]
  >([]);
  // User Selections
  const [selectedBuddy, setSelectedBuddy] =
    useState<PodcastBuddyFromAPI | null>(null);
  const [selectedBookingTone, setSelectedBookingTone] =
    useState<PodcastBookingTone | null>(null);

  const [selectedBookingToneCategory, setSelectedBookingToneCategory] =
    useState<PodcastBookingToneCategoryType | null>(null);
  // Form States
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
  // const [notFoundPodcasterError, setNotFoundPodcasterError] =
  //   useState<boolean>(false);

  const [createBooking, { isLoading: isCreating }] = useCreateMutation();

  // HOOKS
  const navigate = useNavigate();

  // Đầu tiên luôn lấy danh sách các Podcast Booking Tones
  const {
    data: availableBookingTonesFromAPI,
    isLoading: isLoadingAvailableBookingTonesFromAPI,
  } = useGetPodcastBookingTonesQuery();

  // Khi người dùng chọn một Podcast Booking Tone, lấy danh sách Podcast Buddies tương ứng
  const { data: availablePodcastBuddies } =
    useGetPodcastBuddiesByBookingToneQuery(
      { PodcastBookingToneId: selectedBookingTone?.Id! },
      { skip: !selectedBookingTone }
    );

  // Khi người dùng chọn một Podcast Buddies, gọi API để lấy chi tiết Podcaster
  const { data: selectedPodcastBuddyDetails } = useGetPodcastBuddyDetailsQuery(
    { AccountId: selectedBuddy?.Id! },
    { skip: !selectedBuddy }
  );

  // Khi có danh sách Podcast Booking Tones từ API, set vào state và extract categories
  useEffect(() => {
    if (!availableBookingTonesFromAPI || isLoadingAvailableBookingTonesFromAPI)
      return;

    // Step 1: Set available booking tones
    setAvailableBookingTones(
      availableBookingTonesFromAPI.PodcastBookingToneList
    );

    // Step 2: Extract and set unique tone categories
    const uniqueCategories: PodcastBookingToneCategoryType[] = [];
    availableBookingTonesFromAPI.PodcastBookingToneList.forEach((tone) => {
      const category = tone.PodcastBookingToneCategory;
      if (!uniqueCategories.find((cat) => cat.Id === category.Id)) {
        uniqueCategories.push(category);
      }
    });
    setAvailableBookingToneCategories(uniqueCategories);
    // UI can render selection once tones are ready
    setIsLoading(false);
  }, [availableBookingTonesFromAPI, isLoadingAvailableBookingTonesFromAPI]);

  useEffect(() => {
    // Set podcaster from localStorage if available
    const storedPodcaster = localStorage.getItem("selectedPodcaster");
    if (storedPodcaster) {
      const podcasterObj = JSON.parse(storedPodcaster) as PodcastBuddyFromAPI;
      setSelectedBuddy(podcasterObj);
    }
  }, []);
  const dispatch = useDispatch();
  // FUNCTIONS
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

    // Helper function to check if a requirement is complete
    const isRequirementComplete = (req: BookingRequirementInfo): boolean => {
      if (!req.Name.trim()) return false;
      if (!req.Description.trim() || req.Description === "<p><br></p>")
        return false;
      if (!req.PodcastBookingToneId) return false;
      if (!req.ContentType) return false;

      if (req.ContentType === "link") {
        return !!req.ContentValue?.trim();
      }
      if (req.ContentType === "script") {
        return !!req.ContentValue?.trim() && req.ContentValue !== "<p><br></p>";
      }
      if (req.ContentType === "file") {
        return !!bookingFiles.find((f) => f.name.startsWith(`${req.Order}.`));
      }

      return false;
    };

    // Filter only complete requirements
    const completeRequirements = bookingRequirements.filter(
      isRequirementComplete
    );

    if (completeRequirements.length < bookingRequirements.length) {
      dispatch(
        showAlert({
          type: "warning",
          description:
            "Some requirements were incomplete and have been omitted from your booking.",
          title: "Incomplete Requirements",
          isAutoClose: true,
          autoCloseDuration: 5,
          functionalButtonText: "Got it!",
          isClosable: true,
        })
      );
      return;
    }

    // Transform requirements according to ContentType rules described in comments
    const transformedRequirements = completeRequirements.map((item) => {
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
        PodcastBuddyId: selectedBuddy.Id,
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
        navigate("/media-player/management/bookings");
      }
    } catch (err: any) {
      console.error("Create booking failed:", err);
    }
  };

  return (
    <div className="w-full h-full flex flex-col overflow-y-auto scrollbar-hide">
      <p className="text-5xl m-8 font-poppins text-white font-bold">
        Create Booking
      </p>
      {/* notFoundPodcasterError UI removed as state is unused */}
      {isLoading ? (
        <div className="w-full h-[400px] bg-white/20 flex items-center justify-center">
          <Loading />
        </div>
      ) : (
        <PodcastBuddySelectComponent
          buddies={availablePodcastBuddies?.PodcastBuddyList || []}
          selectedBuddy={selectedBuddy}
          selectedBuddyDetails={selectedPodcastBuddyDetails}
          onSelectBuddy={setSelectedBuddy}
          availableBookingTones={availableBookingTones}
          selectedBookingTone={selectedBookingTone}
          onSelectBookingTone={setSelectedBookingTone}
          availableBookingToneCategories={availableBookingToneCategories}
          selectedBookingToneCategory={selectedBookingToneCategory}
          onSelectBookingToneCategory={setSelectedBookingToneCategory}
        />
      )}
      {selectedBuddy && selectedPodcastBuddyDetails && (
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
          onDeleteRequirementInfo={(order: number) => {
            console.log("onDeleteRequirementInfo received:", order);
            setBookingRequirements((prev) =>
              prev.filter((r) => r.Order !== order)
            );
            // Also remove associated files
            setBookingFiles((prev) =>
              prev.filter((f) => !f.name.startsWith(`${order}.`))
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
          selectedBuddy={selectedPodcastBuddyDetails}
          onSubmit={handleSubmit}
          isCreating={isCreating}
        />
      )}
    </div>
  );
};
export default CreateBookingPage;
