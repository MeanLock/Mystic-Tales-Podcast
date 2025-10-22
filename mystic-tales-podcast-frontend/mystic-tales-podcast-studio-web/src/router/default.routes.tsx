import { lazy } from "react";

const Dashboard = lazy(() => import("../views/pages/dashboard"));
const MyChannels = lazy(() => import("../views/pages/my-channel-page"));

const ChannelDashboard = lazy(() => import("../views/pages/my-channel-detail-page/dashboard-page"));
const ChannelOverview = lazy(() => import("../views/pages/my-channel-detail-page/overview-page"));
const ChannelShow = lazy(() => import("../views/pages/my-channel-detail-page/show-page"));
const routes = [
  { path: '/dashboard', exact: true, name: 'Dashboard', element: Dashboard },
  { path: '/earn', exact: true, name: 'Earn', element: () => <div>Earn Page</div> },
  { path: '/copyright', exact: true, name: 'Copyright', element: () => <div>Copyright Page</div> },
  { path: '/my-channels', exact: true, name: 'My Channels', element: MyChannels },

  { path: '/my-channel/:id/dashboard', exact: true, name: 'Channel Dashboard', element: ChannelDashboard },
  { path: '/my-channel/:id/overview', exact: true, name: 'Channel Overview', element: ChannelOverview },
  { path: '/my-channel/:id/earn', exact: true, name: 'Channel Earn',  element: () => <div>Earn Page</div> },
  { path: '/my-channel/:id/show', exact: true, name: 'Channel Show', element: ChannelShow },
  
  { path: '/my-shows', exact: true, name: 'My Shows', element: () => <div>My Shows Page</div> },
  { path: '/booking-management', exact: true, name: 'Booking Management', element: () => <div>Booking Management Page</div> },
]

export default routes
