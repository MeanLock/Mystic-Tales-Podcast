export type PodcastSubCategory = {
  Id: number;
  Name: string;
  PodcastCategoryId: number;
}
export type PodcastCategory = {
  Id: number;
  Name: string;
  PodcastSubCategoryList: PodcastSubCategory[];
}