import { Dispatch } from "redux";
import { tokenStore } from "@/src/features/auth/tokenStore";
import { setCredentials, logoutLocal } from "@/src/features/auth/authSlice";
import { accountApi } from "@/src/services/account/accountApi";

export async function bootstrapAuth(dispatch: Dispatch) {
  try {
    const { token, expiresAt } = await tokenStore.getAccessWithExpiry();
    if (!token) return;

    // expired? clear and exit
    if (expiresAt && Date.now() > expiresAt) {
      await tokenStore.clearAll();
      dispatch(logoutLocal());
      return;
    }

    // set access token into redux so baseApi.prepareHeaders will attach it
    dispatch(setCredentials({ accessToken: token, user: null }));

    // fetch current user (uses baseApi.prepareHeaders to attach Authorization)
    // initiate returns a promise-like result; we don't block UI too long
    const meResult = await (dispatch as any)(
      accountApi.endpoints.getMe.initiate(undefined, { forceRefetch: true })
    );
    if (meResult?.data) {
      dispatch(setCredentials({ accessToken: token, user: meResult.data }));
    }
  } catch (e) {
    // on error, ensure clean state
    await tokenStore.clearAll();
    dispatch(logoutLocal());
  }
}
