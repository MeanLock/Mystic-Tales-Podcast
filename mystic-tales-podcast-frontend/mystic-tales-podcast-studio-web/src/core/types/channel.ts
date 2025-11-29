export type ChannelDetail = {
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
      Description: string | null;
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
        Description: string;
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
    }[];
};
