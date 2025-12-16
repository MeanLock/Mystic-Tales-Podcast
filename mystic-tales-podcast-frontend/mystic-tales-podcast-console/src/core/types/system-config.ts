export type SystemConfigList = {
    Id: number;
    Name: string;
    IsActive: boolean;
    DeletedAt: string;
    CreatedAt: string;
    UpdatedAt: string;
};

export type SystemConfig = {
    Id: number;
    Name: string;
    IsActive: boolean;
    CreatedAt: string;
    UpdatedAt: string;
    PodcastSubsriptionConfigList: PodcastSubsriptionConfig[];
    PodcastSuggestionConfig: PodcastSuggestionConfig;
    BookingConfig: BookingConfig;
    AccountConfig: AccountConfig;
    AccountViolationLevelConfigList: AccountViolationLevelConfig[];
    ReviewSessionConfig: ReviewSessionConfig;
}

export type PodcastSubsriptionConfig = {
    ConfigProfileId: number;
    SubscriptionCycleType: SubscriptionCycleType;
    ProfitRate: number;
    IncomeTakenDelayDays: number;
    CreatedAt: string;
    UpdatedAt: string;
}
export type SubscriptionCycleType = {
    Id: number;
    Name: string;
}
export type PodcastSuggestionConfig = {
    ConfigProfileId: number;
    BehaviorLookbackDayCount: number;
    MinChannelQuery: number;
    MinShowQuery: number;
    CreatedAt: string;
    UpdatedAt: string;
}

export type BookingConfig = {
    ConfigProfileId: number;
    ProfitRate: number;
    DepositRate: number;
    PodcastTrackPreviewListenSlot: number;
    PreviewResponseAllowedDays: number;
    ProducingRequestResponseAllowedDays: number;
    ChatRoomExpiredHours: number;
    ChatRoomFileMessageExpiredHours: number;
    FreeInitalBookingStorageSize: number;
    SingleStorageUnitPurchasePrice: number;
    CreatedAt: string;
    UpdatedAt: string;
}

export type AccountConfig = {
    ConfigProfileId: number;
    ViolationPointDecayHours: number;
    PodcastListenSlotThreshold: number;
    PodcastListenSlotRecoverySeconds: number;
    CreatedAt: string;
    UpdatedAt: string;
}

export type AccountViolationLevelConfig = {
    ConfigProfileId: number;
    ViolationLevel: number;
    ViolationPointThreshold: number;
    PunishmentDays: number;
    CreatedAt: string;
    UpdatedAt: string;
}

export type ReviewSessionConfig = {
    ConfigProfileId: number;
    PodcastBuddyUnResolvedReportStreak: number;
    PodcastShowUnResolvedReportStreak: number;
    PodcastEpisodeUnResolvedReportStreak: number;
    PodcastEpisodePublishEditRequirementExpiredHours: number;
    CreatedAt: string;
    UpdatedAt: string;
}
