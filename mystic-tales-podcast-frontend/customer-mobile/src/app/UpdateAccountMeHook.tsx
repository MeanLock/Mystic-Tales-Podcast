import { useSelector } from "react-redux";
import { useUpdateAccountMeQuery } from "../core/services/account/account.service";
import { RootState } from "../store/store";

const UpdateAccountMeHook = () => {
  // REDUX
  const user = useSelector((state: RootState) => state.auth.accessToken);

  const {
    data: accountData,
    isLoading,
    isError,
    refetch,
  } = useUpdateAccountMeQuery(undefined, {
    skip: !user,
    refetchOnFocus: true,
    refetchOnReconnect: true,
    pollingInterval: 30000,
  });
  return null;
};
export default UpdateAccountMeHook;
