"use client"

import { useEffect } from "react"
import { Box, Avatar, Typography, List, ListItem, ListItemIcon, ListItemText, Icon } from "@mui/material"
import { Link, useLocation } from "react-router-dom"
import { useDispatch, useSelector } from "react-redux"
import { RootState } from "@/redux/rootReducer"
import DashboardOutlinedIcon from '@mui/icons-material/DashboardOutlined';
import {
    ApplePodcastsLogo,
    ListBullets,
    Money,
    Queue,
    Copyright
} from "phosphor-react";
export const mockNavigation = {
    // user context (podcaster)
    userContext: {
        id: "u_1",
        name: "Trần QUANG PHÁT THỊNH",
        email: "samurice@gmail.com",
        avatar: "https://lumiere-a.akamaihd.net/v1/images/a_avatarpandorapedia_neytiri_16x9_1098_01_0e7d844a.jpeg?region=420%2C0%2C1080%2C1080",
        subtitle: "CREATOR",
        type: "user",
    },

    // show context (example when user opened a specific show)
    showContext: {
        id: "s_42",
        name: "Mystic Tales - Episode 42",
        email: "", // not required for show
        avatar: "/assets/images/show-cover.jpg",
        subtitle: "SHOW",
        type: "show",
    },

    // nav items for user/podcaster context
    userNavItems: [
        { label: "Dashboard", icon: <DashboardOutlinedIcon style={{fontSize:'24px'}} />, path: "/dashboard" },
        { label: "Earn", icon: <Money size={24} />, path: "/earn" },
        { label: "Copyright", icon: <Copyright size={24} />, path: "/copyright" },
        { label: "My Channels", icon: <ApplePodcastsLogo size={24} />, path: "/my-channels" },
        { label: "My Shows", icon: <Queue size={24} />, path: "/my-shows" },
        { label: "Booking Management", icon: <ListBullets size={24} />, path: "/booking-management" },
    ],

    // nav items for a specific show context
    showNavItems: [
        { label: "Overview", icon: "info", path: "/show/42/overview" },
        { label: "Episodes", icon: "queue_music", path: "/show/42/episodes" },
        { label: "Analytics", icon: "insights", path: "/show/42/analytics" },
        { label: "Guests", icon: "groups", path: "/show/42/guests" },
        { label: "Settings", icon: "settings", path: "/show/42/settings" },
    ],
}

// helper that returns the shape your Sidebar expects
export function makeNavigationFor(context: "user" | "show" = "user") {
    if (context === "show") {
        return {
            currentContext: mockNavigation.showContext,
            navItems: mockNavigation.showNavItems,
        }
    }
    return {
        currentContext: mockNavigation.userContext,
        navItems: mockNavigation.userNavItems,
    }
}

const DefaultLayoutSideBar = () => {
    const location = useLocation()
    const dispatch = useDispatch()
    const uiSlice = useSelector((state: RootState) => state.ui)
    const navigation = makeNavigationFor("user")

    useEffect(() => {
        const handleResize = () => {
            const isMobile = window.innerWidth <= 768
            if (isMobile && !uiSlice.sidebarNarrow) {
                dispatch({ type: "ui/set", payload: { sidebarNarrow: true } })
            }
        }

        handleResize()
        window.addEventListener("resize", handleResize)
        return () => window.removeEventListener("resize", handleResize)
    }, [dispatch, uiSlice.sidebarNarrow])

    if (!navigation.currentContext) {
        return null
    }

    const sidebarClassName = [
        "default-layout__sidebar",
        uiSlice.sidebarNarrow && window.innerWidth > 768 ? "default-layout__sidebar--narrow" : "",
        uiSlice.sidebarMobileOpen && window.innerWidth <= 768 ? "default-layout__sidebar--mobile-open" : "",
    ]
        .filter(Boolean)
        .join(" ")

    return (
        <Box className={sidebarClassName}>
            {/* Profile Section */}
            <Box className="default-layout__sidebar-profile">
                <Avatar src={navigation.currentContext.avatar} className="default-layout__sidebar-profile-avatar">
                    {navigation.currentContext.name.charAt(0).toUpperCase()}
                </Avatar>

                <Typography className="default-layout__sidebar-profile-name">{navigation.currentContext.name}</Typography>

                {navigation.currentContext.email && (
                    <Typography className="default-layout__sidebar-profile-email">{navigation.currentContext.email}</Typography>
                )}

            </Box>

            {/* Navigation Items */}
            <Box className="default-layout__sidebar-nav">
                <List sx={{ padding: 0 }}>
                    {navigation.navItems.map((item) => (
                        <ListItem
                            key={item.path}
                            component={Link}
                            to={item.path}
                            className={`gap-3 default-layout__sidebar-nav-item ${location.pathname === item.path ? "default-layout__sidebar-nav-item--active" : ""}`}
                        >
                            <ListItemIcon className="default-layout__sidebar-nav-item-icon">
                                {item.icon}
                            </ListItemIcon>
                            <ListItemText primary={item.label} className="default-layout__sidebar-nav-item-text" />
                        </ListItem>
                    ))}
                </List>
            </Box>
        </Box>
    )
}

export default DefaultLayoutSideBar
