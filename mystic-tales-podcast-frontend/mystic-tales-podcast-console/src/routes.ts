import React from 'react'

//Admin
const Dashboard = React.lazy(() => import('./views/role-views/admin/dashboard-view/index'))
const Customer_View = React.lazy(() => import('./views/role-views/admin/customer-view/index'))
const Manager_View = React.lazy(() => import('./views/role-views/admin/manager-view/index'))
const Level_View = React.lazy(() => import('./views/role-views/admin/level-view/index'))
const PlatformFeedback_View = React.lazy(() => import('./views/role-views/admin/platform-feedback-view/index'))

//Manager
const Community_Survey_View = React.lazy(() => import('./views/role-views/manager/community-survey-view/index'))
const Community_Survey_Detail_View = React.lazy(() => import('./views/role-views/manager/community-survey-detail-view/index'))
const Data_Market_View = React.lazy(() => import('./views/role-views/manager/data-market-view/index'))
const Data_Market_Detail_View = React.lazy(() => import('./views/role-views/manager/data-market-detail-view/index'))
const Filter_Survey_View = React.lazy(() => import('./views/role-views/manager/filter-survey-view/index'))
const Filter_Survey_Detail_View = React.lazy(() => import('./views/role-views/manager/filter-survey-detail-view/index'))
const Transaction_View = React.lazy(() => import('./views/role-views/manager/transaction-view/index'))





// ==================== Routes ====================

const routes = [
  { path: '/', exact: true, name: 'Home' },
  { path: '/dashboard', name: 'Dashboard', element: Dashboard, role_id: [1] },
  { path: '/managers/table', name: 'Người Quản Lý', element: Manager_View, role_id: [1] },
  { path: '/customers/table', name: 'Người Dùng', element: Customer_View, role_id: [1] },
  { path: '/level-management', name: 'Cấp Độ', element: Level_View, role_id: [1] },
  { path: '/platform-feedback/table', name: 'Đánh Giá', element: PlatformFeedback_View, role_id: [1] },

  { path: '/community-survey', name: 'Khảo Sát', element: Community_Survey_View, role_id: [2] },
  { path: '/community-survey/detail/:id', name: 'Chi Tiết', element: Community_Survey_Detail_View, role_id: [2], parent: '/community-survey' },
  { path: '/data-market', name: 'Data Market', element: Data_Market_View, role_id: [2] },
  { path: '/data-market/detail/:id', name: 'Chi Tiết', element: Data_Market_Detail_View, role_id: [2], parent: '/data-market' },
  { path: '/filter-survey/table', name: 'Khảo Sát Đầu Vào', element: Filter_Survey_View, role_id: [2] },
  { path: '/filter-survey/detail/:id', name: 'Chi Tiết', element: Filter_Survey_Detail_View, role_id: [2], parent: '/filter-survey/table' },
  { path: '/transactions/table', name: 'Giao Dịch', element: Transaction_View, role_id: [2] },


]

export default routes
