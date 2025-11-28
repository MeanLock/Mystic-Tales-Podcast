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
const Background_Sound_View = React.lazy(() => import('./views/role-views/admin/background-sound-view/index'))

const Admin_Episode_Publish_Review_View = React.lazy(() => import('./views/role-views/admin/episode-view/episode-publish-review-view/index'))
const Admin_Episode_Report_View = React.lazy(() => import('./views/role-views/admin/episode-view/episode-report-view/index'))
const Admin_Episode_Report_Review_View = React.lazy(() => import('./views/role-views/admin/episode-view/episode-report-review-view/index'))

const Admin_Show_Report_View = React.lazy(() => import('./views/role-views/admin/show-view/show-report-view/index'))
const Admin_Show_Report_Review_View = React.lazy(() => import('./views/role-views/admin/show-view/show-report-review-view/index'))

const Admin_Buddy_Report_View = React.lazy(() => import('./views/role-views/admin/buddy-view/buddy-report-view/index'))
const Admin_Buddy_Report_Review_View = React.lazy(() => import('./views/role-views/admin/buddy-view/buddy-report-review-view/index'))


//staff
const Podcaster_Staff_View = React.lazy(() => import('./views/role-views/staff/podcaster-view/index'))
const Staff_Show_Report_Review_View = React.lazy(() => import('./views/role-views/staff/show-report-review-view/index'))
const Staff_Episode_Report_Review_View = React.lazy(() => import('./views/role-views/staff/episode-report-review-view/index'))
const Staff_Buddy_Report_Review_View = React.lazy(() => import('./views/role-views/staff/buddy-report-review-view/index'))
const Staff_DMCA_Accusation_View = React.lazy(() => import('./views/role-views/staff/dmca-accusation-view/index'))
const Staff_DMCA_Accusation_Detail_View = React.lazy(() => import('./views/role-views/staff/dmca-accusation-detail-view/index'))
const Staff_Episode_Publish_Review_View = React.lazy(() => import('./views/role-views/staff/episode-publish-review-view/index'))


const Community_Survey_View = React.lazy(() => import('./views/role-views/manager/community-survey-view/index'))
const Community_Survey_Detail_View = React.lazy(() => import('./views/role-views/manager/community-survey-detail-view/index'))
const Data_Market_View = React.lazy(() => import('./views/role-views/manager/data-market-view/index'))
const Data_Market_Detail_View = React.lazy(() => import('./views/role-views/manager/data-market-detail-view/index'))
const Filter_Survey_View = React.lazy(() => import('./views/role-views/manager/filter-survey-view/index'))
const Filter_Survey_Detail_View = React.lazy(() => import('./views/role-views/manager/filter-survey-detail-view/index'))





// ==================== Routes ====================

const routes = [
  { path: '/', exact: true, name: 'Home' },
  { path: '/dashboard', name: 'Dashboard', element: Dashboard, role_id: [1] },
  { path: '/customer/table', name: 'Customers', element: Customer_View, role_id: [1] },
  { path: '/staff/table', name: 'Staffs', element: Staff_View, role_id: [1] },
  { path: '/podcaster/table', name: 'Podcasters', element: Podcaster_View, role_id: [1] },
  { path: '/dmca-accusation/table', name: 'DMCA Accusation', element: DMCA_Accusation_View, role_id: [1] },
  { path: '/dmca-accusation/detail/:id', name: 'Detail', element: DMCA_Accusation_Detail_View, role_id: [1], parent: '/dmca-accusation/table' },
  { path: '/system-configuration', name: 'System Configuration', element: SystemConfigView, role_id: [1] },
  { path: '/transactions/table', name: 'Transactions', element: Transaction_View, role_id: [1] },
  { path: '/background-sound/table', name: 'Background Sound', element: Background_Sound_View, role_id: [1] },
  { path: '/episode/publish-review-sessions', name: 'Episode', element: Admin_Episode_Publish_Review_View, role_id: [1] },
  { path: '/episode/report', name: 'Episode', element: Admin_Episode_Report_View, role_id: [1] },
  { path: '/episode/report-review-sessions', name: 'Episode', element: Admin_Episode_Report_Review_View, role_id: [1] },

  { path: '/show/report', name: 'Show', element: Admin_Show_Report_View, role_id: [1] },
  { path: '/show/report-review-sessions', name: 'Show', element: Admin_Show_Report_Review_View, role_id: [1] },

  { path: '/buddy/report', name: 'Buddy', element: Admin_Buddy_Report_View, role_id: [1] },
  { path: '/buddy/report-review-sessions', name: 'Buddy', element: Admin_Buddy_Report_Review_View, role_id: [1] },
//staff
  { path: '/staff/podcaster/table', name: 'Podcasters', element: Podcaster_Staff_View, role_id: [2] },
  { path: '/report/show', name: 'Show Report', element: Staff_Show_Report_Review_View, role_id: [2] },
  { path: '/report/episode', name: 'Episode Report', element: Staff_Episode_Report_Review_View, role_id: [2] },
  { path: '/report/buddy', name: 'Buddy Report', element: Staff_Buddy_Report_Review_View, role_id: [2] },
  { path: '/staff/dmca-accusation/table', name: 'DMCA Accusation', element: Staff_DMCA_Accusation_View, role_id: [2] },
  { path: '/staff/dmca-accusation/detail/:id', name: 'Detail', element: Staff_DMCA_Accusation_Detail_View, role_id: [2], parent: '/staff/dmca-accusation/table' },
  { path: '/staff/episode/publish-review-sessions', name: 'Episode', element: Staff_Episode_Publish_Review_View, role_id: [2] },

  { path: '/community-survey', name: 'Khảo Sát', element: Community_Survey_View, role_id: [2] },
  { path: '/community-survey/detail/:id', name: 'Chi Tiết', element: Community_Survey_Detail_View, role_id: [2], parent: '/community-survey' },
  { path: '/data-market', name: 'Data Market', element: Data_Market_View, role_id: [2] },
  { path: '/data-market/detail/:id', name: 'Chi Tiết', element: Data_Market_Detail_View, role_id: [2], parent: '/data-market' },
  { path: '/filter-survey/table', name: 'Khảo Sát Đầu Vào', element: Filter_Survey_View, role_id: [2] },
  { path: '/filter-survey/detail/:id', name: 'Chi Tiết', element: Filter_Survey_Detail_View, role_id: [2], parent: '/filter-survey/table' },


]

export default routes
