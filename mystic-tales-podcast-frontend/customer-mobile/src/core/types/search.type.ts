import { Channel } from "./channel.type";
import { Episode } from "./episode.type";
import { Show } from "./show.type";

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
  ShowList: Show[];
  EpisodeList: Episode[];
  ChannelList: Channel[];
};

export type SearchResultResponseUI = {
  TopSearchResults: ContentRealtimeResponseUI[];
  ShowList: Show[];
  EpisodeList: Episode[];
  ChannelList: Channel[];
};