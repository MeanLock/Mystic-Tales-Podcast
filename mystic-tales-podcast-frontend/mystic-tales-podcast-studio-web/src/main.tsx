import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import App from "./App";
import { Provider } from "react-redux";
import { PersistGate } from "redux-persist/integration/react";
import { BrowserRouter } from "react-router-dom";
import { GoogleOAuthProvider } from "@react-oauth/google";
import { persistor, store } from "./redux/store";
import { AllCommunityModule, ModuleRegistry } from "ag-grid-community";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import queryClient from "@/lib/query";
import { ThemeProvider } from "@mui/material";
import theme from "./views/components/common/mui-ui/theme";
import "./index.css";
import "./index.scss";
ModuleRegistry.registerModules([AllCommunityModule]);
createRoot(document.getElementById("root")!).render(
  <StrictMode>
    <ThemeProvider theme={theme}>
      <Provider store={store}>
        <PersistGate loading={null} persistor={persistor}>
          <GoogleOAuthProvider
            clientId={
              "1050992099919-sr1eikt6f232885198as96ub9fo9hkea.apps.googleusercontent.com"
            }
          >
            <QueryClientProvider client={queryClient}>
              <App />
            </QueryClientProvider>
          </GoogleOAuthProvider>
        </PersistGate>
      </Provider>
    </ThemeProvider>
  </StrictMode>
);
