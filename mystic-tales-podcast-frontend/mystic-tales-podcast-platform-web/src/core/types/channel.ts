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

export type ChannelDetailsFromApi = {
  Channel: {
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
    PodcastSubscriptionList: {
      Id: number;
      Name: string;
      Description: string;
      PodcastChannelId: string;
      PodcastShowId: string;
      IsActive: boolean;
      CurrentVersion: number;
      DeletedAt: string;
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
    ShowList: {
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
    }[];
    IsFavoritedByCurrentUser: boolean;
  };
};

export type ChannelDetailsUI = {
  Channel: {
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
    PodcastSubscriptionList: {
      Id: number;
      Name: string;
      Description: string;
      PodcastChannelId: string | null;
      PodcastShowId: string | null;
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
    ShowList: {
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
      ImageUrl: string;
      TrailerAudioFileKey: string;
      TotalFollow: number;
      ListenCount: number;
      EpisodeCount: number;
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
      PodcastShowSubscriptionType: {
        Id: number;
        Name: string;
      };
      PodcastChannel: {
        Id: string;
        Name: string;
        ImageUrl: string;
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
    }[];
    IsFavoritedByCurrentUser: boolean;
  };
};
