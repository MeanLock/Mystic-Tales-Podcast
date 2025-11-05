import type { AccountRole } from "./account";

export type PodcasterFromAPI = {
  Id: number;
  Email: string;
  Role: AccountRole;
  FullName: string;
  Dob: string;
  Gender: string;
  Address: string;
  Phone: string;
  Balance: number;
  MainImageFileKey: string;
  IsVerified: boolean;
  GoogleId: string | null;
  PodcastListenSlot: number;
  ViolationPoint: number;
  ViolationLevel: number;
  LastViolationPointChanged: string | null;
  LastViolationLevelChanged: string | null;
  LastPodcastListenSlotChanged: string;
  DeactivatedAt: string | null;
  CreatedAt: string;
  UpdatedAt: string;
  PodcasterProfile: PodcasterProfile;
  ReviewList: PodcasterReviewAPI[];
};

export type PodcasterProfile = {
  AccountId: number;
  Name: string;
  Description: string;
  AverageRating: number;
  RatingCount: number;
  TotalFollow: number;
  ListenCount: number;
  CommitmentDocumentFileKey: string;
  BuddyAudioFileKey: string;
  OwnedBookingStorageSize: number;
  UsedBookingStorageSize: number;
  IsVerified: boolean;
  CreatedAt: string;
  UpdatedAt: string;
};

export type PodcasterReviewAPI = {
  Id: string;
  Title: string;
  Content: string;
  Rating: 0;
  Account: {
    Id: 0;
    FullName: string;
    Email: string;
    MainImageFileKey: string;
  };
  DeletedAt: string | null;
  UpdatedAt: string | null;
};

export type PodcasterUI = {
  Id: number;
  Email: string;
  Role: AccountRole;
  FullName: string;
  Dob: string;
  Gender: string;
  Address: string;
  Phone: string;
  Balance: number;
  ImageUrl: string;
  IsVerified: boolean;
  GoogleId: string | null;
  PodcastListenSlot: number;
  ViolationPoint: number;
  ViolationLevel: number;
  LastViolationPointChanged: string | null;
  LastViolationLevelChanged: string | null;
  LastPodcastListenSlotChanged: string;
  DeactivatedAt: string | null;
  CreatedAt: string;
  UpdatedAt: string;
  PodcasterProfile: PodcasterProfile;
  ReviewList: PodcasterReviewUI[];
};

export type PodcasterReviewUI = {
  Id: string;
  Title: string;
  Content: string;
  Rating: 0;
  Account: {
    Id: 0;
    FullName: string;
    Email: string;
    ImageUrl: string;
  };
  DeletedAt: string | null;
  UpdatedAt: string | null;
};

export type PodcastersByCategory = {
  Category: {
    Id: number;
    Name: string;
  };
  Podcasters: PodcasterUI[];
};
