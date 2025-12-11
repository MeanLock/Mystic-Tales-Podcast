export type Show = {
  Id: string;
  Name: string;
  Description: string;
  Language: string;
  ReleaseDate: string;
  IsReleased: boolean;
  Copyright: string;
  UploadFrequency: string;
  RatingCount: number;
  AverageRating: number;
  MainImageFileKey: string;
  TrailerAudioFileKey: string;
  TotalFollow: number;
  ListenCount: number;
  EpisodeCount: number;
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
  PodcastShowSubscriptionType: {
    Id: number;
    Name: string;
  };
  PodcastChannel: {
    Id: string;
    Name: string;
    MainImageFileKey: string;
  };
  Hashtags: {
    Id: number;
    Name: string;
  }[];
  TakenDownReason: string;
  CreatedAt: string;
  UpdatedAt: string;
  CurrentStatus: {
    Id: number;
    Name: string;
  };
};
export type ShowFromChannel = {
  Id: string;
  Name: string;
  Description: string;
  Language: string;
  ReleaseDate: string;
  IsReleased: boolean;
  Copyright: string;
  UploadFrequency: string;
  RatingCount: number;
  AverageRating: number;
  MainImageFileKey: string;
  TrailerAudioFileKey: string;
  TotalFollow: number;
  ListenCount: number;
  EpisodeCount: number;
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
  PodcastShowSubscriptionType: {
    Id: number;
    Name: string;
  };
  PodcastChannel: {
    Id: string;
    Name: string;
    MainImageFileKey: string;
  };
  Hashtags: [
    {
      Id: number;
      Name: string;
    }
  ];
  TakenDownReason: string;
  CreatedAt: string;
  UpdatedAt: string;
  CurrentStatus: {
    Id: number;
    Name: string;
  };
};
export type ShowDetails = {
  Id: string;
  Name: string;
  Description: string;
  Language: string;
  ReleaseDate: string;
  IsReleased: boolean;
  Copyright: string;
  UploadFrequency: string;
  RatingCount: number;
  AverageRating: number;
  MainImageFileKey: string;
  TrailerAudioFileKey: string;
  TotalFollow: number;
  ListenCount: number;
  EpisodeCount: number;
  IsFollowedByCurrentUser: boolean;
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
  PodcastShowSubscriptionType: {
    Id: number;
    Name: string;
  };
  PodcastChannel: {
    Id: string;
    Name: string;
    MainImageFileKey: string;
  };
  Hashtags: {
    Id: number;
    Name: string;
  }[];
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
    PodcastShowId: string;
    DeletedAt: string;
    UpdatedAt: string;
  }[];
  TakenDownReason: string;
  CreatedAt: string;
  UpdatedAt: string;
  CurrentStatus: {
    Id: number;
    Name: string;
  };
  PodcastSubscriptionList: {
    Id: number;
    Name: string;
    Description: string;
    PodcastChannelId: string;
    PodcastShowId: string;
    IsActive: boolean;
    CurrentVersion: number;
    DeletedAt: string | null;
    CreatedAt: string;
    UpdatedAt: string;
    PodcastSubscriptionCycleTypePriceList: {
      PodcastSubscriptionId: number;
      SubscriptionCycleType: {
        Id: number;
        Name: string;
      };
      Version: number;
      Price: number;
      CreatedAt: string;
      UpdatedAt: string;
    }[];
    PodcastSubscriptionBenefitMappingList: {
      PodcastSubscriptionId: number;
      PodcastSubscriptionBenefit: {
        Id: number;
        Name: string;
      };
      Version: number;
      CreatedAt: string;
      UpdatedAt: string;
    }[];
  }[];
  EpisodeList: [
    {
      Id: string;
      Name: string;
      Description: string;
      ExplicitContent: boolean;
      ReleaseDate: string;
      EpisodeOrder: number;
      IsReleased: boolean;
      MainImageFileKey: string;
      AudioFileKey: string;
      AudioFileSize: number;
      AudioLength: number;
      PodcastEpisodeSubscriptionType: {
        Id: number;
        Name: string;
      };
      PodcastShow: {
        Id: string;
        Name: string;
        MainImageFileKey: string;
      };
      Hashtags: {
        Id: number;
        Name: string;
      }[];
      SeasonNumber: number;
      TotalSave: number;
      ListenCount: number;
      IsAudioPublishable: boolean;
      TakenDownReason: string;
      CreatedAt: string;
      UpdatedAt: string;
      CurrentStatus: {
        Id: number;
        Name: string;
      };
    }
  ];
};
