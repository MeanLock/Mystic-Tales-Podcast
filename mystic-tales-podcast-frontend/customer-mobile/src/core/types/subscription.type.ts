export type SubscriptionDetails = {
  Id: number;
  Name: string;
  Description: string | null;
  PodcastChannelId: string | null;
  PodcastShowId: string | null;
  IsActive: boolean;
  CurrentVersion: number;
  DeletedAt: string | null;
  CreatedAt: string;
  UpdatedAt: string;
  PodcastSubscriptionCycleTypePriceList: PodcastSubscriptionCycleTypePriceType[];
  PodcastSubscriptionBenefitMappingList: PodcastSubscriptionBenefitType[];
  PodcastSubscriptionRegistrationList: null;
};

export type PodcastSubscriptionCycleTypePriceType = {
  PodcastSubscriptionId: number;
  SubscriptionCycleType: SubscriptionCycleType;
  Version: number;
  Price: number;
  CreatedAt: string;
  UpdatedAt: string;
};

export type PodcastSubscriptionBenefitType = {
  PodcastSubscriptionId: number;
  PodcastSubscriptionBenefit: PodcastSubscriptionBenefit;
  Version: number;
  CreatedAt: string;
  UpdatedAt: string;
};

export type SubscriptionRegistrationItem = {
  Id: string;
  AccountId: number;
  PodcastSubscriptionId: number;
  SubscriptionCycleType: SubscriptionCycleType;
  CurrentVersion: number;
  IsAcceptNewestVersionSwitch: boolean | null;
  IsIncomeTaken: boolean;
  LastPaidAt: string | null;
  CancelledAt: string | null;
  CreatedAt: string;
  UpdatedAt: string | null;
};

export type SubscriptionCycleType =
  | {
      Id: 1;
      Name: "Monthly";
    }
  | {
      Id: 2;
      Name: "Annually";
    };

export type PodcastSubscriptionBenefit =
  | {
      Id: 1;
      Name: "Non-Quota Listening";
    }
  | {
      Id: 2;
      Name: "Subscriber-Only Shows";
    }
  | {
      Id: 3;
      Name: "Subscriber-Only Episodes";
    }
  | {
      Id: 4;
      Name: "Bonus Episodes";
    }
  | {
      Id: 5;
      Name: "Shows/Episodes Early Access";
    }
  | {
      Id: 6;
      Name: "Archive Episodes Access";
    };

export type PodcastSubscriptionRegistration = {
  Id: string;
  AccountId: string;
  PodcastSubscriptionId: number;
  SubscriptionCycleType: {
    Id: number;
    Name: string;
  };
  CurrentVersion: number;
  Price: number;
  IsAcceptNewestVersionSwitch: boolean | null;
  IsIncomeTaken: boolean;
  LastPaidAt: string;
  CancelledAt: string | null;
  CreatedAt: string;
  UpdatedAt: string;
  PodcastSubscriptionBenefitList: {
    Id: number;
    Name: string;
  }[];
};

export type PodcastSubscriptionRegistrationFromAPI = {
  Id: string;
  AccountId: string;
  PodcastSubscriptionId: number;
  SubscriptionCycleType: {
    Id: number;
    Name: string;
  };
  Price: number;
  PodcastChannel: {
    Id: string;
    Name: string;
    MainFileKey: string;
  } | null;
  PodcastShow: {
    Id: string;
    Name: string;
    MainFileKey: string;
  } | null;
  CurrentVersion: number;
  IsAcceptNewestVersionSwitch: boolean | null;
  IsIncomeTaken: boolean;
  LastPaidAt: string;
  CancelledAt: string | null;
  CreatedAt: string;
  UpdatedAt: string;
};

export type PodcastSubscriptionRegistrationUI = {
  Id: string;
  AccountId: string;
  PodcastSubscriptionId: number;
  SubscriptionCycleType: {
    Id: number;
    Name: string;
  };
  Price: number;
  PodcastChannel: {
    Id: string;
    Name: string;
    ImageUrl: string;
  } | null;
  PodcastShow: {
    Id: string;
    Name: string;
    ImageUrl: string;
  } | null;
  CurrentVersion: number;
  IsAcceptNewestVersionSwitch: boolean | null;
  IsIncomeTaken: boolean;
  LastPaidAt: string;
  CancelledAt: string | null;
  CreatedAt: string;
  UpdatedAt: string;
};

export type PodcastSubscriptionRegistrationDetails = {
  Id: string;
  AccountId: number;
  PodcastSubscriptionId: number;
  SubscriptionCycleType: {
    Id: number;
    Name: string;
  };
  CurrentVersion: number;
  IsAcceptNewestVersionSwitch: boolean | null;
  IsIncomeTaken: boolean;
  LastPaidAt: string;
  CancelledAt: string | null;
  CreatedAt: string;
  UpdatedAt: string;
  PodcastSubscriptionBenefitList: {
    Id: number;
    Name: string;
  }[];
};
