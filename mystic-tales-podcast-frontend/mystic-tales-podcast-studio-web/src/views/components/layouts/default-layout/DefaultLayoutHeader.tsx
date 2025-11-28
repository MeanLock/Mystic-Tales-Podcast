import { useState, type FC } from "react"
import { AppBar, Toolbar, Box, IconButton, Button, InputBase, Avatar, Menu as MuiMenu, MenuItem, ListItemIcon, ListItemText, Typography, Divider, ButtonBase } from "@mui/material";
import { Search as SearchIcon, Upload, ExitToApp, Menu as MenuIcon, NotificationsNoneOutlined, Person } from "@mui/icons-material";
import { useNavigate } from "react-router-dom"
import { useDispatch, useSelector } from "react-redux"
import logo from "../../../../assets/logoMTP2.png"
import { set } from "@/redux/ui/uiSlice"
import { clearAuthToken } from "@/redux/auth/authSlice"
import { RootState } from "@/redux/rootReducer"

export const DefaultLayoutHeader: FC = () => {
  const navigate = useNavigate()
  const dispatch = useDispatch()
  const uiSlice = useSelector((state: RootState) => state.ui)
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);

  const handleLogout = () => {
    setAnchorEl(null);
    dispatch(clearAuthToken())
    navigate("/login")
  }
  const handleOpenProfileMenu = (e: React.MouseEvent<HTMLElement>) => {
    setAnchorEl(e.currentTarget);
  };
  const handleCloseProfileMenu = () => setAnchorEl(null);

  const toggleSidebar = () => {
    const isMobile = window.innerWidth <= 768

    if (isMobile) {
      // On mobile, toggle the mobile open state
      dispatch(set({ sidebarMobileOpen: !uiSlice.sidebarMobileOpen }))
    } else {
      // On desktop, toggle the narrow state
      dispatch(set({ sidebarNarrow: !uiSlice.sidebarNarrow }))
    }
  }

  return (
    <AppBar position="fixed" className="default-layout__header">
      <Toolbar className="default-layout__header-content">
        {/* Brand */}
        <Box className="default-layout__header-brand gap-3">
          <IconButton onClick={toggleSidebar} className="default-layout__header-menu-toggle">
            <MenuIcon className="mr-4" sx={{ fontSize: "2rem", color: "white" }} />
          </IconButton>
          <ButtonBase
            onClick={() => navigate('/')}
            className="default-layout__header-brand-link"
            disableRipple
            sx={{
              display: 'flex',
              alignItems: 'center',
              gap: 1,
              outline: 'none !important',
              '&:hover': { backgroundColor: 'transparent' },
              '&.Mui-focusVisible, &:focus-visible': { backgroundColor: 'transparent', boxShadow: 'none' },
              '&:active': { backgroundColor: 'transparent' }
            }}
            aria-label="Go to home"
          >
            <img src={logo || "/placeholder.svg"} alt="Logo" className="default-layout__header-brand-logo w-10 h-10 aspect-square rounded-full object-cover" />
            <Box className="default-layout__header-brand-text">
              <span className="default-layout__header-brand-text-title">Mystic Tales</span>
              <span className="default-layout__header-brand-text-subtitle">STUDIO</span>
            </Box>
          </ButtonBase>
        </Box>

        {/* Search */}
        <Box className="default-layout__header-search">
          <Box className="default-layout__header-search-container">
            <Box className="default-layout__header-search-icon">
              <SearchIcon />
            </Box>
            <InputBase
              placeholder="Search for anything on your account ..."
              className="default-layout__header-search-input"
            />
          </Box>
        </Box>

        {/* Actions */}
        <Box className="default-layout__header-actions">
          <IconButton className="default-layout__header-actions-notification">
            <NotificationsNoneOutlined />
          </IconButton>

          <Button variant="outlined" startIcon={<Upload />} className="default-layout__header-actions-upload">
            Upload
          </Button>

          {/* <IconButton className="default-layout__header-actions-logout" onClick={handleLogout} title="Logout">
            <ExitToApp />
          </IconButton> */}
          <IconButton
            onClick={handleOpenProfileMenu}
            className="default-layout__header-actions-avatar"
            aria-controls={anchorEl ? "profile-menu" : undefined}
            aria-haspopup="true"
            aria-expanded={anchorEl ? "true" : undefined}
          >
            <Avatar src="https://lumiere-a.akamaihd.net/v1/images/a_avatarpandorapedia_neytiri_16x9_1098_01_0e7d844a.jpeg?region=420%2C0%2C1080%2C1080" className="ml-3" />
          </IconButton>

          <MuiMenu
            id="profile-menu"
            anchorEl={anchorEl}
            open={Boolean(anchorEl)}
            onClose={handleCloseProfileMenu}
            anchorOrigin={{ vertical: "bottom", horizontal: "right" }}
            transformOrigin={{ vertical: "top", horizontal: "right" }}
            disableScrollLock
            className="default-layout__header-actions-menu"
          >
            <MenuItem className="default-layout__header-actions-menu-item  gap-1" onClick={() => { handleCloseProfileMenu(); navigate("/profile"); }}>
              <ListItemIcon><Person fontSize="small" /></ListItemIcon>
              <ListItemText>Profile</ListItemText>
            </MenuItem>
            <MenuItem className="default-layout__header-actions-menu-item gap-1" onClick={handleLogout}>
              <ListItemIcon><ExitToApp fontSize="small" /></ListItemIcon>
              <ListItemText>Logout</ListItemText>
            </MenuItem>
          </MuiMenu>
        </Box>
      </Toolbar>
    </AppBar>
  )
}
