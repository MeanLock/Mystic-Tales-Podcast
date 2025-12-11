import { Channel } from "@/src/core/types/channel.type";
import { createSlice } from "@reduxjs/toolkit";

type MergedChannel = Channel ;
type ChannelState = {
  channels: MergedChannel[];
  title: string;
  from: "Search" | "Feed" | "Saved";
};
const initialState: ChannelState = {
  channels: [],
  title: "",
  from: "Feed"
};

export const channelSlice = createSlice({
  name: "channel",
  initialState,
  reducers: {
    setChannels: (
      state: ChannelState,
      action: { payload: MergedChannel[] }
    ) => {
      state.channels = action.payload;
    },
    setTitle: (state: ChannelState, action: { payload: string }) => {
      state.title = action.payload;
    },
    setChannelsData: (
      state: ChannelState,
      action: { payload: { channels: MergedChannel[]; title: string; from: "Search" | "Feed" | "Saved" } }
    ) => {
      state.channels = action.payload.channels;
      state.title = action.payload.title;
      state.from = action.payload.from;
    },
  },
});
export const { setChannels, setTitle, setChannelsData } = channelSlice.actions;
export default channelSlice.reducer;
