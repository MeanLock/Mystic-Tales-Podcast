// @ts-nocheck

import type { CurrentAudioFromApi, CurrentAudioUI } from "./audio";

export type AccountFromAPI = {
  Id: number;
  Email: string;
  Role: AccountRole;
  FullName: string;
  Dob: string; //Format "YYYY-MM-DD"
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
  LastViolationPointChanged: string | null; // Format ISO String
  LastViolationLevelChanged: string | null; // Format ISO String
  LastPodcastListenSlotChanged: string | null;
  DeactivatedAt: string | null; // Format ISO String
  CreatedAt: string; // Format ISO String
  UpdatedAt: string; // Format ISO String
  IsPodcaster: boolean; //Để check xem User có phải podcaster không, hay là User thường
};

export type AccountMeFromApi = {
  Id: number;
  Email: string;
  FullName: string;
  Dob: string;
  Gender: string;
  Address: string;
  Phone: string;
  Balance: number;
  MainImageFileKey: string;
  PodcastListenSlot: number;
  DeactivatedAt: string;
  IsPodcaster: boolean;
};

export type AccountMeUI = {
  Id: number;
  Email: string;
  FullName: string;
  Dob: string;
  Gender: string;
  Address: string;
  Phone: string;
  Balance: number;
  ImageUrl: string;
  PodcastListenSlot: number;
  DeactivatedAt: string;
  IsPodcaster: boolean;
};

// Customer: {Id: 1, Name: "string"}
export type AccountRole = {
  Id: number;
  Name: string;
};

// Lấy /me từ API
// export type AccountMeFromApi = {
//   Account: AccountFromAPI;
//   CurrentAudio: CurrentAudioFromApi;
// };

