import type { ChannelFromAPI } from "./channel";
import type { EpisodeFromAPI } from "./episode";
import type { ShowFromAPI } from "./show";

export type ContentRealtimeResponse = {
  Episode: {
    Id: string;
    Name: string;
    Description: string;
    MainImageFileKey: string;
    ReleaseDate: string;
    IsReleased: boolean;
  } | null;
  Show: {
    Id: string;
    Name: string;
    Description: string;
    MainImageFileKey: string;
    ReleaseDate: string;
    IsReleased: boolean;
  } | null;
};

export type ContentRealtimeResponseUI = {
  Episode: {
    Id: string;
    Name: string;
    Description: string;
    ImageUrl: string;
    ReleaseDate: string;
    IsReleased: boolean;
  } | null;
  Show: {
    Id: string;
    Name: string;
    Description: string;
    ImageUrl: string;
    ReleaseDate: string;
    IsReleased: boolean;
  } | null;
};

export type SearchResultResponse = {
  TopSearchResults: ContentRealtimeResponse[];
  ShowList: ShowFromAPI[];
  EpisodeList: EpisodeFromAPI[];
  ChannelList: ChannelFromAPI[];
};

export type SearchResultResponseUI = {
  TopSearchResults: ContentRealtimeResponseUI[];
  ShowList: ShowFromAPI[];
  EpisodeList: EpisodeFromAPI[];
  ChannelList: ChannelFromAPI[];
};