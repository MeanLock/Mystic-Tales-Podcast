import { useCallback } from 'react';
import { useDispatch } from 'react-redux';
import { setUserContext, setShowContext } from '../../redux/navigation/navigationSlice';
import { _podcasterNav } from '../../router/_roleNav';

// Navigation items for show context
const _showNav = [
  {
    label: "Show Dashboard",
    icon: "dashboard",
    path: "/show/:id/dashboard",
  },
  {
    label: "Episodes",
    icon: "library_music",
    path: "/show/:id/episodes",
  },
  {
    label: "Analytics",
    icon: "analytics",
    path: "/show/:id/analytics",
  },
  {
    label: "Settings",
    icon: "settings",
    path: "/show/:id/settings",
  },
  {
    label: "Back to Profile",
    icon: "arrow_back",
    path: "/dashboard",
  },
];

export const useNavigationContext = () => {
  const dispatch = useDispatch();

  const switchToUserContext = useCallback((user: {
    id: string;
    name: string;
    email: string;
    avatar?: string;
  }) => {
    dispatch(setUserContext({
      user,
      navItems: _podcasterNav
    }));
  }, [dispatch]);

  const switchToShowContext = useCallback((show: {
    id: string;
    name: string;
    avatar?: string;
  }) => {
    // Replace :id with actual show id in navigation paths
    const showNavItems = _showNav.map(item => ({
      ...item,
      path: item.path.replace(':id', show.id)
    }));

    dispatch(setShowContext({
      show,
      navItems: showNavItems
    }));
  }, [dispatch]);

  return {
    switchToUserContext,
    switchToShowContext
  };
};