// ...existing code...
import { ApolloClient, ApolloLink, InMemoryCache, createHttpLink } from "@apollo/client";
import { setContext } from "@apollo/client/link/context";
import { LocalStorageUtil } from "../../../../../utils/storage.util";
import { loginRequiredAlert } from "../../../../../utils/alert.util";
import { Observable } from "@apollo/client";
import { JwtUtil } from "../../../../../utils/jwt.util";
import { AppError, AppErrorType } from "../../../main/errors";
import { API_CONFIG } from "../../../../../../config";

const httpLink = createHttpLink({
  uri: API_CONFIG.GRAPHQL_API_BASE_URL,
});


const loginRequiredLink = new ApolloLink((operation, forward) => {
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

const authLink = setContext((_, { headers }) => {
  const token = LocalStorageUtil.getAuthTokenFromPersistLocalStorage();
  return {
    headers: {
      ...headers,
      Authorization: token ? `Bearer ${token}` : "",
      "ngrok-skip-browser-warning": "69420",
    },
  };
});

const loginRequiredApolloClient = new ApolloClient({
  link: loginRequiredLink.concat(authLink).concat(httpLink),
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

export { loginRequiredApolloClient };




