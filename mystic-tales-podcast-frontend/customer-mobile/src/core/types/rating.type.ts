export type RatingType = {
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
};
