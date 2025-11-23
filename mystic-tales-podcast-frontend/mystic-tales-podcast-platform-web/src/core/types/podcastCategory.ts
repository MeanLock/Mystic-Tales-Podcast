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

export type PodcastCategoryWithImageFromAPI = {
  Id: number;
  Name: string;
  MainImageFileKey: string;
  PodcastSubCategoryList: {
    Id: number;
    Name: string;
    PodcastCategoryId: number;
  }[];
};

export type PodcastCategoryWithImageUI = {
  Id: number;
  Name: string;
  ImageUrl: string;
  PodcastSubCategoryList: {
    Id: number;
    Name: string;
    PodcastCategoryId: number;
  }[];
};
