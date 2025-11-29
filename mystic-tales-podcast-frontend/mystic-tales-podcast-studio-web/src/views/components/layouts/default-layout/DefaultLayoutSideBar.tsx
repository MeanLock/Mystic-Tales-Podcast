import { useEffect, useMemo } from "react"
import { Box, Avatar, Typography, List, ListItem, ListItemIcon, ListItemText, Icon } from "@mui/material"
import { Link, useLocation, useNavigate } from "react-router-dom"
import { useDispatch, useSelector } from "react-redux"
import { RootState } from "@/redux/rootReducer"
import Image from "../../common/image"
import { _channelDetailNav, _podcasterNav, _showDetailNav } from "@/router/_roleNav"



const DefaultLayoutSideBar = () => {
    const location = useLocation()
    const dispatch = useDispatch()
    const uiSlice = useSelector((state: RootState) => state.ui)
    const navigation = useSelector((state: RootState) => state.navigation);

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

    const navItems = useMemo(() => {
        const ctxId = navigation.currentContext?.id;
        switch (navigation.contextType) {
            case 'channel':
                return _channelDetailNav.map((item) => ({
                    ...item,
                    path: item.path.replace(':id', ctxId || '')
                }));
            case 'show':
                return _showDetailNav.map((item) => ({
                    ...item,
                    path: item.path.replace(':id', ctxId || '')
                }));
            default:
                return _podcasterNav;
        }
    }, [navigation.contextType, navigation.currentContext?.id]);

    if (!navigation.currentContext) {
        return null
    }
        const isActiveRoute = (pathname: string, itemPath: string) => {
        if (pathname === itemPath || pathname.startsWith(itemPath + '/')) return true;

        if (itemPath === '/booking/table' && /^\/booking\/[^/]+$/.test(pathname)) return true;

        return false;
    };
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
                {/* <Avatar src={navigation.currentContext.avatar} className="default-layout__sidebar-profile-avatar">
                    {navigation.currentContext.name.charAt(0).toUpperCase()}
                </Avatar> */}
                {navigation.currentContext.id === "user" ? (
                    <Image
                        mainImageUrl={navigation.currentContext.avatar}
                        alt={navigation.currentContext.name}
                        className="default-layout__sidebar-profile-avatar rounded-full"
                    />
                ) : (
                    <Image
                        mainImageFileKey={navigation.currentContext.avatar}
                        alt={navigation.currentContext.name}
                        className="default-layout__sidebar-profile-avatar rounded-full"
                    />
                )}

                <Typography className="default-layout__sidebar-profile-name">{navigation.currentContext.name}</Typography>

                {navigation.currentContext.email && (
                    <Typography className="default-layout__sidebar-profile-email">{navigation.currentContext.email}</Typography>
                )}

                {navigation.currentContext.type && (
                    <Typography className="default-layout__sidebar-profile-type">{navigation.currentContext.type}</Typography>
                )}
            </Box>

            {/* Navigation Items */}
            <Box className="default-layout__sidebar-nav">
                <List sx={{ padding: 0 }}>
                    {navItems.map((item) => {
                        const isActive = isActiveRoute(location.pathname, item.path);

                        return (
                            <ListItem
                                key={item.path}
                                component={Link}
                                to={item.path}
                                className={`gap-3 default-layout__sidebar-nav-item ${isActive ? "default-layout__sidebar-nav-item--active" : ""}`}
                            >
                                <ListItemIcon className="default-layout__sidebar-nav-item-icon">
                                    {item.icon}
                                </ListItemIcon>
                                <ListItemText primary={item.label} className="default-layout__sidebar-nav-item-text" />
                            </ListItem>
                        );
                    })}
                </List>
            </Box>
        </Box>
    )
}

export default DefaultLayoutSideBar
