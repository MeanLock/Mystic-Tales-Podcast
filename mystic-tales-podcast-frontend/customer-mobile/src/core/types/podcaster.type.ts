export type PodcasterFromApi = {
  AccountId: number;
  Name: string;
  Description: string | null;
  AverageRating: number;
  RatingCount: number;
  TotalFollow: number;
  ListenCount: number;
  IsVerified: boolean;
  VerifiedAt: string;
  MainImageFileKey: string;
  IsBuddy: boolean;
  IsFollowedByCurrentUser: boolean;
};

export type PodcasterUI = {
  AccountId: number;
  Name: string;
  Description: string | null;
  AverageRating: number;
  RatingCount: number;
  TotalFollow: number;
  ListenCount: number;
  IsVerified: boolean;
  VerifiedAt: string;
  ImageUrl: string;
  IsBuddy: boolean;
  IsFollowedByCurrentUser: boolean;
};

export type PodcasterDetailsFromAPI = {
  AccountId: number;
  Name: string;
  Description: string | null;
  AverageRating: number;
  RatingCount: number;
  TotalFollow: number;
  ListenCount: number;
  IsVerified: boolean;
  VerifiedAt: string;
  MainImageFileKey: string;
  IsBuddy: boolean;
  IsFollowedByCurrentUser: boolean;
  ReviewList: PodcasterReviewAPI[];
};

export type PodcasterDetailsUI = {
  AccountId: number;
  Name: string;
  Description: string | null;
  AverageRating: number;
  RatingCount: number;
  TotalFollow: number;
  ListenCount: number;
  IsVerified: boolean;
  VerifiedAt: string;
  ImageUrl: string;
  IsBuddy: boolean;
  IsFollowedByCurrentUser: boolean;
  ReviewList: PodcasterReviewUI[];
};

export type PodcastBuddyDetails = {
  PodcastBuddyProfile: {
    AccountId: number;
    Name: string;
    Description: string;
    AverageRating: number;
    RatingCount: number;
    TotalFollow: number;
    ListenCount: number;
    PricePerBookingWord: number;
    BuddyAudioFileKey: string;
    IsVerified: boolean;
    IsFollowedByCurrentUser: boolean;
  };
  ReviewList: {
    Id: string;
    Title: string;
    Content: string;
    Rating: number;
    Account: {
      Id: number;
      FullName: string;
      Email: string;
      MainImageFileKey: string;
    };
    PodcastBuddyId: number;
    DeletedAt: string;
    UpdatedAt: string;
  }[];
};

export type PodcasterProfile = {
  AccountId: number;
  Name: string;
  Description: string | null;
  AverageRating: number;
  RatingCount: number;
  TotalFollow: number;
  ListenCount: number;
  IsVerified: boolean;
  PricePerBookingWord: number;
  IsBuddy: boolean; //Thêm cái này vào để xác định coi có nhận booking hay không
  VerifiedAt: string;
  CreatedAt: string;
  UpdatedAt: string;
};

export type PodcasterReviewAPI = {
  Id: string;
  Title: string;
  Content: string;
  Rating: number;
  Account: {
    Id: number;
    FullName: string;
    Email: string;
    MainImageFileKey: string;
  };
  DeletedAt: string | null;
  UpdatedAt: string;
};

export type PodcasterReviewUI = {
  Id: string;
  Title: string;
  Content: string;
  Rating: number;
  Account: {
    Id: number;
    FullName: string;
    Email: string;
    ImageUrl: string;
  };
  DeletedAt: string | null;
  UpdatedAt: string;
};

// export type PodcasterUI = {
//   Id: number;
//   Email: string;
//   Role: AccountRole;
//   FullName: string;
//   Dob: string;
//   Gender: string;
//   Address: string;
//   Phone: string;
//   Balance: number;
//   ImageUrl: string;
//   IsVerified: boolean;
//   GoogleId: string | null;
//   PodcastListenSlot: number;
//   ViolationPoint: number;
//   ViolationLevel: number;
//   LastViolationPointChanged: string | null;
//   LastViolationLevelChanged: string | null;
//   LastPodcastListenSlotChanged: string;
//   DeactivatedAt: string | null;
//   CreatedAt: string;
//   UpdatedAt: string;
//   PodcasterProfile: PodcasterProfile;
//   ReviewList: PodcasterReviewUI[];
// };

// export type PodcasterReviewUI = {
//   Id: string;
//   Title: string;
//   Content: string;
//   Rating: 0;
//   Account: {
//     Id: 0;
//     FullName: string;
//     Email: string;
//     ImageUrl: string;
//   };
//   DeletedAt: string | null;
//   UpdatedAt: string | null;
// };

// export type PodcastersByCategory = {
//   Category: {
//     Id: number;
//     Name: string;
//   };
//   Podcasters: PodcasterUI[];
// };
