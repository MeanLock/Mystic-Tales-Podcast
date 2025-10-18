import { useCallback } from 'react';
import { useDispatch } from 'react-redux';
import { setChannelContext, setUserContext,  } from '../../redux/navigation/navigationSlice';
import { _channelDetailNav, _podcasterNav } from '../../router/_roleNav';

export const useNavigationContext = () => {
  const dispatch = useDispatch();

  const switchToUserContext = (user: any) => {
    dispatch(setUserContext({ user, navItems: _podcasterNav }));
  };

  const switchToChannelContext = (channel: any) => {
     const navItemsWithId = _channelDetailNav.map(item => ({
      ...item,
      path: item.path.replace(':id', channel.id) // Thay thế :id bằng ID thực tế
    }));
    dispatch(setChannelContext({ channel, navItems: navItemsWithId }));
  };

 
  const switchContextByType = (contextInfo: { type: string; id: string; basePath: string }) => {
    switch (contextInfo.type) {
      case 'channel':
        const channelInfo = {
          id: contextInfo.id,
          name: `Channel ${contextInfo.id}`,
          avatar: `https://picsum.photos/300/300?random=${contextInfo.id}`
        };
        switchToChannelContext(channelInfo);
        break;
    
      default: 
        const user = {
          id: "u_1",
          name: "SAMURICE",
          email: "samurice@gmail.com",
          avatar: "https://lumiere-a.akamaihd.net/v1/images/a_avatarpandorapedia_neytiri_16x9_1098_01_0e7d844a.jpeg?region=420%2C0%2C1080%2C1080",
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