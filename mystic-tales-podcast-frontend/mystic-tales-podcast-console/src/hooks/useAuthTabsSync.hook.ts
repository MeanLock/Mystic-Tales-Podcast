import { useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { useNavigate } from 'react-router-dom';
import { jwtDecode } from "jwt-decode";
import { clearAuthToken, setAuthToken } from '../redux/auth/auth.slice';
import { JwtUtil } from '../core/utils/jwt.util';
import { LocalStorageUtil } from '../core/utils/storage.util';
import { RootState } from '../redux/root-reducer';

const useAuthTabsSync = () => {
  const dispatch = useDispatch();
  const authSlice = useSelector((state: RootState) => state.auth);

  useEffect(() => {
    const handleStorageChange = () => {
      const token = LocalStorageUtil.getAuthTokenFromPersistLocalStorage();
      const user = LocalStorageUtil.getAuthUserFromPersistLocalStorage();
      if (!token) {
        dispatch(clearAuthToken());
        window.location.href = '/login';
      } else if (authSlice) {
        const tokenValid = JwtUtil.isTokenNotExpired(token);
        if (tokenValid && token !== authSlice.token) {
          dispatch(setAuthToken({ token, user }));
        } else if (!tokenValid) {
          dispatch(clearAuthToken());
          window.location.href = '/login';
        }
      }
    };
    window.addEventListener('storage', handleStorageChange);
    return () => window.removeEventListener('storage', handleStorageChange);
  }, [authSlice, dispatch]);
};

export default useAuthTabsSync;
