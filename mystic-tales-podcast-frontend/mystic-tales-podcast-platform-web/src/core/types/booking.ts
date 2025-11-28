export type BookingFromAPI = {
  Id: number;
  Title: string;
  Description: string;
  Account: {
    Id: number;
    FullName: string;
    Email: string;
    MainImageFileKey: string;
  };
  PodcastBuddy: {
    Id: number;
    FullName: string;
    Email: string;
    MainImageFileKey: string;
  };
  Price: number;
  Deadline: string; //ISO String
  DemoAudioFileKey: string | null;
  BookingManualCancelledReason: string | null;
  BookingAutoCancelReason: string | null;
  CreatedAt: string; //ISO String
  UpdatedAt: string; //ISO String
  CurrentStatus: BookingStatusType;
};

export type BookingStatusType =
  | { Id: 1; Name: "Quotation Request" }
  | { Id: 2; Name: "Quotation Dealing" }
  | { Id: 3; Name: "Quotation Rejected" }
  | { Id: 4; Name: "Quotation Cancelled" }
  | { Id: 5; Name: "Producing" }
  | { Id: 6; Name: "Track Previewing" }
  | { Id: 7; Name: "Producing Requested" }
  | { Id: 8; Name: "Completed" }
  | { Id: 9; Name: "Customer Cancel Request" }
  | { Id: 10; Name: "Podcast Buddy Cancel Request" }
  | { Id: 11; Name: "Cancelled Automatically" }
  | { Id: 12; Name: "Cancelled Manually" };

export type BookingDetailsFromAPI = {
  Id: number;
  Title: string;
  Description: string;
  AccountId: number;
  PodcastBuddy: {
    Id: number;
    FullName: string;
    Email: string;
    MainImageFileKey: string;
    PriceBookingPerWord: number;
  };
  Price: number;
  Deadline: string | null;
  DeadlineDays: number;
  DemoAudioFileKey: string;
  BookingManualCancelledReason: string | null;
  BookingAutoCancelReason: string | null;
  CreatedAt: string;
  UpdatedAt: string;
  BookingRequirementFileList: {
    Id: string;
    BookingId: number;
    Name: string;
    Description: string;
    RequirementDocumentFileKey: string | null;
    Order: number;
    WordCount: number;
    PodcastBookingTone: {
      Id: string;
      Name: string;
      Description: string;
      PodcastBookingToneCategory: PodcastBookingToneCategoryType;
      CreatedAt: string;
      UpdatedAt: string;
    };
  }[];
  BookingProducingRequestList: {
    Id: string;
    BookingId: number;
    Note: string | null;
    Deadline: string;
    IsAccepted: boolean;
    FinishedAt: string | null;
    RejectReason: string | null;
    CreatedAt: string;
  }[];
  CurrentStatus: BookingStatusType;
  StatusTracking: {
    id: string;
    bookingId: number;
    bookingStatusId: number;
    createdAt: string;
  }[];
};

export type BookingDetailsUI = {
  Id: number;
  Title: string;
  Description: string;
  AccountId: number;
  PodcastBuddy: {
    Id: number;
    FullName: string;
    Email: string;
    ImageUrl: string;
    PriceBookingPerWord: number;
  };
  Price: number | null;
  Deadline: string | null;
  DeadlineDays: number;
  DemoAudioFileKey: string;
  BookingManualCancelledReason: string | null;
  BookingAutoCancelReason: string | null;
  CreatedAt: string;
  UpdatedAt: string;
  BookingRequirementFileList: {
    Id: string;
    BookingId: number;
    Name: string;
    Description: string;
    RequirementFile: string | null; //absolute url
    Order: number;
    WordCount: number;
    PodcastBookingTone: {
      Id: string;
      Name: string;
      Description: string;
      PodcastBookingToneCategory: PodcastBookingToneCategoryType;
      CreatedAt: string;
      UpdatedAt: string;
    };
  }[];
  BookingProducingRequestList: {
    Id: string;
    BookingId: number;
    Note: string | null;
    Deadline: string;
    IsAccepted: boolean;
    FinishedAt: string | null;
    RejectReason: string | null;
    CreatedAt: string;
  }[];
  CurrentStatus: BookingStatusType;
  StatusTracking: {
    id: string;
    bookingId: number;
    bookingStatusId: number;
    createdAt: string;
  }[];
};

export type PodcastBookingToneCategoryType = {
  Id: number;
  Name: string;
};

export type PodcastBookingToneType = {
  Id: string;
  Name: string;
  Description: string;
  PodcastBookingToneCategory: PodcastBookingToneCategoryType;
};

export type PodcastBuddyFromAPI = {
  PodcastBuddyProfile: {
    AccountId: number;
    Name: string;
    Description: string;
    MainImageFileKey: string;
    AverageRating: number;
    RatingCount: number;
    TotalFollow: number;
    ListenCount: number;
    PricePerBookingWord: number;
    BuddyAudioFileKey: string;
    IsVerified: boolean;
    PodcastBuddyBookingTone: PodcastBookingToneType[];
  };
};

export type PodcastBuddyUI = {
  PodcastBuddyProfile: {
    AccountId: number;
    Name: string;
    Description: string;
    ImageUrl: string;
    AverageRating: number;
    RatingCount: number;
    TotalFollow: number;
    ListenCount: number;
    PricePerBookingWord: number;
    BuddyAudioFileKey: string;
    IsVerified: boolean;
    PodcastBuddyBookingTone: PodcastBookingToneType[];
  };
};

export type BookingProducingRequestDetails = {
  Id: string;
  BookingId: number;
  Note: string | null;
  DeadlineDays: number;
  Deadline: string;
  IsAccepted: boolean;
  FinishedAt: string;
  RejectReason: string;
  CreatedAt: string;
  BookingPodcastTracks: {
    Id: string;
    BookingId: number;
    BookingRequirementId: string;
    BookingProducingRequestId: string;
    AudioFileKey: string;
    AudioFileSize: number;
    AudioLength: number;
    RemainingPreviewListenSlot: number;
  }[];
  EditRequirementList: {
    Id: string;
    Name: string;
    BookingPodcastTrack: {
      Id: string;
      BookingId: number;
      BookingRequirementId: string;
      BookingProducingRequestId: string;
      AudioFileKey: string;
      AudioFileSize: number;
      AudioLength: number;
      RemainingPreviewListenSlot: number;
    };
  }[];
  CurrentStatus: {
    Id: number;
    Name: string;
  };
};
