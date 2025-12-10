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
<<<<<<< HEAD
  from: "ChannelDetails",
=======
  from: "ChannelDetails"
>>>>>>> d24514da2d90e35a15e7a4d1b3dcfeff93364c25
};

export const showSlice = createSlice({
  name: "show",
  initialState,
  reducers: {
<<<<<<< HEAD
    setShows: (state: ShowState, action: { payload: MergedShow[] }) => {
=======
    setShows: (
      state: ShowState,
      action: { payload: MergedShow[] }
    ) => {
>>>>>>> d24514da2d90e35a15e7a4d1b3dcfeff93364c25
      state.shows = action.payload;
    },
    setTitle: (state: ShowState, action: { payload: string }) => {
      state.title = action.payload;
    },
    setShowsData: (
      state: ShowState,
<<<<<<< HEAD
      action: {
        payload: {
          shows: MergedShow[];
          title: string;
          from: "ChannelDetails" | "Search" | "Feed" | "Saved";
        };
      }
=======
      action: { payload: { shows: MergedShow[]; title: string; from: "ChannelDetails" | "Search" | "Feed" | "Saved" } }
>>>>>>> d24514da2d90e35a15e7a4d1b3dcfeff93364c25
    ) => {
      state.shows = action.payload.shows;
      state.title = action.payload.title;
      state.from = action.payload.from;
    },
  },
});
export const { setShows, setTitle, setShowsData } = showSlice.actions;
export default showSlice.reducer;
