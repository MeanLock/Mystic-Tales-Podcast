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

//=========================================DMCA Accusation=========================================
export type AssignedStaff = {
  Id: number;
  FullName: string;
  Email: string;
};
export type PodcastEpisode = {
  Id: string;
  Title: string;
};
export type PodcastShow = {
  Id: string;
  Name: string;
};
export type DMCAAccusation = {
  Id: number;
  PodcastShow: PodcastShow;
  PodcastEpisode: PodcastEpisode;
  AssignedStaff: AssignedStaff;
  LastLawsuitCheckingAlertAt: string;
  CreatedAt: string;
  UpdatedAt: string;
};
export type DMCAAccusationDetail = {
  Id: number;
  PodcastShowId: string;
  PodcastEpisodeId: string | null;
  AssignedStaff: number | null;
  LastLawsuitCheckingAlertAt: string | null;
  CreatedAt: string;
  UpdatedAt: string | null; 
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