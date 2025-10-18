import { lazy } from "react";

const Dashboard = lazy(() => import("../views/pages/dashboard"));
const MyChannels = lazy(() => import("../views/pages/my-channel-page"));



const routes = [
  { path: '/dashboard', exact: true, name: 'Dashboard', element: Dashboard },
  { path: '/earn', exact: true, name: 'Earn', element: () => <div>Earn Page</div> },
  { path: '/copyright', exact: true, name: 'Copyright', element: () => <div>Copyright Page</div> },
  { path: '/my-channels', exact: true, name: 'My Channels', element: MyChannels },
  { path: '/my-shows', exact: true, name: 'My Shows', element: () => <div>My Shows Page</div> },
  { path: '/booking-management', exact: true, name: 'Booking Management', element: () => <div>Booking Management Page</div> },
]

export default routes
