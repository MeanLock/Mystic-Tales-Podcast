// ...existing code...
import { ApolloClient, InMemoryCache } from "@apollo/client";
import { GraphQLWsLink } from "@apollo/client/link/subscriptions";
import { createClient } from "graphql-ws";

import { LocalStorageUtil } from "../../../../../utils/storage.util";
import { loginRequiredAlert } from "../../../../../utils/alert.util";
import { JwtUtil } from "../../../../../utils/jwt.util";
import { AppError, AppErrorType } from "../../../main/errors";
import { API_CONFIG } from "../../../../../../config";

// 👉 Extract host from GraphqlApiBaseUrl and replace http -> ws
const wsUrl = API_CONFIG.GRAPHQL_API_WS_URL;

const wsLink = new GraphQLWsLink(
  createClient({
    url: wsUrl,
    connectionParams: async () => {
      const token = LocalStorageUtil.getAuthTokenFromPersistLocalStorage();

      if (!token) {
        await loginRequiredAlert();
        throw new AppError(AppErrorType.LoginRequired, "Login required");
      }

      if (!JwtUtil.isTokenValid(token)) {
        await loginRequiredAlert();
        throw new AppError(AppErrorType.Unauthorized, "Unauthorized");
      }

      if (!JwtUtil.isTokenNotExpired(token)) {
        await loginRequiredAlert();
        throw new AppError(AppErrorType.TokenExpired, "Login expired");
      }

      return {
        Authorization: `Bearer ${token}`,
        "ngrok-skip-browser-warning": "69420", // nếu bạn cần giữ behavior này
      };
    },
    lazy: true, // chỉ kết nối khi cần
    retryAttempts: 3,
  })
);

const wsApolloClient = new ApolloClient({
  link: wsLink,
  cache: new InMemoryCache(),
  defaultOptions: {
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

export { wsApolloClient };
