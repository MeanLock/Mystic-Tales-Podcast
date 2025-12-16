import { useCallback } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { setChannelContext, setShowContext, setUserContext, } from '../../redux/navigation/navigationSlice';
import { RootState } from '@/redux/rootReducer';

export const useNavigationContext = () => {
  const dispatch = useDispatch();
  const authUser = useSelector((state: RootState) => state.auth.user);

  const switchToUserContext = (user: any) => {
    // only store serializable user context
    dispatch(setUserContext({ user }));
  };

  const switchToChannelContext = (channel: any) => {
    dispatch(setChannelContext({ channel }));
  };
  const switchToShowContext = (show: any) => {
    dispatch(setShowContext({ show }));
  };

const switchContextByType = (contextInfo: { type: string; id: string; basePath: string; name?: string; avatar?: string }) => {
    switch (contextInfo.type) {
      case 'channel':
        const channelInfo = {
          id: contextInfo.id,
          name: contextInfo.name || `Unknown Channel`,
          avatar: contextInfo.avatar 
        };
        switchToChannelContext(channelInfo);
        break;

      case 'show':
        const showInfo = {
          id: contextInfo.id,
          name: contextInfo.name || `Unknown Show`,
          avatar: contextInfo.avatar 
        };
        switchToShowContext(showInfo);
        break;

      default:
        if (!authUser) return;
        const user = {
          id: 'user',
          type: 'user',
          name: authUser.PodcasterProfile?.Name || authUser.FullName || authUser.Name,
          email: authUser.Email,
          avatar: authUser.MainImageFileKey,
        };
        switchToUserContext(user);
        break;
    }
  };

  return {
    switchToUserContext,
    switchToChannelContext,
    switchContextByType,
  };
};