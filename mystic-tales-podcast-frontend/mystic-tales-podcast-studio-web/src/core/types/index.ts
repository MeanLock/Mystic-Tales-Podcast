export interface HashtagOption {
  id: number;
  name: string;
}

export interface LicenseFile {
  Id: string;
  PodcastEpisodeId: string;
  LicenseDocumentFileKey: string;
  PodcastEpisodeLicenseType: LicenseType;
  CreatedAt: string;
}

export interface LicenseType {
  Id: number;
  Name: string;
}
export interface AudioTuningRequest {
  GeneralTuningProfileRequestInfo: {
    EqualizerProfile: {
      ExpandEqualizer: { Mood: string };
      BaseEqualizer: {
        HighMid: number;
        Mid: number;
        Air: number;
        LowMid: number;
        Treble: number;
        Low: number;
        Presence: number;
        SubBass: number;
        Bass: number;
      };
    };
    BackgroundMergeProfile: null;
    MultipleTimeRangeBackgroundMergeProfile: {
      TimeRangeMergeBackgrounds: BackgroundMergeProfile[] | null;
    } | null;
    AITuningProfile: null
  };
  AudioFile?: File;
}
export type BackgroundMergeProfile = {
  VolumeGainDb: number;
  BackgroundSoundTrackFileKey: string;
  BackgroundCutStartSecond: number;
  BackgroundCutEndSecond: number;
  OriginalMergeStartSecond: number;
  OriginalMergeEndSecond: number;
}

export type BackgroundSound = {
  Id: string;
  Name: string;
  Description: string;
  MainImageFileKey: string;
  AudioFileKey: string;
}
export type BookingTone = {
  Id: string;
  Name: string;
  Description: string;
  PodcastBookingToneCategory: {
    Id: number;
    Name: string;
  };

};
