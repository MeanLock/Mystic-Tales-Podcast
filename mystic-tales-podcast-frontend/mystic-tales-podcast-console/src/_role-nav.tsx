import { CNavGroup, CNavTitle, CNavItem } from '@coreui/react'
import StorefrontIcon from '@mui/icons-material/Storefront';
import PaidOutlinedIcon from '@mui/icons-material/PaidOutlined';
import DescriptionOutlinedIcon from '@mui/icons-material/DescriptionOutlined';
import SettingsOutlinedIcon from '@mui/icons-material/SettingsOutlined';
import AssignmentIcon from '@mui/icons-material/Assignment';
import ManageAccountsIcon from '@mui/icons-material/ManageAccounts';
import InterpreterModeIcon from '@mui/icons-material/InterpreterMode';
import {
  Gauge,
  Users,
  ApplePodcastsLogo,
  ListDashes  ,
  ArrowCircleRight,
  Queue,
  Playlist,
  Warning,
  Money,
  MusicNote,
  AddressBook
} from "phosphor-react";
import { FcFactoryBreakdown } from "react-icons/fc";



const get_roleNav = (role_id: number, account_id: number) => {
  const _roleNav = [
    [],
    [],
    // staff : 2
    [

      {
        component: CNavGroup,
        name: 'Report',
        to: '/report',
        icon: <Warning size={30} weight="duotone" />,
        items: [
          {
            component: CNavItem,
            name: 'Show',
            to: '/report/show',
            icon: <ArrowCircleRight size={17} color="lightsalmon" weight="duotone" />
          },
          {
            component: CNavItem,
            name: 'Episode',
            to: '/report/episode',
            icon: <ArrowCircleRight size={17} color="lightsalmon" weight="duotone" />
          },
          {
            component: CNavItem,
            name: 'Buddy',
            to: '/report/buddy',
            icon: <ArrowCircleRight size={17} color="lightsalmon" weight="duotone" />
          },
        ],
      },
      {
        component: CNavItem,
        name: 'Episode Publish',
        to: '/staff/publish-review-sessions',
        icon: <Playlist size={30} weight="duotone" />
      },
      {
        component: CNavItem,
        name: 'Channels',
        to: '/channel/table',
        icon: <ListDashes size={30} weight="duotone" />,
      },
      {
        component: CNavItem,
        name: 'Shows',
        to: '/show/table',
      icon: <Queue  size={30} weight="duotone" />,
      },
  {
    component: CNavItem,
    name: 'Booking ',
    to: '/staff/booking/table',
    icon: <AddressBook size={30} weight="duotone" />
      },

{
  component: CNavItem,
    name: 'DMCA Accusation',
      to: '/staff/dmca-accusation/table',
        icon: <AssignmentIcon sx={{ fontSize: 32 }} />,

      },

// {
//   component: CNavItem,
//     name: 'Transactions',
//       to: '/transactions/table',
//         icon: <PaidOutlinedIcon sx={{ fontSize: 29 }} />,

//       }

    ],
// admin: 3
[
  {
    component: CNavItem,
    name: 'Dashboard',
    to: '/dashboard',
    icon: <Gauge size={30} weight="duotone" />,
  },
  {
    component: CNavItem,
    name: 'Customer',
    to: '/customer/table',
    icon: <Users size={30} weight="duotone" />,

  },
  {
    component: CNavItem,
    name: 'Podcaster',
    to: '/podcaster/table',
    icon: <InterpreterModeIcon sx={{ fontSize: 32 }} />,

  },
  {
    component: CNavItem,
    name: 'Staff',
    to: '/staff/table',
    icon: <ManageAccountsIcon sx={{ fontSize: 32 }} />,

  },
  {
    component: CNavGroup,
    name: 'Channel',
    to: '/channel',
    icon: <ListDashes size={30} weight="duotone" />,
    items: [
      {
        component: CNavItem,
        name: 'Channels',
        to: '/channel/table',
        icon: <ArrowCircleRight size={17} color="lightsalmon" weight="duotone" />
      }
    ],
  },
  {
    component: CNavGroup,
    name: 'Show',
    to: '/show',
    icon: <Queue size={30} weight="duotone" />,
    items: [
      {
        component: CNavItem,
        name: 'Shows',
        to: '/show/table',
        icon: <ArrowCircleRight size={17} color="lightsalmon" weight="duotone" />
      },
      {
        component: CNavItem,
        name: 'Report',
        to: '/show/report',
        icon: <ArrowCircleRight size={17} color="lightsalmon" weight="duotone" />
      },
      {
        component: CNavItem,
        name: 'Report Review',
        to: '/show/report-review-sessions',
        icon: <ArrowCircleRight size={17} color="lightsalmon" weight="duotone" />
      },
    ],
  },
  {
    component: CNavGroup,
    name: 'Episode',
    to: '/episode',
    icon: <Playlist size={30} weight="duotone" />,
    items: [
      {
        component: CNavItem,
        name: 'Report',
        to: '/episode/report',
        icon: <ArrowCircleRight size={17} color="lightsalmon" weight="duotone" />
      },
      {
        component: CNavItem,
        name: 'Report Review',
        to: '/episode/report-review-sessions',
        icon: <ArrowCircleRight size={17} color="lightsalmon" weight="duotone" />
      },
      {
        component: CNavItem,
        name: 'Publish Review',
        to: '/episode/publish-review-sessions',
        icon: <ArrowCircleRight size={17} color="lightsalmon" weight="duotone" />
      },
    ],
  },
  {
    component: CNavGroup,
    name: 'Buddy',
    to: '/buddy',
    icon: <ApplePodcastsLogo size={30} weight="duotone" />,
    items: [
      {
        component: CNavItem,
        name: 'Report',
        to: '/buddy/report',
        icon: <ArrowCircleRight size={17} color="lightsalmon" weight="duotone" />
      },
      {
        component: CNavItem,
        name: 'Report Review',
        to: '/buddy/report-review-sessions',
        icon: <ArrowCircleRight size={17} color="lightsalmon" weight="duotone" />
      },
    ],
  },
  {
    component: CNavItem,
    name: 'Booking',
    to: '/booking/table',
    icon: <AddressBook size={30} weight="duotone" />,

  },
  {
    component: CNavItem,
    name: 'Background Sound',
    to: '/background-sound/table',
    icon: <MusicNote size={30} weight="duotone" />,

  },
  {
    component: CNavGroup,
    name: 'Transactions',
    to: '/transactions',
    icon: <Money size={30} weight="duotone" />,
    items: [
      {
        component: CNavItem,
        name: 'Withdrawal',
        to: '/transactions/withdrawal',
        icon: <ArrowCircleRight size={17} color="lightsalmon" weight="duotone" />
      },
      {
        component: CNavItem,
        name: 'Booking',
        to: '/transactions/bookings/holding',
        icon: <ArrowCircleRight size={17} color="lightsalmon" weight="duotone" />
      },
      {
        component: CNavItem,
        name: 'Subscription',
        to: '/transactions/subscriptions/holding',
        icon: <ArrowCircleRight size={17} color="lightsalmon" weight="duotone" />
      },
    ],
  },
  {
    component: CNavItem,
    name: 'DMCA Accusation',
    to: '/dmca-accusation/table',
    icon: <AssignmentIcon sx={{ fontSize: 32 }} />,

  },
  // {
  //   component: CNavItem,
  //   name: 'System Config',
  //   to: '/system-configuration',
  //   icon: <SettingsOutlinedIcon sx={{ fontSize: 29 }} />,

  // }

],
  ]
const roleNav = role_id ? _roleNav[role_id] : _roleNav[0];
return roleNav;
}



export { get_roleNav }
