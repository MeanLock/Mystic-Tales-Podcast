import DashboardOutlinedIcon from '@mui/icons-material/DashboardOutlined';
import {
  ApplePodcastsLogo,
  ListBullets,
  Money,
  Queue,
  Copyright,
  Info
} from "phosphor-react";
import React, { JSX } from 'react';

export const _podcasterNav: {
  label: string;
  icon: JSX.Element;
  path: string;
}[] = [
    {
      label: "Dashboard",
      path: "/dashboard",
      icon: React.createElement(DashboardOutlinedIcon, { style: { fontSize: '24px' } }),
    },
    {
      label: "Earn",
      icon: React.createElement(Money, { size: 24 }),
      path: "/earn",
    },
    {
      label: "Copyright",
      icon: React.createElement(Copyright, { size: 24 }),
      path: "/copyright",
    },
    {
      label: "My Channels",
      icon: React.createElement(ApplePodcastsLogo, { size: 24 }),
      path: "/my-channels",
    },
    {
      label: "My Shows",
      icon: React.createElement(Queue, { size: 24 }),
      path: "/my-shows",
    },
    {
      label: "Booking Management",
      icon: React.createElement(ListBullets, { size: 24 }),
      path: "/booking-management",
    },
  ];

export const _channelDetailNav: {
  label: string;
  icon: JSX.Element;
  path: string;
}[] = [
    {
      label: "Dashboard",
      path: "/my-channel/:id/dashboard",
      icon: React.createElement(DashboardOutlinedIcon, { style: { fontSize: '24px' } }),
    },
    {
      label: "Overview",
      path: "/my-channel/:id/overview",
      icon: React.createElement(Info, { size: 24 }),
    },
     {
      label: "Shows",
      path: "/my-channel/:id/show",
      icon: React.createElement(Queue, { size: 24 }),
    },
    {
      label: "Earn",
      path: "/my-channel/:id/earn",
      icon: React.createElement(DashboardOutlinedIcon, { style: { fontSize: '24px' } }),
    },


  ];

