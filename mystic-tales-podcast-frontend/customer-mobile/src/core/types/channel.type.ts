export type Channel = {
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

export type ChannelDetails = {
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
    MainImageFileKey: string;
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
      MainImageFileKey: string;
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
      Description: string;
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

export type ChannelTest = {
  Channel: {
    Id: "3fa85f64-5717-4562-b3fc-2c963f66afa6";
    Name: "string";
    Description: "string";
    BackgroundImageFileKey: "string";
    MainImageFileKey: "string";
    TotalFavorite: 0;
    ListenCount: 0;
    ShowCount: 0;
    Podcaster: {
      Id: 0;
      FullName: "string";
      Email: "string";
      MainImageFileKey: "string";
    };
    PodcastCategory: {
      Id: 0;
      Name: "string";
      MainImageFileKey: "string";
    };
    PodcastSubCategory: {
      Id: 0;
      Name: "string";
      PodcastCategoryId: 0;
    };
    Hashtags: [
      {
        Id: 0;
        Name: "string";
      }
    ];
    CreatedAt: "2025-12-07T07:43:31.795Z";
    UpdatedAt: "2025-12-07T07:43:31.795Z";
    CurrentStatus: {
      Id: 0;
      Name: "string";
    };
    PodcastSubscriptionList: [
      {
        Id: 0;
        Name: "string";
        Description: null;
        PodcastChannelId: "3fa85f64-5717-4562-b3fc-2c963f66afa6";
        PodcastShowId: "3fa85f64-5717-4562-b3fc-2c963f66afa6";
        IsActive: false;
        CurrentVersion: 0;
        DeletedAt: "2025-12-07T07:43:31.795Z";
        CreatedAt: "2025-12-07T07:43:31.795Z";
        UpdatedAt: "2025-12-07T07:43:31.795Z";
        PodcastSubscriptionCycleTypePriceList: [
          {
            PodcastSubscriptionId: 0;
            SubscriptionCycleType: {
              Id: 0;
              Name: "string";
            };
            Version: 0;
            Price: 0;
            CreatedAt: "2025-12-07T07:43:31.795Z";
            UpdatedAt: "2025-12-07T07:43:31.795Z";
          }
        ];
        PodcastSubscriptionBenefitMappingList: [
          {
            PodcastSubscriptionId: 0;
            PodcastSubscriptionBenefit: {
              Id: 0;
              Name: "string";
            };
            Version: 0;
            CreatedAt: "2025-12-07T07:43:31.795Z";
            UpdatedAt: "2025-12-07T07:43:31.795Z";
          }
        ];
      }
    ];
    ShowList: [
      {
        Id: "3fa85f64-5717-4562-b3fc-2c963f66afa6";
        Name: "string";
        Description: "string";
        Language: "string";
        ReleaseDate: "2025-12-07T07:43:31.795Z";
        IsReleased: true;
        Copyright: "string";
        UploadFrequency: "string";
        RatingCount: 0;
        AverageRating: 0;
        MainImageFileKey: "string";
        TrailerAudioFileKey: "string";
        TotalFollow: 0;
        ListenCount: 0;
        EpisodeCount: 0;
        Podcaster: {
          Id: 0;
          FullName: "string";
          Email: "string";
          MainImageFileKey: "string";
        };
        PodcastCategory: {
          Id: 0;
          Name: "string";
          MainImageFileKey: "string";
        };
        PodcastSubCategory: {
          Id: 0;
          Name: "string";
          PodcastCategoryId: 0;
        };
        PodcastShowSubscriptionType: {
          Id: 0;
          Name: "string";
        };
        PodcastChannel: {
          Id: "3fa85f64-5717-4562-b3fc-2c963f66afa6";
          Name: "string";
          Description: "string";
          MainImageFileKey: "string";
        };
        Hashtags: [
          {
            Id: 0;
            Name: "string";
          }
        ];
        TakenDownReason: "string";
        CreatedAt: "2025-12-07T07:43:31.795Z";
        UpdatedAt: "2025-12-07T07:43:31.795Z";
        CurrentStatus: {
          Id: 0;
          Name: "string";
        };
      }
    ];
    IsFavoritedByCurrentUser: true;
  };
};
