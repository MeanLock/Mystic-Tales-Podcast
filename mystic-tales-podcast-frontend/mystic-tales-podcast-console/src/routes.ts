import React from 'react'

//Admin
const Dashboard = React.lazy(() => import('./views/role-views/admin/dashboard-view/index'))
const Customer_View = React.lazy(() => import('./views/role-views/admin/customer-view/index'))
const Podcaster_View = React.lazy(() => import('./views/role-views/admin/podcaster-view/index'))
const Staff_View = React.lazy(() => import('./views/role-views/admin/staff-view/index'))
const DMCA_Accusation_View = React.lazy(() => import('./views/role-views/admin/dmca-accusation-view/index'))
const DMCA_Accusation_Detail_View = React.lazy(() => import('./views/role-views/admin/dmca-accusation-detail-view/index'))
const SystemConfigView = React.lazy(() => import('./views/role-views/admin/system-config-view/index'))
const Transaction_View = React.lazy(() => import('./views/role-views/admin/transaction-view/index'))
const Booking_Holding_View = React.lazy(() => import('./views/role-views/admin/booking-holding-view/index'))
const Subscription_Holding_View = React.lazy(() => import('./views/role-views/admin/subscription-holding-view/index'))
const Background_Sound_View = React.lazy(() => import('./views/role-views/admin/background-sound-view/index'))

const Admin_Episode_Publish_Review_View = React.lazy(() => import('./views/role-views/admin/episode-view/episode-publish-review-view/index'))
const Admin_Episode_Publish_Review_Detail_View = React.lazy(() => import('./views/role-views/admin/episode-view/episode-publish-review-detail-view/index'))
const Admin_Episode_Report_View = React.lazy(() => import('./views/role-views/admin/episode-view/episode-report-view/index'))
const Admin_Episode_Report_Review_View = React.lazy(() => import('./views/role-views/admin/episode-view/episode-report-review-view/index'))

const Admin_Show_Report_View = React.lazy(() => import('./views/role-views/admin/show-view/show-report-view/index'))
const Admin_Show_Report_Review_View = React.lazy(() => import('./views/role-views/admin/show-view/show-report-review-view/index'))

const Admin_Buddy_Report_View = React.lazy(() => import('./views/role-views/admin/buddy-view/buddy-report-view/index'))
const Admin_Buddy_Report_Review_View = React.lazy(() => import('./views/role-views/admin/buddy-view/buddy-report-review-view/index'))

const Booking_View = React.lazy(() => import('./views/role-views/admin/booking-view/index'))
const BookingDetail_View = React.lazy(() => import('./views/role-views/admin/booking-detail-view/index'))

const Show_Management_View = React.lazy(() => import('./views/role-views/admin/show-view/show-management-view/index'))
const Show_Detail_View = React.lazy(() => import('./views/role-views/admin/show-view/show-detail-view/index'))
//staff
const Staff_Show_Report_Review_View = React.lazy(() => import('./views/role-views/staff/show-report-review-view/index'))
const Staff_Episode_Report_Review_View = React.lazy(() => import('./views/role-views/staff/episode-report-review-view/index'))
const Staff_Buddy_Report_Review_View = React.lazy(() => import('./views/role-views/staff/buddy-report-review-view/index'))
const Staff_DMCA_Accusation_View = React.lazy(() => import('./views/role-views/staff/dmca-accusation-view/index'))
const Staff_DMCA_Accusation_Detail_View = React.lazy(() => import('./views/role-views/staff/dmca-accusation-detail-view/index'))
const Staff_Episode_Publish_Review_View = React.lazy(() => import('./views/role-views/staff/episode-publish-review-view/index'))
const BookingView = React.lazy(() => import('./views/role-views/staff/booking-view/index'))
const BookingDetailView = React.lazy(() => import('./views/role-views/staff/booking-detail-view/index'))
const Staff_Episode_Publish_Review_Detail_View = React.lazy(() => import('./views/role-views/staff/episode-publish-review-detail-view/index'))

const Episode_Detail_View = React.lazy(() => import('./views/role-views/admin/episode-view/episode-detail-view/index'))
const Channel_View = React.lazy(() => import('./views/role-views/admin/channel-view/channel-management-view/index'))
const Channel_Detail_View = React.lazy(() => import('./views/role-views/admin/channel-view/channel-detail-view/index'))


// ==================== Routes ====================

