import { Show, ShowFromChannel } from "@/src/core/types/show.type";
import { createSlice } from "@reduxjs/toolkit";

type MergedShow = Show | ShowFromChannel;
type ShowState = {
  shows: MergedShow[];
  title: string;
  from: "ChannelDetails" | "Search" | "Feed" | "Saved";
};
const initialState: ShowState = {
  shows: [],
  title: "",
  from: "ChannelDetails",
};

export const showSlice = createSlice({
  name: "show",
  initialState,
  reducers: {
    setShows: (state: ShowState, action: { payload: MergedShow[] }) => {
      state.shows = action.payload;
    },
    setTitle: (state: ShowState, action: { payload: string }) => {
      state.title = action.payload;
    },
    setShowsData: (
      state: ShowState,
      action: {
        payload: {
          shows: MergedShow[];
          title: string;
          from: "ChannelDetails" | "Search" | "Feed" | "Saved";
        };
      }
    ) => {
      state.shows = action.payload.shows;
      state.title = action.payload.title;
      state.from = action.payload.from;
    },
  },
});
export const { setShows, setTitle, setShowsData } = showSlice.actions;
export default showSlice.reducer;
