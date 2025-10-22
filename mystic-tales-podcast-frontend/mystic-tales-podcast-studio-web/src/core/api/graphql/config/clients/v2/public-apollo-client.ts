// ...existing code...
import { ApolloClient, ApolloLink, InMemoryCache, createHttpLink } from "@apollo/client";
import { setContext } from "@apollo/client/link/context";
import { LocalStorageUtil } from "../../../../../utils/storage.util";
import { API_CONFIG } from "../../../../../../config";

const httpLink = createHttpLink({
  uri: API_CONFIG.GRAPHQL_API_BASE_URL,
});

const publicAuthLink = setContext((_, { headers }) => {
  const token = LocalStorageUtil.getAuthTokenFromPersistLocalStorage();
  return {
    headers: {
      ...headers,
      Authorization: token ? `Bearer ${token}` : "", 
      "ngrok-skip-browser-warning": "69420", 
    },
  };
});

const publicApolloClient = new ApolloClient({
  link: publicAuthLink.concat(httpLink),
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

export { publicApolloClient };


