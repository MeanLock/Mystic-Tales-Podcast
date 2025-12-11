import type { EpisodeFromAPI } from "@/core/types/episode";
import { createSlice, type PayloadAction } from "@reduxjs/toolkit";

interface SeeMoreEpisodeSlice {
  title: string;
  episodes: EpisodeFromAPI[];
}

const initialState: SeeMoreEpisodeSlice = {
  title: "",
  episodes: [],
};

const seeMoreEpisodeSlice = createSlice({
  initialState: initialState,
  name: "seeMoreEpisode",
  reducers: {
    setSeeMoreEpisodeData: (state, action: PayloadAction<{ title: string; episodes: EpisodeFromAPI[] }>) => {
      state.title = action.payload.title;
      state.episodes = action.payload.episodes;
    },
  },
});

export const { setSeeMoreEpisodeData } = seeMoreEpisodeSlice.actions;
export default seeMoreEpisodeSlice.reducer;
