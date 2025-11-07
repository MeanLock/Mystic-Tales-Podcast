export type BookingFromAPI = {
  Id: number;
  Title: string;
  Description: string;
  AccountId: number;
  PodcasterId: number;
  Price: number;
  Deadline: string; //ISO String
  DemoAudioFileKey: string | null;
  BookingManualCancelledReason: string | null;
  BookingAutoCancelReason: string | null;
  CreatedAt: string; //ISO String
  UpdatedAt: string; //ISO String
  CurrentStatus: BookingStatusType
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
