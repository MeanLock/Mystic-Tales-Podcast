export type PodcastSubCategory = {
  Id: number;
  Name: string;
  PodcastCategoryId: number;
  PodcastCategory: PodcastCategory;
};

export type PodcastCategory = {
  Id: number;
  Name: string;
};
