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

  const handleSubmit = async () => {
    // mark submitting
    setIsSubmitting(true);

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
        PodcastBuddyId: selectedBuddy?.PodcastBuddyProfile.AccountId,
        BookingRequirementInfo: transformedRequirements,
      },
      BookingRequirementFiles: bookingFiles,
    };

    // Create FormData if backend expects multipart; attach JSON + files
    const formData = new FormData();
    formData.append(
      "BookingCreateInfo",
      JSON.stringify(payload.BookingCreateInfo)
    );
    bookingFiles.forEach((f) =>
      formData.append("BookingRequirementFiles", f, f.name)
    );

    console.log(
      "Transformed BookingRequirementInfos:",
      transformedRequirements
    );
    console.log(
      "Booking files to send:",
      bookingFiles.map((f) => f.name)
    );
    console.log("Payload (BookingCreateInfo) to send:", payload);

    // Note: Not sending network request here; caller can submit `formData` or `payload` as needed.

    setIsSubmitting(false);
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
