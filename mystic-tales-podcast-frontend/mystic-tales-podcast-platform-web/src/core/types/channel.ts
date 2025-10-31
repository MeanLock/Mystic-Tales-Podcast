export type ChannelFromAPI = {
  Id: string;
  Name: string;
  Description: string;
  BackgroundImageFileKey: string;
  MainImageFileKey: string;
  TotalFavorite: number;
  ListenCount: number;
  ShowCount: number;
  Podcaster: {
    Id: number;
    FullName: string;
    Email: string;
    MainImageFileKey: string;
  };
  PodcastCategory: {
    Id: number;
    Name: string;
  };
  PodcastSubCategory: {
    Id: number;
    Name: string;
    PodcastCategoryId: number;
  };
  Hashtags: {
    Id: number;
    Name: string;
  }[];
  CreatedAt: string;
  UpdatedAt: string;
  CurrentStatus: {
    Id: number;
    Name: string;
  };
};

export type ChannelUI = {
  Id: string;
  Name: string;
  Description: string;
  BackgroundImageUrl: string;
  ImageUrl: string;
  TotalFavorite: number;
  ListenCount: number;
  ShowCount: number;
  Podcaster: {
    Id: number;
    FullName: string;
    Email: string;
    ImageUrl: string;
  };
  PodcastCategory: {
    Id: number;
    Name: string;
  };
  PodcastSubCategory: {
    Id: number;
    Name: string;
    PodcastCategoryId: number;
  };
  Hashtags: {
    Id: number;
    Name: string;
  }[];
  CreatedAt: string;
  UpdatedAt: string;
  CurrentStatus: {
    Id: number;
    Name: string;
  };
};
