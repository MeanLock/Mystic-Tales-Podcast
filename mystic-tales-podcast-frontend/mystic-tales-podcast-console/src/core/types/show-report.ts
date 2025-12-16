export type PodcastShow = {
  Id: string;
  Name: string;
};

export type AssignedStaff = {
  Id: number;
  FullName: string;
  Email: string;
  MainImageFileKey: string;
};

export type PodcastShowReportType = {
  Id: number;
  Name: string;
};

export type ShowReport = {
  Id: string;
  Content: string;
  AccountId: number;
  PodcastShow: PodcastShow;
  PodcastShowReportType: PodcastShowReportType;
  ResolvedAt: string;
  CreatedAt: string;
};

export type ShowReportReviewSession = {
  Id: string;
  PodcastShow: PodcastShow;
  AssignedStaff: AssignedStaff;
  IsResolved: boolean;
  CreatedAt: string;
  UpdatedAt: string;
  ShowReportList: ShowReport[];
};
