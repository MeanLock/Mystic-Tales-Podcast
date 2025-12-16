import { Episode, EpisodeFromShow } from "@/src/core/types/episode.type";
import { createSlice } from "@reduxjs/toolkit";

type MergedEpisode = Episode | EpisodeFromShow;
type EpisodeState = {
  episodes: MergedEpisode[];
  title: string;
  from: "ShowDetails" | "Search" | "Feed" | "Saved";
};
const initialState: EpisodeState = {
  episodes: [],
  title: "",
  from: "ShowDetails"
};

export const episodeSlice = createSlice({
  name: "episode",
  initialState,
  reducers: {
    setEpisodes: (
      state: EpisodeState,
      action: { payload: MergedEpisode[] }
    ) => {
      state.episodes = action.payload;
    },
    setTitle: (state: EpisodeState, action: { payload: string }) => {
      state.title = action.payload;
    },
    setEpisodesData: (
      state: EpisodeState,
      action: { payload: { episodes: MergedEpisode[]; title: string; from: "ShowDetails" | "Search" | "Feed" | "Saved" } }
    ) => {
      state.episodes = action.payload.episodes;
      state.title = action.payload.title;
      state.from = action.payload.from;
    },
  },
});
export const { setEpisodes, setTitle, setEpisodesData } = episodeSlice.actions;
export default episodeSlice.reducer;
