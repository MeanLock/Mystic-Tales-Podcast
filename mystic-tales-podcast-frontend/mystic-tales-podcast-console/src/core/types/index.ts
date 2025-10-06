export type Role = {
  Id: number;
  Name: string;
};

export type Account = {
  Id: number;
  Email: string;
  Role: Role;
  Fullname: string;
  Dob: string;
  Gender: string;
  Address: string;
  Phone: string;
  Balance: number;
  MainImageFileUrl: string;
  IsVerified: boolean;
  GoogleId: string;
  VerifyCode: string;
  PodcastListenSlot: number;
  ViolationPoint: number;
  ViolationLevel: number;
  LastViolationPointChanged: string;
  LastViolationLevelChanged: string;
  LastPodcastListenSlotChanged: string;
  DeactivatedAt: string | null;
  CreatedAt: string;
  UpdatedAt: string;
  IsBeingPunish: boolean;
};

export type CustomerList = {
  CustomerList: Account[];
};
export type StaffList = {
  StaffList: Account[];
};
export type PodcasterList = {
  PodcasterList: Account[];
};
export type PodcasterProfile = {
  AccountId: number;
  Description: string;
  AverageRating: number;
  RatingCount: number;
  CommitmentDocumentFileUrl?: string;
  BuddyAudioFileUrl?: string;
  OwnedBookingStorageSize: number;
  UsedBookingStorageSize: number;
  IsVerified: boolean;
  CreatedAt: string;
  UpdatedAt: string;
}
export type Podcaster = {
  PodcasterProfile: PodcasterProfile;
}

export type FilterSurvey = {
  Id: number;
  Title: string;
  Description: string;
  SurveyTopicId: number;
  SurveySpecificTopicId: number;
  StartDate: string | null;
  EndDate: string | null;
  SecurityModeId: number;
  IsAvailable: boolean;
  PublishedAt: string | null;
  TakerBaseRewardPrice: number | null;
  ConfigJsonString: string;
  SurveyPrivateData: {
    RequesterId: number;
    SurveyTypeId: number;
    Kpi: number;
    TheoryPrice: number;
    ExtraPrice: number;
    ProfitPrice: number;
    AllocBaseAmount: number;
    AllocTimeAmount: number;
    AllocLevelAmount: number;
    MaxXp: number;
    DeletedAt: string;
    CreatedAt: string;
    UpdatedAt: string;
  } | null;
  SurveyStatusId: number;
  MainImageUrl: string | null;
  BackgroundImageUrl: string | null;
  QuestionCount: number;
  Questions: Question[];
};

export type QuestionOption = {
  Id: string;
  SurveyQuestionId: string;
  Content: string;
  Order: number;
  MainImageUrl: string | null;
};

export type QuestionType = {
  Id: number;
  Name: String;
  Price: number;
  DeactivatedAt: string | null;
};
export type FieldInputType = {
  Id: number;
  Name: string;
};
export type Question = {
  Id: string;
  SurveyId: number;
  QuestionTypeId: number;
  Content: string;
  Description: string | null;
  TimeLimit: number;
  IsVoiced: boolean;
  Order: number;
  ConfigJsonString: string | null;
  DeletedAt: string | null;
  Version: number;
  MainImageUrl: string | null;
  Options?: QuestionOption[];
};

export type SurveyTaker = {
  Id: number;
  FullName: string;
  Dob: string;
  Gender: string;
  MainImageUrl: string | null;
};

export type SurveyTakenResult = {
  Id: number;
  IsValid: boolean;
  InvalidReason: string | null;
  MoneyEarned: number;
  XpEarned: number;
  CompletedAt: string;
  Taker: SurveyTaker;
};

export type SurveyRewardTracking = {
  Id: number;
  SurveyId: number;
  RewardPrice: number;
  RewardXp: number;
  CreatedAt: string;
};


export type CommunitySurvey = {
  Id: number;
  Title: string;
  Description: string;
  SurveyTopicId: number;
  SurveySpecificTopicId: number;
  StartDate: string | null;
  EndDate: string | null;
  SecurityModeId: number;
  IsAvailable: boolean;
  PublishedAt: string | null;
  TakerBaseRewardPrice: number | null;
  ConfigJsonString: string | null;
  SurveyPrivateData: {
    RequesterId: number;
    SurveyTypeId: number;
    Kpi: number;
    TheoryPrice: number;
    ExtraPrice: number;
    ProfitPrice: number;
    AllocBaseAmount: number;
    AllocTimeAmount: number;
    AllocLevelAmount: number;
    MaxXp: number;
    DeletedAt: string | null;
    CreatedAt: string;
    UpdatedAt: string;
  } | null;
  SurveyStatusId: number;
  MainImageUrl: string | null;
  BackgroundImageUrl: string | null;
  QuestionCount: number;
  Questions: Question[];
  SurveyTakenResults: SurveyTakenResult[];
  SurveyRewardTrackings: SurveyRewardTracking[];
};


export type PlatformFeedback = {
  AccountId: number;
  RatingScore: number;
  Comment: string;
  CreatedAt: string;
  UpdatedAt: string | null;
};