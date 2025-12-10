import DashboardOutlinedIcon from '@mui/icons-material/DashboardOutlined';
import FeedOutlinedIcon from '@mui/icons-material/FeedOutlined';
import {
  ApplePodcastsLogo,
  ListBullets,
  Money,
  Queue,
  Copyright,
  CreditCard ,
  Equalizer  
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
      label: "My Channels",
      icon: React.createElement(ApplePodcastsLogo, { size: 24 }),
      path: "/channel",
    },
    {
      label: "My Shows",
      icon: React.createElement(Queue, { size: 24 }),
      path: "/show",
    },
    {
      label: "Booking Management",
      icon: React.createElement(ListBullets, { size: 24 }),
      path: "/booking/table",
    },
    {
      label: "Withdrawal",
      icon: React.createElement(Money, { size: 24 }),
      path: "/withdrawal",
    },
    // {
    //   label: "Copyright",
    //   icon: React.createElement(Copyright, { size: 24 }),
    //   path: "/copyright",
    // },
  ];

export const _channelDetailNav: {
  label: string;
  icon: JSX.Element;
  path: string;
}[] = [
    // {
    //   label: "Dashboard",
    //   path: "/my-channel/:id/dashboard",
    //   icon: React.createElement(DashboardOutlinedIcon, { style: { fontSize: '24px' } }),
    // },
    {
      label: "Overview",
      path: "/channel/:id/overview",
      icon: React.createElement(FeedOutlinedIcon, { style: { fontSize: '24px' } }),
    },
    {
      label: "Subscription",
      path: "/channel/:id/subscription",
      icon: React.createElement(CreditCard , { style: { fontSize: '24px' } }),
    },
     {
      label: "Shows",
      path: "/channel/:id/show",
      icon: React.createElement(Queue, { size: 24 }),
    },
    // {
    //   label: "Earn",
    //   path: "/channel/:id/earn",
    //   icon: React.createElement(DashboardOutlinedIcon, { style: { fontSize: '24px' } }),
    // },


  ];

export const _showDetailNav: {
  label: string;
  icon: JSX.Element;
  path: string;
}[] = [
    // {
    //   label: "Dashboard",
    //   path: "/my-channel/:id/dashboard",
    //   icon: React.createElement(DashboardOutlinedIcon, { style: { fontSize: '24px' } }),
    // },
    {
      label: "Overview",
      path: "/show/:id/overview",
      icon: React.createElement(FeedOutlinedIcon, { style: { fontSize: '24px' } }),
    },
    {
      label: "Subscription",
      path: "/show/:id/subscription",
      icon: React.createElement(CreditCard , { style: { fontSize: '24px' } }),
    },
     {
      label: "Episodes",
      path: "/show/:id/episode",
      icon: React.createElement(Queue, { size: 24 }),
    },
    // {
    //   label: "Earn",
    //   path: "/channel/:id/earn",
    //   icon: React.createElement(DashboardOutlinedIcon, { style: { fontSize: '24px' } }),
    // },


  ];