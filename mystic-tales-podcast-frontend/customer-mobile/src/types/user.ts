// Shape returned by the API
export type UserFromAPI = {
  Id: number;
  Email: string;
  FullName: string;
  Dob: string;
  Gender: string;
  Address: string;
  Phone: string;
  Balance: number;
  // File key for the main image as returned by the backend
  MainImageFileKey: string;
  PodcastListenSlot: number;
  DeactivatedAt: string;
  IsPodcaster: boolean;
};

// Shape used in the UI / frontend after resolving file keys to URLs
export type UserUI = {
  Id: number;
  Email: string;
  FullName: string;
  Dob: string;
  Gender: string;
  Address: string;
  Phone: string;
  Balance: number;
  // Resolved image URL for use in the UI
  ImageUrl: string;
  PodcastListenSlot: number;
  DeactivatedAt: string;
  IsPodcaster: boolean;
};

export type UserRole = {
  Id: number;
  Name: string;
};

// Backwards-compatibility: export `User` as the UI shape by default for places
// that previously imported `User`. Prefer importing `UserFromAPI` / `UserUI`
// explicitly when possible.
export type User = UserUI;
