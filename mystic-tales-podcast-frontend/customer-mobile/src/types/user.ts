export type User = {
  Id: number;
  Email: string;
  Role: UserRole;
  Fullname: string;
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

  LastViolationPointChanged: string;
  LastViolationLevelChanged: string;
  LastPodcastListenSlotChanged: string;

  DeactivatedAt: string | null;
  CreatedAt: string | null;
  UpdatedAt: string | null;
  
  IsBeingPunish: boolean;
};

export type UserRole = {
  Id: number;
  Name: string;
};
