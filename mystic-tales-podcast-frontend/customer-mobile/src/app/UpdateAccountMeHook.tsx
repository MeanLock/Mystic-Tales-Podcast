import { useDispatch, useSelector } from "react-redux";
import { useUpdateAccountMeQuery } from "../core/services/account/account.service";
import { RootState } from "../store/store";
import { useEffect } from "react";
import { logoutLocal } from "../features/auth/authSlice";
import { useRouter } from "expo-router";
import { usePlayer } from "../core/services/player/usePlayer";

const UpdateAccountMeHook = () => {
  // REDUX
  const user = useSelector((state: RootState) => state.auth.accessToken);
  const dispatch = useDispatch();
  const router = useRouter();
  const { stop } = usePlayer();

  const {
    data: accountData,
    isLoading,
    isError,
    refetch,
  } = useUpdateAccountMeQuery(undefined, {
    skip: !user,
    refetchOnFocus: true,
    refetchOnReconnect: true,
    pollingInterval: 60 * 60 * 1000, // 1 hour
  });

  // Handle error - logout and redirect to login
  useEffect(() => {
    if (isError && user) {
      // Only logout if user exists (was logged in)
      dispatch(logoutLocal());
      stop();
      router.replace("/(auth)/login");
    }
  }, [isError, user, dispatch, stop, router]);

  return null;
};
export default UpdateAccountMeHook;
