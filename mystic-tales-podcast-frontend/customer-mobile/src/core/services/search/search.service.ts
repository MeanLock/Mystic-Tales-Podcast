import type {
  ContentRealtimeResponse,
  SearchResultResponse,
} from "@/src/core/types/search.type";
import { appApi } from "../../api/appApi";

const searchApi = appApi.injectEndpoints({
  endpoints: (build) => ({
    getAutocompleteWordRealTime: build.query<string[], { keyword: string }>({
      query: ({ keyword }) => ({
        url: `/api/podcast-service/api/misc/feed/podcast-contents/search-keyword-suggestion?keyword=${keyword}&limit=3`,
        method: "GET",
        authMode: "public",
      }),
    }),
    getPodcastContentOnKeywordRealTime: build.query<
      { SearchItemList: ContentRealtimeResponse[] },
      { keyword: string }
    >({
      query: ({ keyword }) => ({
        url: `/api/podcast-service/api/misc/feed/podcast-contents?keyword=${keyword}&limit=8`,
        method: "GET",
        authMode: "public",
      }),
    }),
    getSearchResults: build.query<SearchResultResponse, { keyword: string }>({
      query: ({ keyword }) => ({
        url: `/api/podcast-service/api/misc/feed/podcast-contents/keyword-search?keyword=${keyword}`,
        method: "GET",
        authMode: "public",
      }),
    }),
  }),
});

export const {
  useGetAutocompleteWordRealTimeQuery,
  useLazyGetAutocompleteWordRealTimeQuery,
  useGetPodcastContentOnKeywordRealTimeQuery,
  useLazyGetPodcastContentOnKeywordRealTimeQuery,
  useGetSearchResultsQuery,

} = searchApi;
