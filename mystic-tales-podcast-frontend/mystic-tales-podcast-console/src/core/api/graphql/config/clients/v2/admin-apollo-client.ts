// ...existing code...
import { ApolloClient, ApolloLink, InMemoryCache, Observable, createHttpLink } from "@apollo/client";
import { setContext } from "@apollo/client/link/context";
import { LocalStorageUtil } from "../../../../../utils/storage.util";
import { loginRequiredAlert } from "../../../../../utils/alert.util";
import { JwtUtil } from "../../../../../utils/jwt.util";
import { AppError, AppErrorType } from "../../../main/errors";
import { API_CONFIG } from "../../../../../../config";

const httpLink = createHttpLink({
  uri: API_CONFIG.GRAPHQL_API_BASE_URL,
});


const adminAuthLink = new ApolloLink((operation, forward) => {
  return new Observable((observer) => {
    const token = LocalStorageUtil.getAuthTokenFromPersistLocalStorage();

    if (!token) {
      loginRequiredAlert().then(() => observer.error(new AppError(AppErrorType.LoginRequired, "Login required")));
      return;
    }
    if (!JwtUtil.isTokenValid(token)) {
      loginRequiredAlert().then(() => observer.error(new AppError(AppErrorType.Unauthorized, "Unauthorized")));
      return;
    }
    if (!JwtUtil.isTokenNotExpired(token)) {
      loginRequiredAlert().then(() => observer.error(new AppError(AppErrorType.TokenExpired, "Login expired")));
      return;
    }

    const user = JwtUtil.decodeToken(token);
    if (!user || ![1, 2].includes(Number(user.role_id))) {
      observer.error(new AppError(AppErrorType.Forbidden, "No permission"));
      return;
    }

    // Nếu hợp lệ, gửi request
    if (forward) {
      forward(operation).subscribe({
        next: observer.next.bind(observer),
        error: observer.error.bind(observer),
        complete: observer.complete.bind(observer),
      });
    }
  });
});


const adminLink = setContext((_, { headers }) => {
  const token = LocalStorageUtil.getAuthTokenFromPersistLocalStorage();
  return {
    headers: {
      ...headers,
      Authorization: `Bearer ${token}`,
      "ngrok-skip-browser-warning": "69420",
    },
  };
});

const adminApolloClient = new ApolloClient({
  link: adminAuthLink.concat(adminLink).concat(httpLink),
  cache: new InMemoryCache(),
  defaultOptions: {
    //** [fetchPolicy: "no-cache"]: không cache dữ liệu (network first)
    watchQuery: {
      fetchPolicy: "no-cache",
    },
    query: {
      fetchPolicy: "no-cache",
    },
    mutate: {
      fetchPolicy: "no-cache",
    },
  },
});

export { adminApolloClient };