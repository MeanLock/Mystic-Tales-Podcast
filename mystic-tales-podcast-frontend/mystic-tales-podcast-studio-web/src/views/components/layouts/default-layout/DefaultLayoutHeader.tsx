import { use, useEffect, useState, type FC } from "react"
import { AppBar, Toolbar, Box, IconButton, Button, InputBase, Avatar, Menu as MuiMenu, MenuItem, ListItemIcon, ListItemText, Typography, Divider, ButtonBase } from "@mui/material";
import { Search as SearchIcon, Upload, ExitToApp, Menu as MenuIcon, NotificationsNoneOutlined, Person } from "@mui/icons-material";
import { useNavigate } from "react-router-dom"
import { useDispatch, useSelector } from "react-redux"
import logo from "../../../../assets/logoMTP2.png"
import { set } from "@/redux/ui/uiSlice"
import { clearAuthToken } from "@/redux/auth/authSlice"
import { RootState } from "@/redux/rootReducer"
import Modal_Button from "../../common/modal/ModalButton";
import EpisodeCreate from "@/views/pages/my-show-detail-page/episode-page/EpisodeCreate";

export const DefaultLayoutHeader: FC = () => {
  const navigate = useNavigate()
  const dispatch = useDispatch()
  const uiSlice = useSelector((state: RootState) => state.ui)
  const authSlice = useSelector((state: RootState) => state.auth);
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
  const [notificationAnchorEl, setNotificationAnchorEl] = useState<null | HTMLElement>(null);

  const handleLogout = () => {
    setAnchorEl(null);
    dispatch(clearAuthToken())
    navigate("/login")
  }
  const handleOpenProfileMenu = (e: React.MouseEvent<HTMLElement>) => {
    setAnchorEl(e.currentTarget);
  };
  const handleCloseProfileMenu = () => setAnchorEl(null);

  const handleOpenNotificationMenu = (e: React.MouseEvent<HTMLElement>) => {
    setNotificationAnchorEl(e.currentTarget);
  };
  const handleCloseNotificationMenu = () => setNotificationAnchorEl(null);

  const notifications = [
    { id: 1, title: "New Booking Request", message: "You have a new booking from John Doe", time: "5 mins ago", isRead: false },
    { id: 2, title: "Audio Approved", message: "Your submitted audio has been approved", time: "1 hour ago", isRead: false },
    { id: 3, title: "Edit Required", message: "Client requested edits on Podcast #123", time: "2 hours ago", isRead: true },
    { id: 4, title: "Payment Received", message: "Payment of 500 coins received", time: "1 day ago", isRead: true },
  ];

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
            onClick={() => window.location.replace('/dashboard')}
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
          <IconButton
            className="default-layout__header-actions-notification"
            onClick={handleOpenNotificationMenu}
            aria-controls={notificationAnchorEl ? "notification-menu" : undefined}
            aria-haspopup="true"
            aria-expanded={notificationAnchorEl ? "true" : undefined}
            sx={{ position: 'relative' }}
          >
            <NotificationsNoneOutlined />
            {notifications.filter(n => !n.isRead).length > 0 && (
              <Box
                sx={{
                  position: 'absolute',
                  top: 8,
                  right: 8,
                  width: 8,
                  height: 8,
                  borderRadius: '50%',
                  backgroundColor: '#ef5350',
                }}
              />
            )}
          </IconButton>

          <MuiMenu
            id="notification-menu"
            anchorEl={notificationAnchorEl}
            open={Boolean(notificationAnchorEl)}
            onClose={handleCloseNotificationMenu}
            anchorOrigin={{ vertical: "bottom", horizontal: "right" }}
            transformOrigin={{ vertical: "top", horizontal: "right" }}
            disableScrollLock
            PaperProps={{
              sx: {
                width: 380,
                maxHeight: 500,
                mt: 1.5,
                background: 'linear-gradient(145deg, rgba(42, 42, 42, 0.98), rgba(30, 30, 30, 0.98))',
                backdropFilter: 'blur(12px)',
                border: '1px solid rgba(174, 227, 57, 0.15)',
                borderRadius: '12px',
                boxShadow: '0 8px 32px rgba(0, 0, 0, 0.4)',
              }
            }}
          >
            <Box sx={{ p: 2, borderBottom: '1px solid rgba(255,255,255,0.1)' }}>
              <Typography sx={{ color: '#fff', fontWeight: 700, fontSize: '1rem' }}>
                Notifications
              </Typography>
              {notifications.filter(n => !n.isRead).length > 0 && (
                <Typography sx={{ color: 'var(--primary-green)', fontSize: '0.75rem', mt: 0.5 }}>
                  {notifications.filter(n => !n.isRead).length} unread
                </Typography>
              )}
            </Box>
            <Box sx={{ maxHeight: 400, overflowY: 'auto' }}>
              {notifications.length === 0 ? (
                <Box sx={{ p: 3, textAlign: 'center' }}>
                  <Typography sx={{ color: 'rgba(255,255,255,0.5)', fontSize: '0.85rem' }}>
                    No notifications
                  </Typography>
                </Box>
              ) : (
                notifications.map((notif) => (
                  <MenuItem
                    key={notif.id}
                    onClick={handleCloseNotificationMenu}
                    sx={{
                      p: 2,
                      borderBottom: '1px solid rgba(255,255,255,0.05)',
                      background: notif.isRead ? 'transparent' : 'rgba(174, 227, 57, 0.05)',
                      '&:hover': {
                        background: 'rgba(174, 227, 57, 0.1)',
                      },
                      display: 'flex',
                      flexDirection: 'column',
                      alignItems: 'flex-start',
                      gap: 0.5,
                    }}
                  >
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, width: '100%' }}>
                      <Typography sx={{ color: '#fff', fontWeight: 600, fontSize: '0.85rem', flex: 1 }}>
                        {notif.title}
                      </Typography>
                      {!notif.isRead && (
                        <Box
                          sx={{
                            width: 6,
                            height: 6,
                            borderRadius: '50%',
                            backgroundColor: 'var(--primary-green)',
                          }}
                        />
                      )}
                    </Box>
                    <Typography sx={{ color: 'rgba(255,255,255,0.7)', fontSize: '0.75rem' }}>
                      {notif.message}
                    </Typography>
                    <Typography sx={{ color: 'rgba(255,255,255,0.5)', fontSize: '0.7rem', mt: 0.5 }}>
                      {notif.time}
                    </Typography>
                  </MenuItem>
                ))
              )}
            </Box>
            <Divider sx={{ borderColor: 'rgba(255,255,255,0.1)' }} />
            <Box sx={{ p: 1.5, textAlign: 'center' }}>
              <Button
                fullWidth
                sx={{
                  color: 'var(--primary-green)',
                  textTransform: 'none',
                  fontSize: '0.8rem',
                  fontWeight: 600,
                  '&:hover': {
                    background: 'rgba(174, 227, 57, 0.1)',
                  }
                }}
                onClick={() => {
                  handleCloseNotificationMenu();
                  // Navigate to notifications page
                }}
              >
                View All Notifications
              </Button>
            </Box>
          </MuiMenu>
          <Modal_Button
            className="default-layout__header-actions-upload"
            content="Upload"
            variant="outlined"
            size='lg'
            startIcon={<Upload />}
          >
            <EpisodeCreate />
          </Modal_Button>


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
            <Avatar src={authSlice.user.mainImageUrl} className="ml-3" />
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
