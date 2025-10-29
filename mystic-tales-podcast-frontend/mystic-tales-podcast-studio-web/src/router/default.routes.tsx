import { lazy } from "react";

const Dashboard = lazy(() => import("../views/pages/dashboard"));
const AudioTuning = lazy(() => import("../views/pages/audio-tuning-page"));
const MyChannels = lazy(() => import("../views/pages/my-channel-page"));
const MyShows = lazy(() => import("../views/pages/my-show-page"));
const Profile = lazy(() => import("../views/pages/profile-page"));
const ChannelDashboard = lazy(() => import("../views/pages/my-channel-detail-page/dashboard-page"));
const ChannelOverview = lazy(() => import("../views/pages/my-channel-detail-page/overview-page"));
const ChannelShow = lazy(() => import("../views/pages/my-channel-detail-page/show-page"));
const ChannelSubscription = lazy(() => import("../views/pages/my-channel-detail-page/subscription-page"));

const ShowOverview = lazy(() => import("../views/pages/my-show-detail-page/overview-page"));
const ShowSubscription = lazy(() => import("../views/pages/my-show-detail-page/subscription-page"));
const ShowEpisode = lazy(() => import("../views/pages/my-show-detail-page/episode-page"));

const EpisodeDetail = lazy(() => import("../views/pages/my-show-detail-page/episode-detail-page"));


const hls = lazy(() => import("../views/pages/hls"));

const routes = [
  { path: '/dashboard', exact: true, name: 'Dashboard', element: Dashboard },
  { path: '/audio-tuning', exact: true, name: 'Audio Tuning', element: AudioTuning },
  { path: '/earn', exact: true, name: 'Earn', element: () => <div>Earn Page</div> },
  { path: '/copyright', exact: true, name: 'Copyright', element: () => <div>Copyright Page</div> },
  { path: '/channel', exact: true, name: 'My Channels', element: MyChannels },
  { path: '/show', exact: true, name: 'My Shows', element: MyShows },
  { path: '/profile', exact: true, name: 'Profile', element: Profile },

  { path: '/hls', exact: true, name: 'HLS', element: hls },

  { path: '/channel/:id/dashboard', exact: true, name: 'Channel Dashboard', element: ChannelDashboard },
  { path: '/channel/:id/overview', exact: true, name: 'Channel Overview', element: ChannelOverview },
  { path: '/channel/:id/earn', exact: true, name: 'Channel Earn', element: () => <div>Earn Page</div> },
  { path: '/channel/:id/show', exact: true, name: 'Channel Show', element: ChannelShow },
  { path: '/channel/:id/subscription', exact: true, name: 'Channel Subscription', element: ChannelSubscription },

  { path: '/show/:id/dashboard', exact: true, name: 'Show Dashboard', element: ChannelDashboard },
  { path: '/show/:id/overview', exact: true, name: 'Show Overview', element: ShowOverview },
  { path: '/show/:id/earn', exact: true, name: 'Show Earn', element: () => <div>Earn Page</div> },
  { path: '/show/:id/show', exact: true, name: 'Show Show', element: ChannelShow },
  { path: '/show/:id/subscription', exact: true, name: 'Show Subscription', element: ShowSubscription },
  { path: '/show/:id/episode', exact: true, name: 'Show Episodes', element: ShowEpisode },
  { path: '/show/:id/episode/:episodeId', exact: true, name: 'Episode Detail', element: EpisodeDetail },

  { path: '/booking-management', exact: true, name: 'Booking Management', element: () => <div>Booking Management Page</div> },
]

export default routes
