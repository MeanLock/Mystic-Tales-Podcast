import React from 'react'
import CIcon from '@coreui/icons-react'
import {
  cilBell,
  cilCalculator,
  cilChartPie,
  cilCursor,
  cilDescription,
  cilDrop,
  cilNotes,
  cilPencil,
  cilPuzzle,
  cilSpeedometer,
  cilStar,
} from '@coreui/icons'
import { CNavGroup, CNavTitle, CNavItem } from '@coreui/react'
import StorefrontIcon from '@mui/icons-material/Storefront';
import PaidOutlinedIcon from '@mui/icons-material/PaidOutlined';
import PollOutlinedIcon from '@mui/icons-material/PollOutlined';
import DescriptionOutlinedIcon from '@mui/icons-material/DescriptionOutlined';
import SettingsOutlinedIcon from '@mui/icons-material/SettingsOutlined';
import InsertCommentOutlinedIcon from '@mui/icons-material/InsertCommentOutlined';

import {
  HardDrives,
  MagicWand,
  Gauge,
  Users,
  ShoppingCartSimple,
  FileText,
  IdentificationCard,
  Cube,
  ClipboardText,
  ShoppingBagOpen,
  ArrowCircleRight
} from "phosphor-react";
import { GiDoorRingHandle, GiBigDiamondRing, GiGemPendant, GiCheckeredDiamond, GiMetalBar } from "react-icons/gi";
import { FcOnlineSupport, FcFactoryBreakdown } from "react-icons/fc";



const get_roleNav = (role_id: number, account_id: number) => {
  const _roleNav = [
    [],
    // admin: 1   
    [
      {
        component: CNavItem,
        name: 'Dashboard',
        to: '/dashboard',
        icon: <Gauge size={30} weight="duotone" />,
      },
      {
        component: CNavItem,
        name: 'Người Dùng',
        to: '/customers/table',
        icon: <Users size={30} weight="duotone" />,

      },
      {
        component: CNavItem,
        name: 'Người Quản Lý',
        to: '/managers/table',
        icon: <Users size={30} weight="duotone" />,

      },
      {
        component: CNavItem,
        name: 'Đánh Giá ',
        to: '/platform-feedback/table',
        icon: <InsertCommentOutlinedIcon sx={{ fontSize: 29 }} />,

      },
      {
        component: CNavItem,
        name: 'Cấp độ',
        to: '/level-management',
        icon: <PollOutlinedIcon sx={{ fontSize: 29 }} />,

      },

      {
        component: CNavItem,
        name: 'Cài đặt',
        to: '/staffs_manager/table11',
        icon: <SettingsOutlinedIcon sx={{ fontSize: 29 }} />,

      }

    ],
    // manager ID: 2
    [
      {
        component: CNavItem,
        name: 'Khảo Sát',
        to: '/community-survey',
        icon: <IdentificationCard size={30} weight="duotone" />,

      },
      {
        component: CNavItem,
        name: 'Data Market',
        to: '/data-market',
        icon: <StorefrontIcon sx={{ fontSize: 28 }} />,

      },
      {
        component: CNavItem,
        name: 'Khảo Sát Đầu Vào',
        to: '/filter-survey/table',
        icon: <DescriptionOutlinedIcon sx={{ fontSize: 29 }} />,

      },
      {
        component: CNavItem,
        name: 'Giao dịch',
        to: '/transactions/table',
        icon: <PaidOutlinedIcon sx={{ fontSize: 29 }} />,

      }

    ],
    // design_staff ID: 3
    [
      // {
      //   component: CNavItem,
      //   name: 'Dashboard',
      //   to: '/dashboard',
      //   // icon: <Gauge size={30}  weight="duotone" />,
      // },
      {
        component: CNavTitle,
        name: 'Orders Management',
      },
      {
        component: CNavGroup,
        show: true,
        name: 'Assigned Orders',
        to: '/orders_design_staff',
        icon: <ShoppingCartSimple size={30} weight="duotone" />,
        items: [
          {
            component: CNavItem,
            name: 'Main Orders List',
            to: '/orders_design_staff/table',
            icon: <ArrowCircleRight size={13} color="lightsalmon" weight="duotone" />
          },
          {
            component: CNavItem,
            name: 'Design Processes',
            to: '/orders_design_staff/design_process',
            icon: <ArrowCircleRight size={13} color="lightsalmon" weight="duotone" />
          },
        ],
      }
    ],
    // production_staff ID: 4
    [
      // {
      //   component: CNavItem,
      //   name: 'Dashboard',
      //   to: '/dashboard',
      //   // icon: <Gauge size={30}  weight="duotone" />,
      // },
      {
        component: CNavTitle,
        name: 'Orders Management',
      },
      {
        component: CNavGroup,
        show: true,
        name: 'dcm',
        to: '/orders_production_staff',
        icon: <FcFactoryBreakdown size={30} fontWeight="duotone" />,
        items: [
          {
            component: CNavItem,
            name: 'Main Orders List',
            to: '/orders_production_staff/table',
            icon: <ArrowCircleRight size={13} color="lightsalmon" weight="duotone" />
          },
          {
            component: CNavItem,
            name: 'Completed Orders',
            to: '/orders_design_staff/completed_orders',
            icon: <ArrowCircleRight size={13} color="lightsalmon" weight="duotone" />
          },
        ],
      }

    ],
  ]
  const roleNav = role_id ? _roleNav[role_id] : _roleNav[0];

  return roleNav;
}



export { get_roleNav }