const routes = [
  { path: '/', exact: true, name: 'Home' },
  { path: '/dashboard', name: 'Dashboard', element: Dashboard, role_id: [3] },
  { path: '/customer/table', name: 'Customers', element: Customer_View, role_id: [3] },
  { path: '/staff/table', name: 'Staffs', element: Staff_View, role_id: [3] },
  { path: '/podcaster/table', name: 'Podcasters', element: Podcaster_View, role_id: [3] },
  { path: '/dmca-accusation/table', name: 'DMCA Accusation', element: DMCA_Accusation_View, role_id: [3] },
  { path: '/dmca-accusation/:id/:type', name: 'Detail', element: DMCA_Accusation_Detail_View, role_id: [3], parent: '/dmca-accusation/table' },
  { path: '/system-configuration', name: 'System Configuration', element: SystemConfigView, role_id: [3] },
  { path: '/transactions/withdrawal', name: 'Transactions', element: Transaction_View, role_id: [3] },
  { path: '/transactions/bookings/holding', name: 'Transactions', element: Booking_Holding_View, role_id: [3] },
    { path: '/transactions/subscriptions/holding', name: 'Transactions', element: Subscription_Holding_View, role_id: [3] },

  { path: '/background-sound/table', name: 'Background Sound', element: Background_Sound_View, role_id: [3] },
  { path: '/episode/publish-review-sessions', name: 'Publish Review', element: Admin_Episode_Publish_Review_View, role_id: [3] },
  { path: '/episode/publish-review-sessions/:id', name: 'Publish Review Detail', element: Admin_Episode_Publish_Review_Detail_View, role_id: [3], parent: '/episode/publish-review-sessions' },
  { path: '/episode/report', name: 'Episode', element: Admin_Episode_Report_View, role_id: [3] },
  { path: '/episode/report-review-sessions', name: 'Episode', element: Admin_Episode_Report_Review_View, role_id: [3] },

  { path: '/show/report', name: 'Show', element: Admin_Show_Report_View, role_id: [3] },
  { path: '/show/report-review-sessions', name: 'Show', element: Admin_Show_Report_Review_View, role_id: [3] },

  { path: '/buddy/report', name: 'Buddy', element: Admin_Buddy_Report_View, role_id: [3] },
  { path: '/buddy/report-review-sessions', name: 'Buddy', element: Admin_Buddy_Report_Review_View, role_id: [3] },
//staff
  { path: '/report/show', name: 'Show Report', element: Staff_Show_Report_Review_View, role_id: [2] },
  { path: '/report/episode', name: 'Episode Report', element: Staff_Episode_Report_Review_View, role_id: [2] },
  { path: '/report/buddy', name: 'Buddy Report', element: Staff_Buddy_Report_Review_View, role_id: [2] },
  { path: '/staff/dmca-accusation/table', name: 'DMCA Accusation', element: Staff_DMCA_Accusation_View, role_id: [2] },
  { path: '/staff/dmca-accusation/:id/:type', name: 'Detail', element: Staff_DMCA_Accusation_Detail_View, role_id: [2], parent: '/staff/dmca-accusation/table' },
  { path: '/staff/publish-review-sessions', name: 'Publish Review', element: Staff_Episode_Publish_Review_View, role_id: [2] },
  { path: '/staff/booking/table', name: 'Booking', element: BookingView, role_id: [2] },
  { path: '/staff/booking/:id', name: 'Booking Detail', element: BookingDetailView, role_id: [2], parent: '/staff/booking/table' },
  { path: '/staff/publish-review-sessions/:id', name: 'Publish Review Detail', element: Staff_Episode_Publish_Review_Detail_View, role_id: [2], parent: '/staff/publish-review-sessions' },

{ path: '/booking/table', name: 'Booking', element: Booking_View, role_id: [3] },
  { path: '/booking/:id', name: 'Booking Detail', element: BookingDetail_View, role_id: [3], parent: '/booking/table' },

  { path: '/show/table', name: 'Show', element: Show_Management_View, role_id: [2,3] },
  { path: '/show/:id', name: 'Show Detail', element: Show_Detail_View, role_id: [2,3], parent: '/show/table' },
  { path: '/episode/:id', name: 'Episode Detail', element: Episode_Detail_View, role_id: [2,3], parent: '/episode/table' },
  { path: '/channel/table', name: 'Channel', element: Channel_View, role_id: [2,3] },
  { path: '/channel/:id', name: 'Channel Detail', element: Channel_Detail_View, role_id: [2,3], parent: '/channel/table' },
]

export default routes
