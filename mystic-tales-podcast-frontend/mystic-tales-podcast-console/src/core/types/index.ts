export type Role = {
  Id: number;
  Name: string;
};
export type Account = {
  Id: number;
  Email: string;
  Role: Role;
  FullName: string;
  Dob: string;
  Gender: string;
  Address: string;
  Phone: string;
  Balance: number;
  MainImageFileKey: string;
  IsVerified: boolean;
  GoogleId: string;
  VerifyCode: string;
  PodcastListenSlot: number;
  ViolationPoint: number;
  ViolationLevel: number;
  LastViolationPointChanged: string;
  LastViolationLevelChanged: string;
  LastPodcastListenSlotChanged: string;
  DeactivatedAt?: string | null;
  CreatedAt: string;
  UpdatedAt: string;
};


export type PodcasterProfile = {
  AccountId: number;
  Name: string;
  Description?: string | null;
  AverageRating: number;
  RatingCount: number;
  TotalFollow: number;
  ListenCount: number;
  CommitmentDocumentFileKey: string | null;
  BuddyAudioFileKey: string | null;
  OwnedBookingStorageSize: number;
  UsedBookingStorageSize: number;
  PricePerBookingWord: number;
  IsVerified: boolean;
  CreatedAt: string;
  UpdatedAt: string;
}
export type Podcaster = {
  Id: number;
  Email: string;
  Role: Role;
  FullName: string;
  Dob: string;
  Gender: string;
  Address: string;
  Phone: string;
  Balance: number;
  MainImageFileKey: string;
  IsVerified: boolean;
  PodcastListenSlot: number;
  ViolationPoint: number;
  ViolationLevel: number;
  LastViolationPointChanged: string;
  LastViolationLevelChanged: string;
  LastPodcastListenSlotChanged: string;
  DeactivatedAt?: string | null;
  CreatedAt: string;
  UpdatedAt: string;
  PodcasterProfile: PodcasterProfile;
}

//=========================================DMCA Accusation=========================================
export type AssignedStaff = {
  Id: number;
  FullName: string;
  Email: string;
  MainImageFileKey: string;
};
export type PodcastEpisode = {
  Id: string;
  Name: string;
  MainImageFileKey: string;
};
export type PodcastShow = {
  Id: string;
  Name: string;
  MainImageFileKey: string;
};
export type DMCAAccusation = {
  Id: number;
  AccuserEmail: string;
  AccuserPhone: string;
  AccuserFullName: string;
  PodcastShow: PodcastShow;
  PodcastEpisode: PodcastEpisode;
  AssignedStaff: AssignedStaff;
  CurrentStatus: CurrentStatus;
  CreatedAt: string;
  UpdatedAt: string;
};
export type CurrentStatus = {
  Id: Number;
  Name: string;
};
export type DMCAAccusationDetail = {
  Id: number;
  AccuserEmail: string;
  AccuserPhone: string;
  AccuserFullName: string;
  DismissReason: string;
  PodcastShow: PodcastShow;
  PodcastEpisode: PodcastEpisode;
  AssignedStaff: AssignedStaff;
  CurrentStatus: CurrentStatus;
  CreatedAt: string;
  UpdatedAt: string;
  DMCANotice?: DMCANotice;
  CounterNotice?: CounterNotice;
  LawsuitProof?: LawsuitProof;
};
export type DMCANoticeAttachFile = {
  Id: string;
  AttachFileUrl: string;
  CreatedAt: string;
};

export type DMCANotice = {
  Id: string;
  PodcastShowId: string;
  PodcastEpisodeId: string;
  AccountId: number;
  AccountEmail: string;
  AccountPhone: string;
  GoodFaithStatement: string;
  WorkClaimed: string;
  Signature: string;
  IsValid: boolean;
  InValidReason: string;
  ValidatedBy: number;
  ValidatedAt: string;
  DmcaAccusationId: number;
  CreatedAt: string;
  UpdatedAt: string;
  DMCANoticeAttachFileList: DMCANoticeAttachFile[];
};

export type CounterNoticeAttachFile = {
  Id: string;
  AttachFileUrl: string;
  CreatedAt: string;
};

export type CounterNotice = {
  Id: string;
  AccountId: number;
  AccountEmail: string;
  AccountPhone: string;
  StatementPerjury: string;
  Signature: string;
  DmcaAccusationId: number;
  Jurisdiction: string;
  EvidenceFileKey: string;
  IsValid: boolean;
  InValidReason: string;
  ValidatedBy: number;
  ValidatedAt: string;
  FiledDate: string;
  CreatedAt: string;
  UpdatedAt: string;
  CounterNoticeAttachFileList: CounterNoticeAttachFile[];
};

export type LawsuitProofAttachFile = {
  Id: string;
  AttachFileUrl: string;
  CreatedAt: string;
};

export type LawsuitProof = {
  Id: string;
  AccountId: number;
  GoodFaithStatement: string;
  CourtName: string;
  CaseNumber: string;
  FilingDate: string;
  Signature: string;
  DmcaAccusationId: number;
  IsValid: boolean;
  InValidReason: string;
  ValidatedBy: number;
  ValidatedAt: string;
  JudgmentDetails: string;
  DateResolved: string;
  Outcome: string;
  RulingDocumentFileUrl: string;
  IsDefendantWon: boolean;
  CreatedAt: string;
  UpdatedAt: string;
  LawsuitProofAttachFileList: LawsuitProofAttachFile[];
};

export interface HashtagOption {
    id: number;
    name: string;
}