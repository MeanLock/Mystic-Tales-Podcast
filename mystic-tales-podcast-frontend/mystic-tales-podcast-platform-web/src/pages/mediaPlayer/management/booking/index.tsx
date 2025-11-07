import { Skeleton } from "@/components/ui/skeleton";
import type { BookingFromAPI } from "@/core/types/booking";
import { useState } from "react";

const mockBookingsData: BookingFromAPI[] = [
  {
    Id: 1,
    Title: "Voice-over for Travel Podcast Intro",
    Description:
      "Customer requested a quotation for an energetic travel intro voice-over.",
    AccountId: 101,
    PodcasterId: 501,
    Price: 0,
    Deadline: "2025-11-30T23:59:59Z",
    DemoAudioFileKey: "",
    BookingManualCancelledReason: "",
    BookingAutoCancelReason: "",
    CreatedAt: "2025-10-28T09:00:00Z",
    UpdatedAt: "2025-10-28T09:00:00Z",
    CurrentStatus: { Id: 1, Name: "Quotation Request" },
  },
  {
    Id: 2,
    Title: "Narration for Fitness Podcast Episode 5",
    Description:
      "Podcaster is discussing pricing details for the narration project.",
    AccountId: 102,
    PodcasterId: 502,
    Price: 50,
    Deadline: "2025-11-20T23:59:59Z",
    DemoAudioFileKey: "",
    BookingManualCancelledReason: "",
    BookingAutoCancelReason: "",
    CreatedAt: "2025-10-25T08:30:00Z",
    UpdatedAt: "2025-10-29T10:00:00Z",
    CurrentStatus: { Id: 2, Name: "Quotation Dealing" },
  },
  {
    Id: 3,
    Title: "Character Voice for Fantasy Drama",
    Description:
      "Podcaster rejected the quotation due to mismatch in price range.",
    AccountId: 103,
    PodcasterId: 503,
    Price: 200,
    Deadline: "2025-11-15T23:59:59Z",
    DemoAudioFileKey: "",
    BookingManualCancelledReason: "",
    BookingAutoCancelReason: "",
    CreatedAt: "2025-10-20T09:00:00Z",
    UpdatedAt: "2025-10-21T09:30:00Z",
    CurrentStatus: { Id: 3, Name: "Quotation Rejected" },
  },
  {
    Id: 4,
    Title: "ASMR Recording Session",
    Description: "Customer cancelled quotation after podcaster delay.",
    AccountId: 104,
    PodcasterId: 504,
    Price: 120,
    Deadline: "2025-11-10T23:59:59Z",
    DemoAudioFileKey: "",
    BookingManualCancelledReason: "",
    BookingAutoCancelReason: "",
    CreatedAt: "2025-10-18T09:00:00Z",
    UpdatedAt: "2025-10-19T09:00:00Z",
    CurrentStatus: { Id: 4, Name: "Quotation Cancelled" },
  },
  {
    Id: 5,
    Title: "Documentary Narration",
    Description: "Podcaster is currently producing the track.",
    AccountId: 105,
    PodcasterId: 505,
    Price: 300,
    Deadline: "2025-11-18T23:59:59Z",
    DemoAudioFileKey: "demo_505_track.mp3",
    BookingManualCancelledReason: "",
    BookingAutoCancelReason: "",
    CreatedAt: "2025-10-15T09:00:00Z",
    UpdatedAt: "2025-11-01T09:00:00Z",
    CurrentStatus: { Id: 5, Name: "Producing" },
  },
  {
    Id: 6,
    Title: "Tech Review Podcast Edit",
    Description: "Waiting for customer feedback on preview version.",
    AccountId: 106,
    PodcasterId: 506,
    Price: 180,
    Deadline: "2025-11-22T23:59:59Z",
    DemoAudioFileKey: "preview_506.mp3",
    BookingManualCancelledReason: "",
    BookingAutoCancelReason: "",
    CreatedAt: "2025-10-17T09:00:00Z",
    UpdatedAt: "2025-11-03T09:00:00Z",
    CurrentStatus: { Id: 6, Name: "Track Previewing" },
  },
  {
    Id: 7,
    Title: "Educational Podcast Jingle",
    Description:
      "Customer requested to start production after quotation approval.",
    AccountId: 107,
    PodcasterId: 507,
    Price: 90,
    Deadline: "2025-11-16T23:59:59Z",
    DemoAudioFileKey: "",
    BookingManualCancelledReason: "",
    BookingAutoCancelReason: "",
    CreatedAt: "2025-10-19T09:00:00Z",
    UpdatedAt: "2025-10-25T09:00:00Z",
    CurrentStatus: { Id: 7, Name: "Producing Requested" },
  },
  {
    Id: 8,
    Title: "Comedy Podcast Episode Mix",
    Description: "The track was completed and delivered successfully.",
    AccountId: 108,
    PodcasterId: 508,
    Price: 250,
    Deadline: "2025-11-12T23:59:59Z",
    DemoAudioFileKey: "final_mix_508.mp3",
    BookingManualCancelledReason: "",
    BookingAutoCancelReason: "",
    CreatedAt: "2025-10-10T09:00:00Z",
    UpdatedAt: "2025-10-30T09:00:00Z",
    CurrentStatus: { Id: 8, Name: "Completed" },
  },
  {
    Id: 9,
    Title: "Storytelling Session Episode 2",
    Description: "Customer requested cancellation during production.",
    AccountId: 109,
    PodcasterId: 509,
    Price: 150,
    Deadline: "2025-11-05T23:59:59Z",
    DemoAudioFileKey: "",
    BookingManualCancelledReason: "",
    BookingAutoCancelReason: "",
    CreatedAt: "2025-10-12T09:00:00Z",
    UpdatedAt: "2025-10-20T09:00:00Z",
    CurrentStatus: { Id: 9, Name: "Customer Cancel Request" },
  },
  {
    Id: 10,
    Title: "Podcast Buddy Cancel Test",
    Description: "Podcaster requested cancellation due to schedule conflict.",
    AccountId: 110,
    PodcasterId: 510,
    Price: 180,
    Deadline: "2025-11-10T23:59:59Z",
    DemoAudioFileKey: "",
    BookingManualCancelledReason: "",
    BookingAutoCancelReason: "",
    CreatedAt: "2025-10-11T09:00:00Z",
    UpdatedAt: "2025-10-14T09:00:00Z",
    CurrentStatus: { Id: 10, Name: "Podcast Buddy Cancel Request" },
  },
  {
    Id: 11,
    Title: "Drama Podcast Auto-Cancelled",
    Description:
      "Booking expired without response and was automatically cancelled.",
    AccountId: 111,
    PodcasterId: 511,
    Price: 200,
    Deadline: "2025-10-20T23:59:59Z",
    DemoAudioFileKey: "",
    BookingManualCancelledReason: "",
    BookingAutoCancelReason: "No response from podcaster within 72 hours.",
    CreatedAt: "2025-10-01T09:00:00Z",
    UpdatedAt: "2025-10-23T09:00:00Z",
    CurrentStatus: { Id: 11, Name: "Cancelled Automatically" },
  },
  {
    Id: 12,
    Title: "Music Background Manual Cancel",
    Description:
      "Admin manually cancelled booking due to inappropriate content.",
    AccountId: 112,
    PodcasterId: 512,
    Price: 100,
    Deadline: "2025-10-25T23:59:59Z",
    DemoAudioFileKey: "",
    BookingManualCancelledReason: "Content violated platform policy.",
    BookingAutoCancelReason: "",
    CreatedAt: "2025-10-05T09:00:00Z",
    UpdatedAt: "2025-10-07T09:00:00Z",
    CurrentStatus: { Id: 12, Name: "Cancelled Manually" },
  },
];

const BookingsPage = () => {
  // STATES
  const [bookings, setBookings] = useState<BookingFromAPI[]>([]);
  const [numberOfShowing, setNumberOfShowing] = useState(5);
  const [currentBooking, setCurrentBooking] = useState<BookingFromAPI | null>(
    null
  );
  const [isLoading, setIsLoading] = useState(true);
  // HOOKS

  // FUNCTIONS
  return (
    <div className="w-full h-full flex flex-col">
      {/* Title */}
      <p className="text-5xl m-8 font-bold font-poppins text-white">
        Your Bookings
      </p>

      {/* Contents here */}
      <div className="w-full flex-1 bg-blue-200 relative">
        {/* Action Bar */}
        <div className="w-full absolute top-0 z-50 h-24 px-5 bg-red-200/50 flex items-center text-white font-bold text-lg">
          Actions Bar
        </div>

        {/* Booking Cards */}
        <div className="w-full pt-24 h-full bg-green-200 flex flex-col items-center px-3 gap-5">
          <Skeleton className="w-full h-20" />
        </div>
      </div>
    </div>
  );
};

export default BookingsPage;
